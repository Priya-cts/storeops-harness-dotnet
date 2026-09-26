# StoreOps Harness (.NET / C#)

AI-Native Tech Architect Programme — Capstone Case Study 1, Build Track.

A working Claude Code development harness (Planner → Generator → Evaluator →
Monitor) that governs AI-assisted feature development against the StoreOps
retail-operations API, plus the demonstration run for one governed feature
(shift-handover bulk status update).

## Start here

- **`CLAUDE.md`** — the harness orchestrator. Read this first.
- **`DESIGN_BRIEF.md`** — the architectural reasoning behind the harness
  (intent decomposition, governance framework, non-determinism strategy,
  key decisions). Read this second.
- **`PROMPT.md`** — the exact feature prompt used for the demonstration run.
- **`.harness/reviews/sprint-1-*.md`** — the full contract → generation →
  evaluation → run-log chain for the demonstration run.
- **`REFLECTION.md`** — what worked, what didn't, one concrete next step.
- **`DEPLOYMENT.md`** — how to run this locally or deploy it.

## Repository layout

```
CLAUDE.md                  Root orchestrator
PROMPT.md                  Demonstration-run feature prompt
DESIGN_BRIEF.md            Architectural intent (Sections A-D)
REFLECTION.md              Post-run reflection
JOURNAL.md                 Architecture journal (bonus)
DEPLOYMENT.md              Deployment record
.harness/agents/           4 agent definitions
.harness/skills/           6 skill files
.harness/reviews/          Committed sprint audit trail
.harness/output/           Gitignored working scratch space
src/StoreOps.Api/          StoreOps source (5 modules, 3-layer architecture)
tests/StoreOps.Tests/      xUnit tests
```

## Quick start

```bash
dotnet build
dotnet test
dotnet run --project src/StoreOps.Api
# GET http://localhost:5000/api/activities → 200 OK
```

See `DEPLOYMENT.md` for Docker and cloud options.
