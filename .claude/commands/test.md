Run the integration test suite and summarise results.

## What this command does

Runs `dotnet test` against the live ODS API and gives you a clear pass/fail summary — not raw xUnit output.

## Steps

1. Run `dotnet test --verbosity normal --logger "console;verbosity=detailed"` from the repo root
2. Parse the output and produce a summary in this format:

   ### Test results — {timestamp}
   | Suite | Passed | Failed | Skipped |
   |---|---|---|---|
   | NHSOds.MCP.Tests | N | N | N |

   **Failed tests** (if any):
   - `{TestName}` — {failure message, one line}

   **Verdict:** ✅ All passing / ❌ {N} failures

3. If any test failed with a network error (HttpRequestException, TaskCanceledException), note that the ODS API may be temporarily unavailable and the failure is likely transient

## Notes
- Tests call the live ODS API — a flaky network will produce flaky results
- Do not modify test assertions to make tests pass — fix the implementation instead
