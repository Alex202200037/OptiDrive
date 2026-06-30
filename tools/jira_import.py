#!/usr/bin/env python3
import argparse
import base64
import json
import os
import sys
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path


def eprint(*args):
    print(*args, file=sys.stderr)


def adf_paragraph(text):
    return {
        "type": "paragraph",
        "content": [{"type": "text", "text": text}],
    }


def to_adf(text):
    lines = [line.strip() for line in (text or "").splitlines()]
    blocks = [adf_paragraph(line) for line in lines if line]
    if not blocks:
        blocks = [adf_paragraph("Sem descricao.")]
    return {
        "version": 1,
        "type": "doc",
        "content": blocks,
    }


class JiraClient:
    def __init__(self, base_url, email, token):
        self.base_url = base_url.rstrip("/")
        credentials = f"{email}:{token}".encode("utf-8")
        self.auth_header = "Basic " + base64.b64encode(credentials).decode("ascii")

    def request(self, method, path, payload=None, query=None):
        url = self.base_url + path
        if query:
            url += "?" + urllib.parse.urlencode(query, doseq=True)

        headers = {
            "Accept": "application/json",
            "Authorization": self.auth_header,
        }
        data = None
        if payload is not None:
            headers["Content-Type"] = "application/json"
            data = json.dumps(payload).encode("utf-8")

        req = urllib.request.Request(url, method=method, headers=headers, data=data)
        try:
            with urllib.request.urlopen(req) as resp:
                raw = resp.read().decode("utf-8")
                return resp.status, json.loads(raw) if raw else {}
        except urllib.error.HTTPError as exc:
            raw = exc.read().decode("utf-8")
            body = {}
            if raw:
                try:
                    body = json.loads(raw)
                except json.JSONDecodeError:
                    body = {"raw": raw}
            raise RuntimeError(
                f"Jira request failed: {method} {path} -> {exc.code} {body}"
            ) from exc

    def validate(self):
        return self.request("GET", "/rest/api/3/myself")[1]

    def project(self, project_key):
        return self.request("GET", f"/rest/api/3/project/{project_key}")[1]

    def projects(self):
        return self.request("GET", "/rest/api/3/project/search", query={"maxResults": 100})[1]

    def priorities(self):
        return self.request("GET", "/rest/api/3/priority")[1]

    def fields(self):
        return self.request("GET", "/rest/api/3/field")[1]

    def issue_types_for_project(self, project_key):
        return self.request(
            "GET", f"/rest/api/2/issue/createmeta/{project_key}/issuetypes"
        )[1]

    def create_fields_for_issue_type(self, project_key, issue_type_id):
        return self.request(
            "GET",
            f"/rest/api/2/issue/createmeta/{project_key}/issuetypes/{issue_type_id}",
        )[1]

    def create_issue(self, fields):
        payload = {"fields": fields}
        return self.request("POST", "/rest/api/3/issue", payload=payload)[1]

    def update_issue(self, issue_key, fields):
        payload = {"fields": fields}
        return self.request("PUT", f"/rest/api/3/issue/{issue_key}", payload=payload)[1]


def truthy(value):
    return str(value or "").strip().lower() in {"1", "true", "yes", "on"}


def discover_cloud_id(site_base_url):
    tenant_url = site_base_url.rstrip("/") + "/_edge/tenant_info"
    req = urllib.request.Request(tenant_url, method="GET")
    with urllib.request.urlopen(req) as resp:
        raw = resp.read().decode("utf-8")
        payload = json.loads(raw) if raw else {}
        return payload.get("cloudId")


def load_env(name, default=None, required=False):
    value = os.getenv(name, default)
    if required and not value:
        raise SystemExit(f"Missing required environment variable: {name}")
    return value


def build_client():
    site_base_url = load_env("JIRA_BASE_URL", required=True)
    email = load_env("JIRA_EMAIL", required=True)
    token = load_env("JIRA_API_TOKEN", required=True)
    use_ex_api = truthy(load_env("JIRA_USE_EX_API", ""))

    if use_ex_api:
        cloud_id = load_env("JIRA_CLOUD_ID")
        if not cloud_id:
            cloud_id = discover_cloud_id(site_base_url)
        return JiraClient(f"https://api.atlassian.com/ex/jira/{cloud_id}", email, token)

    primary = JiraClient(site_base_url, email, token)
    try:
        primary.validate()
        return primary
    except RuntimeError as primary_error:
        cloud_id = load_env("JIRA_CLOUD_ID")
        if not cloud_id:
            try:
                cloud_id = discover_cloud_id(site_base_url)
            except Exception:
                raise primary_error

        fallback = JiraClient(f"https://api.atlassian.com/ex/jira/{cloud_id}", email, token)
        try:
            fallback.validate()
            return fallback
        except RuntimeError:
            raise primary_error


def metadata_command(args):
    client = build_client()
    project_key = args.project or load_env("JIRA_PROJECT_KEY", required=True)
    project = client.project(project_key)
    issue_types = client.issue_types_for_project(project_key).get("issueTypes", [])
    priorities = client.priorities()
    fields = client.fields()

    enriched_issue_types = []
    for item in issue_types:
        field_meta = client.create_fields_for_issue_type(project_key, item["id"])
        enriched_issue_types.append(
            {
                "id": item["id"],
                "name": item["name"],
                "subtask": item.get("subtask", False),
                "fields": field_meta.get("fields", {}),
            }
        )

    output = {
        "project": {
            "id": project.get("id"),
            "key": project.get("key"),
            "name": project.get("name"),
            "style": project.get("style"),
            "projectTypeKey": project.get("projectTypeKey"),
        },
        "priorities": [{"id": p.get("id"), "name": p.get("name")} for p in priorities],
        "issueTypes": enriched_issue_types,
        "fields": [{"id": f.get("id"), "name": f.get("name")} for f in fields],
    }
    print(json.dumps(output, indent=2, ensure_ascii=False))


def validate_command(_args):
    client = build_client()
    me = client.validate()
    print(json.dumps({"accountId": me.get("accountId"), "displayName": me.get("displayName"), "emailAddress": me.get("emailAddress")}, indent=2, ensure_ascii=False))


def discover_command(_args):
    client = build_client()
    data = client.projects()
    print(json.dumps(data, indent=2, ensure_ascii=False))


def normalize_field_map(field_meta):
    by_name = {}
    if isinstance(field_meta, dict):
        items = field_meta.items()
    else:
        items = []
        for info in field_meta or []:
            field_id = info.get("fieldId") or info.get("key") or info.get("id")
            if field_id:
                items.append((field_id, info))

    for field_id, info in items:
        by_name[info.get("name", "").lower()] = field_id
    return by_name


def normalize_fields_by_id(field_meta):
    if isinstance(field_meta, dict):
        return field_meta

    normalized = {}
    for info in field_meta or []:
        field_id = info.get("fieldId") or info.get("key") or info.get("id")
        if field_id:
            normalized[field_id] = info
    return normalized


def build_issue_type_catalog(client, project_key):
    issue_types = client.issue_types_for_project(project_key).get("issueTypes", [])
    catalog = {}
    for item in issue_types:
        meta = client.create_fields_for_issue_type(project_key, item["id"])
        fields = normalize_fields_by_id(meta.get("fields", {}))
        catalog[item["name"].lower()] = {
            "id": item["id"],
            "name": item["name"],
            "subtask": item.get("subtask", False),
            "fields": fields,
            "fieldNames": normalize_field_map(fields),
        }
    return catalog


def prepare_fields(project_key, issue, issue_type_info, priority_lookup):
    fields = {
        "project": {"key": project_key},
        "summary": issue["summary"],
        "issuetype": {"id": issue_type_info["id"]},
    }

    available_fields = issue_type_info["fields"]
    field_names = issue_type_info["fieldNames"]

    if "description" in available_fields:
        fields["description"] = to_adf(issue.get("description", ""))

    labels = issue.get("labels") or []
    if labels and "labels" in available_fields:
        fields["labels"] = labels

    due_date = issue.get("dueDate")
    if due_date and "duedate" in available_fields:
        fields["duedate"] = due_date

    estimate_hours = issue.get("estimateHours")
    if estimate_hours is not None and "timetracking" in available_fields:
        fields["timetracking"] = {"originalEstimate": f"{estimate_hours}h"}

    priority_name = (issue.get("priority") or "").lower()
    if priority_name and "priority" in available_fields:
        priority = priority_lookup.get(priority_name)
        if priority:
            fields["priority"] = {"id": priority["id"]}

    epic_name_field = field_names.get("epic name")
    if issue_type_info["name"].lower() == "epic" and epic_name_field:
        fields[epic_name_field] = issue["summary"]

    return fields


def import_command(args):
    client = build_client()
    project_key = args.project or load_env("JIRA_PROJECT_KEY", required=True)
    input_path = Path(args.file)
    payload = json.loads(input_path.read_text(encoding="utf-8"))
    issues = payload.get("issues", [])
    if not issues:
        raise SystemExit(f"No issues found in {input_path}")

    catalog = build_issue_type_catalog(client, project_key)
    priorities = {p["name"].lower(): p for p in client.priorities()}

    refs_to_keys = {}
    pending_parent_updates = []

    preview = []
    for issue in issues:
        type_name = issue["type"].lower()
        if type_name not in catalog:
            raise SystemExit(
                f"Issue type '{issue['type']}' is not available in project {project_key}"
            )
        issue_type_info = catalog[type_name]
        fields = prepare_fields(project_key, issue, issue_type_info, priorities)

        parent_ref = issue.get("parentRef")
        parent_key_direct = issue.get("parentKey")
        if parent_ref and "parent" in issue_type_info["fields"]:
            parent_key = refs_to_keys.get(parent_ref)
            if parent_key:
                fields["parent"] = {"key": parent_key}
            else:
                pending_parent_updates.append((issue["ref"], parent_ref))
        elif parent_key_direct and "parent" in issue_type_info["fields"]:
            fields["parent"] = {"key": parent_key_direct}
        elif parent_ref:
            pending_parent_updates.append((issue["ref"], parent_ref))

        preview.append(
            {
                "ref": issue["ref"],
                "type": issue["type"],
                "summary": issue["summary"],
                "parentRef": parent_ref or parent_key_direct,
                "fields": fields,
            }
        )

    if not args.execute:
        print(json.dumps({"projectKey": project_key, "issues": preview}, indent=2, ensure_ascii=False))
        return

    created = []
    for issue in issues:
        type_name = issue["type"].lower()
        issue_type_info = catalog[type_name]
        fields = prepare_fields(project_key, issue, issue_type_info, priorities)

        parent_ref = issue.get("parentRef")
        parent_key_direct = issue.get("parentKey")
        if parent_ref and "parent" in issue_type_info["fields"]:
            parent_key = refs_to_keys.get(parent_ref)
            if not parent_key:
                raise SystemExit(
                    f"Cannot create issue {issue['ref']} before its parent {parent_ref}"
                )
            fields["parent"] = {"key": parent_key}
        elif parent_key_direct and "parent" in issue_type_info["fields"]:
            fields["parent"] = {"key": parent_key_direct}

        result = client.create_issue(fields)
        issue_key = result.get("key")
        refs_to_keys[issue["ref"]] = issue_key
        created.append({"ref": issue["ref"], "key": issue_key, "type": issue["type"]})

    parent_warnings = []
    for child_ref, parent_ref in pending_parent_updates:
        child_key = refs_to_keys.get(child_ref)
        parent_key = refs_to_keys.get(parent_ref)
        if not child_key or not parent_key:
            parent_warnings.append(
                f"Skipped parent update for {child_ref} -> {parent_ref} because one of the issues was not created."
            )
            continue
        try:
            client.update_issue(child_key, {"parent": {"key": parent_key}})
        except RuntimeError as exc:
            parent_warnings.append(
                f"Could not assign parent {parent_key} to {child_key}: {exc}"
            )

    print(
        json.dumps(
            {
                "created": created,
                "parentWarnings": parent_warnings,
            },
            indent=2,
            ensure_ascii=False,
        )
    )


def main():
    parser = argparse.ArgumentParser(description="Small Jira helper for OptiDrive presets")
    subparsers = parser.add_subparsers(dest="command", required=True)

    validate_parser = subparsers.add_parser("validate", help="Validate Jira credentials")
    validate_parser.set_defaults(func=validate_command)

    discover_parser = subparsers.add_parser("discover", help="List visible Jira projects")
    discover_parser.set_defaults(func=discover_command)

    metadata_parser = subparsers.add_parser("metadata", help="Dump project metadata")
    metadata_parser.add_argument("--project", help="Jira project key")
    metadata_parser.set_defaults(func=metadata_command)

    import_parser = subparsers.add_parser("import", help="Import issues from a JSON bootstrap file")
    import_parser.add_argument("--project", help="Jira project key")
    import_parser.add_argument("--file", required=True, help="Path to the JSON bootstrap file")
    import_parser.add_argument(
        "--execute",
        action="store_true",
        help="Actually create issues. Without this flag the command runs in preview mode.",
    )
    import_parser.set_defaults(func=import_command)

    args = parser.parse_args()
    args.func(args)


if __name__ == "__main__":
    main()
