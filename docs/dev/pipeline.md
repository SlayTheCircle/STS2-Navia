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
./scripts/package.sh             # 重建完整包，再按清单／游戏目标版本生成候选 ZIP（NAVIA_PACKAGE_CHANNEL=release 为公开措辞）
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

## 发行工作流

tag `v*` 推送触发 [Release 工作流](../../.github/workflows/release.yml)：托管 runner 检出私有构建输入仓 circle-refs（`game-refs/<版本>/` 编译参考与 RitsuLib 镜像，组织私有）和美术母版仓，按清单 min_game_version 选择目标目录完整构建，产出候选 ZIP 与校验值并创建草稿 Release；发行说明取自 [CHANGELOG](../../CHANGELOG.md) 对应版本段落。main 推送另有 [Compile 工作流](../../.github/workflows/compile.yml) 做 DLL 编译验证。两者依赖的只读凭据 REFS_TOKEN 存于仓库 Actions secrets；fork PR 拿不到该凭据，只会运行无 secret 的源码检查。

Steam 工坊发布为本地手工步（`local_dev/workshop/publish.sh`，私有）：从 Release 工件取内容经 SteamCMD 上传至固定物品，描述单源 `description.bbcode` 使用真实换行——steamcmd 的 VDF 不解析 `\n` 转义，会按字面透传。

双版本编译已适配：源码以 `NAVIA_GAME_0107_1` 条件编译吸收 0.107.1 与 0.111.0 的 API 差异（伤害加成签名、卡牌去向钩子、克隆 API、LoseBlock，以及 `Content/Compat/` 下注入游戏命名空间的 FromCard／CardPlay.GetPlayer 垫片），csproj 按 `RITSULIB_TARGET` 自动注入 define。双目标回归：分别以 `RITSULIB_TARGET=0.107.1 GAME_REFS_DIR=<0.107.1 引用>` 与默认环境跑 `build.sh --dll-only`，两目标 0 错误且警告剖面一致方为通过。注意声明层错误会让编译在方法体绑定前中止——必须迭代到零错误，以警告回归确认绑定完整。变体候选布局与游戏内状态见 [STATUS](../../STATUS.md)。

Loader 按两版共有的 API 下限 0.107.1 编译：`scripts/build-loader.sh` 输出到 `mods-dist/loader/`，不刷新已有工坊目录或 ZIP。构建变体包时须重新执行 package.sh；部署按工作流中的授权范围执行。游戏程序集登记规则见[架构](architecture.md#变体-loader-与游戏模型发现)。[Loader 验证](../../tests/LoaderProbe/README.md) 使用真实游戏 DLL 检查选择、依赖拒绝与模型发现，仍须用实际游戏复验完整初始化、资源和菜单。

### 单目标安装清单

完整构建的平铺安装目录只承诺当前编译目标：0.107.1 DLL 配 0.107.1 最低版本，0.111.0 DLL 配 0.111.0 最低版本。`build_support/manifest.py` 与双目标打包共用生成规则，不改写根清单。`--dll-only` 不更新安装目录；需要部署时须先完成对应目标的完整构建。工坊布局仍由根 Loader 选择双变体；部署脚本仍只消费平铺目录。
