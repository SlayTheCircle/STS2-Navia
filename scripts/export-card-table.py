#!/usr/bin/env python3
"""从源码与本地化生成 docs/design/cards.md（现行卡表）。
设计目录只体现最新体系（确定性）；本脚本是卡表的唯一作者，检查脚本以 --check 守护其不过时。
原案卡表与差异日志在 docs/history/design/，不在此生成。"""
import glob, os, re, sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, 'docs', 'design', 'cards.md')

TYPE_LABEL = {'Attack': '攻击', 'Skill': '技能', 'Power': '能力'}
RARITY_LABEL = {'Basic': '初始', 'Common': '普通', 'Uncommon': '罕见', 'Rare': '稀有', 'Ancient': '先古', 'Token': '衍生'}
RARITY_ORDER = {r: i for i, r in enumerate(['Basic', 'Common', 'Uncommon', 'Rare', 'Ancient', 'Token'])}
SECTION_ORDER = [('Basic', '初始卡'), ('Attacks', '攻击'), ('Skills', '技能'), ('Powers', '能力'), ('Ancient', '先古强化'), ('Tokens', '衍生')]
CTOR = re.compile(r':\s*base\((\d+)\s*,\s*CardType\.(\w+)\s*,\s*CardRarity\.(\w+)')

# 本地化:STS2_NAVIA_CARD_<类名大写下划线>.title/.description
import json
loc = json.load(open(os.path.join(ROOT, 'localization', 'zhs', 'cards.json'), encoding='utf-8'))

def loc_key(cls, suffix):
    snake = re.sub(r'(?<!^)(?=[A-Z])', '_', cls).upper()
    return f'STS2_NAVIA_CARD_{snake}.{suffix}'

rows = []
pattern = os.path.join(ROOT, 'src', 'STS2-Navia', 'Content', 'Cards', '**', '*.cs')
for path in sorted(glob.glob(pattern, recursive=True)):
    cls = os.path.splitext(os.path.basename(path))[0]
    if cls == 'NaviaCardBase':
        continue
    rel = os.path.relpath(path, os.path.join(ROOT, 'src', 'STS2-Navia', 'Content', 'Cards'))
    parts = rel.split(os.sep)
    section = parts[0]
    src = open(path, encoding='utf-8').read()
    m = CTOR.search(src)
    if not m:
        sys.exit(f'错误: {cls} 的 ctor 不匹配已知 base(...) 形态，请更新导出器')
    cost, ctype, rarity = m.group(1), m.group(2), m.group(3)
    title = loc.get(loc_key(cls, 'title'))
    desc = loc.get(loc_key(cls, 'description'))
    if not title or not desc:
        sys.exit(f'错误: {cls} 缺少本地化键 {loc_key(cls, "title/.description")}')
    rows.append({'section': section, 'cls': cls, 'cost': cost, 'type': TYPE_LABEL[ctype],
                 'rarity': RARITY_LABEL.get(rarity, rarity), 'rorder': RARITY_ORDER.get(rarity, 99),
                 'title': title, 'desc': desc.replace('|', '\\|')})

if len(rows) != len({r['cls'] for r in rows}):
    sys.exit('错误: 类名去重失败')

lines = [
    '# 现行卡表',
    '',
    '<!-- 生成文件:scripts/export-card-table.py 从源码 ctor 与 zhs 本地化生成,勿手改。 -->',
    '<!-- 过时校验:check.sh 调用 --check;再生成:python3 scripts/export-card-table.py -->',
    '',
    f'共 {len(rows)} 张（含衍生 token）。效果文本为当前运行文本;升级数值以源码与游戏内为准。',
    '稀有度颜色对照：普通=白卡，罕见=蓝卡，稀有=金卡。',
    '原案数值与设计过程见[技术历史](../history/design/README.md)。',
    '',
]
for key, label in SECTION_ORDER:
    subset = sorted([r for r in rows if r['section'] == key], key=lambda r: (r['rorder'], r['cls']))
    if not subset:
        continue
    lines += [f'## {label}（{len(subset)}）', '', '| 卡牌 | 稀有度 | 费用 | 类型 | 效果 |', '|---|---|---|---|---|']
    for r in subset:
        lines.append(f"| {r['title']} | {r['rarity']} | {r['cost']} | {r['type']} | {r['desc']} |")
    lines.append('')

content = '\n'.join(lines).rstrip() + '\n'

if '--check' in sys.argv:
    current = open(OUT, encoding='utf-8').read() if os.path.isfile(OUT) else ''
    if current != content:
        sys.exit('错误: docs/design/cards.md 与源码/本地化不一致,请运行 python3 scripts/export-card-table.py 重新生成并提交')
    print(f'现行卡表最新({len(rows)} 张)')
else:
    open(OUT, 'w', encoding='utf-8').write(content)
    print(f'已生成 docs/design/cards.md({len(rows)} 张)')
