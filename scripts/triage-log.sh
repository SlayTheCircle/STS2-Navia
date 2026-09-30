#!/usr/bin/env bash
# 人工实测后的日志分诊:从游戏 godot.log 里抽取与本 mod 相关的错误信号。
# 用法: ./scripts/triage-log.sh [日志路径](默认 Windows 侧标准位置)
set -euo pipefail
source "$(dirname "$0")/dev-env.sh"
LOG="${1:-${GAME_LOG:-}}"
[[ -n "$LOG" ]] || { echo "错误: 需要日志路径参数或 GAME_LOG 配置。" >&2; exit 1; }
if [ ! -f "$LOG" ]; then
    echo "日志不存在: $LOG(游戏启动过才有)"
    exit 0
fi
echo "== 异常/堆栈 =="
grep -n -A6 'Exception\|Unhandled' "$LOG" | head -60 || true
echo
echo "== 资源缺失/NOPE =="
grep -n -i 'nope\|missing.*texture\|resource.*not.*found\|failed to load' "$LOG" | grep -vi 'audio' | head -20 || true
echo
echo "== 本 mod 相关(错误级) =="
grep -n -i 'error' "$LOG" | grep -i 'navia\|ritsulib' | head -20 || true
echo
echo "== 统计 =="
echo "总行数 $(wc -l < "$LOG") / 异常 $(grep -c 'Exception' "$LOG" || true) / NOPE $(grep -ic 'nope' "$LOG" || true)"
echo "完——空段=该类无信号"
