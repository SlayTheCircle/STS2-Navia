# Loader 模型发现验证

本验证保护实际启动契约：内容 DLL 被加载并转发初始化器后，还必须进入游戏的模型类型扫描。遗漏登记、在旧版初始化器内提前写单程序集槽（随后被游戏覆盖），都会使角色进入 RitsuLib 注册表却缺席 ModelDb，启动时报 ModelNotFoundException。另一项验证保护变体依赖下限：旧 RitsuLib 不得触发内容初始化或进入模型扫描。

验证进程加载真实游戏、Loader 和两份内容 DLL，调用实际 WorkshopBootstrap.Initialize 与 ModelDb.AllAbstractModelSubtypes。游戏加载完成时的程序集写入与 OnModDetected 顺序由宿主显式重放；Godot 原生日志、游戏版本文件读取和内容初始化边界被隔离。因此本验证覆盖变体选择、依赖拒绝及游戏模型扫描接线，不覆盖 PCK、RitsuLib 内容初始化、模型实例化或真实游戏启动。

## 输入与执行

先运行 scripts/build-loader.sh，并分别用 scripts/build.sh --dll-only 编译两个目标、在下一次构建前保存各自 DLL。游戏引用目录必须含 sts2.dll、GodotSharp.dll 与 release_info.json；运行时 DLL 目录须包含该目标的附加依赖（例如 Steamworks.NET、Sentry）。缺依赖时报失败，不计通过。

从仓库根执行，参数使用自己的绝对路径或仓库相对路径：

```bash
source scripts/dev-env.sh
"$DOTNET_EXE" build tests/LoaderProbe/LoaderProbe.csproj -c Release
"$DOTNET_EXE" tests/LoaderProbe/bin/Release/net9.0/LoaderProbe.dll \
  '<目标游戏引用目录>' '<同目标游戏运行时 DLL 目录>' "$RITSULIB_DIR" \
  mods-dist/loader/STS2-Navia.dll '<0.107.1 内容 DLL>' '<0.111.0 内容 DLL>'
```

对 0.107.1、0.111.0 分别启动一个验证进程；使用同一份按 0.107.1 编译的 Loader。进程按 release_info.json 选目标，并断言实际转发的 DLL 路径。它核对角色、初始打击和卡池等真实模型，输出扫描到的模型数但不把固定数量作为断言。

只登记壳的对照组必须复现内容模型完全缺失。低于清单下限的依赖必须在转发初始化器前被拒绝，且加载完成后留下带错误的 Failed 条目。验证不改游戏安装、不挂载 PCK、不部署或上传。
