#!/usr/bin/env python3
import argparse
import base64
import json
import mimetypes
import os
import re
import sys
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path
from xml.sax.saxutils import escape


def load_env(name, default=None, required=False):
    value = os.getenv(name, default)
    if required and not value:
        raise SystemExit(f"Missing required environment variable: {name}")
    return value


def truthy(value):
    return str(value or "").strip().lower() in {"1", "true", "yes", "on"}


def load_dotenv_file(path):
    path = Path(path)
    if not path.exists():
        return

    for raw_line in path.read_text(encoding="utf-8").splitlines():
        line = raw_line.strip()
        if not line or line.startswith("#") or "=" not in line:
            continue

        key, value = line.split("=", 1)
        key = key.strip()
        value = value.strip().strip("'\"")
        if key and key not in os.environ:
            os.environ[key] = value


class ConfluenceClient:
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
                f"Confluence request failed: {method} {path} -> {exc.code} {body}"
            ) from exc

    def upload_attachment(self, page_id, file_path):
        existing = self.find_attachment(page_id, Path(file_path).name)
        if existing:
            if not truthy(os.getenv("CONFLUENCE_UPDATE_ATTACHMENTS")):
                return 200, {"skipped": True, "id": existing.get("id")}
            return self.update_attachment_data(page_id, existing["id"], file_path)

        return self.create_attachment(page_id, file_path)

    def find_attachment(self, page_id, filename):
        data = self.request(
            "GET",
            f"/wiki/rest/api/content/{page_id}/child/attachment",
            query={"filename": filename, "expand": "version", "limit": 1},
        )[1]
        results = data.get("results", [])
        return results[0] if results else None

    def create_attachment(self, page_id, file_path):
        return self.send_attachment_data(
            "POST",
            f"/wiki/rest/api/content/{page_id}/child/attachment",
            file_path,
        )

    def update_attachment_data(self, page_id, attachment_id, file_path):
        return self.send_attachment_data(
            "POST",
            f"/wiki/rest/api/content/{page_id}/child/attachment/{attachment_id}/data",
            file_path,
        )

    def send_attachment_data(self, method, path, file_path):
        boundary = "----OptiDriveBoundary7MA4YWxkTrZu0gW"
        filename = Path(file_path).name
        content_type = mimetypes.guess_type(filename)[0] or "application/octet-stream"
        file_bytes = Path(file_path).read_bytes()
        parts = [
            f"--{boundary}\r\n".encode("utf-8"),
            (
                f'Content-Disposition: form-data; name="file"; filename="{filename}"\r\n'
                f"Content-Type: {content_type}\r\n\r\n"
            ).encode("utf-8"),
            file_bytes,
            b"\r\n",
            f"--{boundary}--\r\n".encode("utf-8"),
        ]
        data = b"".join(parts)
        headers = {
            "Accept": "application/json",
            "Authorization": self.auth_header,
            "Content-Type": f"multipart/form-data; boundary={boundary}",
            "X-Atlassian-Token": "no-check",
        }
        req = urllib.request.Request(
            self.base_url + path,
            method=method,
            headers=headers,
            data=data,
        )
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
                f"Confluence attachment upload failed: {filename} -> {exc.code} {body}"
            ) from exc

    def get_space(self, space_key):
        data = self.request(
            "GET",
            "/wiki/api/v2/spaces",
            query={"keys": space_key, "limit": 10},
        )[1]
        items = data.get("results", [])
        if not items:
            raise RuntimeError(f"Space not found: {space_key}")
        return items[0]

    def find_page(self, space_key, title):
        data = self.request(
            "GET",
            "/wiki/rest/api/content",
            query={
                "spaceKey": space_key,
                "title": title,
                "expand": "version,ancestors",
                "limit": 10,
            },
        )[1]
        results = data.get("results", [])
        return results[0] if results else None

    def create_page(self, space_key, title, body_storage, parent_id=None):
        payload = {
            "type": "page",
            "title": title,
            "space": {"key": space_key},
            "body": {
                "storage": {
                    "value": body_storage,
                    "representation": "storage",
                }
            },
        }
        if parent_id:
            payload["ancestors"] = [{"id": str(parent_id)}]
        return self.request("POST", "/wiki/rest/api/content", payload=payload)[1]

    def update_page(self, page_id, title, body_storage, version_number, parent_id=None):
        payload = {
            "id": str(page_id),
            "type": "page",
            "title": title,
            "version": {"number": int(version_number) + 1},
            "body": {
                "storage": {
                    "value": body_storage,
                    "representation": "storage",
                }
            },
        }
        if parent_id:
            payload["ancestors"] = [{"id": str(parent_id)}]
        return self.request("PUT", f"/wiki/rest/api/content/{page_id}", payload=payload)[1]

    def ensure_page(self, space_key, title, body_storage, parent_id=None, dry_run=False):
        existing = self.find_page(space_key, title)
        if dry_run:
            return {
                "action": "update" if existing else "create",
                "title": title,
                "id": existing.get("id") if existing else None,
                "parentId": parent_id,
            }

        if existing:
            updated = self.update_page(
                existing["id"],
                title,
                body_storage,
                existing["version"]["number"],
                parent_id=parent_id,
            )
            return {
                "action": "updated",
                "title": title,
                "id": updated.get("id", existing["id"]),
                "webui": updated.get("_links", {}).get("webui"),
            }

        created = self.create_page(space_key, title, body_storage, parent_id=parent_id)
        return {
            "action": "created",
            "title": title,
            "id": created.get("id"),
            "webui": created.get("_links", {}).get("webui"),
        }


INLINE_CODE_RE = re.compile(r"`([^`]+)`")
LINK_RE = re.compile(r"\[([^\]]+)\]\(([^)]+)\)")
BOLD_RE = re.compile(r"\*\*([^*]+)\*\*")
ITALIC_RE = re.compile(r"(?<!\*)\*([^*]+)\*(?!\*)")
IMAGE_RE = re.compile(r"!\[([^\]]*)\]\(([^)]+)\)")


def render_inline(text):
    placeholders = {}

    def stash(match):
        token = f"__CODE_{len(placeholders)}__"
        placeholders[token] = f"<code>{escape(match.group(1))}</code>"
        return token

    escaped = escape(text)
    escaped = INLINE_CODE_RE.sub(stash, escaped)
    escaped = LINK_RE.sub(
        lambda match: render_link(match.group(1), match.group(2)),
        escaped,
    )
    escaped = BOLD_RE.sub(r"<strong>\1</strong>", escaped)
    escaped = ITALIC_RE.sub(r"<em>\1</em>", escaped)
    for token, value in placeholders.items():
        escaped = escaped.replace(token, value)
    return escaped


def render_link(label, href):
    safe_href = escape(href, {'"': "&quot;"})
    return f'<a href="{safe_href}">{label}</a>'


def render_code_block(code, language):
    lang = (language or "none").strip()
    if lang.lower() == "mermaid":
        lang = "text"
    return (
        '<ac:structured-macro ac:name="code">'
        f'<ac:parameter ac:name="language">{escape(lang)}</ac:parameter>'
        f"<ac:plain-text-body><![CDATA[{code}]]></ac:plain-text-body>"
        "</ac:structured-macro>"
    )


def render_image(alt_text, attachment_name):
    alt = escape(alt_text)
    filename = escape(attachment_name)
    return (
        '<ac:image ac:align="center" ac:layout="center" ac:width="760"'
        f' ac:alt="{alt}">'
        f'<ri:attachment ri:filename="{filename}" />'
        "</ac:image>"
    )


def is_table_separator(line):
    parts = [part.strip() for part in line.strip().strip("|").split("|")]
    if not parts:
        return False
    return all(part and set(part) <= {"-", ":"} for part in parts)


def parse_table(lines):
    rows = []
    for line in lines:
        cells = [render_inline(cell.strip()) for cell in line.strip().strip("|").split("|")]
        rows.append(cells)

    header = rows[0]
    body_rows = rows[2:] if len(rows) > 1 and is_table_separator(lines[1]) else rows[1:]

    html = ["<table><tbody>"]
    html.append("<tr>" + "".join(f"<th>{cell}</th>" for cell in header) + "</tr>")
    for row in body_rows:
        html.append("<tr>" + "".join(f"<td>{cell}</td>" for cell in row) + "</tr>")
    html.append("</tbody></table>")
    return "".join(html)


def markdown_to_storage(markdown_text, source_file=None):
    lines = markdown_text.splitlines()
    output = []
    attachments = []
    source_dir = Path(source_file).parent if source_file else Path.cwd()
    i = 0

    while i < len(lines):
        line = lines[i]
        stripped = line.strip()

        if not stripped:
            i += 1
            continue

        if stripped.startswith("```"):
            language = stripped[3:].strip()
            i += 1
            code_lines = []
            while i < len(lines) and not lines[i].strip().startswith("```"):
                code_lines.append(lines[i])
                i += 1
            if i < len(lines):
                i += 1
            output.append(render_code_block("\n".join(code_lines), language))
            continue

        image = IMAGE_RE.match(stripped)
        if image:
            alt_text = image.group(1).strip()
            target = image.group(2).strip()
            if re.match(r"^[a-zA-Z][a-zA-Z0-9+.-]*:", target):
                output.append(f'<p><img src="{escape(target)}" alt="{escape(alt_text)}" /></p>')
            else:
                file_path = (source_dir / target).resolve()
                attachments.append(file_path)
                output.append(render_image(alt_text, file_path.name))
            i += 1
            continue

        heading = re.match(r"^(#{1,6})\s+(.*)$", stripped)
        if heading:
            level = len(heading.group(1))
            text = render_inline(heading.group(2).strip())
            output.append(f"<h{level}>{text}</h{level}>")
            i += 1
            continue

        if stripped.startswith("|"):
            table_lines = []
            while i < len(lines) and lines[i].strip().startswith("|"):
                table_lines.append(lines[i])
                i += 1
            output.append(parse_table(table_lines))
            continue

        if re.match(r"^[-*]\s+", stripped):
            items = []
            while i < len(lines) and re.match(r"^[-*]\s+", lines[i].strip()):
                item = re.sub(r"^[-*]\s+", "", lines[i].strip())
                items.append(f"<li>{render_inline(item)}</li>")
                i += 1
            output.append("<ul>" + "".join(items) + "</ul>")
            continue

        if re.match(r"^\d+\.\s+", stripped):
            items = []
            while i < len(lines) and re.match(r"^\d+\.\s+", lines[i].strip()):
                item = re.sub(r"^\d+\.\s+", "", lines[i].strip())
                items.append(f"<li>{render_inline(item)}</li>")
                i += 1
            output.append("<ol>" + "".join(items) + "</ol>")
            continue

        paragraph_lines = [stripped]
        i += 1
        while i < len(lines):
            next_line = lines[i].strip()
            if (
                not next_line
                or next_line.startswith("```")
                or next_line.startswith("|")
                or re.match(r"^(#{1,6})\s+", next_line)
                or re.match(r"^[-*]\s+", next_line)
                or re.match(r"^\d+\.\s+", next_line)
            ):
                break
            paragraph_lines.append(next_line)
            i += 1
        output.append(f"<p>{render_inline(' '.join(paragraph_lines))}</p>")

    return "\n".join(output), attachments


def default_page_specs(source_dir):
    return [
        {"title": "OptiDrive - Relatorios Academicos", "file": source_dir / "README.md"},
        {"title": "OptiDrive - Analise e Especificacao de Requisitos", "file": source_dir / "01-analise-especificacao-requisitos.md"},
        {"title": "OptiDrive - Desenho de Alto Nivel", "file": source_dir / "02-desenho-alto-nivel.md"},
        {"title": "OptiDrive - Desenho Detalhado - Sprint 1", "file": source_dir / "03-desenho-detalhado-sprint-1.md"},
        {"title": "OptiDrive - Desenho Detalhado - Sprint 2", "file": source_dir / "04-desenho-detalhado-sprint-2.md"},
        {"title": "OptiDrive - Desenho Detalhado - Sprint 3", "file": source_dir / "05-desenho-detalhado-sprint-3.md"},
        {"title": "OptiDrive - Desenho Detalhado - Sprint 4", "file": source_dir / "20-desenho-detalhado-sprint-4.md"},
        {"title": "OptiDrive - Desenho Detalhado - Sprint 5", "file": source_dir / "21-desenho-detalhado-sprint-5.md"},
        {"title": "OptiDrive - Ata Sprint 1", "file": source_dir / "06-ata-sprint-1.md"},
        {"title": "OptiDrive - Ata Sprint 2", "file": source_dir / "07-ata-sprint-2.md"},
        {"title": "OptiDrive - Ata Sprint 3", "file": source_dir / "08-ata-sprint-3.md"},
        {"title": "OptiDrive - Ata Sprint 4", "file": source_dir / "22-ata-sprint-4.md"},
        {"title": "OptiDrive - Ata Sprint 5", "file": source_dir / "23-ata-sprint-5.md"},
        {"title": "OptiDrive - Estado Atual da Implementacao", "file": source_dir / "09-estado-atual-implementacao.md"},
        {"title": "OptiDrive - Plano de Fecho da Entrega Final", "file": source_dir / "10-plano-fecho-entrega-final.md"},
        {"title": "OptiDrive - Modelacao de Dados e Scripts SQL", "file": source_dir / "11-modelacao-dados-e-sql.md"},
        {"title": "OptiDrive - Plano de Testes e Validacao", "file": source_dir / "12-plano-testes-e-validacao.md"},
        {"title": "OptiDrive - Estrutura da Apresentacao Final", "file": source_dir / "13-apresentacao-final.md"},
        {"title": "OptiDrive - Modulo Social LocalDB e Docker", "file": source_dir / "14-social-localdb-docker.md"},
        {"title": "OptiDrive - Autenticacao Online e Social Produto", "file": source_dir / "15-autenticacao-online-social-produto.md"},
        {"title": "OptiDrive - Readiness Produto e Configuracao", "file": source_dir / "16-readiness-produto-e-configuracao.md"},
        {"title": "OptiDrive - Entrega Final e Guia de Apresentacao", "file": source_dir / "17-entrega-final-e-guia-de-apresentacao.md"},
        {"title": "OptiDrive - Autonomia, Tema e Internacionalizacao", "file": source_dir / "18-autonomia-tema-i18n-planeamento.md"},
        {"title": "OptiDrive - Dataset Realista", "file": source_dir / "19-dados-realistas.md"},
    ]


def publish_command(args):
    base_url = load_env("CONFLUENCE_BASE_URL", load_env("JIRA_BASE_URL"), required=True)
    email = load_env("CONFLUENCE_EMAIL", load_env("JIRA_EMAIL"), required=True)
    token = load_env("CONFLUENCE_API_TOKEN", load_env("JIRA_API_TOKEN"), required=True)
    space_key = args.space or load_env("CONFLUENCE_SPACE_KEY", required=True)
    source_dir = Path(args.source_dir).resolve()

    client = ConfluenceClient(base_url, email, token)
    space = client.get_space(space_key)
    homepage_id = space.get("homepageId")
    specs = default_page_specs(source_dir)

    root_spec = specs[0]
    root_body, root_attachments = markdown_to_storage(
        root_spec["file"].read_text(encoding="utf-8"),
        source_file=root_spec["file"],
    )
    root_result = client.ensure_page(
        space_key,
        root_spec["title"],
        root_body,
        parent_id=homepage_id,
        dry_run=args.dry_run,
    )
    root_result["attachments"] = publish_attachments(
        client,
        root_result.get("id"),
        root_attachments,
        dry_run=args.dry_run,
    )
    root_id = root_result.get("id") or homepage_id

    results = [root_result]
    for spec in specs[1:]:
        body, attachments = markdown_to_storage(
            spec["file"].read_text(encoding="utf-8"),
            source_file=spec["file"],
        )
        result = client.ensure_page(
            space_key,
            spec["title"],
            body,
            parent_id=root_id,
            dry_run=args.dry_run,
        )
        result["attachments"] = publish_attachments(
            client,
            result.get("id"),
            attachments,
            dry_run=args.dry_run,
        )
        results.append(result)

    print(json.dumps({
        "space": {"key": space.get("key"), "name": space.get("name"), "id": space.get("id")},
        "results": results,
    }, indent=2, ensure_ascii=False))


def publish_attachments(client, page_id, attachments, dry_run=False):
    unique_attachments = []
    seen = set()
    for attachment in attachments:
        attachment = Path(attachment)
        if attachment in seen:
            continue
        seen.add(attachment)
        unique_attachments.append(attachment)

    if dry_run:
        return [{"action": "attach", "file": str(path)} for path in unique_attachments]
    if not page_id:
        return []

    uploaded = []
    for path in unique_attachments:
        if not path.exists():
            raise RuntimeError(f"Attachment not found: {path}")
        status, _ = client.upload_attachment(page_id, path)
        uploaded.append({"file": path.name, "status": status})
    return uploaded


def validate_command(args):
    base_url = load_env("CONFLUENCE_BASE_URL", load_env("JIRA_BASE_URL"), required=True)
    email = load_env("CONFLUENCE_EMAIL", load_env("JIRA_EMAIL"), required=True)
    token = load_env("CONFLUENCE_API_TOKEN", load_env("JIRA_API_TOKEN"), required=True)
    space_key = args.space or load_env("CONFLUENCE_SPACE_KEY", required=True)

    client = ConfluenceClient(base_url, email, token)
    space = client.get_space(space_key)
    print(json.dumps({
        "space": {
            "id": space.get("id"),
            "key": space.get("key"),
            "name": space.get("name"),
            "homepageId": space.get("homepageId"),
            "webui": space.get("_links", {}).get("webui"),
        }
    }, indent=2, ensure_ascii=False))


def build_parser():
    parser = argparse.ArgumentParser(description="Publish OptiDrive reports to Confluence.")
    subparsers = parser.add_subparsers(dest="command", required=True)

    validate_parser = subparsers.add_parser("validate", help="Validate Confluence access and space lookup.")
    validate_parser.add_argument("--space", help="Confluence space key")
    validate_parser.set_defaults(func=validate_command)

    publish_parser = subparsers.add_parser("publish", help="Create or update the report page tree.")
    publish_parser.add_argument("--space", help="Confluence space key")
    publish_parser.add_argument(
        "--source-dir",
        default="docs/reports",
        help="Directory containing the Markdown reports",
    )
    publish_parser.add_argument("--dry-run", action="store_true", help="Preview actions without writing pages.")
    publish_parser.set_defaults(func=publish_command)

    return parser


def main():
    load_dotenv_file(Path.cwd() / ".env")
    parser = build_parser()
    args = parser.parse_args()
    args.func(args)


if __name__ == "__main__":
    main()
