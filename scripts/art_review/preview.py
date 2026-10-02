#!/usr/bin/env python3
"""生成自包含图标检查页；仅写指定输出目录，不改变游戏素材。"""
import argparse
import base64
import html
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
# 原版 0.107.1 / 0.111.0 场景逻辑像素：combat/power、relics/relic、
# potions/potion、cards/card。UI 缩放与动画会改变屏幕上的实际像素数。
SIZES = {'powers': 40, 'relics': 60, 'potions': 60, 'enchantments': 35}


def data_uri(path):
    return 'data:image/png;base64,' + base64.b64encode(path.read_bytes()).decode()


def image_tag(path, size):
    return f'<img width="{size}" height="{size}" src="{data_uri(path)}" alt="{html.escape(path.stem)}">'


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--before', type=Path, help='修改前图标目录，包含 powers/relics 等子目录')
    parser.add_argument('--review', type=Path, help='按 folder/stem 索引的人工判断 JSON')
    args = parser.parse_args()
    args.output.mkdir(parents=True, exist_ok=True)
    reviews = json.loads(args.review.read_text()) if args.review else {}
    names = {}
    for table in ('powers', 'relics', 'potions'):
        values = json.loads((ROOT / f'localization/zhs/{table}.json').read_text())
        names[table] = {k.removesuffix('.title'): v for k, v in values.items() if k.endswith('.title')}
    cards, rows = [], []
    for folder in (*SIZES, 'energy'):
        for path in sorted((ROOT / 'assets/STS2-Navia/images' / folder).glob('*.png')):
            if path.stem.endswith('_outline') or (folder == 'energy' and path.stem not in ('navia_energy_big', 'navia_energy_text')):
                continue
            key = f'{folder}/{path.stem}'
            snake = re.sub(r'(?<!^)(?=[A-Z])', '_', path.stem).upper()
            prefix = {'powers': 'POWER', 'relics': 'RELIC', 'potions': 'POTION'}.get(folder)
            name = names.get(folder, {}).get(f'STS2_NAVIA_{prefix}_{snake}', path.stem)
            if folder == 'enchantments':
                name = '统一支援徽记'
            if folder == 'energy':
                name = '费用金玫瑰（卡面）' if path.stem.endswith('big') else '费用金玫瑰（内联）'
            size = SIZES.get(folder, 64 if path.stem.endswith('big') else 24)
            review = reviews.get(key, {'status': '待检查', 'note': ''})
            panes = []
            for theme in ('dark', 'light'):
                panes.append(f'<div class="{theme}">{image_tag(path, size)}<small>基准 {size}px</small></div>')
            panes.append(f'<div class="dark">{image_tag(path, 24)}<small>24px 压力预览</small></div>')
            panes.append(f'<div class="dark">{image_tag(path, 128)}<small>128px 放大检查</small></div>')
            outline = path.with_stem(path.stem + '_outline')
            if outline.is_file():
                panes.append(f'<div class="dark">{image_tag(outline, size)}<small>描边原图</small></div>')
                panes.append(f'<div class="light"><span class="stack" style="width:{size}px;height:{size}px"><span class="outline">{image_tag(outline, size)}</span>{image_tag(path, size)}</span><small>黑色半透明描边叠加</small></div>')
            before = args.before / folder / path.name if args.before else None
            if before and before.is_file():
                panes.append(f'<div class="dark">{image_tag(before, size)}<small>修改前 {size}px</small></div>')
            cards.append(f'<article><h2>{html.escape(name)}</h2><code>{key}</code><p>{html.escape(review["status"])}：{html.escape(review["note"])}</p><section>{"".join(panes)}</section></article>')
            rows.append({'resource': key, 'name': name, 'logical_px': size, **review})
    document = '''<!doctype html><html lang="zh"><meta charset="utf-8"><title>娜维娅图标检查</title>
<style>body{font:16px sans-serif;background:#191b24;color:#eee;margin:24px}article{border-top:1px solid #555;padding:16px 0}h2{font-size:18px}section{display:flex;gap:12px;flex-wrap:wrap}section>div{padding:12px;min-width:90px;display:flex;align-items:center;justify-content:center;flex-direction:column}small{display:block;margin-top:8px;font-size:12px}.dark{background:#242534;color:#ddd}.light{background:#eee9dc;color:#222}.stack{position:relative;display:block}.stack>img,.outline{position:absolute;inset:0}.outline{filter:brightness(0);opacity:.5}img{object-fit:contain}code{font-size:12px}</style>
<h1>娜维娅图标检查</h1><p>基准为原版场景逻辑像素：Power 40、遗物／药水 60、支援 35、卡面费用 64；内联费用使用本项目 24px 派生规格。UI 缩放、字体排版和动画另影响屏幕像素。本页不模拟游戏着色器、数字遮挡或悬停大图。</p>'''
    (args.output / 'index.html').write_text(document + ''.join(cards) + '</html>', encoding='utf-8')
    (args.output / 'inventory.json').write_text(json.dumps(rows, ensure_ascii=False, indent=2), encoding='utf-8')
    print(f'图标检查页：{args.output / "index.html"}（{len(rows)} 项）')


if __name__ == '__main__':
    main()
