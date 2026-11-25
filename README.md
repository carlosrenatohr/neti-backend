# Marcas Autos API

Small ASP.NET Core API for listing car brands from PostgreSQL. The project uses EF Core migrations for the schema and seed data, and Docker Compose to run the database and API together.

## Run it with Docker

Requirements: Docker Engine and Docker Compose v2.

```bash
docker compose up --build -d
docker compose ps
```

Compose starts three services:

- `db`: PostgreSQL 17, with a persistent volume and a readiness check.
- `migrator`: applies EF Core migrations after PostgreSQL is ready, then exits.
- `api`: starts after migrations finish successfully.

The API is available at `http://localhost:8080`. PostgreSQL is published on `localhost:5432` by default. If that port is already in use, set `POSTGRES_PORT=5433` in a local `.env` file. Start with the example if you need custom settings:

```bash
cp .env.example .env
```

### Smoke test

These checks confirm the containers are running, the API can query PostgreSQL, and the migration inserted the sample brands:

```bash
# Both db and api should be running; db should be healthy.
docker compose ps

# The API health check should say Healthy.
curl -fsS http://localhost:8080/health

# Expect four brands: Chevrolet, Ford, Honda and Toyota.
curl -fsS http://localhost:8080/api/v1/marcas-autos

# Check the same records directly in PostgreSQL.
docker compose exec db psql -U postgres -d marcas_autos \
  -c 'SELECT "Id", "Nombre" FROM "MarcasAutos" ORDER BY "Id";'
```

If something fails, inspect the service logs:

```bash
docker compose logs --tail=100 db migrator api
```

Stop the services but keep the database volume:

```bash
docker compose down
```

To remove the database volume and start from a clean database next time:

```bash
docker compose down -v
```

## Run locally

Requirements: .NET 10 SDK and PostgreSQL. The default local connection string is in `src/MarcasAutos.Api/appsettings.json`; override it with an environment variable when needed:

```bash
export ConnectionStrings__MarcasAutos="Host=localhost;Port=5432;Database=marcas_autos;Username=postgres;Password=postgres"
dotnet restore
dotnet ef database update --project src/MarcasAutos.Infrastructure --startup-project src/MarcasAutos.Api
dotnet run --project src/MarcasAutos.Api
```

The development launch profile listens at `http://localhost:5130`.

## API

```http
GET /api/v1/marcas-autos
```

Returns `200 OK` with a JSON array. If there are no records, the response is `200 OK` with `[]`. The endpoint is documented at `/openapi/v1.json` in Development. `GET /health` checks API and database availability.

Example response:

```json
[
  { "id": 4, "nombre": "Chevrolet" },
  { "id": 2, "nombre": "Ford" },
  { "id": 3, "nombre": "Honda" },
  { "id": 1, "nombre": "Toyota" }
]
```

Unhandled errors use `ProblemDetails` responses.

## Project layout

```text
src/
  MarcasAutos.Api/             Controllers, HTTP error handling, health and OpenAPI
  MarcasAutos.Application/     Service, DTOs and repository contract
  MarcasAutos.Infrastructure/  EF Core, PostgreSQL and migrations
tests/
  MarcasAutos.UnitTests/       Service and controller tests
  MarcasAutos.IntegrationTests/ HTTP tests using WebApplicationFactory and SQLite
```

Requests flow from the controller to the application service, then through the repository contract to the EF Core implementation. The controller does not use EF Core directly, and responses use DTOs rather than persistence entities.

## Database and seed data

The `MarcasAutos` table has an integer identity key and a required name up to 100 characters. Brand names have a unique index. The initial EF Core migration inserts four fixed examples: Toyota, Ford, Honda and Chevrolet.

To create a migration after changing the model:

```bash
dotnet ef migrations add <MigrationName> \
  --project src/MarcasAutos.Infrastructure \
  --startup-project src/MarcasAutos.Api \
  --output-dir Persistence/Migrations
```

Apply it with `dotnet ef database update` as shown above. The application uses migrations rather than `EnsureCreated`.

## Tests and coverage

```bash
dotnet test
./scripts/coverage.sh
```

Unit tests use a fresh EF Core InMemory database per test. HTTP integration tests use SQLite in memory so they exercise a relational database through the real API host. The Compose smoke test separately verifies PostgreSQL migrations and seed data.

Coverage reports are written to `coverage/report`. The current report is 100% line and branch coverage for authored code; only generated EF migration and source-generated files are excluded in `coverlet.runsettings`.

## Configuration and design notes

- Compose uses `db` as the database hostname inside the network; containers should not connect to `localhost` for PostgreSQL.
- Migrations run in a one-shot service before the API starts, rather than racing during API startup.
- `.env` is ignored by Git. `.env.example` contains local development defaults only; do not put real credentials in committed files.
- Unit-test InMemory and SQLite do not reproduce every PostgreSQL behavior, so the Compose smoke test checks the real provider as well.
- `scripts/preflight.sh` checks local dependencies and stops after five seconds if one does not respond. It records results in `logs/preflight.log`.
