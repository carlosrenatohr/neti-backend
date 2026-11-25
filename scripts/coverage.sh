#!/usr/bin/env bash
# -- Collect coverage and render a readable report; generated code is excluded in coverlet.runsettings --
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
export PATH="$DOTNET_ROOT:$HOME/.dotnet/tools:$PATH"

rm -rf coverage

dotnet test MarcasAutos.sln \
  --collect:"XPlat Code Coverage" \
  --settings coverlet.runsettings \
  --results-directory coverage

if ! command -v reportgenerator >/dev/null 2>&1; then
  echo "reportgenerator is not installed. Run: dotnet tool install --global dotnet-reportgenerator-globaltool"
  exit 1
fi

reportgenerator \
  -reports:"coverage/**/coverage.cobertura.xml" \
  -targetdir:coverage/report \
  -reporttypes:"Html;TextSummary" \
  -verbosity:Warning

cat coverage/report/Summary.txt
