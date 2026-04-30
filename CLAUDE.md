# NHSOds.MCP — Claude Code Context

## What this is
A C# .NET 9 MCP server wrapping the NHS Organisation Data Service (ODS).
Exposes GP practices, pharmacies, and NHS trusts as agentic tools over stdio.

Built and maintained using a Claude Code multi-agent workflow.

## Project structure
```
NHSOds.MCP/
├── Program.cs                      # Host setup, DI, MCP server registration
├── Tools/NHSOdsTools.cs            # Four MCP tools — all static, DI-injected params
├── Services/OdsApiClient.cs        # Typed HttpClient wrapping ODS REST API
├── Models/OdsModels.cs             # Deserialization records
│                                   # Note: OrgId is a string in search results,
│                                   # an object in detail results — intentional API quirk
└── .github/workflows/ci.yml        # Build on push, publish to NuGet on release
```

## Agentic workflow

### Sub-agents (`.claude/agents/`)
| Agent | Responsibility |
|---|---|
| `tool-scaffolder` | Adds a new NHS org type end-to-end (constant + API method + tool method) |
| `test-writer` | Writes xUnit integration tests for any tool against the live ODS API |
| `api-validator` | Validates live ODS API response shape matches OdsModels.cs before any code is written |

### Slash commands (`.claude/commands/`)
| Command | What it does |
|---|---|
| `/add-tool <role-code> <display-name>` | Full pipeline: validate → scaffold → test → build |
| `/test` | Runs dotnet test and summarises pass/fail clearly |
| `/validate-api <role-code>` | Runs api-validator and reports verdict |

### Hooks (`.claude/settings.json`)
| Hook | Trigger | Action |
|---|---|---|
| PreToolUse | Write/Edit on NHSOdsTools.cs | Reminds to include [McpServerTool] + [Description] |
| PostToolUse | Write/Edit on any .cs file | Reminds to run dotnet build |

## Key decisions
- **Static tool methods**: dependencies injected as method params, not constructor,
  matching the official MCP C# SDK pattern (ModelContextProtocol 1.2.0)
- **Typed HttpClient**: registered via `AddHttpClient<T>` — IHttpClientFactory manages
  connection pooling and lifetime
- **Logging to stderr**: mandatory for stdio MCP — anything to stdout corrupts
  the JSON-RPC stream
- **No auth**: the ODS API at `directory.spineservices.nhs.uk` is completely open
- **OrgId quirk**: search results return OrgId as a plain string, detail endpoint
  returns it as an object with an `extension` field — models handle both

## Running locally
```bash
dotnet run
```
Then add to Claude Desktop's config:
```json
{
  "mcpServers": {
    "nhs-ods": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/NHSOds.MCP"]
    }
  }
}
```
Or run `bash add-to-claude.sh` to configure automatically.

## Adding a new org type
The fastest path — one command does everything:
```
/add-tool RO110 "General Dental Practice"
```
This validates the API, scaffolds the code, writes tests, and builds.

## ODS role codes for common org types
| Role | Code |
|---|---|
| GP Practice | RO76 |
| Pharmacy | RO182 |
| NHS Trust | RO197 |
| NHS Trust Site | RO198 |
| General Dental Practice | RO110 |
| Optical Site | RO167 |
| Walk-in Centre | RO87 |

Full list: https://directory.spineservices.nhs.uk/ORD/2-0-0/roles
