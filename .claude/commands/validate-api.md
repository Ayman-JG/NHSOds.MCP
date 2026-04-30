Validate that ODS API responses match our C# model definitions.

## What this command does

Invokes the `api-validator` sub-agent for a given ODS role code and gives you a clear verdict before you write any code.

## Usage

```
/validate-api <ODS role code>
```

Examples:
```
/validate-api RO110
/validate-api RO167
```

## Steps

1. If no role code provided, ask the user for one and show the roles endpoint:
   https://directory.spineservices.nhs.uk/ORD/2-0-0/roles
2. Invoke the `api-validator` sub-agent with the role code and search term "London"
3. Present the validator's report directly — do not summarise or paraphrase it
4. If verdict is SAFE TO PROCEED, suggest running `/add-tool {roleCode} "{displayName}"` as the next step
5. If verdict is ISSUES FOUND, explain what would need to change in `OdsModels.cs` before proceeding
