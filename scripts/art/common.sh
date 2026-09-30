# convert 包装:输出走「临时文件 + 字节比对,相同不替换」——ImageMagick 会在 PNG 里嵌生成时间戳,
# 无条件重跑会让全部素材每次假性变更(三次实测事故);mtime 守卫在 git checkout 刷新时间后也不可靠。
# 配合 -strip 剥元数据,同母版+同参数 → 字节级确定,重跑零 diff。
convert() {
    local args=("$@") out="${@: -1}" tmp
    tmp="$(dirname "$out")/.$(basename "$out").tmp$$.png"
    command convert "${args[@]:0:$(( $# - 1 ))}" -strip "png:$tmp"
    if [ -f "$out" ] && cmp -s "$tmp" "$out"; then
        rm -f "$tmp"
    else
        mv "$tmp" "$out"
    fi
}

# 全流程共享的描边临时目录，退出时统一清理。
TMP=$(mktemp -d)
trap 'rm -rf "$TMP"' EXIT
