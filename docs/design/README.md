# 设计资料

本目录体现**当前设计体系**，内容保持确定性：任何时点阅读，得到的都是当下生效的设计。过程性记录——设计者原案、设计实现差异日志、已采纳方案评估——都在[技术历史](../history/design/README.md)下。

| 文档 | 内容 |
|---|---|
| [现行卡表](cards.md) | 全部卡牌的类型、稀有度、费用与效果文本；由 `scripts/export-card-table.py` 从源码与本地化生成，检查脚本守护其不过时 |
| [美术贡献需求](artwork.md) | 核心资源规格、可选打磨及贡献方式 |

## 权威范围

实际行为由源码和本地化体现；当前数量、验收与限制由 [STATUS](../../STATUS.md) 维护。本文档不承载历史数值、待确认内容或开发过程记录——需要它们时去[技术历史](../history/design/README.md)。设计变更落地时同步重新生成卡表并提交。

## 卡表再生成

```bash
python3 scripts/export-card-table.py          # 重新生成 docs/design/cards.md
python3 scripts/export-card-table.py --check  # 校验提交的卡表是否最新（检查脚本调用）
```
