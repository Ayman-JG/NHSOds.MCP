# NHSOds.MCP

A **C# .NET 9 Model Context Protocol (MCP) server** wrapping the [NHS Organisation Data Service (ODS)](https://digital.nhs.uk/services/organisation-data-service) — exposing GP practices, pharmacies, and NHS trusts as agentic tools consumable by Claude Code, Claude Desktop, or any MCP-compatible client.

No API key required. The ODS API is completely open.

---

## Tools

| Tool | Description |
|---|---|
| `FindGpPractice` | Search for GP practices by name or town, with optional postcode filter |
| `FindPharmacy` | Search for pharmacies by name or town |
| `FindNhsTrust` | Search for NHS trusts and hospital sites by name or region |
| `GetOrganisation` | Get full address, phone, and status for any NHS org by ODS code |

---

## Getting started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- Claude Desktop (for local testing)

### Run locally

```bash
git clone https://github.com/Ayman-JG/NHSOds.MCP
cd NHSOds.MCP
dotnet run
```

### Connect to Claude Desktop

**Quickest way (macOS):** run the included setup script — it edits the config file for you:

```bash
bash add-to-claude.sh
```

**Manual setup:** add to `~/Library/Application Support/Claude/claude_desktop_config.json` (macOS) or `%APPDATA%\Claude\claude_desktop_config.json` (Windows):

```json
{
  "mcpServers": {
    "nhs-ods": {
      "command": "dotnet",
      "args": ["run", "--project", "/absolute/path/to/NHSOds.MCP"]
    }
  }
}
```

Restart Claude Desktop. You should see the NHS ODS tools available.

---

## Example prompts

Once connected to Claude Desktop, try:

> *"Find GP practices in Harrogate"*

> *"Look up ODS code B86013 and give me the full address and phone number"*

> *"Are there any Boots pharmacies in Leeds?"*

> *"Find the NHS trust for Leeds Teaching Hospital"*

---

## Architecture

```
Program.cs
  └── AddHttpClient<OdsApiClient>  (typed client, connection-pooled)
  └── AddMcpServer()
        └── WithStdioServerTransport()
        └── WithTools<NHSOdsTools>()   (SDK discovers [McpServerTool] methods)

Tools/NHSOdsTools.cs
  └── Static methods, dependencies injected per-call from DI
  └── FindGpPractice → OdsApiClient.FindGpPracticesAsync
  └── FindPharmacy   → OdsApiClient.FindPharmaciesAsync
  └── FindNhsTrust   → OdsApiClient.FindNhsTrustsAsync
  └── GetOrganisation → OdsApiClient.GetOrganisationAsync

Services/OdsApiClient.cs
  └── Typed HttpClient → directory.spineservices.nhs.uk/ORD/2-0-0/
  └── ODS role codes: RO76 (GP), RO182 (Pharmacy), RO197 (Trust)

Models/OdsModels.cs
  └── Note: OrgId is a string in search results, an object in detail results
```

## Extending

**Add a new org type** — find the role code at the [ODS roles endpoint](https://directory.spineservices.nhs.uk/ORD/2-0-0/roles), add a constant and method to `OdsApiClient`, and a new `[McpServerTool]` method to `NHSOdsTools`.

**Add HTTP transport** — add `ModelContextProtocol.AspNetCore`, swap `.WithStdioServerTransport()` for `.WithHttpTransport()` in `Program.cs`. Deployable to AWS Lambda Function URL or Azure Container Apps.

---

## CI/CD

**Build & test** (`.github/workflows/ci.yml`):
- **Push / PR to main**: build + test
- **Release published**: pack and publish to NuGet

**Automated PR review** (`.github/workflows/pr-review.yml`):

Every pull request is reviewed by Claude Code via [`anthropics/claude-code-action`](https://github.com/anthropics/claude-code-action). Claude reads the diff and posts a comment enforcing project conventions — `[McpServerTool]`/`[Description]` attributes, the `OrgId` model duality, stderr-only logging, and `HttpClient` lifetime rules.

You can also trigger a review on demand by commenting `@claude` on any PR.

### Required secret

Add your Anthropic API key to the repository before the workflow can run:

1. Go to **Settings → Secrets and variables → Actions**
2. Click **New repository secret**
3. Name: `ANTHROPIC_API_KEY`, value: your key from [console.anthropic.com](https://console.anthropic.com)

---

## License

MIT

---

## Agentic development workflow

This project is built and maintained using a Claude Code multi-agent workflow defined in `.claude/`.

### Sub-agents

| Agent | When to use |
|---|---|
| `tool-scaffolder` | Adding a new NHS org type — writes the role constant, API method, and MCP tool end-to-end |
| `test-writer` | Writing xUnit integration tests for any tool against the live ODS API |
| `api-validator` | Checking that live ODS API responses match `OdsModels.cs` before writing any code |

### Slash commands

| Command | What it does |
|---|---|
| `/add-tool <role-code> <display-name>` | Full pipeline: validate API → scaffold code → write tests → build |
| `/validate-api <role-code>` | Run `api-validator` and get a SAFE/ISSUES verdict before touching code |
| `/test` | Run the test suite and get a clean pass/fail summary |

### Hooks

Two hooks in `.claude/settings.json` guard code quality automatically:
- **PreToolUse** on `NHSOdsTools.cs` — reminds to include `[McpServerTool]` and `[Description]` on every new method
- **PostToolUse** on any `.cs` file — reminds to run `dotnet build` after edits

### Adding a new org type

The fastest path — one command orchestrates the full pipeline:

```
/add-tool RO110 "General Dental Practice"
```

Under the hood this: validates the API shape, scaffolds the C# code, writes integration tests, and runs `dotnet build` to confirm everything compiles.
