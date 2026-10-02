"""拒绝安装清单与编译目标错配，以及生成器误覆盖源码清单。"""
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "scripts"))
from build_support.manifest import SUPPORTED_TARGETS, for_target


class ManifestTests(unittest.TestCase):
    def test_targets_preserve_metadata_and_source(self):
        original = (ROOT / "STS2-Navia.json").read_bytes()
        source = json.loads(original)
        with tempfile.TemporaryDirectory() as directory:
            for target in SUPPORTED_TARGETS:
                destination = Path(directory) / f"{target}.json"
                subprocess.run([sys.executable, str(ROOT / "scripts/build_support/manifest.py"),
                                str(ROOT / "STS2-Navia.json"), str(destination), target], check=True)
                manifest = json.loads(destination.read_text())
                self.assertEqual(manifest.pop("min_game_version"), target)
                self.assertEqual(manifest, {key: value for key, value in source.items() if key != "min_game_version"})
        self.assertEqual((ROOT / "STS2-Navia.json").read_bytes(), original)
        self.assertEqual(source, json.loads(original))

    def test_reject_invalid_target_before_writing(self):
        with self.assertRaises(ValueError):
            for_target({}, "0.999.0")
        with tempfile.TemporaryDirectory() as directory:
            destination = Path(directory) / "output.json"
            result = subprocess.run([sys.executable, str(ROOT / "scripts/build_support/manifest.py"),
                                     str(ROOT / "STS2-Navia.json"), str(destination), "0.999.0"], capture_output=True)
            self.assertNotEqual(result.returncode, 0)
            self.assertFalse(destination.exists())

    def test_reject_overwriting_source(self):
        with tempfile.TemporaryDirectory() as directory:
            source = Path(directory) / "source.json"
            source.write_text('{"version":"1.2.3"}\n')
            original = source.read_bytes()
            result = subprocess.run([sys.executable, str(ROOT / "scripts/build_support/manifest.py"),
                                     str(source), str(source), "0.111.0"], capture_output=True)
            self.assertNotEqual(result.returncode, 0)
            self.assertEqual(source.read_bytes(), original)


if __name__ == "__main__":
    unittest.main()
