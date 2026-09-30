#!/usr/bin/env python3
"""双语键、文本资源与完整素材检查；源码模式明确跳过多媒体存在性。"""
import argparse
import json
import re
from pathlib import Path

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--source-only', action='store_true', help='不要求未公开的多媒体文件；保留双语键与文本资源检查')
args = parser.parse_args()
ROOT = Path(__file__).resolve().parent.parent
IMG = ROOT / 'assets/STS2-Navia/images'
errors = []
required_tables = {'cards', 'powers', 'relics', 'potions', 'characters', 'card_keywords',
                   'card_selection', 'events', 'enchantments', 'epochs', 'ancients'}
tables = required_tables | {p.stem for lang in ('zhs', 'eng')
                           for p in (ROOT / 'localization' / lang).glob('*.json')}
for table in sorted(tables):
    keys = {}
    for lang in ('zhs', 'eng'):
        path = ROOT / 'localization' / lang / f'{table}.json'
        try:
            values = json.loads(path.read_text(encoding='utf-8'))
            if not isinstance(values, dict):
                raise ValueError('本地化表必须为 JSON 对象')
            keys[lang] = set(values)
        except (OSError, ValueError) as error:
            errors.append(f'{lang}/{table}.json: {error}')
    if len(keys) == 2:
        for a, b in (('zhs', 'eng'), ('eng', 'zhs')):
            extra = keys[a] - keys[b]
            if extra:
                errors.append(f'键不对齐: {a}/{table}.json 多出 {sorted(extra)[:5]}')


def need(path, what):
    if not path.is_file():
        errors.append(f'{what}: 缺 {path.relative_to(ROOT)}')


def classes(folder, base):
    for path in (ROOT / 'src/STS2-Navia/Content' / folder).rglob('*.cs'):
        match = re.search(r'class (\w+)\s*:\s*' + base + r'\b', path.read_text(encoding='utf-8'))
        if match:
            yield match.group(1)


if not args.source_only:
    for folder, base, asset_folder, slots in (
        ('Cards', 'NaviaCardBase', 'cards', ('',)),
        ('Relics', 'NaviaRelicBase', 'relics', ('', '_outline')),
        ('Potions', 'NaviaPotionBase', 'potions', ('', '_outline')),
        ('Powers', 'NaviaPowerBase', 'powers', ('',)),
    ):
        for cls in classes(folder, base):
            for slot in slots:
                need(IMG / asset_folder / f'{cls}{slot}.png', f'{folder} {cls}{slot}')
    for cls in classes('Enchantments', 'NaviaSupportEnchantment'):
        if not (IMG / 'enchantments' / f'{cls}.png').is_file():
            need(IMG / 'enchantments/navia_support.png', f'附魔 {cls} 的共享徽记')
    for name in ('navia_energy_big', 'navia_energy_text'):
        need(IMG / 'energy' / f'{name}.png', '能量图标')
    for chapter in range(1, 5):
        need(ROOT / 'assets/global/images/timeline/epoch_portraits' / f'sts2_navia_epoch_{chapter}.png', '纪元立绘')

# 显式自有资源路径覆盖事件和角色；源码模式仍检查文本资源。
for source in (ROOT / 'src/STS2-Navia').rglob('*.cs'):
    for resource in re.findall(r'"res://STS2-Navia/([^"{}]+)"', source.read_text(encoding='utf-8')):
        path = ROOT / 'assets/STS2-Navia' / resource
        if not args.source_only or path.suffix in ('.tscn', '.tres', '.gd'):
            need(path, f'代码资源 {source.relative_to(ROOT)}')

for scene in (ROOT / 'assets/STS2-Navia/scenes').rglob('*.tscn'):
    for resource in re.findall(r'path="res://STS2-Navia/([^"{}]+)"', scene.read_text(encoding='utf-8')):
        path = ROOT / 'assets/STS2-Navia' / resource
        if not args.source_only or path.suffix in ('.tscn', '.tres', '.gd'):
            need(path, f'场景资源 {scene.relative_to(ROOT)}')

for error in errors:
    print('错误:', error)
if errors:
    raise SystemExit(1)
print(f'双语键与资源检查通过（{len(tables)} 张本地化表）')
if args.source_only:
    print('未执行：多媒体存在性检查（--source-only）；完整素材验收需不带此参数运行。')
else:
    print('完整素材存在性检查通过。')
