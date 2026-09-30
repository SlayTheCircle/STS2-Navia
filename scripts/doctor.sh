#!/usr/bin/env bash
# 当前工具与版本配对预检；完整美术由 audit-assets.py 严格检查。
set -euo pipefail
source "$(dirname "$0")/dev-env.sh"
sdk_version="$(navia_dotnet --version)"
echo "SDK: $sdk_version"
navia_require_refs
echo "游戏引用 / RitsuLib compat: $RITSULIB_TARGET"
navia_require_godot
echo "Godot: $NAVIA_GODOT_VERSION stable"
python3 --version
echo '开发环境预检通过；尚未验证编译、PCK 或游戏运行时。'
