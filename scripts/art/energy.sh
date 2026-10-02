# 已交付母版为单张透明金玫瑰；不伪称具有独立球体/光晕/粒子图层。
# 同一主图供静态费用图标与战斗能量计；主图在场景中旋转，粒子按场景参数生成。
mkdir -p "$DST/energy"
convert "$SRC/图片/能量球分层.png" -resize 256x256 "$DST/energy/navia_energy_big.png"
convert "$DST/energy/navia_energy_big.png" -resize 24x24 "$DST/energy/navia_energy_text.png"
# 程序化白色柔光，颜色由场景调制；不从玫瑰母版拆层、不改变费用图。
convert -size 64x64 xc:white -alpha set \
    -channel A -fx 'max(0,1-hypot(i-31.5,j-31.5)/31.5)^2' +channel \
    "$DST/energy/navia_spark.png"
convert -size 128x128 xc:white -alpha set \
    -channel A -fx 'max(0,1-hypot(i-63.5,j-63.5)/63.5)^2' +channel \
    "$DST/energy/navia_energy_glow.png"
echo "能量玫瑰: big 256² + text 24² 就绪(使用已交付母版)"
