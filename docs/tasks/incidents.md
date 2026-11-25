# Incidents and service health protocol

## 2026-11-25 — Harness restart took down Docker and PostgreSQL

**Symptom:** `docker run` for the verification database was interrupted; afterwards `docker info` and `pg_isready` failed with "no response".

**Cause:** the agent harness restarted mid-command, and the WSL session restarted with it, stopping `dockerd` and the PostgreSQL cluster. Not a code or migration failure.

**Impact:** Phase 3 database verification was paused. No repository damage: build stayed green and commits were intact.

**Resolution:** Docker was started again by the operator, then the PostgreSQL check container was recreated and the migration/seed verification completed.

**Prevention:**

- `scripts/preflight.sh` checks .NET SDK, Docker daemon and PostgreSQL with a 5-second timeout each and writes to `logs/preflight.log`.
- Run the preflight before every phase gate and after any environment restart.
- Any dependency with no response in 5 seconds is treated as blocked: stop, report, and ask before continuing.
- Long commands are always run with an explicit timeout; a command that exceeds it is reported, never silently retried.

## Service checks

| Service | Check | Expected |
|---------|-------|----------|
| .NET SDK | `"$HOME/.dotnet/dotnet" --version` | `10.0.404` |
| Docker | `docker info` | server version present |
| PostgreSQL | `pg_isready -h localhost -p 5433` | `accepting connections` |
| API | `curl -fsS localhost:8080/health` | `Healthy` |

Port 5433 is used for the local verification container because the host cluster owns 5432.
