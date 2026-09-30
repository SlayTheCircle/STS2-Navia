#!/usr/bin/env bash
# 完整构建，或 --dll-only 编译源码；DLL-only 不生成可安装的模组目录。
set -euo pipefail
source "$(dirname "$0")/dev-env.sh"
mode="${1:-full}"
if [[ "$#" -gt 1 || ( "$mode" != full && "$mode" != --dll-only ) ]]; then
    echo '用法: scripts/build.sh [--dll-only]' >&2
    exit 2
fi
navia_require_refs
if [[ "$mode" == full ]]; then
    navia_require_godot
    python3 "$NAVIA_ROOT/scripts/audit-assets.py"
fi
python3 "$NAVIA_ROOT/scripts/audit-placeholders.py"
python3 "$NAVIA_ROOT/scripts/audit-loc-coverage.py"
navia_dotnet build "$NAVIA_ROOT/src/STS2-Navia/STS2-Navia.csproj" -c Release
DLL="$NAVIA_ROOT/src/STS2-Navia/bin/Release/net9.0/STS2-Navia.dll"
if [[ "$(strings -n 6 "$DLL" | grep -c 'ModInitializer' || true)" -eq 0 ]]; then
    echo '错误: DLL 缺少 ModInitializer 入口。' >&2
    exit 1
fi
if [[ "$mode" == --dll-only ]]; then
    echo 'DLL 编译完成；未打包 PCK，未组装安装目录。'
    exit 0
fi
mkdir -p "$NAVIA_ROOT/mods-dist"
stage="$(mktemp -d "$NAVIA_ROOT/mods-dist/.build.XXXXXX")"
trap 'rm -rf "$stage"' EXIT
cp "$NAVIA_ROOT/STS2-Navia.json" "$stage/"
cp "$DLL" "$stage/"
NAVIA_MOD_PCK="$stage/STS2-Navia.pck" "$NAVIA_ROOT/scripts/build-pck.sh"
NAVIA_MOD_PCK="$stage/STS2-Navia.pck" "$NAVIA_ROOT/scripts/verify-pck.sh"
OUT="$NAVIA_ROOT/mods-dist/STS2-Navia"
mkdir -p "$OUT"
cp "$stage/STS2-Navia.json" "$stage/STS2-Navia.dll" "$stage/STS2-Navia.pck" "$OUT/"
echo '完整构建与 PCK 纹理验证完成：'
ls -l "$OUT"
