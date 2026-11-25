# Deployment

Production topology:

```text
GitHub (main) → Render web service (Docker) → Neon PostgreSQL
```

Render builds the repository's `Dockerfile` (final `runtime` stage); it does not rely on a locally built image. Neon hosts PostgreSQL. The API is stateless.

## Environment variables

Set these in Render. Values are never committed.

| Name | Required | Notes |
|------|----------|-------|
| `ConnectionStrings__MarcasAutos` | Yes | Npgsql connection string. Use Neon's pooled connection string for the API. |
| `ASPNETCORE_ENVIRONMENT` | No | `Production` is the default when unset. |
| `PORT` | Auto | Render injects it; the app binds `http://0.0.0.0:$PORT`. Do not hardcode it. |

Local Compose also uses `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_PORT` and `API_PORT`, but these are development-only and are not needed in Render.

## Neon

- One dedicated Neon project per application; do not mix unrelated data in an existing project.
- Use the **direct** connection string for migrations and the **pooled** connection string for the running API.
- Keep SSL enabled (`sslmode=require`, the default in the Neon connection string).
- EF Core 9+ takes a session-level advisory lock while applying migrations; transaction-mode poolers can reject it, which is why migrations use the direct endpoint.

## Migrations

The API never migrates on startup. Apply migrations as an explicit release step before the new version receives traffic:

```bash
export ConnectionStrings__MarcasAutos="<direct Neon connection string>"
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

1. Create a Web Service from the GitHub repository.
2. Runtime: Docker. Branch: `main`. Dockerfile: `./Dockerfile`. Build context: repository root.
3. Plan: free. Region: the one closest to the Neon region.
4. Health check path: `/health/live`.
5. Add `ConnectionStrings__MarcasAutos` with the pooled Neon connection string.
6. Keep auto-deploy on `main`; merges to `main` are validated by build and tests before pushing.

Do not set `ASPNETCORE_URLS`: the application reads `PORT` and binds to it.

## Verification

```bash
curl -fsS https://<service>.onrender.com/health
curl -fsS https://<service>.onrender.com/api/v1/marcas-autos
curl -fsS https://<service>.onrender.com/openapi/v1.json
curl -fsS https://<service>.onrender.com/scalar
```

Expected: `Healthy`, four seeded brands (or the current data), a valid OpenAPI 3.1 document, and the Scalar HTML page. Confirm the deployed commit in Render matches the validated `main` commit.

## Rollback

- Redeploy the previous commit from Render; the API is stateless.
- Keep migrations additive. A rollback of application code does not roll back the schema; if a migration must be reverted, review it and apply a new corrective migration instead of deleting data.
- Health check failures block the rollout and keep the previous version serving.
