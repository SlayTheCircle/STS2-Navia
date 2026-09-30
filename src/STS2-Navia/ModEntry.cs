using HarmonyLib;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Characters;
using NaviaMod.Content.Cards;
using NaviaMod.Content.Characters;
using NaviaMod.Content.Keywords;

namespace NaviaMod;

/// <summary>
/// Mod 入口。加载清单见仓库根 STS2-Navia.json;本文件只做注册接线,
/// 游戏内容逻辑全部位于 Content/ 下的纯模型类中。
/// </summary>
[ModInitializer("Init")]
public static class ModEntry
{
    public const string ModId = "STS2-Navia";

    public static void Init()
    {
        // 归属声明:让本程序集内的 RitsuLib 自动注册特性([RegisterCard]/[RegisterCharacter]/…)正确归属到本 mod。
        // 内容注册全部由各模型类上的特性完成,本入口不再手工登记内容清单。
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, typeof(ModEntry).Assembly);

        // 专有名词关键词(横幅+悬停解释,文本在 localization/{lang}/card_keywords.json;
        // stem 必须与 NaviaKeywords 常量的 KEYWORD_ 后缀一致)。卡牌名词(礼炮/摩拉)走卡面预览,不在此注册。
        ModKeywordRegistry keywords = ModKeywordRegistry.For(ModId);
        keywords.RegisterCardKeywordOwnedByLocNamespace("LOAD");   // → STS2_NAVIA_KEYWORD_LOAD(装填)
        keywords.RegisterCardKeywordOwnedByLocNamespace("SALVO");  // → STS2_NAVIA_KEYWORD_SALVO(礼炮轰鸣)
        keywords.RegisterCardKeywordOwnedByLocNamespace("SUPPORT"); // → STS2_NAVIA_KEYWORD_SUPPORT(支援)

        // 角色资产档案(⑥阶段,无 Spine 2D 路线):战斗形象/选人背景/图标/休息点/商店用自建场景,
        // 能量计使用已交付金玫瑰及原版 NEnergyCounter 节点契约；Spine/音频资产集为 null。
        // 场景内脚本按 res:// 路径引用游戏本体 C# 类(正式包内同样解析)。
        string naviaEntry = ModContentRegistry.GetCompoundId(ModId, "character", nameof(Navia)).ToLowerInvariant();
        ModContentRegistry.For(ModId).RegisterCharacterAssetReplacement(naviaEntry, new CharacterAssetProfile(
            new CharacterSceneAssetSet(
                "res://STS2-Navia/scenes/characters/navia_character.tscn",
                "res://STS2-Navia/scenes/combat/navia_energy_counter.tscn",
                "res://STS2-Navia/scenes/characters/navia_merchant.tscn",
                "res://STS2-Navia/scenes/characters/navia_rest_site.tscn"),
            new CharacterUiAssetSet(
                "res://STS2-Navia/images/characters/navia_character_icon.png",
                "res://STS2-Navia/images/characters/navia_character_icon_outline.png",
                "res://STS2-Navia/scenes/characters/navia_icon.tscn",
                "res://STS2-Navia/scenes/characters/navia_char_select_bg.tscn",
                "res://STS2-Navia/images/characters/navia_select.png",
                "res://STS2-Navia/images/characters/navia_select_locked.png",
                // 开局转场材质:留空会按条目名推导 res://materials/transitions/<entry>_transition_mat.tres
                // (CharacterModel.cs:171)——mod 条目下该文件不存在,AssetLoadException 直接炸开局。
                // 先指原版通用淡入淡出,金色专属转场后补。
                "res://materials/transitions/fade_transition_mat.tres",
                "res://STS2-Navia/images/characters/navia_map_marker.png"),
            // 出牌轨迹:留空会按条目名推导 vfx/card_trail_<entry>(CharacterModel.TrailPath)——
            // mod 条目下不存在,NCardFlyVfx._Ready 抛空引用,换牌堆动画队列卡死(打出/弃掉的牌悬停不动)。
            // 先借原版铁甲轨迹,金色专属轨迹后补。
            new CharacterVfxAssetSet("res://scenes/vfx/card_trail_ironclad.tscn"),
            null,
            null,
            new CharacterMultiplayerAssetSet(
                "res://STS2-Navia/images/hands/navia_hand_pointing.png",
                "res://STS2-Navia/images/hands/navia_hand_rock.png",
                "res://STS2-Navia/images/hands/navia_hand_paper.png",
                "res://STS2-Navia/images/hands/navia_hand_scissors.png")));

        // 自带 Harmony 补丁:ModInitializer 通道与游戏自动 PatchAll 是官方二选一语义——
        // 本类有 [ModInitializer] 则游戏只调 Init() 不再自动 PatchAll,需在此手动补
        // (Content/Events/VanillaEventNaviaOptions.cs 的原版事件追加选项依赖本调用)。
        new Harmony(ModId + ".patches").PatchAll(typeof(ModEntry).Assembly);
    }
}
