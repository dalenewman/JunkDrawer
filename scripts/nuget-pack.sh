#!/usr/bin/env bash
set -euo pipefail

root_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
output_dir="$root_dir/artifacts/nuget"
package_version=""
dry_run=0

while [[ $# -gt 0 ]]; do
  case "$1" in
    --output-dir) output_dir="$2"; shift 2 ;;
    --package-version) package_version="$2"; shift 2 ;;
    --dry-run) dry_run=1; shift ;;
    --help|-h)
      echo "Usage: $0 [--output-dir PATH] [--package-version VERSION] [--dry-run]"
      exit 0 ;;
    *) echo "Unknown option: $1" >&2; exit 2 ;;
  esac
done

projects=()
while IFS= read -r project; do
  [[ "$project" == *'/obj/'* ]] && continue
  [[ "$project" == *'/bin/'* ]] && continue
  if [[ "$(dotnet msbuild "$project" -nologo -getProperty:IsPackable)" == "true" ]]; then
    package_id="$(dotnet msbuild "$project" -nologo -getProperty:PackageId)"
    if [[ "$package_id" == JunkDrawer* ]]; then
      projects+=("$project")
    fi
  fi
done < <(find "$root_dir" -name '*.csproj' -type f | sort -r)

printf 'Packable projects:\n'
printf '  %s\n' "${projects[@]}"
[[ "$dry_run" == 1 ]] && exit 0
[[ ${#projects[@]} -gt 0 ]] || { echo "No packable JunkDrawer projects found" >&2; exit 1; }

mkdir -p "$output_dir"
for project in "${projects[@]}"; do
  args=(dotnet pack "$project" -c Release -o "$output_dir" -p:ContinuousIntegrationBuild=true)
  if [[ -n "$package_version" ]]; then
    args+=(-p:PackageVersion="$package_version" -p:Version="$package_version")
  fi
  "${args[@]}"
done
