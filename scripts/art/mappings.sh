# 已实装卡:中文名 → 类名(与 Content/Cards/ 一一对应)。再生成前全量检查母版。
declare -A CARDS=(
    ["打击"]="StrikeNavia" ["防御"]="DefendNavia" ["快速装填"]="QuickReload"
    ["铳弹齐射"]="VolleyFire" ["请君入瓮"]="InvitationUrn"
    ["猛铳打击"]="MightyShot" ["阳伞打击"]="UmbrellaStrike" ["热情枪火"]="PassionFire"
    ["金花礼炮"]="GoldenRoseCannon" ["闪耀摩拉"]="ShiningMora"
    ["稳扎稳打"]="SteadfastAdvance" ["破甲兵装"]="ArmorPiercer" ["据守阵地"]="HoldPosition"
    ["捍卫战线"]="DefendLine" ["固若金汤"]="Impregnable" ["利斧强袭"]="AxeOnslaught"
    ["穿心膛线"]="RifledBarrel"
    ["乘胜追击"]="SurgingPursuit" ["紧急避险"]="EmergencyEvade" ["抛射弹壳"]="EjectedShell"
    ["鸣枪示意"]="WarningShot" ["高压弹膛"]="HighPressureChamber" ["危险改装"]="DangerousRetrofit"
    ["指挥形态"]="CommandStance"
    ["兜售枪火"]="PeddlingFirearms" ["牟取利益"]="Profiteering" ["豪掷千金"]="ExtravagantSpend"
    ["高价买入"]="PremiumPurchase" ["强制买断"]="ForcedBuyout" ["金融市场"]="FinancialMarket"
    ["做空市场"]="ShortSelling" ["风险投资"]="VentureCapital"
    ["黄金打击"]="GoldenStrike" ["金融防线"]="FinancialLine" ["华丽开场"]="GrandEntrance"
    ["众志成城"]="UnitedFront" ["囤积物资"]="HoardingSupplies" ["鸟枪换炮"]="UpgradeArsenal"
    ["军火流通"]="ArmsCirculation"
    ["火力覆盖"]="SuppressingFire" ["精制装药"]="RefinedCharge" ["意外事故"]="AccidentalBlast"
    ["火力增幅"]="FireAmplification" ["例行检查"]="RoutineInspection" ["轻装上阵"]="TravelLight"
    # 支援系批(2026-09-29)
    ["会长号令"]="PresidentOrder" ["军火大亨"]="ArmsDealer" ["热情昂扬"]="HighMorale"
    ["超额支出"]="Overspend" ["铸剑为犁"]="SwordsToPlowshares" ["募集资金"]="Fundraising"
    ["邻里互助"]="MutualAid" ["高歌猛进"]="ForwardMarch" ["率直作风"]="FrankManner"
    ["刺玫手段"]="RosulaMethod" ["横行灰河"]="GrayRiverRampage" ["伸出援手"]="LendAHand"
    ["高额回报"]="HighPayout" ["乐观估测"]="OptimisticForecast"
    # 常规批(2026-09-29)
    ["炮火连天"]="Cannonade" ["贪婪枪火"]="GreedyGunfire" ["补充物资"]="Resupply"
    ["紧急调度"]="UrgentDispatch" ["回收利息"]="CollectInterest" ["砥兵备战"]="WarPreparation"
    ["先发制人"]="PreemptiveStrike" ["出其不意"]="OffGuard" ["软硬兼施"]="CarrotAndStick"
    ["开辟航道"]="ChartCourse" ["冒险收获"]="DaringHaul" ["简易护甲"]="MakeshiftArmor"
    ["生财有道"]="GoldenTouch" ["坚船利炮"]="Gunboat" ["枪刺拼杀"]="BayonetCharge"
    ["亏空平账"]="BalanceTheBooks" ["灰河硝烟"]="GrayRiverHaze" ["最终突击"]="FinalAssault"
    ["空心弹头"]="HollowPoint" ["庆贺礼炮"]="FestiveCannon" ["炮身加固"]="ReinforcedBarrel"
    ["一枪爆头"]="Headshot" ["双管齐下"]="DoubleBarrel" ["扩容弹夹"]="ExtendedMagazine"
    ["追加订单"]="AdditionalOrder" ["引导轰炸"]="GuidedBombardment"
    # 炮修改批(2026-09-29,亲手)
    ["饱和爆破"]="SaturationBlast" ["武器保养"]="WeaponMaintenance" ["通货膨胀"]="Inflation"
    # 先古卡(2026-09-29)
    ["枪炮轰鸣"]="CannonRoar" ["掩护轰炸"]="CoveringFire" ["神速装填"]="LightningReload"
)

# 遗物(9 张全量):中文名 → 类名
declare -A RELICS=(
    ["刺玫会徽"]="RosulaEmblem"
    ["刺玫会特制铳枪"]="RosulaMusket"
    ["明黄缎带"]="GoldenRibbon"
    ["美味的马卡龙"]="Macaron"
    ["摩拉袋子"]="MoraPouch"
    ["精致的阳伞"]="ElegantParasol"
    ["裁断"]="Verdict"
    ["刺玫会月度报表"]="MonthlyReport"
    ["仅留余香的野蔷薇"]="RosulaFragrance"
)

# 药水(5 张全量):中文名 → 类名
declare -A POTIONS=(
    ["枫达"]="Fonta"
    ["刺玫佳酿"]="RosulaWine"
    ["乐斯"]="Lesse"
    ["铳枪润滑油"]="FirearmLubricant"
    ["沉玉茶露"]="ChenyuTea"
)

# 可见 Power 母版映射；格挡保留程序化金盾和乐斯药水复用在 icons.sh 维护。
declare -A POWERS=(
    ["指挥形态-图标"]="CommandStancePower"
    ["兜售枪火-图标"]="PeddlingFirearmsPower"
    ["危险改装-图标"]="DangerousRetrofitPower"
    ["危险改装·补满-图标"]="DangerousRetrofitFillPower"
    ["紧急避险-图标"]="EmergencyEvadePower"
    ["金融市场-图标"]="FinancialMarketPower"
    ["高压弹膛-图标"]="HighPressureChamberPower"
    ["众志成城-图标"]="UnitedFrontPower"
    ["牟取利益-图标"]="ProfiteeringPower"
    # 第三包新到(2026-09-29):延迟装填=待发装填(ExtravagantSpend,装填家族+转向箭头)
    ["装填-图标"]="LoadPower"
    ["囤积物资-图标"]="HoardingSuppliesPower"
    ["风险投资-图标"]="VentureCapitalPower"
    ["延迟装填-图标"]="ExtravagantSpendPower"
    # 支援系 4 Power(第六版需求单勘误:此4张早在23图标包已交付,此前误列待补)
    ["刺玫手段-图标"]="RosulaMethodPower"
    ["伸出援手-图标"]="LendAHandPower"
    ["高额回报-图标"]="HighPayoutPower"
    ["乐观估测-图标"]="OptimisticForecastPower"
    ["饱和爆破-图标"]="SaturationBlastPower"
    ["通货膨胀-图标"]="InflationPower"
    # 已交付六图及本轮补齐三图；全部经过母版生成，不手改成品。
    ["炮火连天-图标"]="CannonadePower"
    ["炮身加固-图标"]="ReinforcedBarrelPower"
    ["引导轰炸-图标"]="GuidedBombardmentPower"
    ["贪婪枪火-图标"]="GreedyGunfirePower"
    ["生财有道-图标"]="GoldenTouchPower"
    ["亏空平账-图标"]="BalanceTheBooksPower"
    ["回收利息-图标"]="CollectInterestPower"
    ["掩护轰炸-图标"]="CoveringFirePower"
    # 数值调整V1 新增双光环：两张母版均已齐，意外事故按当前格挡／装填机制补图。
    ["军火大亨-图标"]="ArmsDealerPower"
    ["意外事故-图标"]="AccidentalBlastPower"
)
