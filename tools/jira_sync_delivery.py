#!/usr/bin/env python3
import base64
import json
import os
import re
import sys
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path


PROJECT_KEY = "OP"
BOARD_ID = 68

SPRINTS = [
    {
        "key": "sprint-1",
        "name": "S1 - Identidade e Garagem",
        "startDate": "2026-05-11T08:00:00.000Z",
        "endDate": "2026-05-20T17:00:00.000Z",
        "goal": "Concluir a base formal após a análise de requisitos enviada ao docente em 11/05: estrutura ASP.NET Core MVC, identidade local, segurança, garagem e documentação inicial.",
        "issues": [
            "T1.1 - Levantamento e Engenharia de Requisitos",
            "T1.2 - Modelacao de Casos de Uso (UML) e Atores",
            "T1.3 - Desenho da Matriz de Rastreabilidade Geral",
            "T2.1 - Desenho do Modelo Entidade-Relacao",
            "T2.2 - Criacao de Scripts SQL",
            "T3.1 - Setup de Arquitetura e Configuracao ASP.NET",
            "T3.2 - Implementacao do Modulo de Identidade e Seguranca",
            "T4.1 - Criacao de Interfaces de Utilizador e Dashboard",
            "Corrigir Analise e Especificacao de Requisitos apos feedback do stor",
            "Completar planeamento 3.5 e Gantt do projeto OptiDrive",
            "Normalizar modulos, atores, requisitos e use cases com IDs unicos",
            "Elaborar desenho detalhado - Sprint 1",
            "Redigir ata - Sprint 1",
        ],
    },
    {
        "key": "sprint-2",
        "name": "S2 - Mapas e Energia",
        "startDate": "2026-05-21T08:00:00.000Z",
        "endDate": "2026-05-30T17:00:00.000Z",
        "goal": "Transformar o OptiDrive num cockpit de planeamento com mapa, Google Maps, postos de combustível, carregadores, filtros e desenho de alto nível consolidado.",
        "issues": [
            "T3.3 - Integracao com APIs de Mapas e Portagens",
            "T4.2 - Implementacao do Mapa Dinamico e Filtros Visuais",
            "Elaborar desenho de alto nivel do OptiDrive",
            "Elaborar desenho detalhado - Sprint 2",
            "Redigir ata - Sprint 2",
        ],
    },
    {
        "key": "sprint-3",
        "name": "S3 - Smart Save",
        "startDate": "2026-06-01T08:00:00.000Z",
        "endDate": "2026-06-08T17:00:00.000Z",
        "goal": "Implementar a camada inteligente de decisão: Smart Save, autonomia, consumo por velocidade, reforços na rota, testes e documentação da Sprint 3.",
        "issues": [
            "T3.4 - Desenvolvimento do Algoritmo Smart Save",
            "T5.1 - Execucao de Testes Unitarios de Backend",
            "T5.2 - Testes de Compatibilidade e Responsividade",
            "T6.1 - Consolidacao do Relatorio de Especificacao Final",
            "T6.2 - Preparacao da Apresentacao do Projeto",
            "Elaborar desenho detalhado - Sprint 3",
            "Redigir ata - Sprint 3",
            "Corrigir autonomia final, consumo por velocidade e UI bilingue",
        ],
    },
    {
        "key": "sprint-4",
        "name": "S4 - Social e OAuth",
        "startDate": "2026-06-09T08:00:00.000Z",
        "endDate": "2026-06-16T17:00:00.000Z",
        "goal": "Valorizar a componente social: perfis, contactos, mensagens, presença, viagens colaborativas, autenticação Google/Microsoft/Apple e segurança MFA.",
        "issues": [
            "Implementar modulo social avancado",
            "Preparar login com Google OAuth",
            "Endurecer login local com password hashing e lockout",
            "Implementar MFA compativel com Microsoft Authenticator",
            "Preparar login federado Google, Microsoft e Apple",
            "Melhorar modulo social com presenca e live pulse",
            "Validar autenticacao social em build, testes e Docker",
            "Elaborar desenho detalhado - Sprint 4",
            "Redigir ata - Sprint 4",
        ],
    },
    {
        "key": "sprint-5",
        "name": "S5 - Entrega Final",
        "startDate": "2026-06-17T08:00:00.000Z",
        "endDate": "2026-06-30T17:00:00.000Z",
        "goal": "Fechar o produto até 30/06/2026: Docker, SQLite, readiness, dados realistas, i18n, Jira, Confluence, relatórios, bugfix final e validação de apresentação.",
        "issues": [
            "Implementar LocalDB SQLite com EF Core",
            "Preparar Docker para MacBook",
            "Validar social, LocalDB e Docker",
            "Rever readiness global e hardening transversal",
            "Adicionar healthcheck e painel operacional",
            "Documentar configuracao OAuth, Apple e Docker",
            "Consolidar entrega final e guia de apresentacao",
            "Popular base de dados com utilizadores e veiculos realistas para demo",
            "Elaborar desenho detalhado - Sprint 5",
            "Redigir ata - Sprint 5",
            "Atualizar Jira final com 5 sprints, relatórios e links Confluence",
            "Corrigir bugs finais e validar entrega em 30/06",
        ],
    },
]

FINAL_ISSUES = [
    {
        "summary": "Atualizar Jira final com 5 sprints, relatórios e links Confluence",
        "type": "Task",
        "description": (
            "Sincronizar o Jira do OptiDrive para a entrega final: criar/normalizar 5 sprints, "
            "associar as tarefas técnicas e documentais, marcar trabalho concluído e ligar os "
            "relatórios publicados no Confluence."
        ),
        "labels": ["optidrive", "jira", "confluence", "entrega-final", "sprint-5"],
    },
    {
        "summary": "Corrigir bugs finais e validar entrega em 30/06",
        "type": "Task",
        "description": (
            "Corrigir bugs finais identificados na véspera da entrega, validar autenticação, "
            "garagem, planeamento, social, i18n, tema, Docker e fluxo de apresentação."
        ),
        "labels": ["optidrive", "bugfix", "validacao", "entrega-final", "sprint-5"],
    },
]

CONFLUENCE_LINKS = {
    "Corrigir Analise e Especificacao de Requisitos apos feedback do stor": (
        "OptiDrive - Análise e Especificação de Requisitos",
        "https://estudantes-team-jwsj3xh7.atlassian.net/wiki/spaces/OptiDrive/pages/80773139/OptiDrive+-+Analise+e+Especificacao+de+Requisitos",
    ),
    "Elaborar desenho de alto nivel do OptiDrive": (
        "OptiDrive - Desenho de Alto Nível",
        "https://estudantes-team-jwsj3xh7.atlassian.net/wiki/spaces/OptiDrive/pages/80773156/OptiDrive+-+Desenho+de+Alto+Nivel",
    ),
    "Elaborar desenho detalhado - Sprint 1": (
        "OptiDrive - Desenho Detalhado Sprint 1",
        "https://estudantes-team-jwsj3xh7.atlassian.net/wiki/spaces/OptiDrive/pages/80773173/OptiDrive+-+Desenho+Detalhado+-+Sprint+1",
    ),
    "Redigir ata - Sprint 1": (
        "OptiDrive - Ata Sprint 1",
        "https://estudantes-team-jwsj3xh7.atlassian.net/wiki/spaces/OptiDrive/pages/80904193/OptiDrive+-+Ata+Sprint+1",
    ),
    "Elaborar desenho detalhado - Sprint 2": (
        "OptiDrive - Desenho Detalhado Sprint 2",
        "https://estudantes-team-jwsj3xh7.atlassian.net/wiki/spaces/OptiDrive/pages/80806247/OptiDrive+-+Desenho+Detalhado+-+Sprint+2",
    ),
    "Redigir ata - Sprint 2": (
        "OptiDrive - Ata Sprint 2",
        "https://estudantes-team-jwsj3xh7.atlassian.net/wiki/spaces/OptiDrive/pages/80936961/OptiDrive+-+Ata+Sprint+2",
    ),
    "Elaborar desenho detalhado - Sprint 3": (
        "OptiDrive - Desenho Detalhado Sprint 3",
        "https://estudantes-team-jwsj3xh7.atlassian.net/wiki/spaces/OptiDrive/pages/80773190/OptiDrive+-+Desenho+Detalhado+-+Sprint+3",
    ),
    "Redigir ata - Sprint 3": (
        "OptiDrive - Ata Sprint 3",
        "https://estudantes-team-jwsj3xh7.atlassian.net/wiki/spaces/OptiDrive/pages/80773207/OptiDrive+-+Ata+Sprint+3",
    ),
    "Elaborar desenho detalhado - Sprint 4": (
        "OptiDrive - Desenho Detalhado Sprint 4",
        "https://estudantes-team-jwsj3xh7.atlassian.net/wiki/spaces/OptiDrive/pages/88440842/OptiDrive+-+Desenho+Detalhado+-+Sprint+4",
    ),
    "Redigir ata - Sprint 4": (
        "OptiDrive - Ata Sprint 4",
        "https://estudantes-team-jwsj3xh7.atlassian.net/wiki/spaces/OptiDrive/pages/88309802/OptiDrive+-+Ata+Sprint+4",
    ),
    "Elaborar desenho detalhado - Sprint 5": (
        "OptiDrive - Desenho Detalhado Sprint 5",
        "https://estudantes-team-jwsj3xh7.atlassian.net/wiki/spaces/OptiDrive/pages/88309780/OptiDrive+-+Desenho+Detalhado+-+Sprint+5",
    ),
    "Redigir ata - Sprint 5": (
        "OptiDrive - Ata Sprint 5",
        "https://estudantes-team-jwsj3xh7.atlassian.net/wiki/spaces/OptiDrive/pages/88309818/OptiDrive+-+Ata+Sprint+5",
    ),
    "Consolidar entrega final e guia de apresentacao": (
        "OptiDrive - Entrega Final e Guia de Apresentação",
        "https://estudantes-team-jwsj3xh7.atlassian.net/wiki/spaces/OptiDrive/pages/88113162/OptiDrive+-+Entrega+Final+e+Guia+de+Apresentacao",
    ),
    "Atualizar Jira final com 5 sprints, relatórios e links Confluence": (
        "OptiDrive - Relatórios Académicos",
        "https://estudantes-team-jwsj3xh7.atlassian.net/wiki/spaces/OptiDrive/pages/80773122/OptiDrive+-+Relatorios+Academicos",
    ),
}


def env(name):
    value = os.getenv(name)
    if not value:
        raise SystemExit(f"Missing required environment variable: {name}")
    return value


def adf(text):
    paragraphs = []
    for raw in (text or "").splitlines():
        line = raw.strip()
        if line:
            paragraphs.append({"type": "paragraph", "content": [{"type": "text", "text": line}]})
    if not paragraphs:
        paragraphs.append({"type": "paragraph", "content": [{"type": "text", "text": "Sem descrição."}]})
    return {"version": 1, "type": "doc", "content": paragraphs}


class Jira:
    def __init__(self):
        self.base = env("JIRA_BASE_URL").rstrip("/")
        credentials = f"{env('JIRA_EMAIL')}:{env('JIRA_API_TOKEN')}".encode("utf-8")
        self.auth = "Basic " + base64.b64encode(credentials).decode("ascii")

    def request(self, method, path, payload=None, query=None):
        url = self.base + path
        if query:
            url += "?" + urllib.parse.urlencode(query, doseq=True)
        headers = {"Accept": "application/json", "Authorization": self.auth}
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
            try:
                body = json.loads(raw)
            except Exception:
                body = {"raw": raw}
            raise RuntimeError(f"{method} {path} -> {exc.code}: {body}") from exc

    def paged_get(self, path, query=None):
        query = dict(query or {})
        start = 0
        values = []
        while True:
            page_query = dict(query)
            page_query.setdefault("maxResults", 100)
            page_query["startAt"] = start
            data = self.request("GET", path, query=page_query)[1]
            if "values" in data:
                batch = data.get("values", [])
                values.extend(batch)
                if data.get("isLast", True):
                    return values
                start += len(batch)
            elif "issues" in data:
                batch = data.get("issues", [])
                values.extend(batch)
                if start + len(batch) >= data.get("total", 0):
                    return values
                start += len(batch)
            else:
                return values


def load_all_preset_issues():
    issues = []
    seen = set()
    for path in sorted(Path("docs/jira").glob("*.json")):
        payload = json.loads(path.read_text(encoding="utf-8"))
        for issue in payload.get("issues", []):
            summary = issue.get("summary")
            if not summary or summary in seen:
                continue
            seen.add(summary)
            issues.append(
                {
                    "summary": summary,
                    "type": issue.get("type", "Task"),
                    "description": issue.get("description", ""),
                    "labels": issue.get("labels", []),
                }
            )
    for issue in FINAL_ISSUES:
        if issue["summary"] not in seen:
            issues.append(issue)
    return issues


def issue_type_ids(client):
    project = client.request("GET", f"/rest/api/3/project/{PROJECT_KEY}")[1]
    return {item["name"].lower(): item["id"] for item in project.get("issueTypes", [])}


def all_issues_by_summary(client):
    issues = client.paged_get(
        "/rest/api/3/search/jql",
        {
            "jql": f"project = {PROJECT_KEY} ORDER BY created ASC",
            "fields": "summary,status,issuetype,labels",
        },
    )
    return {issue["fields"]["summary"]: issue for issue in issues}


def create_missing_issues(client, type_ids, existing):
    created = []
    for issue in load_all_preset_issues():
        if issue["summary"] in existing:
            continue
        type_name = issue.get("type", "Task").lower()
        issue_type_id = type_ids.get(type_name) or type_ids.get("task")
        fields = {
            "project": {"key": PROJECT_KEY},
            "summary": issue["summary"],
            "issuetype": {"id": issue_type_id},
            "description": adf(issue.get("description", "")),
        }
        labels = issue.get("labels") or []
        if labels:
            fields["labels"] = labels
        result = client.request("POST", "/rest/api/3/issue", payload={"fields": fields})[1]
        created.append(result["key"])
        existing[issue["summary"]] = client.request(
            "GET",
            f"/rest/api/3/issue/{result['key']}",
            query={"fields": "summary,status,issuetype,labels"},
        )[1]
    return created


def desired_sprints(client):
    current = client.paged_get(
        f"/rest/agile/1.0/board/{BOARD_ID}/sprint",
        {"state": "active,future,closed"},
    )
    by_number = {}
    for sprint in current:
        name = sprint.get("name", "")
        match = re.match(r"^(?:Sprint\s+|S)(\d+)\b", name)
        if match:
            number = match.group(1)
            if number not in by_number:
                by_number[number] = sprint

    results = []
    for index, spec in enumerate(SPRINTS, start=1):
        existing = by_number.get(str(index))
        payload = {
            "name": spec["name"],
            "startDate": spec["startDate"],
            "endDate": spec["endDate"],
            "originBoardId": BOARD_ID,
            "goal": spec.get("goal", ""),
        }
        if existing:
            sprint_id = existing["id"]
            payload["state"] = existing.get("state", "future")
            client.request("PUT", f"/rest/agile/1.0/sprint/{sprint_id}", payload=payload)
            action = "updated"
        else:
            created = client.request("POST", "/rest/agile/1.0/sprint", payload=payload)[1]
            sprint_id = created["id"]
            action = "created"
        results.append({"id": sprint_id, "name": spec["name"], "action": action, "spec": spec})
    return results


def add_issues_to_sprints(client, sprints, existing):
    assignments = []
    for sprint in sprints:
        keys = []
        missing = []
        for summary in sprint["spec"]["issues"]:
            issue = existing.get(summary)
            if issue:
                keys.append(issue["key"])
            else:
                missing.append(summary)
        if keys:
            client.request(
                "POST",
                f"/rest/agile/1.0/sprint/{sprint['id']}/issue",
                payload={"issues": keys},
            )
        assignments.append(
            {"sprint": sprint["name"], "sprintId": sprint["id"], "issues": keys, "missing": missing}
        )
    return assignments


def transition_to_done(client, issue_key):
    issue = client.request("GET", f"/rest/api/3/issue/{issue_key}", query={"fields": "status"})[1]
    if issue["fields"]["status"]["name"].lower() == "done":
        return False
    transitions = client.request("GET", f"/rest/api/3/issue/{issue_key}/transitions")[1].get("transitions", [])
    done = next((t for t in transitions if t["name"].lower() == "done"), None)
    if not done:
        return False
    client.request(
        "POST",
        f"/rest/api/3/issue/{issue_key}/transitions",
        payload={"transition": {"id": done["id"]}},
    )
    return True


def close_all_desired_issues(client, assignments, existing):
    status_by_key = {
        issue["key"]: issue["fields"]["status"]["name"].lower()
        for issue in existing.values()
        if issue.get("fields", {}).get("status")
    }
    transitioned = []
    for assignment in assignments:
        for key in assignment["issues"]:
            if status_by_key.get(key) == "done":
                continue
            if transition_to_done(client, key):
                transitioned.append(key)
    return sorted(set(transitioned))


def sync_remote_links(client, existing):
    linked = []
    for summary, (title, url) in CONFLUENCE_LINKS.items():
        issue = existing.get(summary)
        if not issue:
            continue
        key = issue["key"]
        current = client.request("GET", f"/rest/api/3/issue/{key}/remotelink")[1]
        already = any((item.get("object") or {}).get("url") == url for item in current)
        if already:
            continue
        payload = {
            "relationship": "documentado em",
            "object": {
                "url": url,
                "title": title,
                "summary": "Relatório publicado no Confluence para evidência académica.",
            },
        }
        client.request("POST", f"/rest/api/3/issue/{key}/remotelink", payload=payload)
        linked.append(key)
    return sorted(set(linked))


def write_summary(result):
    lines = [
        "# OptiDrive - Sincronização Jira Final",
        "",
        "| Campo | Valor |",
        "| --- | --- |",
        f"| Projeto | {PROJECT_KEY} |",
        f"| Board | {BOARD_ID} |",
        "| Link | https://estudantes-team-jwsj3xh7.atlassian.net/jira/software/projects/OP/boards/68/backlog |",
        f"| Issues criadas nesta sincronização | {len(result['createdIssues'])} |",
        f"| Issues movidas para Done nesta sincronização | {len(result['transitionedToDone'])} |",
        f"| Links Confluence adicionados nesta sincronização | {len(result['linkedIssues'])} |",
        "",
        "## Sprints",
        "",
        "| Sprint | Jira ID | Ação | Nº Issues |",
        "| --- | ---: | --- | ---: |",
    ]
    for sprint, assignment in zip(result["sprints"], result["assignments"]):
        lines.append(
            f"| {sprint['name']} | {sprint['id']} | {sprint['action']} | {len(assignment['issues'])} |"
        )
    lines.extend(["", "## Issues por Sprint", ""])
    for assignment in result["assignments"]:
        lines.append(f"### {assignment['sprint']}")
        lines.append("")
        for key in assignment["issues"]:
            lines.append(f"- {key}")
        if assignment["missing"]:
            lines.append("")
            lines.append("Pendentes não encontrados:")
            for item in assignment["missing"]:
                lines.append(f"- {item}")
        lines.append("")

    Path("docs/jira/jira-final-sync-summary.md").write_text("\n".join(lines), encoding="utf-8")


def main():
    client = Jira()
    client.request("GET", "/rest/api/3/myself")
    type_ids = issue_type_ids(client)
    existing = all_issues_by_summary(client)
    created = create_missing_issues(client, type_ids, existing)
    existing = all_issues_by_summary(client)
    sprints = desired_sprints(client)
    assignments = add_issues_to_sprints(client, sprints, existing)
    transitioned = close_all_desired_issues(client, assignments, existing)
    linked = sync_remote_links(client, existing)
    result = {
        "createdIssues": created,
        "sprints": [{"id": s["id"], "name": s["name"], "action": s["action"]} for s in sprints],
        "assignments": assignments,
        "transitionedToDone": transitioned,
        "linkedIssues": linked,
        "jiraBacklog": "https://estudantes-team-jwsj3xh7.atlassian.net/jira/software/projects/OP/boards/68/backlog",
    }
    write_summary(result)
    print(json.dumps(result, indent=2, ensure_ascii=False))


if __name__ == "__main__":
    try:
        main()
    except Exception as exc:
        print(f"Jira sync failed: {exc}", file=sys.stderr)
        raise
