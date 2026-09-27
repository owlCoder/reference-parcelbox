#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"

dotnet build ParcelBox.sln

pids=()

cleanup() {
  for pid in "${pids[@]:-}"; do
    kill "$pid" 2>/dev/null || true
  done

  wait 2>/dev/null || true
}

trap cleanup EXIT INT TERM

dotnet run --project simulators/ParcelBox.Simulators.LockerController --no-build --no-launch-profile --urls http://localhost:5101 &
pids+=("$!")

dotnet run --project simulators/ParcelBox.Simulators.MessageGateway --no-build --no-launch-profile --urls http://localhost:5102 &
pids+=("$!")

dotnet run --project src/ParcelBox.Api --no-build --no-launch-profile --urls http://localhost:5100 &
pids+=("$!")

dotnet run --project src/ParcelBox.Web --no-build --no-launch-profile --urls http://localhost:5000 &
pids+=("$!")

echo "ParcelBox Web:             http://localhost:5000"
echo "ParcelBox API:             http://localhost:5100"
echo "Locker Controller:         http://localhost:5101"
echo "Message Gateway:           http://localhost:5102"
echo "Press Ctrl+C to stop all processes."

wait
