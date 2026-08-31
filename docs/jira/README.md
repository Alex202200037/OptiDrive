# Jira Integration Notes

This repo now includes a small importer to push OptiDrive presets into Jira by API.

## Files

- `tools/jira_import.py`: CLI utility for validating Jira access, inspecting metadata, and creating issues.
- `docs/jira/optidrive-presets.json`: starter bootstrap file based on the project work breakdown.
- `docs/jira/optidrive-improvement-sprints-6-7.json`: backlog for the two improvement sprints requested after feedback.
- `docs/jira/jira-sprints-6-7-operacao.md`: practical operating guide for opening, moving, closing and commenting the two sprints.

## Required environment variables

```bash
export JIRA_BASE_URL="https://your-site.atlassian.net"
export JIRA_EMAIL="you@example.com"
export JIRA_API_TOKEN="your-token"
export JIRA_PROJECT_KEY="OP"
```

Optional, only if you want to force the scoped `ex/jira` flow:

```bash
export JIRA_USE_EX_API="1"
export JIRA_CLOUD_ID="7151c7c0-a1d9-4aca-96ae-7e2f53dda944"
```

## Quick checks

Validate authentication:

```bash
python3 tools/jira_import.py validate
```

Inspect the target project metadata:

```bash
python3 tools/jira_import.py metadata --project OP
```

Preview what would be created:

```bash
python3 tools/jira_import.py import \
  --project OP \
  --file docs/jira/optidrive-presets.json
```

Actually create the issues:

```bash
python3 tools/jira_import.py import \
  --project OP \
  --file docs/jira/optidrive-presets.json \
  --execute
```

## Important Jira token note

Atlassian currently supports two common token flows for simple scripting:

- classic/basic token calls against `https://your-site.atlassian.net/rest/api/...`
- scoped API tokens that may require the Atlassian `ex/jira/{cloudId}` path

The importer auto-tries the site REST path first and can also use the scoped `ex/jira/{cloudId}` path.

If validation returns `401 Unauthorized` on `/rest/api/3/myself`, rotate the API token before importing issues.

## Practical fix

Create a fresh Jira API token from:

- [Atlassian API token management](https://id.atlassian.com/manage-profile/security/api-tokens)

Then test again with:

```bash
python3 tools/jira_import.py validate
```

If you want, we can keep using this same importer after the token is rotated.
