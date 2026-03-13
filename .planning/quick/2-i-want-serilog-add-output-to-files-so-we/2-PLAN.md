---
phase: quick-2
plan: 01
type: execute
wave: 1
depends_on: []
files_modified:
  - backend/src/Bootstrapper/Bootstrapper.csproj
  - backend/src/Bootstrapper/Program.cs
  - backend/src/Bootstrapper/appsettings.Development.json
  - backend/.gitignore
  - CLAUDE.md
autonomous: true
requirements: [SERILOG-FILE-SINK, CLAUDE-MD-UPDATE]

must_haves:
  truths:
    - "Backend logs are written to daily rolling files in backend/logs/ during development"
    - "Log files are plain text, human-readable, easy to tail/grep"
    - "Log files rotate daily and keep 7 days of history"
    - "logs/ directory is gitignored"
    - "CLAUDE.md documents where to find backend logs"
  artifacts:
    - path: "backend/src/Bootstrapper/Bootstrapper.csproj"
      provides: "Serilog NuGet package references"
      contains: "Serilog.AspNetCore"
    - path: "backend/src/Bootstrapper/Program.cs"
      provides: "Serilog integration in host builder"
      contains: "UseSerilog"
    - path: "backend/src/Bootstrapper/appsettings.Development.json"
      provides: "File sink configuration for development"
      contains: "Serilog"
    - path: "backend/.gitignore"
      provides: "logs/ directory exclusion"
      contains: "logs/"
    - path: "CLAUDE.md"
      provides: "Log location documentation"
      contains: "logs/"
  key_links:
    - from: "appsettings.Development.json"
      to: "Program.cs"
      via: "Serilog reads config from IConfiguration"
      pattern: "ReadFrom\\.Configuration"
---

<objective>
Add Serilog with file output to the backend for development use, and update CLAUDE.md to document where logs are stored.

Purpose: Enable easy log inspection during development by writing structured logs to daily rolling plain-text files.
Output: Serilog integrated into Bootstrapper, file sink active in Development, logs/ gitignored, CLAUDE.md updated.
</objective>

<execution_context>
@C:/Users/minhh/.claude/get-shit-done/workflows/execute-plan.md
@C:/Users/minhh/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@.planning/quick/2-i-want-serilog-add-output-to-files-so-we/2-CONTEXT.md
@backend/src/Bootstrapper/Program.cs
@backend/src/Bootstrapper/Bootstrapper.csproj
@backend/src/Bootstrapper/appsettings.Development.json
@backend/.gitignore
@CLAUDE.md
</context>

<tasks>

<task type="auto">
  <name>Task 1: Add Serilog packages, configure file sink, integrate into Program.cs</name>
  <files>
    backend/src/Bootstrapper/Bootstrapper.csproj,
    backend/src/Bootstrapper/Program.cs,
    backend/src/Bootstrapper/appsettings.Development.json
  </files>
  <action>
1. Add NuGet packages to Bootstrapper.csproj:
   - Serilog.AspNetCore (includes Serilog core, Serilog.Extensions.Hosting, Serilog.Sinks.Console)
   - Serilog.Sinks.File

2. In Program.cs, add Serilog integration right after `var builder = WebApplication.CreateBuilder(args);`:
   ```csharp
   builder.Host.UseSerilog((context, loggerConfiguration) =>
       loggerConfiguration.ReadFrom.Configuration(context.Configuration));
   ```
   This replaces the default Microsoft logging with Serilog and reads config from appsettings.
   Add `using Serilog;` at the top of the file.

3. In appsettings.Development.json, replace the existing "Logging" section with a "Serilog" section:
   ```json
   "Serilog": {
     "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
     "MinimumLevel": {
       "Default": "Information",
       "Override": {
         "Microsoft.AspNetCore": "Information",
         "Microsoft.EntityFrameworkCore.Database.Command": "Information",
         "Wolverine": "Debug"
       }
     },
     "WriteTo": [
       { "Name": "Console" },
       {
         "Name": "File",
         "Args": {
           "path": "logs/ganka-.log",
           "rollingInterval": "Day",
           "retainedFileCountLimit": 7,
           "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"
         }
       }
     ]
   }
   ```
   Keep the "Logging" section as-is (Serilog reads its own section; the Microsoft section is harmless fallback).
   Actually, REMOVE the "Logging" section since Serilog replaces it entirely. Keep ConnectionStrings, Jwt, and Admin sections unchanged.

4. In appsettings.json (production), add a minimal Serilog section with Console only (no file sink):
   ```json
   "Serilog": {
     "MinimumLevel": {
       "Default": "Information",
       "Override": {
         "Microsoft.AspNetCore": "Warning",
         "Microsoft.EntityFrameworkCore": "Warning",
         "Wolverine": "Information"
       }
     },
     "WriteTo": [
       { "Name": "Console" }
     ]
   }
   ```
   Remove the "Logging" section from appsettings.json as well.

IMPORTANT: The `builder.Host.UseSerilog(...)` call MUST come BEFORE the `builder.Host.UseWolverine(...)` call. Place it right after `var builder = WebApplication.CreateBuilder(args);` on line 49.

Note: The log file path "logs/ganka-.log" is relative to the working directory (backend/src/Bootstrapper when running via `dotnet run`). Serilog will create the logs/ directory automatically. The rolling interval appends the date, producing files like `logs/ganka-20260313.log`.
  </action>
  <verify>
    <automated>cd D:/projects/ganka/backend && dotnet build src/Bootstrapper/Bootstrapper.csproj --no-restore 2>&1 | tail -5</automated>
  </verify>
  <done>
    - Bootstrapper.csproj has Serilog.AspNetCore and Serilog.Sinks.File package references
    - Program.cs calls builder.Host.UseSerilog with ReadFrom.Configuration
    - appsettings.Development.json has Serilog section with File sink (daily rolling, 7 days retention, plain text template)
    - Project builds successfully
  </done>
</task>

<task type="auto">
  <name>Task 2: Gitignore logs directory and update CLAUDE.md</name>
  <files>
    backend/.gitignore,
    CLAUDE.md
  </files>
  <action>
1. Add to backend/.gitignore (at the end, with a comment):
   ```
   ## Serilog log files
   logs/
   ```

2. Add a new section to CLAUDE.md after the "# BACKEND" section (before "# Test Account Credentials"):
   ```markdown
   # LOGS

   - Backend logs are written to `backend/src/Bootstrapper/logs/` (dev only, gitignored)
   - Log files rotate daily with pattern `ganka-YYYYMMDD.log`
   - Retained for 7 days, plain text format
   - To tail logs: `Get-Content backend/src/Bootstrapper/logs/ganka-*.log -Tail 50 -Wait` (PowerShell) or `tail -f backend/src/Bootstrapper/logs/ganka-*.log` (bash)
   ```
  </action>
  <verify>
    <automated>grep -q "logs/" D:/projects/ganka/backend/.gitignore && grep -q "logs/" D:/projects/ganka/CLAUDE.md && echo "PASS" || echo "FAIL"</automated>
  </verify>
  <done>
    - backend/.gitignore contains logs/ entry
    - CLAUDE.md has a LOGS section documenting file location, rotation, and how to tail logs
  </done>
</task>

</tasks>

<verification>
1. `cd backend && dotnet build src/Bootstrapper/Bootstrapper.csproj` succeeds
2. `grep "Serilog" backend/src/Bootstrapper/Bootstrapper.csproj` shows package references
3. `grep "UseSerilog" backend/src/Bootstrapper/Program.cs` shows integration
4. `grep "logs/" backend/.gitignore` shows gitignore entry
5. `grep -A5 "# LOGS" CLAUDE.md` shows documentation
</verification>

<success_criteria>
- Backend builds and runs with Serilog replacing default Microsoft logging
- In Development mode, logs are written to daily rolling plain-text files in logs/
- Log files are gitignored
- CLAUDE.md documents the log location and how to access logs
</success_criteria>

<output>
After completion, create `.planning/quick/2-i-want-serilog-add-output-to-files-so-we/2-SUMMARY.md`
</output>
