# STS2-Navia · Navia character mod

[简体中文](README.md)

A Navia character mod for Slay the Spire 2, built around Load, Salvo, Mora, and Support. The main content batches are implemented; compatibility work, final acceptance, and open-source preparation are in progress. See [STATUS.md](STATUS.md) for implemented state and limits.

## Installation

This checkout provides source code, bilingual localization, text resource configuration, and organized public design documents. The official release package and artwork distribution are still being decided. The current build targets game version `0.111.0`; install a matching RitsuLib bundle. Minimum dependency versions are defined in the [mod manifest](STS2-Navia.json).

For an accepted complete package, place its `STS2-Navia/` directory under the game's `mods/` directory and install RitsuLib. Close the game before installing or replacing files, and keep the previous package for rollback. Multimedia is absent from source checkouts. A DLL alone is not a complete installable package.

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
- Community files: [Code of conduct](CODE_OF_CONDUCT.md), [Security policy](SECURITY.md), [Support](SUPPORT.md), and [Credits](CREDITS.md). These are placeholders to be finalized after the project becomes open source.

The developer documents are maintained in Chinese. The two README files are updated together.

## License and artwork

Original software is licensed under [MIT](LICENSE). See the [licensing scope](LICENSING.md) and [third-party notices](THIRD_PARTY_NOTICES.md). Art masters live in a private organization repository under a restricted license (building, testing, and Steam Workshop distribution of this mod only). The public source repository stays media-free, and Git LFS is not used.
