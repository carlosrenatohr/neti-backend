# Implementation Plan — Vehicle Brands API

Single source of truth for task decomposition. Update it when decisions change; keep README and Codebase Memory aligned with the implemented state.

## Scope

Backend technical challenge: PostgreSQL persistence with EF Core, `MarcasAutos` table with deterministic seed data, `GET /api/v1/marcas-autos`, xUnit tests (InMemory + SQLite via `WebApplicationFactory`), ≥70% coverage, Docker Compose, OpenAPI and documentation.

## Solution layout

```text
src/
  MarcasAutos.Api/
  MarcasAutos.Application/
  MarcasAutos.Infrastructure/
tests/
  MarcasAutos.UnitTests/
  MarcasAutos.IntegrationTests/
```

## Tasks

Status legend: `[ ]` pending · `[x]` done and verified.

### Phase 0 — Environment discovery and baseline

| ID | Task | Files | Depends on | Acceptance | Validation |
|----|------|-------|------------|------------|------------|
| 0.1 | Confirm workspace, Git state, conventions | — | — | Real state documented | `git status`, `ls` |
| 0.2 | Check Codebase Memory availability | — | 0.1 | Index state known | `list_projects` |
| 0.3 | Establish baseline and toolchain | — | 0.1, 0.2 | SDK/tooling usable or blocker reported | `dotnet --info` |

### Phase 1 — Task documentation

| ID | Task | Files | Depends on | Acceptance | Validation |
|----|------|-------|------------|------------|------------|
| 1.1 | Document phases, tasks and decisions | `docs/tasks/*.md` | Phase 0 | Tasks have scope, criteria and validation | `git diff --cached` |
| 1.2 | Create working branch from `main` | — | 1.1 | Branch `feat/marcas-autos-api` | `git branch --show-current` |

### Phase 2 — Solution structure

| ID | Task | Files | Depends on | Acceptance | Validation |
|----|------|-------|------------|------------|------------|
| 2.1 | Create solution and five projects | `MarcasAutos.sln`, `src/**`, `tests/**` | Phase 1 | Projects and references match responsibilities | `dotnet sln list`, `dotnet build` |
| 2.2 | Add compatible package versions | `*.csproj` | 2.1 | EF Core 10 family aligned, stable packages only | `dotnet restore`, `dotnet build` |

### Phase 3 — Application and persistence

| ID | Task | Files | Depends on | Acceptance | Validation |
|----|------|-------|------------|------------|------------|
| 3.1 | Domain model and application service | `Application/**` | 2.2 | Async query with `CancellationToken`, no EF types leaked | `dotnet build` |
| 3.2 | EF Core context and PostgreSQL setup | `Infrastructure/**` | 3.1 | Fluent API config, connection string from configuration | `dotnet build` |
| 3.3 | Migration and seed data | `Infrastructure/Migrations/**` | 3.2 | `MarcasAutos` table + ≥3 deterministic rows | `dotnet ef migrations script`, `psql` checks |

### Phase 4 — REST API

| ID | Task | Files | Depends on | Acceptance | Validation |
|----|------|-------|------------|------------|------------|
| 4.1 | Controller and endpoint | `Api/Controllers/**`, `Api/Contracts/**` | 3.1, 3.2 | `GET /api/v1/marcas-autos`, thin controller, DTOs | `dotnet build`, manual `curl` |
| 4.2 | ProblemDetails and OpenAPI | `Api/Program.cs`, `Api/**` | 4.1 | Standard error shape, documented endpoint | `dotnet build`, `curl` error path |

### Phase 5 — Tests and coverage

| ID | Task | Files | Depends on | Acceptance | Validation |
|----|------|-------|------------|------------|------------|
| 5.1 | Service unit tests (InMemory) | `tests/UnitTests/**` | 3.1, 3.2 | Success, empty set, isolation per test | `dotnet test tests/UnitTests` |
| 5.2 | Controller unit tests | `tests/UnitTests/**` | 4.1 | Status codes and payload delegation | `dotnet test tests/UnitTests` |
| 5.3 | HTTP integration tests (SQLite) | `tests/IntegrationTests/**` | 4.2 | Route, serialization, query path | `dotnet test tests/IntegrationTests` |
| 5.4 | Coverage ≥70% | `Directory.Build.props`, scripts | 5.1–5.3 | Measured coverage, no artificial exclusions | `dotnet test --collect:"XPlat Code Coverage"` |

### Phase 6 — Docker and environment

| ID | Task | Files | Depends on | Acceptance | Validation |
|----|------|-------|------------|------------|------------|
| 6.1 | Multi-stage Dockerfile | `Dockerfile`, `.dockerignore` | 2.1, 4.1 | Non-root runtime image builds | `docker build` |
| 6.2 | Compose with db and api | `docker-compose.yml` | 6.1 | Volume, health check, service-name host | `docker compose config` |
| 6.3 | Integrated runtime verification | — | 6.2 | Migration applied, seed present, endpoint returns data | `docker compose up -d`, `curl`, `psql` |
| 6.4 | Config/secret review | `.env.example`, `appsettings*.json` | 6.2 | No real secrets, reproducible setup | manual review |

### Phase 7 — Documentation and Codebase Memory

| ID | Task | Files | Depends on | Acceptance | Validation |
|----|------|-------|------------|------------|------------|
| 7.1 | Maintain memory during structural changes | Codebase Memory | Phases 2–6 | Graph reflects implemented architecture | `detect_changes`, `index_repository` |
| 7.2 | Write README | `README.md` | Phases 3–6 | Setup, migrations, tests, coverage, Compose documented | command verification |
| 7.3 | Final consistency review | Memory, README, code | 7.1, 7.2 | No contradictions or duplicated memory | manual review |

### Phase 8 — Review and delivery

| ID | Task | Files | Depends on | Acceptance | Validation |
|----|------|-------|------------|------------|------------|
| 8.1 | Engineering review | full diff | Phases 0–7 | Findings triaged and blocking issues fixed | review notes |
| 8.2 | Final verification battery | — | 8.1 | Build, migrations, seed, tests, coverage, Compose verified | all commands re-run |
| 8.3 | Delivery package | ZIP | 8.2 | Complete project, no secrets or artifacts | archive inspection |

## Workflow per task

1. Implement only the task in scope.
2. Review the actual diff.
3. Run the task validations and a brief local gate (`dotnet build` and applicable tests).
4. Report results and pending issues.
5. Propose one Conventional Commit and wait for explicit approval.

Never advance on a red gate. Never claim a validation passed without output evidence.

## Decisions

| ID | Decision | Rationale |
|----|----------|-----------|
| D1 | Three production projects + two test projects | Proportional Clean Architecture without extra layers |
| D2 | `HasData` for seed | Deterministic, migration-owned reference data |
| D3 | InMemory for unit tests, SQLite for HTTP integration | Fast isolation plus relational behavior; PostgreSQL verified separately |
| D4 | Local .NET SDK in `~/.dotnet` | No `sudo` in this WSL environment |
| D5 | `docs/tasks/` in repository | Planning stays version-controlled and portable |
| D6 | Empty result returns `200` with `[]` | Collection exists and query succeeded |
| D7 | No CQRS, generic repositories, extra layers | Keep the solution proportional to the challenge |

## Open items

- Docker daemon and PostgreSQL cluster are down; verification strategy decided: Docker Desktop/WSL integration (Phase 6).
- `dotnet-ef` global tool not installed yet (Phase 3).
- Remote repository not configured; no push planned without authorization.
