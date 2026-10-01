# STS2-Navia · 娜维娅角色 Mod

[English](README.en.md)

《杀戮尖塔 2》的娜维娅角色模组，以装填、礼炮轰鸣、摩拉与支援构筑为主题。0.1.6 已发布（GitHub Release 与 Steam 工坊），支持游戏 0.107.1（稳定版）与 0.111.0（测试版）双分支。当前实现与限制见 [STATUS.md](STATUS.md)。

## 获取与安装

当前仓库提供源码、双语本地化、文本资源配置及整理后的公开设计资料。推荐经 [Steam 工坊](https://steamcommunity.com/sharedfiles/filedetails/?id=3810999936) 订阅安装，并一并订阅 [RitsuLib](https://steamcommunity.com/sharedfiles/filedetails/?id=3747602295)——工坊物品按当前游戏版本自动选择内容，切换分支无需换装；也可从 [GitHub Releases](https://github.com/SlayTheCircle/STS2-Navia/releases) 下载对应游戏目标（0.107.1／0.111.0）的安装包，将其中 `STS2-Navia/` 目录放入游戏 `mods/`。安装或覆盖前关闭游戏；保存原版本包，便于回退。依赖最低版本以 [模组清单](STS2-Navia.json) 为准。源码检出中不包含多媒体，单独编译 DLL 不构成可安装的完整包。

## 开发

前置：Bash、Python 3.11+、由 `global.json` 选择的 .NET SDK。完整素材构建还需要 Godot 4.5.1 标准版及本地美术。游戏引用来自贡献者自己安装的对应游戏分支；RitsuLib 需要完整的 `compat/`、`shared/` 和 `RitsuLib.References.props`。

```bash
cp .local-dev.env.example .local-dev.env
# 在本机配置中填写依赖和工具位置
./scripts/check.sh                 # 源码检查，不要求游戏 DLL 或美术
./scripts/restore-refs.sh          # 从 GAME_DIR 复制对应分支编译引用
./scripts/build.sh --dll-only      # 只编译 DLL
./scripts/check.sh --full          # 完整素材、编译、PCK 检查
```

详细配置、平台范围和构建步骤见 [贡献指南](CONTRIBUTING.md)及[构建管线](docs/dev/pipeline.md)。本地开发工作区存在时，先阅读私有入口 `local_dev/README.md`；该文件不随仓库分发，负责导航到实际本机资源。

## 文档

- [文档导航](docs/README.md)：用户与开发者入口。
- [开发规范](docs/dev/README.md)：架构、内容写法、素材、验证与交付。
- [设计资料](docs/design/README.md)：原案、卡表、升级参考、实现差异与美术需求。
- [技术历史](docs/history/README.md)：排错案例、方案取舍和审计证据。
- [变更记录](CHANGELOG.md)：用户可感知的变动。
- [路线图](docs/roadmap.md)：尚未完成的工作。
- 社区文件：[行为准则](CODE_OF_CONDUCT.md)、[安全策略](SECURITY.md)、[获取帮助](SUPPORT.md)、[贡献署名](CREDITS.md)。

## 许可与素材

原创软件采用 [MIT](LICENSE)，具体范围见 [许可说明](LICENSING.md)和[第三方说明](THIRD_PARTY_NOTICES.md)。美术母版存放于组织私有美术仓（限定授权，仅限本模组构建、测试与 Steam 工坊分发），不随源码仓公开，也不使用 Git LFS。
