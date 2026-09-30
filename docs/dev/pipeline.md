# 构建、部署与素材管线

## 配置与引用

工具版本和本机变量见 [贡献指南](../../CONTRIBUTING.md)。脚本加载根目录可信 `.local-dev.env`；公开示例不含实际本机路径。游戏引用从 GAME_DIR 复制到 GAME_REFS_DIR，RitsuLib 包通过 RITSULIB_DIR 选择。游戏引用版本与 RITSULIB_TARGET 不一致时中止。花名册默认读取公开的 docs/history/design/card-roster.txt；可用 ROSTER_DESIGN_FILE 检查另一卡表，不设置时无需任何私有文件。

```bash
./scripts/restore-refs.sh [游戏安装目录]
./scripts/fetch-godot.sh
./scripts/doctor.sh
```

Godot 自动下载脚本将 Linux x86_64 标准版放到不追踪的 .tools 目录。已有共享工具可直接配置 GODOT_EXE；SDK 版本由 global.json 选择，DOTNET_EXE 指向已有安装。

## VSCode／WSL 编辑器环境

开发脚本读取 .local-dev.env 后可以用 DOTNET_EXE 的绝对路径构建。编辑器的 C# 项目服务不会自动执行该 Bash 文件，必须能从自身环境找到 SDK；终端里构建成功不能证明扩展宿主的 SDK 发现正常。

手工安装 SDK 时，将安装根目录加入 PATH，并设置 DOTNET_ROOT。例如 SDK 安装在用户目录的 .dotnet 下，可在本机环境配置中使用：

```sh
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"
```

这些命令用于对应安装位置，其他安装方式按实际路径设置；说明见 [Microsoft 手工安装文档](https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual)。VSCode Remote 在 WSL 启动时不加载普通 shell 启动文件，应使用其用户级 server-env-setup 启动配置，并按 Bourne shell 语法维护。修改后需重启远程 Server，单独改终端 PATH 或重载编辑器窗口不能保证更新现有 Server 的环境，见 [VSCode WSL 环境说明](https://code.visualstudio.com/docs/remote/wsl#_advanced-environment-setup-script)。

C# 扩展自身使用的 .NET Runtime 与项目所需 SDK 分别发现。dotnetAcquisitionExtension.existingDotnetPath 用于扩展运行宿主，不应将它当作项目 SDK 选择器；项目 SDK 仍按 global.json 与可发现安装选择。

编辑器直接加载 csproj 时，游戏引用与 RitsuLib 同样必须可见。可在本机环境设置 GAME_REFS_DIR／RITSULIB_DIR，或准备 csproj 的默认 libs/game／libs/RitsuLib。Linux／WSL 可在 libs/RitsuLib 尚不存在时，用本地符号链接指向已准备的完整包；此目录被整体忽略，真实路径不进入公开文件。不要将项目专用引用变量写成所有工作区共享的固定值。

遇到 SDK 错误先在对应 WSL 环境运行 dotnet --list-sdks 与 dotnet --version，再核对项目是否能直接评估；遇到 MSB4019 或缺程序集时继续检查依赖路径。分别报告 SDK 发现、项目加载与实际编译的结果。

## 分层构建

```bash
./scripts/check.sh --source-only  # Git 边界、文档、公开卡表、本地化和文本资源
./scripts/build.sh --dll-only    # 当前游戏引用下编译 DLL；不生成安装目录
./scripts/prep-art.sh            # ART_SOURCE_DIR 母版 → assets 成品
./scripts/check.sh --full        # 源码检查 + 环境 + 完整构建
./scripts/build.sh               # 完整素材检查、编译、导入、打包和 PCK 验证
./scripts/verify-pck.sh           # 检查已有 PCK 的代表性纹理和本地化
./scripts/package.sh             # 重建完整包，再按清单／游戏目标版本生成内部 ZIP
```

源码检查不依赖游戏 DLL、Godot 或多媒体。完整构建先检查素材和引用，再在临时目录组装，经 PCK 验证成功后复制到 mods-dist/STS2-Navia/。入口必须存在。导入失败会输出诊断并中止。编译和素材检查不能替代实际游戏验收。

## PCK 布局

- 本地化：res://STS2-Navia/localization/语言/表.json。
- 自有资源：assets/STS2-Navia/ → res://STS2-Navia/。
- 全局资源：assets/global/ → res://；当前用于纪元立绘。
- 带 .import 的源图不重复入包；侧车与 res://.godot/imported/ 纹理一起分发。
- .tscn 文本场景引用游戏本体 C# 类，独立 Godot 只验证纹理与包数据，完整角色场景仍需游戏加载。

## 部署与分诊

```bash
./scripts/deploy.sh [游戏安装目录]
./scripts/triage-log.sh [游戏日志路径]
```

当前部署脚本服务于 WSL 下的 Windows 游戏，依赖 TASKLIST_EXE。游戏运行或进程检测失败时拒绝部署。文件占用则中止；不得改名让路。缺日志时只能报告未取得证据，不能据此认定无错误。

双版本发行尚未实现。准备游戏 0.107.1 变体时分别恢复引用、选对应 RitsuLib compat、编译并试玩验收，发布布局与流程另行落实。
