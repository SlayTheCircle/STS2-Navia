# 战斗机制与视觉分发回归验证

本探针保护三类已确认缺陷：浅克隆导致通货膨胀共享摩拉集合；炮火连天把能力份数误作累计次数、嵌套出牌吞掉首炮触发；初始遗物升级后视觉事件失去宿主；原生动画的死亡被普通触发覆盖、动作结束回到过时生命姿态、技能兜底打断原生攻击与多玩家状态共享。

使用真实游戏、RitsuLib 和本模组程序集，调用实际模型克隆、Power 钩子、礼炮生成和 ModHelper 订阅流。玩家与牌堆由最小夹具组装；抽牌和加牌命令的 UI 边界、战斗进行中标记、Godot 日志与视觉输出被隔离。原生动画规则使用实际 RitsuLib 状态机与最小回放后端验证。断言覆盖抽牌接收者、实例数值和视觉事件路由；渲染、网络同步、完整出牌流程与存档恢复在游戏内确认。

## 执行

在 Bash 中，从仓库根运行。对每个游戏目标分别编译并启动新进程；设置与目标一致的 `GAME_REFS_DIR`、`RITSULIB_TARGET` 和完整游戏运行时 DLL 目录。运行时目录除了游戏附加依赖，还须包含 RitsuLib 使用的 System.IO.Hashing 等程序集。

```bash
source scripts/dev-env.sh
# 切换目标时覆盖以下变量；路径由本机配置提供。
export RITSULIB_TARGET=0.111.0
export GAME_REFS_DIR="$NAVIA_GAME_REFS_1110"
"$DOTNET_EXE" build tests/CombatProbe/CombatProbe.csproj -c Release
"$DOTNET_EXE" tests/CombatProbe/bin/Release/net9.0/CombatProbe.dll \
  "$GAME_REFS_DIR" "$NAVIA_RUNTIME_DLL_DIR" "$RITSULIB_DIR" "$RITSULIB_TARGET"
```

测试覆盖：双玩家抽牌隔离、旧／新摩拉与重复进场、换主后不误抽、下场能力无旧记录；首炮前零加成、五个战斗牌堆、新生成礼炮继承累计值、嵌套／自动／重复出牌与两份能力跨回合叠加；装填力量等价的取整／归属／伤害类型口径、穿心膛线可叠加上限与 Gain 截断、危险改装先失后补满的回合流与永续流失接管；原版订阅流中的唯一视觉宿主、两种遗物及无遗物时的预热／生命刷新／技能兜底分发与多角色隔离；原生工厂声明、生命敏感回位、打断优先级、死亡终止、复活回位及动画状态隔离。

## 游戏内验收

两版本分别检查一份／两份炮火连天与自动出炮；联机双方持有通货膨胀并交替生成、打出摩拉；会徽升级前后与移除后观察受击、技能、死亡及复活动作；退出并恢复存档后重新进入战斗。实际完成情况记录在 STATUS。

清单生成回归位于 `tests/build/`，随源码检查运行，不需要游戏 DLL。Loader 注册接线继续由独立的 [LoaderProbe](../LoaderProbe/README.md) 验证。
