# 角色本体资产(⑥阶段):四态立绘原图直入 + 程序化派生(选人灰阶/图标描边/地图标记)
mkdir -p "$DST/characters" "$DST/hands"
convert "$SRC/立绘/立绘——常规.png" "$DST/characters/navia_normal.png"
convert "$SRC/立绘/立绘——技能.png" "$DST/characters/navia_skill.png"
convert "$SRC/立绘/立绘——受击.png" "$DST/characters/navia_hit.png"
convert "$SRC/立绘/立绘——倒下.png" "$DST/characters/navia_down.png"
# 商店/篝火专属立绘(第三包交付,1024×1536;此前两场景用「常规」顶替)
convert "$SRC/立绘/立绘——商店.png" "$DST/characters/navia_merchant.png"
convert "$SRC/立绘/立绘——火堆.png" "$DST/characters/navia_rest_site.png"
# 选人立绘:用户手裁的半身图(图片/选人头像.png,637x917≈2:3)等比缩放 + 灰阶锁定版。
# 裁框为人工校准,勿改回自动裁切(自动方案曾裁歪,2026-09-29)。
convert "$SRC/图片/选人头像.png" -resize 264x "$DST/characters/navia_select.png"
convert "$DST/characters/navia_select.png" -modulate 60,0,100 "$DST/characters/navia_select_locked.png"
# 头像:128²(原版规格 88²,取 2 倍保高清)+ 程序化描边 + 地图标记
convert "$SRC/头像.png" -resize 128x128 "$DST/characters/navia_character_icon.png"
convert "$DST/characters/navia_character_icon.png" -bordercolor none -border 4 -alpha extract \
    -morphology Dilate Disk:3 -gravity center -crop 128x128+0+0 +repage "$TMP/iconmask.png"
convert -size 128x128 xc:'#f4cf70' "$TMP/iconmask.png" -alpha off -compose CopyOpacity \
    -composite "$DST/characters/navia_character_icon_outline.png"
convert "$SRC/头像.png" -resize 128x128 "$DST/characters/navia_map_marker.png"
# 选人背景(1672×941 原尺寸)
convert "$SRC/图片/选人背景.png" "$DST/characters/navia_char_select_bg.png"
# 联机手 ×4(1254²→512²)
convert "$SRC/猜拳/石头.png" -resize 512x512 "$DST/hands/navia_hand_rock.png"
convert "$SRC/猜拳/剪刀.png" -resize 512x512 "$DST/hands/navia_hand_scissors.png"
convert "$SRC/猜拳/布.png" -resize 512x512 "$DST/hands/navia_hand_paper.png"
convert "$SRC/猜拳/指.png" -resize 512x512 "$DST/hands/navia_hand_pointing.png"
echo "角色资产: 四态立绘 + 选人/头像/描边/标记/背景 + 联机手×4 就绪"
