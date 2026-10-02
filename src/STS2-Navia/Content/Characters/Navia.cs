using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Cards;
using NaviaMod.Content.PotionPools;
using NaviaMod.Content.RelicPools;
using NaviaMod.Content.Relics;
using NaviaMod.Content.Timeline;

namespace NaviaMod.Content.Characters;

/// <summary>
/// 娜维娅:来自白淞镇的刺玫会会长,用铳弹与礼炮扫平一切障碍。初始生命 75。
/// 核心机制:装填(见 <see cref="Powers.LoadPower"/>)、礼炮轰鸣(见 <see cref="Mechanics.Salvo"/>)。
/// 角色与能量计通过 RitsuLib 资产档案接入；出牌轨迹使用专属金色场景，转场和音效仍复用原版资源。
/// </summary>
[RegisterCharacter]
[UnlockEpochAfterRunAs(typeof(Navia1Epoch))]        // 第一章·启程:完成一局娜维娅
[UnlockEpochAfterWinAs(typeof(Navia2Epoch))]        // 第二章·荒疫:首次通关
[UnlockEpochAfterBossVictories(typeof(Navia3Epoch), 3)] // 第三章·好奇心:累计击败 3 首领
[UnlockEpochAfterAscensionOneWin(typeof(Navia4Epoch))] // 第四章·尖塔的尽头:进阶 1 通关(vanilla 第七章同型)
public sealed class Navia : CharacterModel
{
    public override Color NameColor => new Color("E8B23AFF");

    public override CharacterGender Gender => CharacterGender.Feminine;

    // 角色不锁定(设计裁定 2026-09-29:玩家可自由选择)——UnlocksAfterRunAs 维持 null。
    protected override CharacterModel? UnlocksAfterRunAs => null;

    public override int StartingHp => 75;

    public override int StartingGold => 99;

    public override CardPoolModel CardPool => ModelDb.CardPool<NaviaCardPool>();

    public override RelicPoolModel RelicPool => ModelDb.RelicPool<NaviaRelicPool>();

    public override PotionPoolModel PotionPool => ModelDb.PotionPool<NaviaPotionPool>();

    public override IEnumerable<CardModel> StartingDeck => new CardModel[]
    {
        ModelDb.Card<StrikeNavia>(),
        ModelDb.Card<StrikeNavia>(),
        ModelDb.Card<StrikeNavia>(),
        ModelDb.Card<StrikeNavia>(),
        ModelDb.Card<DefendNavia>(),
        ModelDb.Card<DefendNavia>(),
        ModelDb.Card<DefendNavia>(),
        ModelDb.Card<DefendNavia>(),
        ModelDb.Card<QuickReload>(),
        ModelDb.Card<VolleyFire>(),
    };

    // 初始遗物恒为刺玫会徽(设计裁定 2026-09-29 勘误):进阶换装不走开局判定,
    // 而是欧罗巴斯事件给的「欧罗巴斯之触」遗物在获取时用 RelicCmd.Replace 就地替换——
    // 映射见 RosulaEmblem 上的 [RegisterTouchOfOrobasRefinement(typeof(RosulaFragrance))]。
    public override IReadOnlyList<RelicModel> StartingRelics => new RelicModel[] { ModelDb.Relic<RosulaEmblem>() };

    public override float AttackAnimDelay => 0.15f;

    public override float CastAnimDelay => 0.4f;

    public override Color EnergyLabelOutlineColor => new Color("8A6210");

    public override Color DialogueColor => new Color("9A7B2D");

    public override Color MapDrawingColor => new Color("D4A017");

    public override Color RemoteTargetingLineColor => new Color("E8B23AFF");

    public override Color RemoteTargetingLineOutline => new Color("8A6210");

    // 占位:复用铁甲的切场音效,待配音接入后替换。
    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";

#if !NAVIA_GAME_0107_1
    // 0.107.1 无此虚属性,其 GenerateAnimator 硬编码的默认映射与本覆写逐项相同,省略即等价。
    protected override List<(AnimState, string)> AnimationStates => new List<(AnimState, string)>
    {
        (new AnimState("attack"), "Attack"),
        (new AnimState("hurt"), "Hit"),
        (new AnimState("cast"), "Cast"),
    };
#endif

    public override List<string> GetArchitectAttackVfx()
    {
        return new List<string> { "vfx/vfx_attack_slash", "vfx/vfx_heavy_blunt", "vfx/vfx_bloody_impact" };
    }
}
