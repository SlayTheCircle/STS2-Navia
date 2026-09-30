#!/usr/bin/env bash
# 默认执行可公开重现的源码检查；--full 要求完整本地环境、素材与 PCK。
set -euo pipefail
source "$(dirname "$0")/dev-env.sh"
mode="${1:---source-only}"
if [[ "$#" -gt 1 || ( "$mode" != --source-only && "$mode" != --full ) ]]; then
    echo '用法: scripts/check.sh [--source-only|--full]' >&2
    exit 2
fi
python3 "$NAVIA_ROOT/scripts/audit-repository.py"
python3 "$NAVIA_ROOT/scripts/audit-docs.py"
python3 "$NAVIA_ROOT/scripts/audit-placeholders.py"
python3 "$NAVIA_ROOT/scripts/audit-loc-coverage.py"
python3 "$NAVIA_ROOT/scripts/audit-roster.py"
python3 "$NAVIA_ROOT/scripts/export-card-table.py" --check
python3 "$NAVIA_ROOT/scripts/audit-assets.py" --source-only
if [[ "$mode" == --source-only ]]; then
    echo '源码检查完成；未执行编译、完整素材、PCK 或游戏运行时验收。'
    exit 0
fi
"$NAVIA_ROOT/scripts/doctor.sh"
"$NAVIA_ROOT/scripts/build.sh"
echo '本地完整构建检查完成；游戏运行时验收由实际试玩确认。'
