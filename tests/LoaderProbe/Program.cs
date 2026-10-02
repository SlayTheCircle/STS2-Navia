using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;

if (args.Length != 6)
{
    Console.Error.WriteLine("Usage: LoaderProbe <game-refs> <game-runtime-dll-dir> <RitsuLib> <loader-dll> <content-0.107.1-dll> <content-0.111.0-dll>");
    return 2;
}
string[] inputs = args.Select(Path.GetFullPath).ToArray();
string target = JsonDocument.Parse(File.ReadAllText(Path.Combine(inputs[0], "release_info.json")))
    .RootElement.GetProperty("version").GetString()!.TrimStart('v');
Require(target is "0.107.1" or "0.111.0", "Probe only supports the two declared game targets");
string[] dependencyDirs = [inputs[0], Path.Combine(inputs[2], "compat", target), Path.Combine(inputs[2], "shared"), inputs[1]];
AssemblyLoadContext.Default.Resolving += (_, name) =>
{
    string? path = dependencyDirs.Select(dir => Path.Combine(dir, name.Name + ".dll")).FirstOrDefault(File.Exists);
    return path is null ? null : AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
};

string bundle = Path.Combine(Path.GetTempPath(), "navia-loader-probe-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(bundle);
try
{
    File.Copy(inputs[3], Path.Combine(bundle, "STS2-Navia.dll"));
    string[] targets = ["0.107.1", "0.111.0"];
    for (int i = 0; i < targets.Length; i++)
    {
        string directory = Path.Combine(bundle, "lib", "game-" + targets[i]);
        Directory.CreateDirectory(directory);
        File.Copy(inputs[i + 4], Path.Combine(directory, "STS2-Navia.dll"));
    }
    File.WriteAllText(Path.Combine(bundle, "mod-variants.manifest"), JsonSerializer.Serialize(new
    {
        schema = 1,
        variants = targets.Select(branch => new
        {
            modVersion = "0.1.5", minGameVersion = branch, directory = "lib/game-" + branch,
            dependencies = new[] { new { id = "STS2-RitsuLib", min_version = "0.6.2" } }
        })
    }));
    Assembly game = Assembly.LoadFrom(Path.Combine(inputs[0], "sts2.dll"));
    Assembly godot = Assembly.LoadFrom(Path.Combine(inputs[0], "GodotSharp.dll"));
    Assembly loader = Assembly.LoadFrom(Path.Combine(bundle, "STS2-Navia.dll"));
    ProbeBoundaries.Install(game, godot, loader);
    var host = new GameProbeHost(game, target);
    MethodInfo initialize = loader.GetType("NaviaMod.Loader.WorkshopBootstrap")!.GetMethod("Initialize")!;

    object owner = host.NewMod("STS2-Navia", bundle, "0.1.5", "None");
    host.NewMod("STS2-RitsuLib", bundle, "0.6.2", "Loaded");
    initialize.Invoke(null, null);
    Assembly content = ProbeBoundaries.DispatchedAssembly
        ?? throw new InvalidOperationException("Loader did not dispatch a content initializer");
    Require(content.Location == Path.Combine(bundle, "lib", "game-" + target, "STS2-Navia.dll"),
        "Loader selected the wrong game target");
    host.CompleteLoad(owner, loader);
    Type[] discovered = host.ScanModels();
    foreach (string model in new[] { "NaviaMod.Content.Characters.Navia", "NaviaMod.Content.Cards.StrikeNavia", "NaviaMod.Content.CardPools.NaviaCardPool", "NaviaMod.Content.Visuals.NaviaVisualHooks" })
    {
        Require(discovered.Any(type => type.FullName == model), "Game model scanner missing " + model);
    }
    Console.WriteLine($"PASS {target}: Loader selects the matching DLL; ModelDb sees {discovered.Count(type => type.Assembly == content)} Navia models after game completion");

    // 对照：程序集存在于进程但游戏只登记壳时，ModelDb 仍完全看不到内容。
    host.Reset();
    object baseline = host.NewMod("STS2-Navia", bundle, "0.1.5", "None");
    host.CompleteLoad(baseline, loader);
    Require(!host.ScanModels().Any(type => type.Assembly == content), "Loader-only baseline unexpectedly discovers content");
    Console.WriteLine($"PASS {target}: loader-only control reproduces missing content models");

    host.Reset();
    object rejected = host.NewMod("STS2-Navia", bundle, "0.1.5", "None");
    host.NewMod("STS2-RitsuLib", bundle, "0.6.1", "Loaded");
    initialize.Invoke(null, null);
    Require(ProbeBoundaries.DispatchedAssembly is null, "Loader invoked content despite an unsupported dependency version");
    host.CompleteLoad(rejected, loader);
    Require(host.IsFailedWithErrors(rejected), "Dependency rejection did not produce a failed mod entry");
    Require(!host.ScanModels().Any(type => type.Assembly == content), "Rejected mod leaked into game model discovery");
    Console.WriteLine($"PASS {target}: old RitsuLib is rejected before content dispatch and model discovery");
    return 0;
}
finally
{
    Directory.Delete(bundle, recursive: true);
}

static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
