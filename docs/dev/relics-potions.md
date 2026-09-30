# 遗物与药水

原案见[遗物与药水设计](../history/design/relics-potions.md)，共用机制与文本约定见[卡牌、Power 与支援](cards.md)。可以同步修改所需实现、本地化及接线，按[贡献指南](../../CONTRIBUTING.md)执行检查；不沿用内部批次的只新建文件、禁止构建或 staging 交付规则。

## 1. 基类与注册(内容发现与资源槽)

- 遗物:`[RegisterRelic(typeof(NaviaRelicPool))]` + 继承 **`NaviaRelicBase`**(图标三槽已按类名自动解析,**不要写任何图标代码**)
- 药水:`[RegisterPotion(typeof(NaviaPotionPool))]` + 继承 **`NaviaPotionBase`**(图标双槽自动)
- 池是 TypeList 模式,注册特性即自动聚合,无需改任何共享文件。

## 2. 遗物骨架

以 `Content/Relics/RosulaEmblem.cs` 为基准。要点:

- 稀有度 `RelicRarity`:Common / Uncommon / Rare / Shop / Event(任务指派给定)
- 「战斗开始时」类效果:参考 vanilla Akabeko / 本仓 RosulaEmblem 的钩子组合(回合开始钩子 + `AfterCombatEnd` 重置标记)
- 触发时 `Flash();` 打闪光;持续型状态可用计数器(查 vanilla 同类遗物的 counter API)
- 需要自定义可见 Power 的,按 [Power 约定](cards.md)维护实现与双语本地化
- 子系统 API(装填/摩拉/礼炮)见 [cards.md](cards.md),**消耗/给予装填一律走 `LoadPower.Gain`**

## 3. 药水骨架(vanilla Ashwater 是好模板)

```csharp
public override PotionRarity Rarity => PotionRarity.Common;
public override PotionUsage Usage => PotionUsage.CombatOnly;   // 治疗类用 AnyTime
public override TargetType TargetType => TargetType.Self;
protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target) { ... }
```

- 目标自己以外的队友需要 `TargetType.AnyPlayer`(参考 Ashwater 的选人写法)
- 药水数值不升级,无 OnUpgrade

## 4. 本地化与生命周期

- 遗物键:`STS2_NAVIA_RELIC_<类名蛇形大写>.{title,description,flavor}`；flavor 对应原案风味文本，核对后维护
- 药水键:`STS2_NAVIA_POTION_<类名蛇形大写>.{title,description}`(无 flavor)
- 描述占位符／富文本规范见 [cards.md](cards.md)(`[gold]名词[/gold]`;`{LoadPower:diff()}` 等)

持久状态使用实际序列化约定；战斗临时标记在对应钩子重置，Flash 与计数器反映实际触发。PowerCmd.Apply 在场外不会赋予战斗 Power，因此 AnyTime 药水不得假定场外 Power 会自然带入下一场。当前乐斯使用 CombatOnly；真需要跨战斗代价时必须明确状态宿主与存档行为。

## 5. 原版角色联动

会徽恒为初始遗物，使用 RegisterTouchOfOrobasRefinement 注册野蔷薇替换关系；野蔷薇不作普通掉落。古老牙齿映射挂在初始特色卡上，见[世界线实现](worldline.md)。

## 6. 变更与验收

同步中英文本、数值、使用目标、资源槽与设计差异。依据行为检查战斗开始／结束、回合重置、场外使用、多人目标及读档；图标需确认静态、小／大图动效和描边，不能仅凭主图存在认为接入完成。当前已知 flavor 缺件见 [STATUS](../../STATUS.md)。
