# 参与贡献

公开开发规范见 [docs/dev/README.md](docs/dev/README.md)，原案与实现差异见[设计资料](docs/history/design/README.md)。原创软件采用 [MIT](LICENSE)，许可范围与第三方权利见 [LICENSING.md](LICENSING.md)。文档与私有资料边界见[文档规范](docs/dev/documentation.md)。

## 环境与路径

开发脚本使用 Bash，当前验证环境为 Linux / WSL。.NET SDK 由 `global.json` 选择；当前基线 9.0.318，允许同一 9.0.3xx feature band 的更新补丁。Godot 使用 4.5.1 标准版，负责资源导入和 PCK 打包。

将 `.local-dev.env.example` 复制为 `.local-dev.env`，填写实际路径。它是由开发脚本执行的可信 Bash 配置，请只使用自己维护的本机文件。路径应为绝对路径；参数优先于进程环境，进程环境优先于本机配置，本机配置优先于仓库默认值。

| 变量 | 用途 | 默认值 |
|---|---|---|
| `DOTNET_EXE` | .NET 可执行文件 | PATH 中的 `dotnet` |
| `GODOT_EXE` | Godot 标准版可执行文件 | 仓库 `.tools/godot/4.5.1/` 下的 Linux x86_64 二进制 |
| `GAME_DIR` | 游戏安装目录 | 无；使用前必须提供 |
| `GAME_LOG` | 试玩日志 | 无；分诊前必须提供 |
| `GAME_REFS_DIR` | 游戏编译引用 | 仓库 `libs/game/` |
| `RITSULIB_DIR` | 完整 RitsuLib 依赖包 | 仓库 `libs/RitsuLib/` |
| `RITSULIB_TARGET` | RitsuLib compat 对应游戏版本 | `0.111.0` |
| `ART_SOURCE_DIR` | 美术母版目录（组织私有美术仓的克隆） | 无；重生成素材前必须提供 |
| `ROSTER_DESIGN_FILE` | 卡表审计输入覆盖 | 仓库 `docs/history/design/card-roster.txt` |
| `TASKLIST_EXE` | Windows 进程检测工具 | PATH 中的 `tasklist.exe` |

公开源码检查只需 Bash、Git 和 Python 3.11+。编译还需要 .NET、游戏引用和 RitsuLib；完整打包需要本地多媒体、Godot 和 `strings`；美术再生成另需 ImageMagick。自动下载 Godot 的脚本支持 Linux x86_64；其他平台手动安装后配置可执行文件。本项目提供 Bash／WSL 开发脚本，WSL 部署使用 Windows `tasklist.exe` 检测游戏进程。平台验证结果记录在 STATUS。

VSCode 的 C# 项目服务通过扩展宿主环境读取 SDK 和引用配置。SDK 必须在扩展宿主的 PATH 可见，项目引用也要通过它的环境或默认 libs 路径可读；WSL 的 Server 环境与普通终端分别配置。配置与排错见[编辑器环境](docs/dev/pipeline.md#vscodewsl-编辑器环境)。

```bash
./scripts/check.sh --source-only
./scripts/restore-refs.sh
./scripts/build.sh --dll-only
./scripts/doctor.sh
./scripts/check.sh --full
```

游戏引用版本与 `RITSULIB_TARGET` 必须一致。完整 RitsuLib 包中的 props 会选择同版本 compat DLL 和共享 DLL。直接使用 .NET CLI 时，可在 Bash 中先 `source scripts/dev-env.sh`，再执行 `"$DOTNET_EXE" build src/STS2-Navia/STS2-Navia.csproj -c Release`；需要从仓库根执行，确保 `global.json` 生效。

## 本地导航与仓库边界

本机工作区的导航入口是私有文件 `local_dev/README.md`；它再指向实际游戏源码、依赖、参考 Mod、工具和未整理原件。`local_dev/` 由 Git 整体忽略。有公开价值的设计、调查和技术案例已提取到 docs 的对应模块；贡献者可直接使用公开资料与本页配置完成开发起步。

公开花名册检查默认读取仓库中的设计卡表。只有检查其他卡表时才设置 ROSTER_DESIGN_FILE；指定文件不存在或解析不到条目会失败。它核对名称与内容覆盖；费用、数值、升级和剧情由对应设计审查与试玩确认。

源码仓维护程序、本地化、文本场景、导入工程配置、规格与生成脚本。游戏引用、依赖和构建产物在本地准备；完整打包通过 `ART_SOURCE_DIR` 接入组织私有美术仓。

## 修改与提交

提供原创软件贡献时采用本项目 MIT 许可，并确认自己有权授予相应权利。第三方代码保留适用的原版权与许可声明，注明来源；素材按各自授权处理，提交时提供明确的来源与许可记录。

遵循 [工程规范](docs/dev/style.md)、[验证纪律](docs/dev/testing.md)和[工作流](docs/dev/workflow.md)。一个变更承载一个主要意图；行为变化同步更新对应权威文档。当前 AI 协作只有收到明确指令才 commit、push、开 PR、合并、打 tag 或重写历史。

结构评审落实模块化与解耦：不新增或扩张上帝文件、上帝类或上帝函数；目录内平铺文件增多时优先按职责建立子目录。移动内容同时维护审计扫描、注册和资源契约，详见工程规范与架构说明。

提交采用 Conventional Commits：`type(scope): summary`，一律使用英文；正文说明动机和必要证据，主题与正文分别经 `-m` 提交。常用类型为 `feat`、`fix`、`docs`、`refactor`、`test`、`chore`；scope 可用 `cards`、`powers`、`events`、`timeline`、`assets`、`build`、`docs`。CHANGELOG 面向用户描述变化；开发草稿保存在私有工作区，发布前汇总。

PR 说明触发条件、行为变化、验证命令和未验证部分。完成 Issue 使用 `Closes #<issue>`，关联工作使用 `Refs #<issue>`。尚未配置远程时，先完成可本地审查的结果。

## 文档与 hooks

Markdown 段落和列表项保持自然换行；文本使用 UTF-8 / LF。README 中英同步更新，开发文档以中文为主。当前状态只在 STATUS 维护，安装入口在 README，长期规则在对应开发文档。变更交付前搜索旧术语、版本与路径，修正过时说明。

```bash
git config core.hooksPath .githooks
```

pre-commit 执行源码检查，CI 按工作流运行审计、编译或发行构建。完整素材、PCK 与游戏试玩结果统一记录在 STATUS，验证步骤见验证指南。
