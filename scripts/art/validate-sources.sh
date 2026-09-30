# 全量再生成前检查母版；缺输入时失败，不能靠已有成品伪装交付完成。
require_art_source() {
    [[ -f "$SRC/$1" ]] || { echo "错误: 美术母版缺失: $1" >&2; return 1; }
}

for zh in "${!CARDS[@]}"; do require_art_source "卡图/$zh.png"; done
for zh in "${!RELICS[@]}"; do require_art_source "遗物/$zh.png"; done
for zh in "${!POTIONS[@]}"; do require_art_source "药水/$zh.png"; done
for zh in "${!POWERS[@]}"; do require_art_source "buff图标/$zh.png"; done
for name in 常规 技能 受击 倒下 商店 火堆; do require_art_source "立绘/立绘——$name.png"; done
for name in 石头 剪刀 布 指; do require_art_source "猜拳/$name.png"; done
for name in 选人头像 选人背景 能量球分层 世界线启程 世界线1 世界线2 世界线3 事件1 事件2 事件3; do
    require_art_source "图片/$name.png"
done
require_art_source '头像.png'
require_art_source 'buff图标/支援徽记.png'
