using System.Runtime.Loader;

if (args.Length != 4)
{
    Console.Error.WriteLine("Usage: CombatProbe <game-refs> <runtime-dll-dir> <RitsuLib> <target>");
    return 2;
}
string[] dirs = [Path.GetFullPath(args[0]), Path.GetFullPath(args[1]),
    Path.GetFullPath(Path.Combine(args[2], "compat", args[3])), Path.GetFullPath(Path.Combine(args[2], "shared"))];
AssemblyLoadContext.Default.Resolving += (_, name) =>
{
    string? path = dirs.Select(dir => Path.Combine(dir, name.Name + ".dll")).FirstOrDefault(File.Exists);
    return path is null ? null : AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
};
await CombatCases.Run();
Console.WriteLine($"PASS {args[3]}: combat regression probe");
return 0;
