# NHSOds.MCP — Claude Code Context

## What this is
A C# .NET 9 MCP server wrapping the NHS Organisation Data Service (ODS).
Exposes GP practices, pharmacies, and NHS trusts as agentic tools over stdio.

## Project structure
```
NHSOds.MCP/
├── Program.cs                  # Host setup, DI, MCP server registration
├── Tools/NHSOdsTools.cs        # Four MCP tools — all static, DI-injected params
├── Services/OdsApiClient.cs    # Typed HttpClient wrapping ODS REST API
├── Models/OdsModels.cs         # Deserialization records (note: OrgId differs
│                               # between search (string) and detail (object))
└── .github/workflows/ci.yml    # Build on push, publish to NuGet on release
```

## Key decisions
- **Static tool methods**: dependencies injected as method params, not constructor,
  matching the official MCP C# SDK pattern.
- **Typed HttpClient**: `OdsApiClient` registered via `AddHttpClient<T>` so
  `IHttpClientFactory` manages connection pooling and lifetime.
- **Logging to stderr**: mandatory for stdio MCP — anything to stdout corrupts
  the JSON-RPC stream.
- **No auth**: the ODS API at `directory.spineservices.nhs.uk` is completely open.

## Running locally
```bash
dotnet run
```
Then add to Claude Desktop's `claude_desktop_config.json`:
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

## Common tasks
- **Add a new tool**: add a static method to `NHSOdsTools.cs` with `[McpServerTool]`
  and `[Description]`. The SDK discovers it automatically.
- **Add a new org type**: add a role constant to `OdsApiClient` (see ODS role list:
  https://directory.spineservices.nhs.uk/ORD/2-0-0/roles) and a corresponding method.
- **Streamable HTTP**: add `ModelContextProtocol.AspNetCore` package and swap
  `.WithStdioServerTransport()` for `.WithHttpTransport()` in Program.cs.
