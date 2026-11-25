# Marcas Autos API

Read-only ASP.NET Core API that lists car brands stored in PostgreSQL. The endpoint contract is stable and versioned in the URL (`/api/v1`), the schema evolves through EF Core migrations, and the API ships as a container image.

| Resource | Local | Public |
|----------|-------|--------|
| Interactive reference (Scalar) | `http://localhost:8080/scalar` | https://marcas-autos-api.onrender.com/scalar |
| OpenAPI 3.1 document | `http://localhost:8080/openapi/v1.json` | https://marcas-autos-api.onrender.com/openapi/v1.json |
| API endpoint | `http://localhost:8080/api/v1/marcas-autos` | https://marcas-autos-api.onrender.com/api/v1/marcas-autos |
| Liveness / readiness | `/health/live` / `/health` | `/health/live` / `/health` |

Deployment details are in [docs/deployment.md](docs/deployment.md).

## Stack

- .NET 10, C# 14, ASP.NET Core Web API
- Entity Framework Core 10 with Npgsql (PostgreSQL 17 locally, Neon in production)
- Native OpenAPI (`Microsoft.AspNetCore.OpenApi`) and Scalar as the only visual UI
- xUnit, EF Core InMemory, SQLite in-memory, `WebApplicationFactory`, Coverlet
- Multi-stage Dockerfile and Docker Compose

## Architecture

```text
src/
  MarcasAutos.Api/             Controllers, OpenAPI, Scalar, error handling, health, composition
  MarcasAutos.Application/     Service, DTOs and repository contract
  MarcasAutos.Infrastructure/  EF Core, PostgreSQL, migrations and seed
tests/
  MarcasAutos.UnitTests/       Service, controller and DI tests
  MarcasAutos.IntegrationTests/ HTTP tests through WebApplicationFactory and SQLite
```

The controller delegates to the application service, which uses a repository contract implemented in Infrastructure. The controller never touches EF Core, and responses use DTOs instead of persistence entities. Dependencies point inward: Api → Application and Infrastructure, Infrastructure → Application.

Current scope: a single read-only endpoint. No authentication, paging, caching or write operations are part of this version.

## Requirements

- .NET 10 SDK
- Docker Engine with Compose v2 (optional, for the containerized setup)
- PostgreSQL 17 (optional, if you run the API outside Docker)

## Run with Docker

```bash
docker compose up --build -d
docker compose ps
```

Compose starts three services:

- `db`: PostgreSQL 17 with a persistent volume and a readiness check.
- `migrator`: applies EF Core migrations once the database is healthy, then exits.
- `api`: starts only after the migrator completes successfully.

The API listens on `http://localhost:8080`. PostgreSQL is published on `localhost:5432`; override it with `POSTGRES_PORT` in a local `.env` if that port is taken.

```bash
cp .env.example .env
docker compose up --build -d
curl -fsS http://localhost:8080/health
curl -fsS http://localhost:8080/api/v1/marcas-autos
```

Stop the stack without touching data, or remove the development volume:

```bash
docker compose down
docker compose down -v
```

## Run locally

```bash
export ConnectionStrings__MarcasAutos="Host=localhost;Port=5432;Database=marcas_autos;Username=postgres;Password=postgres"
dotnet restore
dotnet ef database update --project src/MarcasAutos.Infrastructure --startup-project src/MarcasAutos.Api
dotnet run --project src/MarcasAutos.Api
```

The development profile listens on `http://localhost:5130`. `appsettings.Development.json` contains the same local-only connection string; production must provide `ConnectionStrings__MarcasAutos` through the environment, otherwise startup fails fast.

## Configuration

| Variable | Required | Purpose |
|----------|----------|---------|
| `ConnectionStrings__MarcasAutos` | Yes | Npgsql connection string used by EF Core. |
| `ASPNETCORE_ENVIRONMENT` | No | `Production` in containers by default. |
| `PORT` | No | When set (Render), Kestrel binds `http://0.0.0.0:$PORT`. |
| `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_PORT` | Compose only | Local database settings, defaults in `.env.example`. |
| `API_PORT` | Compose only | Host port mapped to the API (default `8080`). |

Never commit real credentials. `.env` is ignored; `.env.example` only holds local defaults.

## API

```http
GET /api/v1/marcas-autos
```

No parameters. Returns `200 OK` with a JSON array ordered by name, or `200 OK` with `[]` when the table is empty. Unhandled failures return `500 Internal Server Error` as RFC 7807 `application/problem+json` without internal details.

```json
[
  { "id": 4, "nombre": "Chevrolet" },
  { "id": 2, "nombre": "Ford" },
  { "id": 3, "nombre": "Honda" },
  { "id": 1, "nombre": "Toyota" }
]
```

### API documentation

- Scalar UI: `/scalar` (interactive, reads the generated document).
- OpenAPI document: `/openapi/v1.json`.
- The document is generated from the actual controllers and DTOs; there is no parallel specification. It is exposed in every environment because the API contract is public.

### Health

- `/health/live`: liveness only, used by platform probes.
- `/health`: readiness, includes a PostgreSQL check.

## Database

The `MarcasAutos` table has an integer identity key and a required name of up to 100 characters, with a unique index on the name. The initial migration inserts four fixed rows: Toyota, Ford, Honda and Chevrolet.

Migrations are the only schema mechanism; `EnsureCreated()` is not used outside the SQLite test factory.

```bash
# Create a migration after changing the model
dotnet ef migrations add <MigrationName> \
  --project src/MarcasAutos.Infrastructure \
  --startup-project src/MarcasAutos.Api \
  --output-dir Persistence/Migrations

# Apply pending migrations
dotnet ef database update \
  --project src/MarcasAutos.Infrastructure \
  --startup-project src/MarcasAutos.Api
```

Migrations never run automatically on API startup. Locally, the one-shot `migrator` Compose service owns them; in production, the release process applies them before the new version receives traffic (see [Deployment](docs/deployment.md)).

## Tests and coverage

```bash
dotnet build -c Release
dotnet test -c Release
./scripts/coverage.sh
```

- Unit tests use a fresh EF Core InMemory database per test.
- Integration tests run the real HTTP pipeline on SQLite in memory.
- `scripts/coverage.sh` collects Coverlet coverage and renders an HTML report in `coverage/report`.

Coverlet is a quality signal, not a target: tests assert real behavior and contracts, and only generated EF migrations are excluded in `coverlet.runsettings`. The latest run reports 96.5% line coverage and 66.6% branch coverage.

## Deployment

Production runs on Render (Docker web service) with a Neon PostgreSQL database. High-level flow:

```text
GitHub main → Render (Dockerfile) → Neon PostgreSQL
```

- API: https://marcas-autos-api.onrender.com
- Scalar: https://marcas-autos-api.onrender.com/scalar
- OpenAPI: https://marcas-autos-api.onrender.com/openapi/v1.json

The service receives `ConnectionStrings__MarcasAutos` in Npgsql keyword/value format and exposes `/health/live` for platform probes. Secrets are configured in Render, never in the repository. Full steps, migration policy and rollback notes are in [docs/deployment.md](docs/deployment.md).

## Version

The current software version is `1.0.0` (tag `v1.0.0`). It is independent from the HTTP API version `/api/v1`. Release and branching rules are in [docs/versioning.md](docs/versioning.md).
