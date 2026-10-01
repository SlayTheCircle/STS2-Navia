using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

// 操作真实游戏的模组条目与扫描缓存，不提供游戏模型扫描的替身。
internal sealed class GameProbeHost
{
    private const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;
    private readonly Assembly _game;
    private readonly Type _mod;
    private readonly Type _manager;
    private readonly Type _manifest;
    private readonly Type _reflection;

    public GameProbeHost(Assembly game, string target)
    {
        _game = game;
        _mod = GameType("Modding.Mod");
        _manager = GameType("Modding.ModManager");
        _manifest = GameType("Modding.ModManifest");
        _reflection = GameType("Helpers.ReflectionHelper");
        _manager.GetProperty("State")!.GetSetMethod(true)!.Invoke(null,
            [Enum.Parse(GameType("Modding.ModManagerState"), "Initialized")]);
        Type releaseType = GameType("Debug.ReleaseInfoManager");
        object release = RuntimeHelpers.GetUninitializedObject(releaseType);
        releaseType.GetField("<SemVer>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(release, Version(target));
        releaseType.GetField("_instance", PrivateStatic)!.SetValue(null, release);
    }

    private Type GameType(string name) => _game.GetType("MegaCrit.Sts2.Core." + name, true)!;

    private object Version(string text)
    {
        object?[] args = [text, null];
        bool success = (bool)GameType("Debug.SemanticVersion").GetMethod("TryFromString")!.Invoke(null, args)!;
        return success ? args[1]! : throw new ArgumentException("Invalid test version " + text);
    }

    public object NewMod(string id, string path, string version, string state)
    {
        object mod = Activator.CreateInstance(_mod)!;
        object manifest = Activator.CreateInstance(_manifest)!;
        _manifest.GetField("id")!.SetValue(manifest, id);
        _manifest.GetField("version")!.SetValue(manifest, version);
        _mod.GetField("manifest")!.SetValue(mod, manifest);
        _mod.GetField("path")!.SetValue(mod, path);
        _mod.GetField("version")!.SetValue(mod, Version(version));
        SetState(mod, state);
        ((IList)_manager.GetField("_mods", PrivateStatic)!.GetValue(null)!).Add(mod);
        return mod;
    }

    public void SetState(object mod, string state)
    {
        FieldInfo slot = _mod.GetField("state")!;
        slot.SetValue(mod, Enum.Parse(slot.FieldType, state));
    }

    public void Reset()
    {
        ((IList)_manager.GetField("_mods", PrivateStatic)!.GetValue(null)!).Clear();
        ResetScan();
        ProbeBoundaries.ResetDispatch();
    }

    public void ResetScan()
    {
        _reflection.GetField("_modTypes", PrivateStatic)!.SetValue(null, null);
        // 0.111.0 在 ModelDb 内另有一层类型缓存；独立用例不能沿用上一例的结果。
        GameType("Models.ModelDb").GetField("_allAbstractModelSubtypes", PrivateStatic)?.SetValue(null, null);
    }

    public Type[] ScanModels() => (Type[])GameType("Models.ModelDb")
        .GetProperty("AllAbstractModelSubtypes")!.GetValue(null)!;

    public void CompleteLoad(object mod, Assembly loader)
    {
        // 与两版 TryLoadMod 的初始化器后序一致：先保存壳、设 Loaded，再发事件。
        FieldInfo? legacy = _mod.GetField("assembly");
        if (legacy is not null)
        {
            legacy.SetValue(mod, loader);
        }
        else
        {
            ((IList)_mod.GetField("assemblies")!.GetValue(mod)!).Add(loader);
        }
        SetState(mod, "Loaded");
        PublishDetected(mod);
    }

    public void PublishDetected(object mod) =>
        ((Delegate?)_manager.GetField("OnModDetected", PrivateStatic)!.GetValue(null))?.DynamicInvoke(mod);

    public bool IsFailedWithErrors(object mod) =>
        _mod.GetField("state")!.GetValue(mod)!.ToString() == "Failed"
        && _mod.GetField("errors")!.GetValue(mod) is IList { Count: > 0 };
}
