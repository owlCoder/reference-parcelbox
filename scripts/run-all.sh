#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"

export ASPNETCORE_ENVIRONMENT=Development

dotnet build ParcelBox.sln

pids=()

cleanup() {
  for pid in "${pids[@]:-}"; do
    kill "$pid" 2>/dev/null || true
  done

  wait 2>/dev/null || true
}

start_project() {
  local project_path="$1"
  local url="$2"

  (
    cd "$repo_root/$project_path"
    exec dotnet run --no-build --no-launch-profile --urls "$url"
  ) &

  pids+=("$!")
}

trap cleanup EXIT INT TERM

start_project "simulators/ParcelBox.Simulators.LockerController" "http://localhost:5101"
start_project "simulators/ParcelBox.Simulators.MessageGateway" "http://localhost:5102"
start_project "src/ParcelBox.Api" "http://localhost:5100"
start_project "src/ParcelBox.Web" "http://localhost:5000"

echo "ParcelBox Web:             http://localhost:5000"
echo "ParcelBox API:             http://localhost:5100"
echo "Locker Controller:         http://localhost:5101"
echo "Message Gateway:           http://localhost:5102"
echo "Environment:               Development"
echo "Press Ctrl+C to stop all processes."

wait
