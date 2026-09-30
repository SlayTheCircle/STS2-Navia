mkdir -p "$DST/cards"
ok=0
# 卡图窗口为 25:19 横幅(vanilla card.tscn Portrait 250×190 + KEEP_ASPECT_COVERED,Hikari 1000×760 同比例)。
# 方图母版先 cover 缩放再居中裁到 750×570,避免两侧留白;美工后续请直接按 25:19 构图出图。
# 先古卡是竖版图窗 606×852(vanilla suppress/apparition 实测)——与常规 25:19 横窗不同,
# 常规裁法塞竖窗会被横向压扁(实测事故 2026-09-29)。
ANCIENT_CARDS="CannonRoar CoveringFire LightningReload"
for zh in "${!CARDS[@]}"; do
    cls="${CARDS[$zh]}"
    src="$SRC/卡图/$zh.png"
    case " $ANCIENT_CARDS " in
        *" $cls "*)
            convert "$src" -resize 606x852^ -gravity center -extent 606x852 "$DST/cards/$cls.png" ;;
        *)
            convert "$src" -resize 750x570^ -gravity center -extent 750x570 "$DST/cards/$cls.png" ;;
    esac
    ok=$((ok+1))
done
echo "卡图: $ok 张就绪"
