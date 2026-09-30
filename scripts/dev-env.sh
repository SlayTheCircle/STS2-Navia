#!/usr/bin/env bash
# 本机配置、工具选择与编译引用预检。供其他开发脚本 source。
NAVIA_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
NAVIA_GODOT_VERSION="4.5.1"

# 保留调用者显式设置的值，本地可信配置只提供后备值。
navia_env_names=()
navia_env_values=()
for navia_name in GAME_DIR GAME_LOG GAME_REFS_DIR RITSULIB_DIR RITSULIB_TARGET DOTNET_EXE GODOT_EXE ART_SOURCE_DIR ROSTER_DESIGN_FILE TASKLIST_EXE; do
    if [[ -v "$navia_name" ]]; then
        navia_env_names+=("$navia_name")
        navia_env_values+=("${!navia_name}")
    fi
done
if [[ -f "$NAVIA_ROOT/.local-dev.env" ]]; then
    source "$NAVIA_ROOT/.local-dev.env"
fi
for navia_i in "${!navia_env_names[@]}"; do
    printf -v "${navia_env_names[$navia_i]}" '%s' "${navia_env_values[$navia_i]}"
done
unset navia_env_names navia_env_values navia_name navia_i

DOTNET_EXE="${DOTNET_EXE:-dotnet}"
GODOT_EXE="${GODOT_EXE:-$NAVIA_ROOT/.tools/godot/$NAVIA_GODOT_VERSION/Godot_v${NAVIA_GODOT_VERSION}-stable_linux.x86_64}"
GAME_REFS_DIR="${GAME_REFS_DIR:-$NAVIA_ROOT/libs/game}"
RITSULIB_DIR="${RITSULIB_DIR:-$NAVIA_ROOT/libs/RitsuLib}"
RITSULIB_TARGET="${RITSULIB_TARGET:-0.111.0}"
TASKLIST_EXE="${TASKLIST_EXE:-tasklist.exe}"
ROSTER_DESIGN_FILE="${ROSTER_DESIGN_FILE:-$NAVIA_ROOT/docs/history/design/card-roster.txt}"
export GAME_REFS_DIR RITSULIB_DIR RITSULIB_TARGET
export ROSTER_DESIGN_FILE

navia_dotnet() {
    (cd "$NAVIA_ROOT" && "$DOTNET_EXE" "$@")
}

navia_require_refs() {
    local navia_file navia_version
    for navia_file in sts2.dll GodotSharp.dll 0Harmony.dll release_info.json; do
        [[ -f "$GAME_REFS_DIR/$navia_file" ]] || {
            echo "错误: 编译引用不完整。配置 GAME_DIR 并运行 scripts/restore-refs.sh。" >&2
            return 1
        }
    done
    navia_version="$(python3 -c 'import json,sys; print(json.load(open(sys.argv[1]))["version"].removeprefix("v"))' "$GAME_REFS_DIR/release_info.json")" || return 1
    [[ "$navia_version" == "$RITSULIB_TARGET" ]] || {
        echo "错误: 游戏引用版本 $navia_version 与 RITSULIB_TARGET=$RITSULIB_TARGET 不一致。" >&2
        return 1
    }
    [[ -f "$RITSULIB_DIR/RitsuLib.References.props" ]] || {
        echo "错误: 缺 RitsuLib.References.props；请配置 RITSULIB_DIR 指向完整依赖包。" >&2
        return 1
    }
    for navia_file in "compat/$RITSULIB_TARGET/STS2-RitsuLib.dll" "compat/$RITSULIB_TARGET/STS2-RitsuLib.Runtime.dll" shared/STS2-RitsuLib.Ui.dll shared/STS2-RitsuLib.Shared.dll shared/STS2-RitsuLib.Settings.dll; do
        [[ -f "$RITSULIB_DIR/$navia_file" ]] || {
            echo "错误: RitsuLib 依赖包缺少 $navia_file。" >&2
            return 1
        }
    done
}

navia_require_godot() {
    local navia_version
    navia_version="$("$GODOT_EXE" --version)" || {
        echo "错误: Godot 不可用；配置 GODOT_EXE 或运行 scripts/fetch-godot.sh。" >&2
        return 1
    }
    [[ "$navia_version" == "$NAVIA_GODOT_VERSION.stable"* ]] || {
        echo "错误: 需要 Godot $NAVIA_GODOT_VERSION stable，当前为 $navia_version。" >&2
        return 1
    }
}
