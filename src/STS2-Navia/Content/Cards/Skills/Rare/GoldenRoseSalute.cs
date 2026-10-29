using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using NaviaMod.Content.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Mechanics;

namespace NaviaMod.Content.Cards;

/// <summary>金花致意(稀有,2 费技能,联机限定):礼炮轰鸣 2，选择手中一张金花礼炮对指定敌人打出；其他玩家获得等于其实际未被格挡伤害的格挡。升级:轰鸣 4，改为随机敌人。</summary>
[RegisterCard(typeof(NaviaCardPool))]
public sealed class GoldenRoseSalute : NaviaCardBase
{
    private GoldenRoseCannon? _playedCannon;
    private int _unblockedDamage;

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override TargetType TargetType => IsUpgraded ? TargetType.RandomEnemy : TargetType.AnyEnemy;

    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            HashSet<CardKeyword> set = new HashSet<CardKeyword>();
            NaviaKeywords.AddTo(set, NaviaKeywords.Salvo);
            return set;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("Salvo", 2m),
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => new[] { HoverTipFactory.FromCard<GoldenRoseCannon>() };

    public GoldenRoseSalute()
        : base(2, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await Salvo.Fire(base.CombatState, choiceContext, base.Owner, base.DynamicVars["Salvo"].BaseValue);
        GoldenRoseCannon? cannon = (await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(new LocString("card_selection", "STS2_NAVIA_TO_PLAY_CANNON"), 1),
            context: choiceContext,
            player: base.Owner,
            filter: c => c is GoldenRoseCannon,
            source: this)).OfType<GoldenRoseCannon>().FirstOrDefault();
        if (cannon == null)
        {
            return;
        }

        _playedCannon = cannon;
        _unblockedDamage = 0;
        try
        {
            await CardCmd.AutoPlay(choiceContext, cannon, IsUpgraded ? null : cardPlay.Target);
        }
        finally
        {
            _playedCannon = null;
        }

        if (_unblockedDamage <= 0)
        {
            return;
        }
        Creature creature = base.Owner.Creature;
        foreach (Creature teammate in creature.CombatState!.GetTeammatesOf(creature).Where(c => c.IsAlive && c.Player != null && c != creature))
        {
            await CreatureCmd.GainBlock(teammate, _unblockedDamage, ValueProp.Unpowered, cardPlay);
        }
    }

    public override Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer == base.Owner.Creature && ReferenceEquals(cardSource, _playedCannon))
        {
            _unblockedDamage += result.UnblockedDamage;
        }
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Salvo"].UpgradeValueBy(2m);
    }
}
