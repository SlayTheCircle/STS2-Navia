#!/usr/bin/env python3
"""定版前禁止 Git 索引包含多媒体、本机配置和构建产物。"""
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
MEDIA = set('png jpg jpeg webp gif svg psd aseprite kra xcf blend avif bmp tif tiff ico icns '
            'ogg wav mp3 flac aiff aac m4a opus mp4 webm mov avi mkv '
            'ttf otf woff woff2 fbx glb gltf ctex'.split())
GENERATED = {'import', 'md5', 'dll', 'pck'}
PRIVATE = ('local_dev/', 'libs/', 'mods-dist/', '.tools/', 'cards-staging/', 'assets/.godot/')
tracked = subprocess.check_output(['git', 'ls-files', '-z'], cwd=ROOT).decode().split('\0')
errors = []
for name in filter(None, tracked):
    path = Path(name)
    suffix = path.suffix.lower().lstrip('.')
    if suffix in MEDIA | GENERATED:
        errors.append(f'禁止追踪媒体或生成物: {name}')
    elif name.startswith(PRIVATE) or name == '.local-dev.env' or suffix in {'env', 'token'}:
        errors.append(f'禁止追踪本机配置或私有工作材料: {name}')
    elif any(part in ('bin', 'obj') for part in path.parts):
        errors.append(f'禁止追踪编译产物: {name}')
for error in errors:
    print('错误:', error)
if errors:
    raise SystemExit(1)
print('Git 索引边界检查通过：无多媒体、本机配置或构建产物。')
