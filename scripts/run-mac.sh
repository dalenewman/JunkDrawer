#!/usr/bin/env bash
set -euo pipefail

root_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root_dir"
dotnet build JunkDrawer.Eto.Mac64/JunkDrawer.Eto.Mac64.csproj --nologo -v quiet
exec JunkDrawer.Eto.Mac64/bin/Debug/net10.0/JunkDrawer.Eto.Mac64.app/Contents/MacOS/jdgui "$@"
