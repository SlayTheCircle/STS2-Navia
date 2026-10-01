using System;
using System.Reflection;
using MegaCrit.Sts2.Core.Modding;

namespace NaviaMod.Loader;

/// <summary>让游戏原生模型扫描发现内容变体；RitsuLib 的归属登记不能替代这一步。</summary>
internal static class GameAssemblyRegistration
{
    public static void Register(Mod owner, Assembly contentAssembly)
    {
        MethodInfo? associate = typeof(ModManager).GetMethod("AssociateAssemblyWithMod",
            BindingFlags.Public | BindingFlags.Static, new[] { typeof(string), typeof(Assembly) });
        if (associate is not null)
        {
            // 0.111.0 使用多程序集列表，同时由原生 API 维护 AssemblyInfo 的归属。
            associate.Invoke(null, new object[] { owner.manifest!.id!, contentAssembly });
            return;
        }

        RegisterLegacy(owner, contentAssembly);
    }

    private static void RegisterLegacy(Mod owner, Assembly contentAssembly)
    {
        // 0.107.1 只有单个 assembly 槽。TryLoadMod 在初始化器返回后才写入壳程序集，
        // 必须在随后的 OnModDetected 中换为内容程序集，否则提前写入会被覆盖。
        // 用反射隔离旧字段，避免共用 Loader 在新游戏上绑定已移除的 Mod.assembly。
        FieldInfo assemblySlot = typeof(Mod).GetField("assembly", BindingFlags.Public | BindingFlags.Instance)
            ?? throw new MissingFieldException(typeof(Mod).FullName, "assembly");
        ModManager.OnModDetected += OnModDetected;

        void OnModDetected(Mod detected)
        {
            if (!ReferenceEquals(detected, owner))
            {
                return;
            }
            ModManager.OnModDetected -= OnModDetected;
            if (detected.state == ModLoadState.Loaded && detected.errors is not { Count: > 0 })
            {
                assemblySlot.SetValue(detected, contentAssembly);
            }
        }
    }
}
