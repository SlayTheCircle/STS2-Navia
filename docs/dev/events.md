# 事件实现

当前事件实现位于 [Content/Events](../../src/STS2-Navia/Content/Events/NaviaEventBase.cs)。叙述与原始效果见[事件设计](../history/design/events.md)，方案调查与勘误见[技术历史](../history/README.md)。

## 注册与门控

新事件继承 NaviaEventBase，由 RitsuLib 的 RegisterActEvent 注册到对应 Act。IsAllowed 查询队伍中是否有娜维娅，因此新事件允许“队伍含娜维娅”；原版追加选项查询事件 Owner.Character，仅对归属者是娜维娅的事件追加。这两种门控不同，多人验收应覆盖它们。

| 新事件 | Act | 效果与实现 |
|---|---|---|
| HometownMemory | Overgrowth，一层 | 晴天移除一卡；阴天移除两卡并将 Guilty 加入主卡组 |
| ForeignBall | Hive，二层 | 变化一卡；治疗二十并给随机药水；支付一百金币并给普通遗物 |
| HeavyRain | Glory，三层 | 从可移除牌中随机移除一卡；普通遗物并将 PoorSleep 加入主卡组 |

原案阴天写“加入手牌”，场外实际实现加入主卡组；差异见[对照说明](../history/design/implementation-notes.md)。不要依照历史调查中的待查项替换已实现 API。

## 原版追加选项

[VanillaEventNaviaOptions](../../src/STS2-Navia/Content/Events/VanillaEventNaviaOptions.cs) 在 EventModel.GenerateInitialOptionsWrapper 基方法上打 Harmony postfix。当前目标未覆写该方法；古事件的独立覆写不依赖此补丁。游戏升级时重新核对方法及目标类。

| 原版类 | 追加选项 | 行为 |
|---|---|---|
| StoneOfAllTime | 尝试将巨石打碎 | 选卡附魔 Glam 一层 |
| Trial | 大声斥责 | 选一卡移除 |
| SpiralingWhirlpool | 回忆往事 | 选一卡升级 |
| Bugslayer | 分享自己的经验 | 获得随机遗物 |
| TeaMaster | 分享茶点 | 恢复五生命，获得五十金币 |

SetEventFinished 是受保护成员，外部处理器使用启动时缓存的反射委托。找不到方法时记录错误并停止追加，保留原版选项；追加失败也记录错误。目标游戏更新后应核对签名和错误日志。

## 初始化通道

ModEntry 带 ModInitializer，游戏加载器不会再自动 PatchAll。因此 [ModEntry.Init](../../src/STS2-Navia/ModEntry.cs) 完成内容／关键词／资产注册后，必须显式调用 Harmony.PatchAll。三个新事件的注册和五个原版追加选项的补丁接线分别验证。

补丁声明、DLL 编译、日志与真实选项出现提供不同层次的证据。历史报告中仅凭加载器日志判定整个事件链通过的结论已修正，见[事件审计](../history/audits/2026-09-29-event-static-audit.md)。

## 本地化与资源

新事件复合 Entry 为 STS2_NAVIA_EVENT_<类名蛇形大写>，在 localization/{lang}/events.json 维护：

- Entry.title。
- Entry.pages.INITIAL.description。
- Entry.pages.INITIAL.options.<KEY>.title／description。
- 各结果页的 description。

ModEventTemplate 的 InitialOptionKey 和 PageDescription 拼接层级；原版追加选项使用原版 Entry 下的 NAVIA_* 键。动态变量随 CanonicalVars 声明，选牌提示、代价、悬停预览与效果一致。

事件肖像使用 images/events/<类名>.png。当前三张已接入，背景复用游戏的 unrest_site、round_tea_party、drowning_beacon 场景。资源位置与尺寸见[素材规范](assets.md)；需求见[美术贡献](../design/artwork.md)。

## 修改与验收

同步修改事件类、中英本地化、资源映射和对应设计差异。源码检查覆盖键和文本资源，PCK 检查覆盖代表性资源解析，真实游戏还需确认门控、选项、支付不足、不可操作卡、空池和奖励结果。

DevConsole 的 event <Entry> 可直达目标事件，例如 event STS2_NAVIA_EVENT_HOMETOWN_MEMORY 或 event TEA_MASTER。该命令绕过自然生成门控，因此只能验收事件处理与呈现，不能证明 IsAllowed 或自然生成概率正确。未进行的多人、存档或边界场景如实记录。
