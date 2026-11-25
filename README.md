# Marcas Autos API

Backend technical challenge implemented with .NET 10, ASP.NET Core, Entity Framework Core and PostgreSQL. It exposes a single read endpoint backed by a migrated, seeded `MarcasAutos` table, with unit and HTTP integration tests and a reproducible Docker Compose environment.

## 1. Overview

- Domain: vehicle brands (`MarcasAutos`).
- Endpoint: `GET /api/v1/marcas-autos`.
- Persistence: PostgreSQL through Npgsql; schema owned by EF Core migrations.
- Delivery: multi-stage Docker image, Compose stack with database, one-shot migrator and API.
- Quality: 12 tests, 100% line and branch coverage on authored code.

## 2. Architecture

```text
src/
  MarcasAutos.Api/             HTTP layer: controller, ProblemDetails, health checks, OpenAPI
  MarcasAutos.Application/     Entity, DTO, repository port, service and DI registration
  MarcasAutos.Infrastructure/  DbContext, Fluent API configuration, repository, migrations
tests/
  MarcasAutos.UnitTests/       Service/controller tests with EF Core InMemory and NSubstitute
  MarcasAutos.IntegrationTests/ HTTP tests with WebApplicationFactory and SQLite in-memory
```

Dependency direction:

```text
Api ──> Application <── Infrastructure
 │                              ▲
 └──────────────────────────────┘
tests ──> the projects they exercise
```

Request flow:

```text
GET /api/v1/marcas-autos
  -> MarcasAutosController (HTTP concerns)
  -> IMarcasAutosService / MarcasAutosService (maps entity to DTO)
  -> IMarcasAutosRepository / MarcasAutosRepository (AsNoTracking, ordered query)
  -> MarcasAutosDbContext -> PostgreSQL
```

## 3. Technology stack

| Concern | Choice |
|---------|--------|
| Runtime | .NET 10 LTS (SDK 10.0.404, ASP.NET Core runtime 10.0.15) |
| Language | C# 14 |
| API | ASP.NET Core controllers, OpenAPI |
| Persistence | EF Core 10.0.15, Npgsql.EntityFrameworkCore.PostgreSQL 10.0.4 |
| Tests | xUnit 2.9, NSubstitute 6.2, EF Core InMemory, SQLite in-memory, WebApplicationFactory |
| Coverage | Coverlet collector + ReportGenerator |
| Containers | Docker multi-stage, Docker Compose v2 |

## 4. Prerequisites

- .NET 10 SDK.
- Docker Engine with Compose v2 (only needed for the containerized flow).
- PostgreSQL 14+ if you run the database outside Docker.
- Optional: `dotnet-ef` global tool and `dotnet-reportgenerator-globaltool`.

```bash
dotnet tool install --global dotnet-ef --version 10.0.15
dotnet tool install --global dotnet-reportgenerator-globaltool
```

If the SDK is installed user-local, export it first:

```bash
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$HOME/.dotnet/tools:$PATH"
```

## 5. Local development

```bash
dotnet restore
dotnet build
dotnet run --project src/MarcasAutos.Api
```

`launchSettings.json` exposes the API on `http://localhost:5130` during `dotnet run`.

## 6. PostgreSQL configuration

The connection string is read from `ConnectionStrings:MarcasAutos`:

- Development default in `src/MarcasAutos.Api/appsettings.json` (local, non-secret values).
- Override with the standard environment variable:

```bash
export ConnectionStrings__MarcasAutos="Host=localhost;Port=5432;Database=marcas_autos;Username=postgres;Password=postgres"
```

Inside Compose the host is the service name `db`, never `localhost`. Startup fails fast if the connection string is missing.

## 7. Migrations

```bash
# Apply migrations
dotnet ef database update \
  --project src/MarcasAutos.Infrastructure \
  --startup-project src/MarcasAutos.Api

# Add a new migration
dotnet ef migrations add <Name> \
  --project src/MarcasAutos.Infrastructure \
  --startup-project src/MarcasAutos.Api \
  --output-dir Persistence/Migrations

# Inspect the generated SQL
dotnet ef migrations script \
  --project src/MarcasAutos.Infrastructure \
  --startup-project src/MarcasAutos.Api
```

Migrations are the only schema mechanism; `EnsureCreated` is not used by the application.

## 8. Seed data

`MarcaAutoConfiguration` declares deterministic seed rows through `HasData`, so the initial migration inserts them:

| Id | Nombre |
|----|--------|
| 1 | Toyota |
| 2 | Ford |
| 3 | Honda |
| 4 | Chevrolet |

Re-running `database update` is idempotent: EF Core records the migration in `__EFMigrationsHistory`.

## 9. API

```http
GET /api/v1/marcas-autos
```

Sample response (`200 OK`):

```json
[
  { "id": 4, "nombre": "Chevrolet" },
  { "id": 2, "nombre": "Ford" },
  { "id": 3, "nombre": "Honda" },
  { "id": 1, "nombre": "Toyota" }
]
```

- Empty table: `200 OK` with `[]`.
- Unhandled errors and status-only responses use RFC 7807 `ProblemDetails`.
- OpenAPI document: `/openapi/v1.json` in Development.
- Health: `GET /health` (includes a database connectivity check).

## 10. Docker Compose

```bash
cp .env.example .env    # optional overrides
docker compose up --build
```

Services:

| Service | Purpose |
|---------|---------|
| `db` | PostgreSQL 17 with a persistent volume and `pg_isready` health check |
| `migrator` | One-shot container that applies EF Core migrations, waits for a healthy database |
| `api` | Published API image, starts only after the migrator exits successfully |

The API is available at `http://localhost:8080`. Port mappings are configurable with `POSTGRES_PORT` and `API_PORT`.

Verification:

```bash
curl -fsS http://localhost:8080/health
curl -fsS http://localhost:8080/api/v1/marcas-autos
docker exec marcas-autos-db psql -U postgres -d marcas_autos -c 'SELECT * FROM "MarcasAutos";'
```

Because the API waits for `migrator` to complete, the schema and seed data exist before the first request.

## 11. Testing

```bash
dotnet test
```

- Unit tests: isolated InMemory database per test; success, empty and cancellation paths; controller delegation, empty payload and token forwarding; missing-connection-string fail-fast.
- Integration tests: `WebApplicationFactory` with a relational SQLite in-memory database; seeded response, empty database, health check and `ProblemDetails` for unknown routes.

## 12. Coverage

```bash
./scripts/coverage.sh
```

The script runs both test projects with `coverlet.runsettings` and writes an HTML report plus summary in `coverage/report`.

Current result on authored code: **100% line, 100% branch**. Exclusions are limited to generated code: EF Core migrations (`**/Migrations/*.cs`) and source-generated files (`**/*.generated.cs`).

## 13. Design decisions and trade-offs

| Decision | Rationale |
|----------|-----------|
| Three production projects | Proportional layering; no separate domain project for one entity |
| Repository port owned by Application | Keeps EF Core out of application and HTTP layers without a generic repository |
| `HasData` for seed | Deterministic, migration-owned reference data |
| InMemory for unit tests, SQLite for HTTP tests | Fast isolation plus relational behavior; PostgreSQL validated separately through migrations and Compose |
| Dedicated migrator service | One migration owner instead of racing multiple API replicas at startup |
| Local dev connection string in `appsettings.json` | Non-secret defaults for first run; environment variables override them |
| No CQRS/MediatR/generic repositories | Unnecessary for a single read endpoint |

## 14. Environment checks and limitations

- `scripts/preflight.sh` checks the SDK, Docker daemon and PostgreSQL with a 5-second timeout per dependency and logs to `logs/preflight.log`; operational incidents are recorded in `docs/tasks/incidents.md`.
- SQLite and InMemory do not reproduce every PostgreSQL behavior; migration execution and seed data are verified against PostgreSQL.
- The Compose stack runs a single API instance. Horizontal scaling would require a separate migration job, which the `migrator` service already models.
