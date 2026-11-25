#!/usr/bin/env bash
# -- Fail fast when an external dependency stops responding instead of stalling the workflow --
set -uo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
LOG_DIR="$ROOT_DIR/logs"
LOG_FILE="$LOG_DIR/preflight.log"
TIMEOUT_SECONDS="${PREFLIGHT_TIMEOUT:-5}"
POSTGRES_HOST="${POSTGRES_HOST:-localhost}"
POSTGRES_PORT="${POSTGRES_PORT:-5433}"
API_HEALTH_URL="${API_HEALTH_URL:-}"

STATUS=0

mkdir -p "$LOG_DIR"

log() {
  printf '%s %s\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)" "$*" >>"$LOG_FILE"
}

check() {
  local name="$1"
  shift

  if timeout "$TIMEOUT_SECONDS" "$@" >/dev/null 2>&1; then
    log "OK   $name"
  else
    log "FAIL $name (no response within ${TIMEOUT_SECONDS}s)"
    echo "preflight: $name is not responding"
    STATUS=1
  fi
}

check "dotnet-sdk" bash -c '"$HOME/.dotnet/dotnet" --version'
check "docker-daemon" docker info
check "postgres" pg_isready -h "$POSTGRES_HOST" -p "$POSTGRES_PORT"

if [ -n "$API_HEALTH_URL" ]; then
  check "api-health" curl -fsS --max-time "$TIMEOUT_SECONDS" "$API_HEALTH_URL"
fi

if [ "$STATUS" -ne 0 ]; then
  log "RESULT blocked"
  echo "preflight: BLOCKED (details in $LOG_FILE)"
  exit 1
fi

log "RESULT ok"
echo "preflight: OK"
