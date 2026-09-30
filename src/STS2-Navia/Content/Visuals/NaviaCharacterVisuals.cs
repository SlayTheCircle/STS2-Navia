using System;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace NaviaMod.Content.Visuals;

/// <summary>
/// 娜维娅战斗形象的四态 2D 驱动(无 Spine 路线):按生物状态换装纹理 + 待机浮动 + 受击/出牌动效。
/// 接线模式参考 Hikari 同类实现;生命周期钩子挂在常驻的初始遗物刺玫会徽上。
/// 场景节点契约:navia_character.tscn 根挂 NCreatureVisuals,子节点 Sprite2D 名为 "Visuals"。
/// </summary>
public static class NaviaCharacterVisuals
{
    private const string SpritePath = "Visuals";

    private const string NormalPath = "res://STS2-Navia/images/characters/navia_normal.png";
    private const string SkillPath = "res://STS2-Navia/images/characters/navia_skill.png";
    private const string HitPath = "res://STS2-Navia/images/characters/navia_hit.png";
    private const string DownPath = "res://STS2-Navia/images/characters/navia_down.png";

    private const float GroundY = 14f;
    private const float ContactRatio = 0.96f;
    private const float BaseScale = 0.22f;
    private const float FloatAmplitude = 5f;
    private const float FloatDuration = 1.15f;
    private const float SkillPoseDuration = 0.35f;

    private static readonly Vector2 HurtOffset = new(-5f, 6f);

    private const string MetaRestPos = "navia_rest_pos";
    private const string MetaFloatTween = "navia_float_tween";
    private const string MetaPoseTween = "navia_pose_tween";

    public static bool IsMainThread => OS.GetThreadCallerId() == OS.GetMainThreadId();

    /// <summary>预热四态纹理:在无关卡画面的时机(开局获得初始遗物时)同步加载进缓存,消除战斗入场的白板延迟。</summary>
    public static void Prewarm()
    {
        if (!IsMainThread)
        {
            return;
        }
        _ = ResourceLoader.Load<Texture2D>(NormalPath);
        _ = ResourceLoader.Load<Texture2D>(SkillPath);
        _ = ResourceLoader.Load<Texture2D>(HitPath);
        _ = ResourceLoader.Load<Texture2D>(DownPath);
    }

    /// <summary>按状态换装并重摆位:死亡→倒下图,存活→常规图 + 恢复浮动。</summary>
    public static void Refresh(Creature? creature)
    {
        if (!IsMainThread || creature == null)
        {
            return;
        }
        Sprite2D? sprite = GetSprite(creature);
        if (sprite == null)
        {
            return;
        }
        Texture2D? texture = ResourceLoader.Load<Texture2D>(creature.IsAlive ? NormalPath : DownPath);
        if (texture == null)
        {
            return;
        }
        KillPoseTween(sprite);
        if (sprite.Texture != texture)
        {
            sprite.Texture = texture;
        }
        // 摆位:脚底贴在 GroundY,按纹理高度算接触点(横版倒下图同样适用)。
        float half = texture.GetHeight() * 0.5f;
        Vector2 rest = new(sprite.Position.X, GroundY - (ContactRatio * texture.GetHeight() - half) * BaseScale);
        // 夹击(SurroundedPower)翻转的就是本 Sprite 的 Scale.X 符号(NCreature.Body=%Visuals 即此节点):
        // 归一化幅值时必须保留符号,否则每次换装都会把朝后状态拉回正面(螃蟹战倒下立绘事故 2026-09-30)。
        float facing = sprite.Scale.X < 0f ? -1f : 1f;
        sprite.Scale = new Vector2(BaseScale * facing, BaseScale);
        sprite.Position = rest;
        sprite.SetMeta(MetaRestPos, rest);
        if (creature.IsAlive)
        {
            StartFloat(sprite);
        }
        else
        {
            StopFloat(sprite);
        }
    }

    /// <summary>受击:切受击图 + 位移抖动,随后回到常规图并恢复浮动。</summary>
    public static void PlayHurt(Creature? creature)
    {
        if (!IsMainThread || creature == null)
        {
            return;
        }
        Sprite2D? sprite = GetSprite(creature);
        if (sprite == null || !sprite.HasMeta(MetaRestPos) || !creature.IsAlive)
        {
            return;
        }
        Texture2D? hit = ResourceLoader.Load<Texture2D>(HitPath);
        Vector2 rest = sprite.GetMeta(MetaRestPos).AsVector2();
        StopFloat(sprite);
        KillPoseTween(sprite);
        if (hit != null)
        {
            sprite.Texture = hit;
        }
        Tween tween = sprite.CreateTween();
        sprite.SetMeta(MetaPoseTween, tween);
        tween.TweenProperty(sprite, "position", rest + HurtOffset, 0.05f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        tween.TweenProperty(sprite, "position", rest, 0.16f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        tween.TweenCallback(Callable.From(() =>
        {
            if (IsMainThread && GodotObject.IsInstanceValid(sprite) && sprite.IsInsideTree())
            {
                Refresh(creature);
            }
        }));
    }

    /// <summary>出牌:短暂切技能姿态图,随后回常规图恢复浮动。</summary>
    public static void PlaySkillPose(Creature? creature)
    {
        if (!IsMainThread || creature == null || !creature.IsAlive)
        {
            return;
        }
        Sprite2D? sprite = GetSprite(creature);
        if (sprite == null || !sprite.HasMeta(MetaRestPos))
        {
            return;
        }
        Texture2D? skill = ResourceLoader.Load<Texture2D>(SkillPath);
        if (skill == null)
        {
            return;
        }
        StopFloat(sprite);
        KillPoseTween(sprite);
        sprite.Texture = skill;
        Tween tween = sprite.CreateTween();
        sprite.SetMeta(MetaPoseTween, tween);
        tween.TweenInterval(SkillPoseDuration);
        tween.TweenCallback(Callable.From(() =>
        {
            if (IsMainThread && GodotObject.IsInstanceValid(sprite) && sprite.IsInsideTree())
            {
                Refresh(creature);
            }
        }));
    }

    private static void StartFloat(Sprite2D sprite)
    {
        if (!IsMainThread || !sprite.HasMeta(MetaRestPos) || !sprite.IsInsideTree())
        {
            return;
        }
        if (sprite.HasMeta(MetaFloatTween))
        {
            Tween? existing = sprite.GetMeta(MetaFloatTween).As<Tween>();
            if (existing != null && existing.IsValid())
            {
                return;
            }
        }
        Vector2 rest = sprite.Position = sprite.GetMeta(MetaRestPos).AsVector2();
        Tween tween = sprite.CreateTween().SetLoops();
        tween.TweenProperty(sprite, "position:y", rest.Y - FloatAmplitude, FloatDuration).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        tween.TweenProperty(sprite, "position:y", rest.Y + FloatAmplitude, FloatDuration).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
        sprite.SetMeta(MetaFloatTween, tween);
    }

    private static void StopFloat(Sprite2D sprite)
    {
        if (sprite.HasMeta(MetaFloatTween))
        {
            Tween? tween = sprite.GetMeta(MetaFloatTween).As<Tween>();
            if (tween != null && tween.IsValid())
            {
                tween.Kill();
            }
            sprite.RemoveMeta(MetaFloatTween);
        }
    }

    private static void KillPoseTween(Sprite2D sprite)
    {
        if (sprite.HasMeta(MetaPoseTween))
        {
            Tween? tween = sprite.GetMeta(MetaPoseTween).As<Tween>();
            if (tween != null && tween.IsValid())
            {
                tween.Kill();
            }
            sprite.RemoveMeta(MetaPoseTween);
        }
    }

    private static Sprite2D? GetSprite(Creature creature)
    {
        NCombatRoom? room = NCombatRoom.Instance;
        NCreature? node = room?.GetCreatureNode(creature);
        return node?.Visuals?.GetNodeOrNull<Sprite2D>(SpritePath);
    }
}
