# 已交付母版为单张透明金玫瑰；不伪称具有独立球体/光晕/粒子图层。
# 同一主图供静态费用图标与战斗能量计；主图在场景中旋转，粒子按场景参数生成。
mkdir -p "$DST/energy"
convert "$SRC/图片/能量球分层.png" -resize 256x256 "$DST/energy/navia_energy_big.png"
convert "$DST/energy/navia_energy_big.png" -resize 24x24 "$DST/energy/navia_energy_text.png"
echo "能量玫瑰: big 256² + text 24² 就绪(使用已交付母版)"
