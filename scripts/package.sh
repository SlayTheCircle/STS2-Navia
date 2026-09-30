#!/usr/bin/env bash
# 生成候选 ZIP；命名从实际清单读取，不执行上传。NAVIA_PACKAGE_CHANNEL=release 时安装说明用公开渠道措辞。
set -euo pipefail
source "$(dirname "$0")/dev-env.sh"
# 缺许可材料时在构建前中止，避免生成没有声明的候选包。
for notice in LICENSE LICENSING.md THIRD_PARTY_NOTICES.md; do
    [[ -s "$NAVIA_ROOT/$notice" ]] || { echo "错误: 缺许可文件 $notice。" >&2; exit 1; }
done
"$NAVIA_ROOT/scripts/build.sh"
python3 - "$NAVIA_ROOT" "$RITSULIB_TARGET" <<'PYCODE'
import hashlib
import json
import os
import re
import sys
import zipfile
from pathlib import Path
root = Path(sys.argv[1])
target = sys.argv[2]
built = root / 'mods-dist/STS2-Navia'
manifest = json.loads((built / 'STS2-Navia.json').read_text())
version = manifest['version']
if not re.fullmatch(r'[0-9]+\.[0-9]+\.[0-9]+(?:-[0-9A-Za-z.]+)?', version):
    raise SystemExit('清单 version 无法用于发行包命名')
if manifest.get('min_game_version') != target:
    raise SystemExit(f"清单 min_game_version {manifest.get('min_game_version')} 与编译游戏目标 {target} 不一致")
release = os.environ.get('NAVIA_PACKAGE_CHANNEL') == 'release'
install_notice = (
    'Close the game before installing. Place STS2-Navia/ under the game mods/ folder. '
    'Install the matching RitsuLib dependency. Original software is MIT-licensed; '
    'see STS2-Navia/LICENSE, LICENSING.md and THIRD_PARTY_NOTICES.md for scope and third-party rights. '
    'Artwork is not released under the software license.'
)
if not release:
    install_notice += ' This is an internal candidate; see release notes for acceptance and compatibility.'
archive = root / 'mods-dist' / f'STS2-Navia-{version}-game-{target}.zip'
with zipfile.ZipFile(archive, 'w', zipfile.ZIP_DEFLATED) as package:
    for name in ('STS2-Navia.json', 'STS2-Navia.dll', 'STS2-Navia.pck'):
        package.write(built / name, f'STS2-Navia/{name}')
    for name in ('LICENSE', 'LICENSING.md', 'THIRD_PARTY_NOTICES.md'):
        package.write(root / name, f'STS2-Navia/{name}')
    package.writestr('INSTALL.txt', install_notice + '\n')
checksum = hashlib.sha256(archive.read_bytes()).hexdigest()
checksum_file = archive.with_suffix(archive.suffix + '.sha256')
checksum_file.write_text(f'{checksum}  {archive.name}\n')
print(f'{"公开渠道候选包" if release else "内部候选包"}: {archive.name}')
print(f'包内清单版本: {version}; 编译游戏目标: {target}')
print(f'SHA-256: {checksum}')
if release:
    print('MIT 许可材料已随包附带；本脚本不上传，发布由 Release 工作流与维护者验收完成。')
else:
    print('MIT 许可材料已随包附带；未上传，素材授权与正式发布范围待确认。')
PYCODE
