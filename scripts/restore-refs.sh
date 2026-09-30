#!/usr/bin/env bash
# 从已安装游戏复制编译引用；与 RITSULIB_TARGET 版本配对。
set -euo pipefail
source "$(dirname "$0")/dev-env.sh"
GAME_DIR="${1:-${GAME_DIR:-}}"
[[ -n "$GAME_DIR" ]] || { echo '错误: 需要游戏目录参数或 GAME_DIR 本机配置。' >&2; exit 1; }
DATA_DIR=''
for candidate in data_sts2_windows_x86_64 data_sts2_linux_x86_64 data_sts2_macos_arm64 data_sts2_macos_x86_64; do
    if [[ -f "$GAME_DIR/$candidate/sts2.dll" ]]; then DATA_DIR="$GAME_DIR/$candidate"; break; fi
done
[[ -n "$DATA_DIR" ]] || { echo '错误: 所选游戏目录未找到 sts2.dll。' >&2; exit 1; }
for name in sts2.dll GodotSharp.dll 0Harmony.dll; do
    [[ -f "$DATA_DIR/$name" ]] || { echo "错误: 游戏安装缺少 $name。" >&2; exit 1; }
done
[[ -f "$GAME_DIR/release_info.json" ]] || { echo '错误: 游戏安装缺 release_info.json。' >&2; exit 1; }
version="$(python3 -c 'import json,sys; print(json.load(open(sys.argv[1]))["version"].removeprefix("v"))' "$GAME_DIR/release_info.json")"
[[ "$version" == "$RITSULIB_TARGET" ]] || { echo "错误: 游戏版本 $version 与 RITSULIB_TARGET=$RITSULIB_TARGET 不一致；未复制引用。" >&2; exit 1; }
mkdir -p "$GAME_REFS_DIR"
for name in sts2.dll GodotSharp.dll 0Harmony.dll; do cp "$DATA_DIR/$name" "$GAME_REFS_DIR/"; done
cp "$GAME_DIR/release_info.json" "$GAME_REFS_DIR/"
echo "编译引用已复制，版本 $version。"
