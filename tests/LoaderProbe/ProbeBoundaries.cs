using System.Reflection;
using HarmonyLib;

// 仅供独立验证进程：隔离 Godot 原生日志和内容初始化，保留 Loader 的选择、
// 依赖检查、程序集登记及真实 ReflectionHelper/ModelDb 类型扫描。
internal static class ProbeBoundaries
{
    public static Assembly? DispatchedAssembly { get; private set; }

    public static void Install(Assembly game, Assembly godot, Assembly loader)
    {
        var harmony = new Harmony("navia.loader.probe.boundaries");
        Patch(godot.GetType("Godot.OS")!.GetMethod("GetCmdlineArgs")!, nameof(CommandLine));
        Patch(godot.GetType("Godot.OS")!.GetMethod("HasFeature")!, nameof(Feature));
        Patch(game.GetType("MegaCrit.Sts2.Core.Logging.ConsoleLogPrinter")!.GetMethod("Print")!, nameof(SkipPrint));
        Patch(loader.GetType("NaviaMod.Loader.WorkshopBootstrap")!.GetMethod("InvokeModInitializers",
            BindingFlags.Static | BindingFlags.NonPublic)!, nameof(RecordDispatch));

        void Patch(MethodInfo method, string prefix) =>
            harmony.Patch(method, prefix: new HarmonyMethod(typeof(ProbeBoundaries), prefix));
    }

    public static void ResetDispatch() => DispatchedAssembly = null;

    public static bool CommandLine(ref string[] __result)
    {
        __result = [];
        return false;
    }

    public static bool Feature(ref bool __result)
    {
        __result = false;
        return false;
    }

    public static bool SkipPrint() => false;

    public static bool RecordDispatch(Assembly assembly)
    {
        DispatchedAssembly = assembly;
        return false;
    }
}
