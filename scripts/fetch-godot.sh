#!/usr/bin/env bash
# 下载固定版本 Linux x86_64 标准版；其他平台通过 GODOT_EXE 指定官方安装。
set -euo pipefail
source "$(dirname "$0")/dev-env.sh"
if command -v "$GODOT_EXE" >/dev/null 2>&1; then
    navia_require_godot
    echo '指定的 Godot 已可用。'
    exit 0
fi
if [[ "$(uname -s)" != Linux || "$(uname -m)" != x86_64 ]]; then
    echo '错误: 自动下载只支持 Linux x86_64；请配置 GODOT_EXE。' >&2
    exit 1
fi
DEST="$NAVIA_ROOT/.tools/godot/$NAVIA_GODOT_VERSION"
mkdir -p "$DEST"
archive="$(mktemp "$DEST/.download.XXXXXX.zip")"
trap 'rm -f "$archive"' EXIT
curl --fail --location --show-error --silent -o "$archive" \
    "https://github.com/godotengine/godot/releases/download/$NAVIA_GODOT_VERSION-stable/Godot_v$NAVIA_GODOT_VERSION-stable_linux.x86_64.zip"
unzip -o -q "$archive" -d "$DEST"
GODOT_EXE="$DEST/Godot_v$NAVIA_GODOT_VERSION-stable_linux.x86_64"
chmod +x "$GODOT_EXE"
navia_require_godot
echo "Godot 已就绪: $GODOT_EXE"
echo '使用共享安装时，将可执行文件位置写入本机配置。'
