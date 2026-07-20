# Debugging Notes

Session notes for environment and tooling issues encountered while building SmeErp.


## Issue 1 — Port Already In Use When Starting the Web App

### Problem
After Prompt #6 (authentication), running `dotnet run` failed because the
app could not bind to its configured ports (`https://localhost:7211` /
`http://localhost:5057`) — another instance of SmeErp.Web was already
listening from a prior run that had not been stopped.

### How I Investigated
- Read the console output from `dotnet run`; it reported that the address
  was already in use.
- Checked which process was holding the port (Task Manager / `netstat`
  for listeners on 7211 and 5057).
- Confirmed a leftover `dotnet` host process from an earlier dev session.

### How AI Helped
- Cursor had previously started the web app in the background to verify
  Identity seeding and login flow.
- Pointed out that the background `dotnet run` was still running and
  blocking the port on the next launch attempt.

### What I Validated
- Stopped the orphaned `dotnet` process.
- Re-ran `dotnet run` from `src/SmeErp.Web` — app started cleanly.
- Logged in as both seeded users; dashboard showed the correct CompanyId
  for each account.

### Final Fix
Stop any stale `dotnet run` / SmeErp.Web host before starting a new dev
session. On Windows: `Get-Process dotnet` and end the orphaned process,
or close the terminal that launched the prior run.


## Issue 2 — Build Failure Due to Locked DLL from Stale Process

### Problem
While working through Prompt #7 (DB-stored JWT signing key), `dotnet build`
and/or `dotnet ef database update` failed with an MSBuild copy error:
`SmeErp.Infrastructure.dll` could not be written because it was locked by
another process. This blocked generating or applying migration
`20260720054641_AddSigningKeyTable`.

### How I Investigated
- Read the full build output; MSBuild reported access denied / file in use
  when copying to `src\SmeErp.Infrastructure\bin\Debug\net6.0\SmeErp.Infrastructure.dll`
  (MSB3027-style error).
- Listed running `dotnet` processes with `Get-Process dotnet` in PowerShell.
- Matched the locking process to a background `dotnet run --no-build` that
  Cursor had started earlier to verify `SigningKeySeeder` inserted a row on
  first startup (PID 80624 in the agent session).

### How AI Helped
- Cursor identified that the verification step (`dotnet run` in the
  background after implementing `ISigningKeyService` / `SigningKeyService`)
  was still holding `SmeErp.Infrastructure.dll` and `SmeErp.Web.dll` in memory.
- Suggested killing the stale process rather than changing any project code,
  since the failure was environmental rather than a compile error.

### What I Validated
- Ran `Stop-Process -Id 80624 -Force` (or killed the matching `dotnet`
  process via Task Manager).
- Re-ran `dotnet build SmeErp.sln` — succeeded with 0 warnings / 0 errors.
- Re-ran `dotnet ef database update` with startup project `SmeErp.Web` —
  migration `20260720054641_AddSigningKeyTable` applied successfully.
- Queried `SigningKeys` in SSMS: one active row with a base64 `KeyValue`,
  `IsActive = 1`, and `ExpiresAt` approximately 30 days after `CreatedAt`
  (no hardcoded secret in source or config).

### Final Fix
Before rebuilding or running EF migrations after a Cursor/agent verification
run, ensure no background `dotnet run` is still alive:
`Get-Process dotnet | Stop-Process -Force` (target the specific PID if
multiple dotnet processes are running). Then rebuild and apply migrations
as normal.
