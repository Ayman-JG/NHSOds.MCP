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

GitHub Actions pipeline (`.github/workflows/ci.yml`):
- **Push / PR to main**: build + test
- **Release published**: pack and publish to NuGet

---

## License

MIT
