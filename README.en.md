# STS2-Navia · Navia character mod

[简体中文](README.md)

A Navia character mod for Slay the Spire 2, built around Load, Salvo, Mora, and Support. Version 0.1.6 is released (GitHub Releases and Steam Workshop) for both game branches: 0.107.1 (stable) and 0.111.0 (beta). See [STATUS.md](STATUS.md) for implemented state and limits.

## Installation

This checkout provides source code, bilingual localization, text resource configuration, and organized public design documents. The preferred install path is subscribing on [Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3810999936) together with [RitsuLib](https://steamcommunity.com/sharedfiles/filedetails/?id=3747602295) — the workshop item picks the matching content for the running game version, so switching branches needs no reinstall. Alternatively, download the zip for your game target (0.107.1 or 0.111.0) from [GitHub Releases](https://github.com/SlayTheCircle/STS2-Navia/releases) and place its `STS2-Navia/` directory under the game's `mods/` directory. Close the game before installing or replacing files, and keep the previous package for rollback. Minimum dependency versions are defined in the [mod manifest](STS2-Navia.json). Multimedia is absent from source checkouts. A DLL alone is not a complete installable package.

## Development

Requirements: Bash, Python 3.11+, and the .NET SDK selected by `global.json`. Full asset builds also require the Godot 4.5.1 standard editor and local artwork. Obtain game reference assemblies from your own matching game installation. RitsuLib must include `compat/`, `shared/`, and `RitsuLib.References.props`.

```bash
cp .local-dev.env.example .local-dev.env
# Configure local dependency and tool locations
./scripts/check.sh                 # Source checks; no game DLLs or artwork required
./scripts/restore-refs.sh          # Copy references from GAME_DIR
./scripts/build.sh --dll-only      # Compile the DLL only
./scripts/check.sh --full          # Full assets, compilation, and PCK checks
```

See the [contributor guide](CONTRIBUTING.md) and [build pipeline](docs/dev/pipeline.md) for configuration and platform scope. Local collaborators should first read `local_dev/README.md` when it exists; this private entry point maps reference projects and dependencies to actual machine paths and is not distributed.

## Documentation

- [Documentation index](docs/README.md)
- [Developer documentation](docs/dev/README.md)
- [Design documents](docs/design/README.md)
- [Technical history](docs/history/README.md)
- [Changelog](CHANGELOG.md)
- [Roadmap](docs/roadmap.md)
- Community files: [Code of conduct](CODE_OF_CONDUCT.md), [Security policy](SECURITY.md), [Support](SUPPORT.md), and [Credits](CREDITS.md).

The developer documents are maintained in Chinese. The two README files are updated together.

## License and artwork

Original software is licensed under [MIT](LICENSE). See the [licensing scope](LICENSING.md) and [third-party notices](THIRD_PARTY_NOTICES.md). Art masters live in a private organization repository under a restricted license (building, testing, and Steam Workshop distribution of this mod only). The public source repository stays media-free, and Git LFS is not used.
