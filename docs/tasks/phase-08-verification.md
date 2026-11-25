# Phase 8 — Final verification

Executed 2026-11-25 against the delivered branch state. Every result below comes from a command run in-session.

## Build and tests

| Check | Command | Result |
|-------|---------|--------|
| Preflight | `./scripts/preflight.sh` | OK (SDK, Docker, PostgreSQL) |
| Restore | `dotnet restore MarcasAutos.sln` | Up to date |
| Release build | `dotnet build -c Release` | 0 warnings, 0 errors |
| Unit tests | `dotnet test -c Release` | 8 passed |
| Integration tests | `dotnet test -c Release` | 4 passed |
| Coverage | `./scripts/coverage.sh` | 100% lines (68/68), 100% branches (4/4) |

## Database

| Check | Evidence |
|-------|----------|
| Migration applied | `__EFMigrationsHistory` contains `20261125175125_InitialCreate` |
| Table created | `MarcasAutos (Id identity PK, Nombre varchar(100) NOT NULL)` |
| Unique index | `IX_MarcasAutos_Nombre UNIQUE` |
| Seed rows | Toyota, Ford, Honda, Chevrolet with ids 1–4 |

Verified on a fresh Compose volume (`docker compose down -v` before startup).

## API and containers

| Check | Evidence |
|-------|----------|
| Health | `GET /health` -> `200 Healthy` (database check included) |
| Endpoint | `GET /api/v1/marcas-autos` -> `200` with 4 brands ordered by name |
| OpenAPI | `/openapi/v1.json` contains `/api/v1/marcas-autos` and `MarcaAutoDto` |
| Migration ordering | `migrator` exits 0 before `api` starts |
| Container user | API process runs as `uid=1654(app)`, not root |
| Service host | Connection string uses `Host=db` inside Compose |

## Security and configuration

- No real secrets in tracked files; `.env` is git-ignored and `.env.example` holds development-only values.
- No credentials in logs.
- Errors return RFC 7807 `ProblemDetails` without stack traces.
- Container runtime uses the built-in non-root user.

## Delivery

- Branch: `feat/marcas-autos-api`.
- Commits follow Conventional Commits and are listed in the final report.
- ZIP: `marcas-autos-api.zip` next to the repository, excluding `.git`, `bin`, `obj`, `coverage`, `logs` and `.env`.
