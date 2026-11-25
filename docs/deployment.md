# Deployment

Production topology:

```text
GitHub (main) → Render web service (Docker) → Neon PostgreSQL
```

Render builds the repository's `Dockerfile` (final `runtime` stage); it does not rely on a locally built image. Neon hosts PostgreSQL. The API is stateless.

## Current production resources

| Resource | Value |
|----------|-------|
| Render service | `marcas-autos-api`, Docker, branch `main`, region Ohio, free plan |
| Public URL | https://marcas-autos-api.onrender.com |
| Neon project | `marcas-autos`, AWS `us-east-2` (Ohio), PostgreSQL 17.12 |
| Neon database | `marcas_autos` |
| Schema | EF Core migration `20261125175125_InitialCreate` applied; 4 seed rows |

## Environment variables

Set these in Render. Values are never committed.

| Name | Required | Notes |
|------|----------|-------|
| `ConnectionStrings__MarcasAutos` | Yes | Npgsql keyword/value connection string (see format below). Use the pooled Neon host. |
| `ASPNETCORE_ENVIRONMENT` | No | `Production` is the default when unset. |
| `PORT` | Auto | Render injects it; the app binds `http://0.0.0.0:$PORT`. Do not hardcode it. |

Local Compose also uses `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_PORT` and `API_PORT`, but these are development-only and are not needed in Render.

### Connection string format

Npgsql does not accept the `postgresql://user:password@host/db?...` URI that Neon shows in its console. Convert it to keyword/value form:

```text
Host=<pooled-host>;Port=5432;Database=marcas_autos;Username=<role>;Password=<password>;SSL Mode=Require;Channel Binding=Require
```

The pooled host contains `-pooler` (for example `ep-...-pooler.c-8.us-east-2.aws.neon.tech`). Keep the password only in Render and in the local shell that runs migrations; never commit it.

## Neon

- One dedicated Neon project per application; unrelated projects are not reused.
- Use the pooled connection string for the running API.
- Use the **direct** (non-pooled) host when applying migrations: EF Core 9+ takes a session-level advisory lock, and transaction-mode poolers can reject it.
- SSL stays required (`SSL Mode=Require`).

## Migrations

The API never migrates on startup. Apply migrations as an explicit release step before the new version receives traffic:

```bash
export ConnectionStrings__MarcasAutos="<direct Neon connection string in keyword/value format>"
dotnet ef database update \
  --project src/MarcasAutos.Infrastructure \
  --startup-project src/MarcasAutos.Api
```

The command is idempotent: it only applies migrations missing from `__EFMigrationsHistory`. Review the SQL first when a migration changes existing data:

```bash
dotnet ef migrations script --idempotent \
  --project src/MarcasAutos.Infrastructure \
  --startup-project src/MarcasAutos.Api
```

Render's pre-deploy command requires a paid plan. On the free plan, run the command above from a trusted workstation as part of the release checklist.

## Render

1. Web Service from the GitHub repository, runtime Docker.
2. Branch `main`, Dockerfile `./Dockerfile`, build context repository root.
3. Free plan, region Ohio (same region as Neon).
4. Health check path: `/health/live`. Set it in the service's Settings page; the MCP-created service starts with this field empty.
5. `ASPNETCORE_ENVIRONMENT` is not needed; `PORT` is injected automatically.
6. Add `ConnectionStrings__MarcasAutos` as a secret environment variable.

Do not set `ASPNETCORE_URLS`: the application reads `PORT` and binds to it.

### Auto-deploy

The service is created with auto-deploy enabled, but automatic deployments on push require the Render GitHub integration to be authorized for the repository. After the initial setup, confirm that a push to `main` starts a deploy. If it does not, either authorize the Render GitHub App for the repository or trigger deploys manually after the CI workflow succeeds.

## Verification

```bash
curl -fsS https://marcas-autos-api.onrender.com/health
curl -fsS https://marcas-autos-api.onrender.com/api/v1/marcas-autos
curl -fsS https://marcas-autos-api.onrender.com/openapi/v1.json
curl -fsS https://marcas-autos-api.onrender.com/scalar
```

Expected: `Healthy`, the four seeded brands (or the current data), a valid OpenAPI 3.1 document, and the Scalar HTML page. Confirm the deployed commit in Render matches the validated `main` commit.

## Rollback

- Redeploy the previous commit from Render; the API is stateless.
- Keep migrations additive. A rollback of application code does not roll back the schema; if a migration must be reverted, review it and apply a new corrective migration instead of deleting data.
- Health check failures block the rollout and keep the previous version serving.
