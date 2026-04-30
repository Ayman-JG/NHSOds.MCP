Add a new NHS organisation type to this MCP server.

## What this command does

Orchestrates three sub-agents in sequence to safely add a new tool end-to-end:
1. `api-validator` — confirms the ODS API returns data for this role and the response matches our models
2. `tool-scaffolder` — writes the new tool method and API client method
3. `test-writer` — writes integration tests for the new tool

## Usage

```
/add-tool <ODS role code> <display name>
```

Examples:
```
/add-tool RO110 "General Dental Practice"
/add-tool RO182 "Optician"
/add-tool RO87 "Walk-in Centre"
```

## Steps

1. Look up the role code at https://directory.spineservices.nhs.uk/ORD/2-0-0/roles to confirm it exists and get the exact display name
2. Derive the tool name: strip spaces and special characters from the display name, prefix with `Find` (e.g. `FindDentalPractice`)
3. Invoke `api-validator` with the role code and a generic search term ("London")
4. If validator says ISSUES FOUND — stop and report to the user before proceeding
5. If validator says SAFE TO PROCEED — invoke `tool-scaffolder` with the role code, display name, and tool name
6. Invoke `test-writer` for the new tool
7. Run `dotnet build` to confirm no compilation errors
8. Summarise what was added and remind the user to run `dotnet test` and push
