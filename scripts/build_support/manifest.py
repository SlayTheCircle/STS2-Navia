"""生成与编译目标配对的安装清单，不改写源码清单。"""
import argparse
import json
from pathlib import Path

SUPPORTED_TARGETS = ("0.107.1", "0.111.0")


def for_target(source: dict, target: str) -> dict:
    if target not in SUPPORTED_TARGETS:
        raise ValueError(f"不支持的游戏目标: {target}")
    return {**source, "min_game_version": target}


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source", type=Path)
    parser.add_argument("destination", type=Path)
    parser.add_argument("target", choices=SUPPORTED_TARGETS)
    args = parser.parse_args()
    if args.source.resolve() == args.destination.resolve():
        parser.error("输出不得覆盖源码清单")
    source = json.loads(args.source.read_text(encoding="utf-8"))
    args.destination.write_text(
        json.dumps(for_target(source, args.target), ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )


if __name__ == "__main__":
    main()
