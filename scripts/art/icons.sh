mkdir -p "$DST/relics" "$DST/potions" "$DST/powers" "$DST/enchantments"
source "$ROOT/scripts/art/icon-finishing.sh"
# 遗物图标 256²(原版规格);同时从主图生成描边变体 <类名>_outline.png
# (遗物条背景的 outline 槽缺图会露出游戏 NOPE 缺图纹理,Watcher 同样三槽全供)。
ok=0
for zh in "${!RELICS[@]}"; do
    cls="${RELICS[$zh]}"
    src="$SRC/遗物/$zh.png"
    convert "$src" -resize 256x256 "$DST/relics/$cls.png"
    # 描边:alpha 通道外扩(dilate)成剪影 → 染成金色 → 作为 alpha 合成纯色图
    convert "$DST/relics/$cls.png" -bordercolor none -border 6 -alpha extract \
        -morphology Dilate Disk:5 -gravity center -crop 256x256+0+0 +repage "$TMP/mask.png"
    convert -size 256x256 xc:'#f4cf70' "$TMP/mask.png" -alpha off -compose CopyOpacity \
        -composite "$DST/relics/${cls}_outline.png"
    ok=$((ok+1))
done
echo "遗物图标: $ok 张就绪(含描边变体)"

# 药水图标 256² + 描边变体(与遗物同构)
ok=0
for zh in "${!POTIONS[@]}"; do
    cls="${POTIONS[$zh]}"
    src="$SRC/药水/$zh.png"
    convert "$src" -resize 256x256 "$DST/potions/$cls.png"
    convert "$DST/potions/$cls.png" -bordercolor none -border 6 -alpha extract \
        -morphology Dilate Disk:5 -gravity center -crop 256x256+0+0 +repage "$TMP/mask.png"
    convert -size 256x256 xc:'#f4cf70' "$TMP/mask.png" -alpha off -compose CopyOpacity \
        -composite "$DST/potions/${cls}_outline.png"
    ok=$((ok+1))
done
echo "药水图标: $ok 张就绪(含描边变体)"

# Power 图标 256²(透明底,无描边变体——原版 mod 走法只需主图)
ok=0
for zh in "${!POWERS[@]}"; do
    cls="${POWERS[$zh]}"
    src="$SRC/buff图标/$zh.png"
    # 完成修饰后再写成品，避免重跑时先写原图再写描边导致 mtime 假变化。
    convert "$src" -resize 256x256 "$TMP/power.png"
    finish_power_icon "$cls" "$TMP/power.png"
    convert "$TMP/power.png" "$DST/powers/$cls.png"
    ok=$((ok+1))
done
echo "Power 图标: $ok 张就绪"

GOLD='#E8B23A'; DARK='#8A6210'; LIGHT='#F7E08A'
# 格挡保留:程序化金盾——2026-09-29 转正为正式图标(用户裁定),非占位,勿因「缺素材」覆写
convert -size 256x256 xc:none \
    -fill "$DARK" -draw 'polygon 128,24 216,64 216,140 128,232 40,140 40,64' \
    -fill "$GOLD" -draw 'polygon 128,48 192,78 192,138 128,208 64,138 64,78' \
    "$DST/powers/OneTurnBlockPersistPower.png"

# 乐斯能力图复用既有药水母版，统一支援徽记使用新母版。
convert "$SRC/药水/乐斯.png" -resize 256x256 "$DST/powers/LesseWitheringPower.png"
convert "$SRC/buff图标/支援徽记.png" -resize 256x256 "$DST/enchantments/navia_support.png"
echo "正式金盾、乐斯复用图与统一支援徽记就绪"
