# 事件肖像保持母版比例；世界线图→章节映射已经设计者追认(2026-09-30,见实现对照)。
# 全部按字节比较更新，避免母版换图后被存在性或 mtime 守卫跳过。
mkdir -p "$DST/events" "$DST/timeline"
convert "$SRC/图片/事件1.png" "$DST/events/HometownMemory.png"
convert "$SRC/图片/事件2.png" "$DST/events/ForeignBall.png"
convert "$SRC/图片/事件3.png" "$DST/events/HeavyRain.png"
echo "事件立绘: 三张既有母版就绪"

# 纪元大图使用原版推导的全局路径；缩略图走 RitsuLib 的 mod 资源槽。
DST_G="$ROOT/assets/global/images/timeline/epoch_portraits"
mkdir -p "$DST_G"
convert "$SRC/图片/世界线启程.png" -resize 1672x941^ -gravity center -extent 1672x941 "$DST_G/sts2_navia_epoch_1.png"
for n in 1 2 3; do
    convert "$SRC/图片/世界线$n.png" "$DST_G/sts2_navia_epoch_$((n + 1)).png"
done
for n in 1 2 3 4; do
    convert "$DST_G/sts2_navia_epoch_$n.png" -resize 272x174^ -gravity center -extent 272x174 \
        "$DST/timeline/sts2_navia_epoch_${n}_thumb.png"
done
echo "纪元立绘: 启程新母版 + 三张既有世界线图 + 四张派生缩略图就绪"
