using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Text.Json.Serialization;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace NaviaMod.Loader;

/// <summary>
/// 工坊变体引导壳:物品根目录的 STS2-Navia.dll 即本程序集。游戏调用 <see cref="Initialize"/> 后,
/// 按 mod-variants.manifest 选出与当前游戏版本最贴合的变体(≤host 的最高 minGameVersion),
/// 加载其程序集并把 <c>ModInitializer</c> 转发给真正的模组入口。
/// 资源包与游戏目标无关,由根目录 STS2-Navia.pck 走游戏原生挂载(has_pck=true)。
/// 变体目录只含 dll,限制在 lib/ 内;内容程序集还须登记到游戏模型扫描通道,
/// 0.107.1 仅转发初始化器与 RitsuLib 归属登记会导致启动时 ModelNotFoundException。
/// 依赖按变体各自校验并复用游戏原生 MOD_ERROR 呈现。
/// 多变体加载设计改自 STS2-RitsuLib(MIT,OLC),经 RandomForeseer 工坊壳验证;归属见 THIRD_PARTY_NOTICES。
/// </summary>
[ModInitializer("Initialize")]
public static class WorkshopBootstrap
{
    private sealed record VariantCandidate(
        string ModVersion,
        SemanticVersion ModSemanticVersion,
        string MinGameVersion,
        SemanticVersion MinGameSemanticVersion,
        string DllPath,
        IReadOnlyList<DependencyCandidate> Dependencies);

    private sealed record DependencyCandidate(string Id, string? MinVersion, SemanticVersion? MinSemanticVersion);

    private sealed class BundleManifest
    {
        public int Schema { get; set; }

        public List<BundleVariant>? Variants { get; set; }
    }

    private sealed class BundleVariant
    {
        public string? ModVersion { get; set; }

        public string? MinGameVersion { get; set; }

        public string? Directory { get; set; }

        public List<BundleDependency>? Dependencies { get; set; }
    }

    private sealed class BundleDependency
    {
        public string? Id { get; set; }

        [JsonPropertyName("min_version")]
        public string? MinVersion { get; set; }
    }

    private const string ModId = "STS2-Navia";
    private const string VariantManifestName = "mod-variants.manifest";
    private const string VariantDllName = "STS2-Navia.dll";

    public static void Initialize()
    {
        string? loaderDirectory = Path.GetDirectoryName(typeof(WorkshopBootstrap).Assembly.Location);
        if (string.IsNullOrWhiteSpace(loaderDirectory))
        {
            throw new InvalidOperationException("[Navia.Loader] 无法定位引导壳所在目录。");
        }
        List<VariantCandidate> variants = LoadVariants(loaderDirectory);
        SemanticVersion? hostVersion = ResolveHostVersion();
        VariantCandidate selected = SelectVariant(variants, hostVersion);
        Log.Info($"[Navia.Loader] host {hostVersion?.ToString() ?? "<未知>"} → 变体 {selected.ModVersion} (minGame {selected.MinGameVersion})。", 2);
        SynchronizeModManagerVersion(loaderDirectory, selected);
        if (!TryValidateDependencies(selected, out List<LocString> errors))
        {
            ReportDependencyFailure(errors);
            return;
        }
        Assembly assembly = (AssemblyLoadContext.GetLoadContext(typeof(WorkshopBootstrap).Assembly) ?? AssemblyLoadContext.Default)
            .LoadFromAssemblyPath(selected.DllPath);
        InvokeModInitializers(assembly);
        Mod owner = ModManager.Mods.Single(mod =>
            mod.manifest?.id == ModId && PathsEqual(mod.path, loaderDirectory));
        GameAssemblyRegistration.Register(owner, assembly);
    }

    /// <summary>把 ModManager 里的本模组条目改写为实际加载变体的版本——版本显示与本地/工坊去重规则随之正确。</summary>
    private static void SynchronizeModManagerVersion(string loaderDirectory, VariantCandidate selected)
    {
        Mod? entry = ModManager.Mods?.SingleOrDefault(mod =>
            mod.manifest?.id == ModId && PathsEqual(mod.path, loaderDirectory));
        if (entry?.manifest == null)
        {
            Log.Warn($"[Navia.Loader] 未找到本模组的 ModManager 条目:{loaderDirectory}", 2);
            return;
        }
        string? reported = entry.manifest.version;
        entry.manifest.version = selected.ModVersion;
        entry.version = selected.ModSemanticVersion;
        if (reported != selected.ModVersion)
        {
            Log.Info($"[Navia.Loader] 版本显示已同步:{reported ?? "<null>"} → {selected.ModVersion}", 2);
        }
    }

    private static List<VariantCandidate> LoadVariants(string loaderDirectory)
    {
        string manifestPath = Path.Combine(loaderDirectory, VariantManifestName);
        if (!File.Exists(manifestPath))
        {
            throw new FileNotFoundException("缺少工坊变体清单。", manifestPath);
        }
        BundleManifest bundle;
        try
        {
            bundle = JsonSerializer.Deserialize<BundleManifest>(File.ReadAllText(manifestPath),
                       new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? throw new InvalidDataException("工坊变体清单为空: " + manifestPath);
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("工坊变体清单解析失败: " + manifestPath, exception);
        }
        if (bundle.Variants is not { Count: > 0 })
        {
            throw new InvalidDataException("工坊变体清单不含任何变体: " + manifestPath);
        }
        string libRoot = Path.GetFullPath(Path.Combine(loaderDirectory, "lib"));
        return bundle.Variants.Select(entry => CreateCandidate(loaderDirectory, libRoot, entry)).ToList();
    }

    private static VariantCandidate CreateCandidate(string loaderDirectory, string libRoot, BundleVariant entry)
    {
        string? modVersion = entry.ModVersion?.Trim();
        if (string.IsNullOrWhiteSpace(modVersion) || !SemanticVersion.TryFromString(modVersion, out SemanticVersion? modSemVer)
            || modSemVer is null)
        {
            throw new InvalidDataException("非法变体模组版本: " + entry.ModVersion);
        }
        string? minGame = entry.MinGameVersion?.Trim();
        if (string.IsNullOrWhiteSpace(minGame) || !SemanticVersion.TryFromString(minGame, out SemanticVersion? gameSemVer)
            || gameSemVer is null)
        {
            throw new InvalidDataException("非法变体最低游戏版本: " + entry.MinGameVersion);
        }
        if (string.IsNullOrWhiteSpace(entry.Directory))
        {
            throw new InvalidDataException("变体 " + entry.ModVersion + " 缺少目录。");
        }
        string variantRoot = Path.GetFullPath(Path.Combine(loaderDirectory, entry.Directory));
        if (!IsUnderDirectory(variantRoot, libRoot))
        {
            throw new InvalidDataException("变体目录越出 lib/:" + entry.Directory);
        }
        List<DependencyCandidate> dependencies = (entry.Dependencies ?? new List<BundleDependency>())
            .Select(dependency => CreateDependencyCandidate(entry.ModVersion, dependency))
            .ToList();
        return new VariantCandidate(modVersion, modSemVer, minGame, gameSemVer,
            Path.Combine(variantRoot, VariantDllName), dependencies);
    }

    private static DependencyCandidate CreateDependencyCandidate(string? modVersion, BundleDependency dependency)
    {
        string? id = dependency.Id?.Trim();
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidDataException("变体 " + modVersion + " 存在缺少 id 的依赖。");
        }
        string? minVersion = dependency.MinVersion?.Trim();
        SemanticVersion? minSemVer = null;
        if (!string.IsNullOrWhiteSpace(minVersion))
        {
            if (!SemanticVersion.TryFromString(minVersion, out minSemVer) || minSemVer is null)
            {
                throw new InvalidDataException($"变体 {modVersion} 的依赖 {id} 非法最低版本:{minVersion}");
            }
        }
        return new DependencyCandidate(id, minVersion, minSemVer);
    }

    /// <summary>同模组版本双游戏目标:取「声明支持当前游戏」的变体里 minGameVersion 最高(最贴合)的一个。</summary>
    private static VariantCandidate SelectVariant(IReadOnlyCollection<VariantCandidate> variants, SemanticVersion? hostVersion)
    {
        if (hostVersion is not null)
        {
            VariantCandidate? match = variants
                .Where(candidate => candidate.MinGameSemanticVersion.CompareTo(hostVersion) <= 0)
                .OrderByDescending(candidate => candidate.MinGameSemanticVersion)
                .ThenByDescending(candidate => candidate.ModSemanticVersion)
                .FirstOrDefault();
            if (match is not null)
            {
                return match;
            }
            Log.Warn($"[Navia.Loader] 清单中没有支持游戏 {hostVersion} 的变体;回退到最高目标。", 2);
        }
        else
        {
            Log.Warn("[Navia.Loader] 无法确定游戏版本;回退到最高目标。", 2);
        }
        return variants
            .OrderByDescending(candidate => candidate.MinGameSemanticVersion)
            .ThenByDescending(candidate => candidate.ModSemanticVersion)
            .First();
    }

    private static bool TryValidateDependencies(VariantCandidate selected, out List<LocString> errors)
    {
        List<Mod> loaded = ModManager.GetLoadedMods().ToList();
        List<string> missing = new();
        errors = new List<LocString>();
        foreach (DependencyCandidate dependency in selected.Dependencies)
        {
            Mod? provider = loaded.FirstOrDefault(mod => mod.manifest?.id == dependency.Id);
            if (provider is null)
            {
                Log.Error($"[Navia.Loader] 变体 {selected.ModVersion} 缺少依赖 {dependency.Id}。", 2);
                missing.Add(dependency.Id);
            }
            else if (dependency.MinSemanticVersion is not null)
            {
                if (provider.manifest?.version is null)
                {
                    errors.Add(DependencyVersionError("MOD_ERROR.DEPENDENCY_VERSION_MISSING", dependency, null));
                }
                else if (provider.version is null)
                {
                    errors.Add(DependencyVersionError("MOD_ERROR.DEPENDENCY_INVALID_VERSION", dependency, provider.manifest.version));
                }
                else if (provider.version.CompareTo(dependency.MinSemanticVersion) < 0)
                {
                    errors.Add(DependencyVersionError("MOD_ERROR.DEPENDENCY_VERSION_UNSUPPORTED", dependency, provider.manifest.version));
                }
            }
        }
        if (missing.Count > 0)
        {
            LocString missingError = new("main_menu_ui", "MOD_ERROR.MISSING_DEPENDENCY");
            missingError.Add("id", ModId);
            missingError.Add("missingCount", (decimal)missing.Count);
            missingError.Add("missingDependencies", string.Join(",", missing));
            errors.Add(missingError);
        }
        return errors.Count == 0;
    }

    private static LocString DependencyVersionError(string localizationKey, DependencyCandidate dependency, string? installedVersion)
    {
        LocString error = new("main_menu_ui", localizationKey);
        error.Add("id", ModId);
        error.Add("dependency", dependency.Id);
        error.Add("minVersion", dependency.MinVersion ?? "<null>");
        if (installedVersion is not null)
        {
            error.Add("version", installedVersion);
        }
        return error;
    }

    /// <summary>失败信息经 OnModDetected 延迟挂到本模组条目上,复用游戏 Mods 菜单的原生错误呈现。</summary>
    private static void ReportDependencyFailure(List<LocString> errors)
    {
        ModManager.OnModDetected += OnModDetected;
        return;
        void OnModDetected(Mod mod)
        {
            if (mod.manifest?.id != ModId)
            {
                return;
            }
            ModManager.OnModDetected -= OnModDetected;
            try
            {
                mod.state = ModLoadState.Failed;
                (mod.errors ??= new List<LocString>()).AddRange(errors);
            }
            catch (Exception exception)
            {
                Log.Error("[Navia.Loader] 写入依赖错误失败: " + exception, 2);
            }
        }
    }

    private static SemanticVersion? ResolveHostVersion()
    {
        try
        {
            return ReleaseInfoManager.Instance.SemVer;
        }
        catch (Exception exception)
        {
            Log.Warn("[Navia.Loader] 读取游戏版本失败: " + exception.Message, 2);
            return null;
        }
    }

    private static void InvokeModInitializers(Assembly assembly)
    {
        List<Type> initializers = assembly.GetTypes()
            .Where(type => type.GetCustomAttribute<ModInitializerAttribute>() is not null)
            .ToList();
        if (initializers.Count == 0)
        {
            throw new InvalidOperationException("变体程序集缺少 ModInitializer: " + assembly.FullName);
        }
        foreach (Type type in initializers)
        {
            ModInitializerAttribute attribute = type.GetCustomAttribute<ModInitializerAttribute>()!;
            MethodInfo? method = type.GetMethod(attribute.initializerMethod,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (method is null)
            {
                throw new MissingMethodException(type.FullName, attribute.initializerMethod);
            }
            method.Invoke(null, null);
        }
    }

    private static bool IsUnderDirectory(string path, string root)
    {
        string normalizedRoot = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        string normalizedPath = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        return normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static bool PathsEqual(string left, string right)
    {
        StringComparison comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        return string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(left)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(right)),
            comparison);
    }
}
