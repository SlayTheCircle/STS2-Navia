#!/usr/bin/env bash
# 验证真实产物的纹理与本地化可解析；不能替代游戏内模型、场景和机制验收。
set -euo pipefail
source "$(dirname "$0")/dev-env.sh"
navia_require_godot
export NAVIA_MOD_PCK="${NAVIA_MOD_PCK:-$NAVIA_ROOT/mods-dist/STS2-Navia/STS2-Navia.pck}"
[[ -f "$NAVIA_MOD_PCK" ]] || { echo '错误: PCK 不存在；先完整构建。' >&2; exit 1; }
"$GODOT_EXE" --headless --script "$NAVIA_ROOT/tools/verify_pck.gd"

"$GODOT_EXE" --headless --script "$NAVIA_ROOT/tools/character_rig/validation/scene.gd"
