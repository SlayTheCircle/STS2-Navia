#!/usr/bin/env bash
# 使用指定版本 Godot 导入本地素材并打包。
set -euo pipefail
source "$(dirname "$0")/dev-env.sh"
navia_require_godot
python3 "$NAVIA_ROOT/scripts/audit-assets.py"
export NAVIA_MOD_ROOT="$NAVIA_ROOT"
export NAVIA_MOD_PCK="${NAVIA_MOD_PCK:-$NAVIA_ROOT/mods-dist/STS2-Navia/STS2-Navia.pck}"
mkdir -p "$(dirname "$NAVIA_MOD_PCK")"
import_log="$(mktemp)"
trap 'rm -f "$import_log"' EXIT
if ! "$GODOT_EXE" --headless --path "$NAVIA_ROOT/assets" --import >"$import_log" 2>&1; then
    cat "$import_log" >&2
    echo '错误: Godot 素材导入失败。' >&2
    exit 1
fi
"$GODOT_EXE" --headless --path "$NAVIA_ROOT/assets" --script "$NAVIA_ROOT/tools/character_rig/generate.gd"
"$GODOT_EXE" --headless --script "$NAVIA_ROOT/tools/pack_mod.gd"
