#!/usr/bin/env bash
# 双变体打包:对每个游戏目标完整构建一次,产出
#   ① 每目标安装 ZIP(GitHub Release 用,平铺布局,清单 min_game_version 按目标改写)
#   ② 工坊变体布局目录+ZIP(根引导壳 + lib/game-<目标>/,publish.sh 消费)
# 命名从实际清单读取,不执行上传。NAVIA_PACKAGE_CHANNEL=release 时安装说明用公开渠道措辞。
# 目标引用目录可用 NAVIA_GAME_REFS_1110 / NAVIA_GAME_REFS_1071 覆盖(默认本机 libs/,CI 指向 checkout)。
set -euo pipefail
source "$(dirname "$0")/dev-env.sh"
# 缺许可材料时在构建前中止,避免生成没有声明的候选包。
for notice in LICENSE LICENSING.md THIRD_PARTY_NOTICES.md; do
    [[ -s "$NAVIA_ROOT/$notice" ]] || { echo "错误: 缺许可文件 $notice。" >&2; exit 1; }
done
REFS_1110="${NAVIA_GAME_REFS_1110:-$NAVIA_ROOT/libs/game}"
REFS_1071="${NAVIA_GAME_REFS_1071:-$NAVIA_ROOT/libs/game-0.107.1}"
for d in "$REFS_1110" "$REFS_1071"; do
    [[ -f "$d/sts2.dll" ]] || { echo "错误: 缺游戏引用目录 $d。" >&2; exit 1; }
done

STAGE="$NAVIA_ROOT/mods-dist/variants"
rm -rf "$STAGE"
mkdir -p "$STAGE/0.111.0" "$STAGE/0.107.1"

echo '== 目标 0.111.0 完整构建 =='
GAME_REFS_DIR="$REFS_1110" RITSULIB_TARGET=0.111.0 "$NAVIA_ROOT/scripts/build.sh"
cp "$NAVIA_ROOT"/mods-dist/STS2-Navia/{STS2-Navia.json,STS2-Navia.dll,STS2-Navia.pck} "$STAGE/0.111.0/"

echo '== 目标 0.107.1 完整构建 =='
GAME_REFS_DIR="$REFS_1071" RITSULIB_TARGET=0.107.1 "$NAVIA_ROOT/scripts/build.sh"
cp "$NAVIA_ROOT"/mods-dist/STS2-Navia/{STS2-Navia.json,STS2-Navia.dll,STS2-Navia.pck} "$STAGE/0.107.1/"

echo '== 引导壳构建 =='
NAVIA_LOADER_GAME_REFS="$REFS_1071" "$NAVIA_ROOT/scripts/build-loader.sh"

python3 - "$NAVIA_ROOT" "$STAGE" "$RITSULIB_TARGET" <<'PYCODE'
import hashlib
import json
import os
import re
import sys
import zipfile
from pathlib import Path
root, stage = Path(sys.argv[1]), Path(sys.argv[2])
targets = ['0.107.1', '0.111.0']
channel_release = os.environ.get('NAVIA_PACKAGE_CHANNEL') == 'release'

base = json.loads((stage / '0.111.0' / 'STS2-Navia.json').read_text(encoding='utf-8'))
version = base['version']
if not re.fullmatch(r'[0-9]+\.[0-9]+\.[0-9]+(?:-[0-9A-Za-z.]+)?', version):
    raise SystemExit('清单 version 无法用于发行包命名')

def semver(key):
    return tuple(int(part) for part in key.split('.'))

def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def write_zip(archive, folder_pairs, extra_texts):
    with zipfile.ZipFile(archive, 'w', zipfile.ZIP_DEFLATED) as package:
        for src, arc in folder_pairs:
            package.write(src, arc)
        for arc, text in extra_texts.items():
            package.writestr(arc, text)

install_notice = (
    'Close the game before installing. Place STS2-Navia/ under the game mods/ folder. '
    'Install the matching RitsuLib dependency. Original software is MIT-licensed; '
    'see STS2-Navia/LICENSE, LICENSING.md and THIRD_PARTY_NOTICES.md for scope and third-party rights. '
    'Artwork is not released under the software license.'
)
if not channel_release:
    install_notice += ' This is an internal candidate; see release notes for acceptance and compatibility.'

licenses = {name: root / name for name in ('LICENSE', 'LICENSING.md', 'THIRD_PARTY_NOTICES.md')}
dist = root / 'mods-dist'
artifacts = []

# ① 每目标安装 ZIP(平铺,清单按目标改写 min_game_version)
for target in targets:
    src = stage / target
    manifest = dict(base)
    manifest['min_game_version'] = target
    archive = dist / f'STS2-Navia-{version}-game-{target}.zip'
    pairs = [(src / 'STS2-Navia.dll', 'STS2-Navia/STS2-Navia.dll'),
             (src / 'STS2-Navia.pck', 'STS2-Navia/STS2-Navia.pck')]
    pairs += [(path, f'STS2-Navia/{name}') for name, path in licenses.items()]
    write_zip(archive, pairs,
              {'STS2-Navia/STS2-Navia.json': json.dumps(manifest, ensure_ascii=False, indent=2) + '\n',
               'STS2-Navia/INSTALL.txt': install_notice + '\n'})
    artifacts.append(archive)

# ② 工坊变体布局:根 = 引导壳 dll + 与目标无关的共享 pck(has_pck=true 走游戏原生挂载);
#    lib/game-<目标>/ 只放变体 dll。内容程序集须由 Loader 登记到游戏模型扫描通道。
#    根清单取最低游戏版本与既有依赖下限。
workshop = dist / 'workshop' / 'STS2-Navia'
if workshop.exists():
    import shutil
    shutil.rmtree(workshop)
(workshop / 'lib').mkdir(parents=True)
root_manifest = dict(base)
root_manifest['min_game_version'] = min(targets, key=semver)
(workshop / 'STS2-Navia.json').write_text(json.dumps(root_manifest, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
(workshop / 'STS2-Navia.dll').write_bytes((root / 'mods-dist' / 'loader' / 'STS2-Navia.dll').read_bytes())
(workshop / 'STS2-Navia.pck').write_bytes((stage / '0.111.0' / 'STS2-Navia.pck').read_bytes())
for name, path in licenses.items():
    (workshop / name).write_bytes(path.read_bytes())
variants_doc = {'schema': 1, 'variants': [
    {'modVersion': version, 'minGameVersion': target, 'directory': f'lib/game-{target}',
     'dependencies': base['dependencies']}
    for target in targets
]}
(workshop / 'mod-variants.manifest').write_text(json.dumps(variants_doc, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
for target in targets:
    variant_dir = workshop / 'lib' / f'game-{target}'
    variant_dir.mkdir(parents=True)
    (variant_dir / 'STS2-Navia.dll').write_bytes((stage / target / 'STS2-Navia.dll').read_bytes())

workshop_zip = dist / f'STS2-Navia-{version}-workshop.zip'
with zipfile.ZipFile(workshop_zip, 'w', zipfile.ZIP_DEFLATED) as package:
    for path in sorted(workshop.rglob('*')):
        if path.is_file():
            package.write(path, path.relative_to(workshop.parent))
artifacts.append(workshop_zip)

for archive in artifacts:
    checksum = sha256(archive)
    (archive.with_suffix(archive.suffix + '.sha256')).write_text(f'{checksum}  {archive.name}\n', encoding='utf-8')
    print(f'{"公开渠道候选包" if channel_release else "内部候选包"}: {archive.name}  SHA-256 {checksum[:16]}…')
print(f'包内清单版本: {version}; 变体目标: {", ".join(targets)}')
print('工坊布局目录: mods-dist/workshop/STS2-Navia(根引导壳 + lib/game-<目标>/)')
if channel_release:
    print('本脚本不上传,发布由 Release 工作流与维护者验收完成。')
else:
    print('未上传,素材授权与正式发布范围待确认。')
PYCODE
