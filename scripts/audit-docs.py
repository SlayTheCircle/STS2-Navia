#!/usr/bin/env python3
"""检查公开 Markdown 的本地文件链接；私有导航使用逻辑入口。"""
import re
from pathlib import Path
from urllib.parse import unquote, urlsplit

ROOT = Path(__file__).resolve().parent.parent
paths = (set(ROOT.glob('*.md')) | set((ROOT / 'docs').rglob('*.md'))
         | set((ROOT / '.github').rglob('*.md')) | set((ROOT / 'assets').glob('*.md')))
errors = []
count = 0
for path in sorted(paths):
    text = path.read_text(encoding='utf-8')
    if '\r' in path.read_bytes().decode('utf-8'):
        errors.append(f'{path.relative_to(ROOT)}: 使用了 CRLF')
    # 本地化片段中的 [gold]、数组和代码示例不是 Markdown 链接。
    text = re.sub(r'(?ms)^```[^\n]*\n.*?^```[^\n]*$', '', text)
    text = re.sub(r'`+[^`\n]*`+', '', text)
    # 引擎富文本的 [/gold](说明) 不作为文件链接。
    for target in re.findall(r'\[(?!/)[^\]\n]*\]\(([^)\n]+)\)', text):
        target = target.strip()
        if target.startswith('<') and '>' in target:
            target = target[1:target.index('>')]
        else:
            target = target.split()[0]
        parts = urlsplit(target)
        if parts.scheme or parts.netloc or not parts.path:
            continue
        link = (path.parent / unquote(parts.path)).resolve()
        count += 1
        try:
            relative = link.relative_to(ROOT)
        except ValueError:
            errors.append(f'{path.relative_to(ROOT)}: 链接越出仓库 {target}')
            continue
        if relative.parts[0] == 'local_dev':
            if relative != Path('local_dev/README.md'):
                errors.append(f'{path.relative_to(ROOT)}: 私有导航应从 local_dev/README.md 进入')
            continue
        if not link.exists():
            errors.append(f'{path.relative_to(ROOT)}: 失效链接 {target}')
for error in errors:
    print('错误:', error)
if errors:
    raise SystemExit(1)
print(f'文档文件链接检查通过（{len(paths)} 个 Markdown，{count} 个本地链接）；不验证标题锚点或外部网页。')
