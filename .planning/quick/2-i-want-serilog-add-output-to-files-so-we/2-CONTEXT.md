# Quick Task 2: Serilog file output for development + CLAUDE.md update - Context

**Gathered:** 2026-03-13
**Status:** Ready for planning

<domain>
## Task Boundary

Add Serilog with file output so backend logs can be checked easily in development. Update CLAUDE.md to document where to find backend logs.

</domain>

<decisions>
## Implementation Decisions

### Log file location & rotation
- Store logs in `logs/` folder at project root (gitignored)
- Daily rolling strategy, keep 7 days of files

### Log output format
- Plain text for the dev file sink (human-readable, easy to tail/grep)
- JSON structured logging available as separate configuration

### Environment scoping
- File sink configured only in appsettings.Development.json
- Production uses other sinks later (not in scope)

### Claude's Discretion
- Serilog package selection (core + sinks)
- Exact log template format
- Integration approach in Program.cs

</decisions>

<specifics>
## Specific Ideas

- Project currently has NO Serilog — uses default Microsoft.Extensions.Logging
- Need to add Serilog NuGet packages from scratch
- CLAUDE.md should document the logs/ directory location and how to access logs

</specifics>
