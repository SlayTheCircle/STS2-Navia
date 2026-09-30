#!/usr/bin/env python3
"""占位符审计:本地化文本里引用的 {Var...} 键必须存在于对应卡类的变量集。
实测事故:E 批换 ExtraDamageVar 后 loc 仍写 {CalculationExtra:diff()},格式化器解析失败,
卡面显示原始占位符代码并炸断卡牌生命周期(手牌消失/打出不清理)。构建时跑,缺失即失败。"""
import json, re, glob, sys

INTRINSIC = {  # 无显式名字参数的构造 → 内建键名
    'CalculationBaseVar': 'CalculationBase',
    'CalculationExtraVar': 'CalculationExtra',
    'ExtraDamageVar': 'ExtraDamage',
    'CalculatedDamageVar': 'CalculatedDamage',
    'CalculatedBlockVar': 'CalculatedBlock',
    'DamageVar': 'Damage',
    'BlockVar': 'Block',
    'CardsVar': 'Cards',
    'EnergyVar': 'Energy',
}

def card_vars(path):
    src = open(path).read()
    keys = set()
    for m in re.finditer(r'new (\w+)\("([^"]+)"', src):
        keys.add(m.group(2))  # DynamicVar("Hits") / CalculatedVar("CalculatedX") 等
    for m in re.finditer(r'new (\w+)\(', src):
        if m.group(1) in INTRINSIC:
            keys.add(INTRINSIC[m.group(1)])
    for m in re.finditer(r'new PowerVar<(\w+)>', src):
        keys.add(m.group(1))
    for m in re.finditer(r'new PowerVar<\w+>\("([^"]+)"', src):
        keys.add(m.group(1))  # 自定义键名 PowerVar<T>("Name", ...) → 运行时键为 Name(vanilla MadScience 同款)
    return keys

bad = []
varmap = {}
for path in sorted(glob.glob('src/STS2-Navia/Content/Cards/**/*.cs', recursive=True)):
    source = open(path, encoding='utf-8').read()
    model = re.search(r'class (\w+)\s*:\s*NaviaCardBase\b', source)
    if not model:
        continue
    cls = model.group(1)
    if cls in varmap:
        bad.append(f'重复卡牌类: {cls} ({path})')
        continue
    varmap[cls] = card_vars(path)
for lang in ('zhs', 'eng'):
    cards = json.load(open(f'localization/{lang}/cards.json', encoding='utf-8'))
    for k, v in cards.items():
        if not k.endswith('.description'):
            continue
        stem = k.split('.')[0].replace('STS2_NAVIA_CARD_', '')
        cls = ''.join(w.capitalize() for w in stem.split('_'))
        if cls not in varmap:
            bad.append(f'[{lang}] {cls}: 未找到对应卡牌类，无法检查占位符')
            continue
        used = set(re.findall(r'\{(\w+)(?::[^}]*)?\}', v)) - {'InCombat'}
        missing = used - varmap[cls]
        if missing:
            bad.append(f'[{lang}] {cls}: {sorted(missing)}')

if bad:
    print('占位符审计失败——以下卡牌文本引用了不存在的变量:')
    print('\n'.join(bad))
    sys.exit(1)
print(f'占位符审计通过({len(varmap)} 卡 × zhs/eng)')
