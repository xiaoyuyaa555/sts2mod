# HextechRunes

HextechRunes is a Slay the Spire 2 mod. This branch contains a v0.99.1-lite compatibility build while preserving the normal newer-version code paths where possible.

## Scope of this branch

- Target game version for this branch: Slay the Spire 2 v0.99.1.
- Package type: DLL + PCK + JSON manifest.
- v0.99.1 manifest source: `assets/HextechRunes.v0.99.1.json`.
- Output manifest name in the deployed mod folder: `HextechRunes.json`.

This branch does not require AI teammate, STS2MCP, LAN bridge, or other local test helper mods to build or run HextechRunes.

## Prerequisites

Install:

- .NET 9 SDK
- Godot 4.5.x Mono editor
- Git Bash / zsh-compatible shell if using `tools/build_and_deploy.sh`

Prepare game reference assemblies for the target game version:

```text
hextech-runes/core/versioned-dll-backups/0.99.1/game-refs/
```

That folder must contain at least:

```text
sts2.dll
GodotSharp.dll
0Harmony.dll
```

These files must come from the matching Slay the Spire 2 v0.99.1 installation.

## Build the DLL

From `hextech-runes/core`:

```powershell
dotnet build .\src\HextechRunes.csproj -c Release `
  -p:HextechSts2Target=0.99.1 `
  -p:GameDataDir=".\versioned-dll-backups\0.99.1\game-refs"
```

The built DLL is written under:

```text
src/bin/Release/net9.0/HextechRunes.dll
```

## Package files

A deployable HextechRunes v0.99.1 folder must contain:

```text
HextechRunes.dll
HextechRunes.pck
HextechRunes.json
```

For v0.99.1, use this source manifest:

```text
assets/HextechRunes.v0.99.1.json
```

Copy it into the package as:

```text
HextechRunes.json
```

Do not include `min_game_version` in the v0.99.1 deployed manifest.

## PCK packaging

The existing packaging script is:

```text
tools/build_and_deploy.sh
```

It was written for the original author's macOS/zsh environment. If your environment matches it, run from `hextech-runes/core`:

```bash
HEXTECH_STS2_TARGET=0.99.1 HEXTECH_DEPLOY=0 ./tools/build_and_deploy.sh
```

On Windows, use Godot 4.5.x Mono to import/package the assets equivalently, then place the generated `HextechRunes.pck` beside the DLL and manifest.

## Deploy

Create a separate folder in the game's `mods` directory, for example:

```text
mods/HextechRunes_v0991_test/
```

Copy these files into it:

```text
HextechRunes.dll
HextechRunes.pck
HextechRunes.json
```

Do not load this local package together with a Steam Workshop copy of HextechRunes. Duplicate mod IDs can cause multiplayer mod mismatch or load conflicts.

## Quick verification

After launching the game:

1. Confirm HextechRunes appears in the mod list.
2. Start a singleplayer run.
3. Confirm the Act 1 Hextech selection appears.
4. Choose a rune and enter combat.
5. Check the game log for:

```text
MissingMethodException
MissingFieldException
TypeLoadException
Harmony patch failure
FileNotFoundException
```

Those errors should not appear for HextechRunes in a successful v0.99.1-lite load.

For multiplayer, both players must use the same HextechRunes package files.

## Notes

- `docs/v0.99.1-api-notes.md` records confirmed v0.99.1 API and manifest compatibility notes.
- `docs/v0.99.1-build-deploy.md` contains a shorter build/deploy checklist.
- Local-only automation helpers outside `hextech-runes/core` are not required by this mod package.
