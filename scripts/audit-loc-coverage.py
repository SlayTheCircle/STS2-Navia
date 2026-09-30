#!/usr/bin/env python3
"""本地化覆盖审计:每个注册内容类必须在 zhs+eng 有对应复合键。
卡=错误(title+description);可见 Power=错误(三件套);遗物=错误(title/description)+警告(flavor);药水=错误。"""
import json, re, glob, sys

SNAKE = lambda s: re.sub(r'(?<!^)(?=[A-Z])', '_', s).upper()
errors, warns = [], []

def load(lang, table):
    return json.load(open(f'localization/{lang}/{table}.json', encoding='utf-8'))

def need(cls, table, keys, severity='error', flavor_optional=False):
    for lang in ('zhs', 'eng'):
        d = load(lang, table)
        for k in keys:
            # 表名→类别词:cards→CARD powers→POWER relics→RELIC potions→POTION
            full = f'STS2_NAVIA_{ {"cards":"CARD","powers":"POWER","relics":"RELIC","potions":"POTION","enchantments":"ENCHANTMENT"}[table] }_{SNAKE(cls)}.{k}'
            if full not in d:
                (errors if severity == 'error' else warns).append(f'[{lang}] {cls} 缺 {full}')

for f in glob.glob('src/STS2-Navia/Content/Cards/**/*.cs', recursive=True):
    m = re.search(r'class (\w+)\s*:\s*NaviaCardBase', open(f).read())
    if m:
        need(m.group(1), 'cards', ['title', 'description'])
for f in glob.glob('src/STS2-Navia/Content/Powers/*.cs'):
    src = open(f).read()
    m = re.search(r'class (\w+)\s*:\s*NaviaPowerBase', src)
    if m:
        need(m.group(1), 'powers', ['title', 'description', 'smartDescription'])
for f in glob.glob('src/STS2-Navia/Content/Relics/*.cs'):
    m = re.search(r'class (\w+)\s*:\s*NaviaRelicBase', open(f).read())
    if m:
        need(m.group(1), 'relics', ['title', 'description'])
        need(m.group(1), 'relics', ['flavor'], severity='warn')
for f in glob.glob('src/STS2-Navia/Content/Potions/*.cs'):
    m = re.search(r'class (\w+)\s*:\s*NaviaPotionBase', open(f).read())
    if m:
        need(m.group(1), 'potions', ['title', 'description'])
for f in glob.glob('src/STS2-Navia/Content/Enchantments/*.cs'):
    m = re.search(r'class (\w+)\s*:\s*NaviaSupportEnchantment', open(f).read())
    if m:
        need(m.group(1), 'enchantments', ['title', 'description', 'extraCardText'])

for w in warns:
    print('警告:', w)
for e in errors:
    print('错误:', e)
if errors:
    sys.exit(1)
print(f'本地化覆盖审计通过(警告 {len(warns)} 条)')
