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

wait_endpoint() {
  local url="$1"

  for _ in {1..30}; do
    if curl --fail --silent --show-error "$url" >/dev/null 2>&1; then
      return
    fi

    sleep 0.5
  done

  echo "Service did not become ready: $url" >&2
  exit 1
}

trap cleanup EXIT INT TERM

start_project "simulators/ParcelBox.Simulators.LockerController" "http://localhost:5101"
start_project "simulators/ParcelBox.Simulators.MessageGateway" "http://localhost:5102"
start_project "src/ParcelBox.Api" "http://localhost:5100"
start_project "src/ParcelBox.Web" "http://localhost:5200"

wait_endpoint "http://localhost:5101/health"
wait_endpoint "http://localhost:5102/health"
wait_endpoint "http://localhost:5100/health"
wait_endpoint "http://localhost:5200/"

echo
echo "ParcelBox is ready."
echo "ParcelBox Web:             http://localhost:5200"
echo "ParcelBox API:             http://localhost:5100"
echo "Locker Controller:         http://localhost:5101"
echo "Message Gateway:           http://localhost:5102"
echo "Environment:               Development"
echo "Press Ctrl+C to stop all processes."

wait
