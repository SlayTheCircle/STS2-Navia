using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Models.CardPools;
using NaviaMod.Content.Keywords;
using NaviaMod.Content.Powers;

namespace NaviaMod.Content.Cards;

/// <summary>
/// 闪耀摩拉(token,无色,0 费攻击):造成 3 点伤害。消耗。保留。
/// 每打出 3 张闪耀摩拉,装填 1(计数由隐藏的 <see cref="MoraCounterPower"/> 承载)。
/// TODO:「摩拉袋子」等按摩拉结算的遗物/卡实装时挂接 <see cref="MoraCounterPower"/>。
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
public sealed class ShiningMora : NaviaCardBase
{
    public const int LoadsPerMoraCount = 3;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DamageVar(3m, ValueProp.Move) };

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            // 描述提到「装填」,挂关键词横幅+解释框(同其他摩拉系卡)。
            HashSet<CardKeyword> set = new HashSet<CardKeyword> { CardKeyword.Exhaust, CardKeyword.Retain };
            NaviaKeywords.AddTo(set, NaviaKeywords.Load);
            return set;
        }
    }

    public ShiningMora()
        : base(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await CountMoraPlayed(choiceContext, base.Owner.Creature, this);
    }

    public static async Task<IReadOnlyList<ShiningMora>> CreateInHand(Player owner, int count, ICombatState? combatState)
    {
        if (count <= 0 || combatState == null || CombatManager.Instance.IsOverOrEnding)
        {
            return Array.Empty<ShiningMora>();
        }
        List<ShiningMora> moras = new List<ShiningMora>(count);
        for (int i = 0; i < count; i++)
        {
            moras.Add(combatState.CreateCard<ShiningMora>(owner));
        }
        await CardPileCmd.AddGeneratedCardsToCombat(moras, PileType.Hand, owner);
        return moras;
    }

    /// <summary>打出一张闪耀摩拉后调用:计数 +1,每满 3 张消耗 3 层计数并装填 1。</summary>
    public static async Task CountMoraPlayed(PlayerChoiceContext choiceContext, Creature creature, CardModel? cardSource)
    {
        MoraCounterPower? counter = await PowerCmd.Apply<MoraCounterPower>(choiceContext, creature, 1, creature, cardSource);
        if (counter != null && counter.Amount >= LoadsPerMoraCount)
        {
            await PowerCmd.ModifyAmount(choiceContext, counter, -LoadsPerMoraCount, creature, cardSource);
            await LoadPower.Gain(choiceContext, creature, 1, creature, cardSource);
        }
    }
}
