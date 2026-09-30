#!/usr/bin/env bash
# 把 mods-dist/STS2-Navia 部署到本机 Steam 安装的 mods/ 目录(WSL 视角)。
# 用法: ./scripts/deploy.sh [游戏安装目录]
set -euo pipefail

source "$(dirname "$0")/dev-env.sh"
GAME_DIR="${1:-${GAME_DIR:-}}"
[[ -n "$GAME_DIR" ]] || { echo "错误: 需要游戏目录参数或 GAME_DIR 配置。" >&2; exit 1; }
SRC="$(cd "$(dirname "$0")/.." && pwd)/mods-dist/STS2-Navia"

if [ ! -f "$SRC/STS2-Navia.dll" ] || [ ! -f "$SRC/STS2-Navia.pck" ] || [ ! -f "$SRC/STS2-Navia.json" ]; then
    echo "错误: $SRC 下没有构建产物,先运行 scripts/build.sh" >&2
    exit 1
fi
if [ ! -d "$GAME_DIR/data_sts2_windows_x86_64" ]; then
    echo "错误: 游戏目录不存在: $GAME_DIR" >&2
    exit 1
fi

# 防呆:游戏运行中禁止部署。pck 不上锁会被就地覆写,运行中的游戏懒加载读到截断包
# → 卡图消失/闪退(两次实测事故)。tasklist 不可用时也拒绝部署(失败即关闭,不静默放行)。
if ! tasklist_out=$("$TASKLIST_EXE" 2>&1); then
    echo "错误: 无法调用 tasklist.exe 检测游戏进程,拒绝部署(可先手动确认游戏已关闭)。" >&2
    exit 1
fi
if printf '%s' "$tasklist_out" | grep -qiE 'spire|sts2'; then
    echo "错误: 检测到游戏正在运行。运行中覆写 pck 会导致卡图丢失与闪退,请先关闭游戏再部署。" >&2
    exit 1
fi

DEST="$GAME_DIR/mods/STS2-Navia"
mkdir -p "$DEST"
# 文件被占用 = 游戏可能仍在运行(或杀软扫描)——直接中止,绝不就地覆写 pck。
for f in "$SRC"/*; do
    base="$(basename "$f")"
    if ! cp "$f" "$DEST/$base" 2>/dev/null; then
        echo "错误: 覆盖 $base 失败(文件被占用)。请确认游戏已完全关闭后重试;本脚本不再做改名让路(会损坏运行中游戏的包视图)。" >&2
        exit 1
    fi
done
rm -f "$DEST"/*.old 2>/dev/null || true

# 部署物防呆:入口必须在
if [ "$(strings -n 6 "$DEST/STS2-Navia.dll" | grep -c 'ModInitializer')" -eq 0 ]; then
    echo "错误: 部署物 dll 缺少 ModInitializer 入口!" >&2
    exit 1
fi

echo "已部署到: $DEST"
echo "注意: 还需在 mods/ 下安装 RitsuLib(workshop 订阅 id 3747602295,或手动复制)。"
