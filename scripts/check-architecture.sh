#!/usr/bin/env bash
set -euo pipefail

status=0

report() {
  echo "ARCHITECTURE ERROR: $1" >&2
  status=1
}

while IFS= read -r file; do
  if grep -Eq '^[[:space:]]*(public|internal)[[:space:]]+(sealed[[:space:]]+)?(class|record|enum|struct)[[:space:]]+' "$file"; then
    report "$file is under Application/Interfaces but declares a non-interface type."
  fi

  if ! grep -Eq '^[[:space:]]*(public|internal)[[:space:]]+interface[[:space:]]+I[A-Za-z0-9_]+' "$file"; then
    report "$file is under Application/Interfaces but does not declare an interface."
  fi
done < <(find src/ParcelBox.Application/Interfaces -type f -name '*.cs' -print | sort)

while IFS= read -r file; do
  if ! grep -Eq '^[[:space:]]*(public|internal)[[:space:]]+enum[[:space:]]+[A-Za-z0-9_]+' "$file"; then
    report "$file is under Application/Enums but does not declare an enum."
  fi
done < <(find src/ParcelBox.Application/Enums -type f -name '*.cs' -print | sort)

if grep -R -nE '^[[:space:]]*(public|internal)[[:space:]]+(interface|enum)[[:space:]]+' \
  src/ParcelBox.Application/DTOs --include='*.cs'; then
  report "Application/DTOs contains an interface or enum."
fi

unexpected_repository_files=$(find src/ParcelBox.Application/Interfaces/Repositories \
  -type f -name '*.cs' ! -name 'I*Repository.cs' -print)
if [[ -n "$unexpected_repository_files" ]]; then
  echo "$unexpected_repository_files" >&2
  report "Interfaces/Repositories may contain repository interfaces only."
fi

while IFS= read -r file; do
  if ! grep -Eq '^namespace ParcelBox\.Domain\.[A-Za-z0-9_]+\.Models;' "$file"; then
    report "$file is under Domain/*/Models but its namespace does not end in .Models."
  fi
done < <(find src/ParcelBox.Domain -path '*/Models/*.cs' -type f -print | sort)

while IFS= read -r file; do
  if ! grep -Eq '^namespace ParcelBox\.Domain\.[A-Za-z0-9_]+\.Enums;' "$file"; then
    report "$file is under Domain/*/Enums but its namespace does not end in .Enums."
  fi

  if ! grep -Eq '^[[:space:]]*public[[:space:]]+enum[[:space:]]+[A-Za-z0-9_]+' "$file"; then
    report "$file is under Domain/*/Enums but does not declare an enum."
  fi
done < <(find src/ParcelBox.Domain -path '*/Enums/*.cs' -type f -print | sort)

while IFS= read -r file; do
  if ! grep -Eq '^namespace ParcelBox\.Infrastructure\.Persistence\.Repositories;' "$file"; then
    report "$file is under Infrastructure/Persistence/Repositories but has a mismatched namespace."
  fi
done < <(find src/ParcelBox.Infrastructure/Persistence/Repositories -type f -name '*.cs' -print | sort)

while IFS= read -r file; do
  folder=$(basename "$(dirname "$file")")

  if [[ "$folder" == "Contracts" || "$folder" == "Enums" || "$folder" == "Models" ]]; then
    if ! grep -Eq "^namespace ParcelBox\\.Simulators\\.[A-Za-z0-9_]+\\.${folder};" "$file"; then
      report "$file has a namespace that does not match its simulator folder."
    fi
  fi
done < <(find simulators -type f -name '*.cs' -print | sort)

if grep -R -nE 'Microsoft\.EntityFrameworkCore|ParcelBox\.Infrastructure' \
  src/ParcelBox.Application src/ParcelBox.Domain --include='*.cs'; then
  report "Application or Domain depends on Infrastructure/EF Core."
fi

if grep -R -nE 'Application\.Interfaces\.(Repositories|Persistence)|ParcelBoxDbContext|Infrastructure\.Persistence' \
  src/ParcelBox.Api/Endpoints --include='*.cs'; then
  report "API endpoint depends directly on persistence instead of a service interface."
fi

if [[ "$status" -ne 0 ]]; then
  exit "$status"
fi

echo "Architecture boundaries OK"
