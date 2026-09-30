#!/usr/bin/env bash
# 从配置的美术母版生成资源；各类派生职责位于 scripts/art/。
set -euo pipefail
source "$(dirname "$0")/dev-env.sh"
ROOT="$NAVIA_ROOT"
SRC="${ART_SOURCE_DIR:-}"
DST="$ROOT/assets/STS2-Navia/images"
[[ -n "$SRC" && -d "$SRC" ]] || { echo '错误: 请配置存在的 ART_SOURCE_DIR。' >&2; exit 1; }
source "$ROOT/scripts/art/common.sh"
source "$ROOT/scripts/art/mappings.sh"
source "$ROOT/scripts/art/validate-sources.sh"
source "$ROOT/scripts/art/cards.sh"
source "$ROOT/scripts/art/icons.sh"
source "$ROOT/scripts/art/characters.sh"
source "$ROOT/scripts/art/energy.sh"
source "$ROOT/scripts/art/stories.sh"
