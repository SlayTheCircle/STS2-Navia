#!/usr/bin/env bash
# 构建工坊变体引导壳:固定对 0.107.1(两版 API 下限)编译,单份产物在两个游戏目标上运行。
# 引用目录用专用变量注入(csproj 读 NAVIA_LOADER_GAME_REFS),不受 GAME_REFS_DIR 干扰;产物收纳到 mods-dist/loader/。
set -euo pipefail
source "$(dirname "$0")/dev-env.sh"
LOADER_REFS="${NAVIA_LOADER_GAME_REFS:-$NAVIA_ROOT/libs/game-0.107.1}"
[[ -f "$LOADER_REFS/sts2.dll" ]] || { echo "错误: 缺 0.107.1 编译引用:$LOADER_REFS" >&2; exit 1; }
export NAVIA_LOADER_GAME_REFS="$LOADER_REFS"
navia_dotnet build "$NAVIA_ROOT/src/STS2-Navia.Loader/STS2-Navia.Loader.csproj" -c Release
BUILT="$NAVIA_ROOT/src/STS2-Navia.Loader/bin/Release/net9.0/STS2-Navia.Loader.dll"
if [[ "$(strings -n 6 "$BUILT" | grep -c 'ModInitializer' || true)" -eq 0 ]]; then
    echo '错误: 引导壳 dll 缺少 ModInitializer 入口。' >&2
    exit 1
fi
mkdir -p "$NAVIA_ROOT/mods-dist/loader"
# 程序集身份是 STS2-Navia.Loader,文件名落位为模组 id 同名的 STS2-Navia.dll。
cp "$BUILT" "$NAVIA_ROOT/mods-dist/loader/STS2-Navia.dll"
echo "引导壳就绪: mods-dist/loader/STS2-Navia.dll (程序集 STS2-Navia.Loader)"
