# 安全策略 / Security Policy

## 支持范围

| 项目 | 约定 |
|---|---|
| 接受安全修复的版本 | 最新发行 tag 与 `main` 分支 |
| 历史版本与开发构建 | 按问题影响与维护资源评估 |
| 漏洞范围 | 本模组的代码与发行包内容；游戏本体、RitsuLib、Harmony、Godot 等依赖的漏洞请报告对应上游项目 |

当前实现与兼容状态见 [STATUS](STATUS.md)。

## 报告漏洞

- **私密渠道**：GitHub 私密漏洞报告（Private Vulnerability Reporting，随公开仓库启用）。仓库页 Security → Report a vulnerability。
- **报告内容**：受影响的模组版本、游戏／依赖环境（游戏分支、RitsuLib 版本）、复现步骤、实际影响，以及经过脱敏的相关证据。

请勿在公开 Issue 或讨论中发布未披露漏洞的利用细节、凭据或个人信息。普通使用问题的入口见 [SUPPORT](SUPPORT.md)。

## 响应与披露

- 响应为尽力而为（best-effort），不承诺固定时限。
- 确认后的修复随下一个发行版本发布，并在 Release Notes 中披露。
- 不设置漏洞赏金。

## English summary

Security fixes target the latest release tag and `main`. Report privately via GitHub Private Vulnerability Reporting; include affected mod version, game/dependency environment, reproduction steps, and impact. Response is best-effort; fixes ship with the next release. Do not post undisclosed exploit details publicly. Vulnerabilities in the game or dependencies should go to their upstream projects.
