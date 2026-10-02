# 仅处理实尺寸检查确认的派生问题；半径单位为 256² 成品像素。
# 装填家族的亮黄细线在浅色背景上缺少边界，补 3px 深金轮廓。
declare -A POWER_EDGE_RADIUS=([LoadPower]=3 [ExtravagantSpendPower]=3)

finish_power_icon() {
    local cls="$1" output="$2" radius="${POWER_EDGE_RADIUS[$1]:-0}"
    [[ "$radius" != 0 ]] || return 0
    convert "$output" -alpha extract -morphology Dilate "Disk:$radius" "$TMP/power-edge-mask.png"
    convert -size 256x256 xc:'#8A6210' "$TMP/power-edge-mask.png" \
        -alpha off -compose CopyOpacity -composite "$TMP/power-edge.png"
    convert "$TMP/power-edge.png" "$output" -compose Over -composite "$output"
}
