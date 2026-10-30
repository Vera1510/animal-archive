# ANIMAL ARCHIVE

A short incremental management game about guiding animals to an ark. Crops attract animals, staff guide them, and upgrades increase boarding capacity. A late-game demon interrupts the growing operation and asks the player to protect the animals with holy light before setting sail.

**[Play the Web version](https://archive.leungpuisze.com/)**

The game uses Japanese interface text. This repository contains the editable Unity project and required assets. It is a prototype, not a commercial release.

## Open the project

1. Install **Unity 6000.3.16f1**, with Web Build Support for browser builds.
2. Open this repository as a Unity project.
3. Open `Assets/Scenes/Archive.unity` and enter Play Mode. `VoyageGame` creates the runtime scene.

Unity regenerates `Library` locally on first import. Generated builds and caches are excluded from Git.

## Controls and loop

- Click a staff member, then the ground to move and guide nearby animals.
- Drag the ground to move the camera; use the zoom buttons to change the view.
- Use the growth drawer to improve arrivals, boarding capacity and skills.
- Hire guides and a gardener, expand farmland, and let staff work and rest automatically.
- During the demon encounter, use prayer to damage and interrupt it and protect animals in danger.
- Reach the contribution target, resolve the crisis, then depart to see the result.

The active animal population is bounded. Losing focus suspends simulation; on return, an open session can receive a bounded passive-income calculation. Closing the page or application does not create long-term offline income. Page exit requests Unity cleanup and cancels shell timers. No separate background game service is installed.

## Code map

| Path | Responsibility |
| --- | --- |
| `Assets/Scripts/VoyageModel.cs` | Contribution, progression and boarding |
| `Assets/Scripts/VoyageFlowV9.cs` | Animal movement, crops and demon behavior |
| `Assets/Scripts/VoyageEconomyV8.cs` | Staff, farming and session background settlement |
| `Assets/Scripts/VoyageGame.cs` | Screens, save data and application lifecycle |
| `Assets/Scripts/VoyageUIV9.cs` | Player input and interface |
| `Assets/Scripts/VoyagePerformanceV10.cs` | Population and performance limits |
| `Assets/Scripts/ArkSound.cs` | Music and sound playback |
| `Assets/Resources/Voyage/config.json` | Species and economy configuration |

Some filenames retain earlier iteration numbers. Earlier puzzle scripts remain for project continuity; the current entry point is `VoyageGame`.

## Build and validation

Use **Animal Archive → Build Web and verify rules**, or call `BuildArchive.Build` in Unity batch mode. The command runs model checks, writes a Web build to sibling `Web`, writes reports to sibling `Evidence`, and copies `WebShell/index.html` into the build. Open the project first to generate Unity's local import cache.

The current game source was previously compiled and built for Web and Windows. This publication does not claim a fresh gameplay test. The separate course submission variant and personal submission materials are not included.

## Credits and reuse

This individual project used substantial generative-AI assistance: planning and Japanese text drafts, implementation, generated visuals, audio synthesis code and validation tooling. The creator supplied direction and revision decisions. See [asset sources](Documentation/ASSET_SOURCES.md).

Public source access does not grant a blanket redistribution license for the game's code or artwork. Except for separately licensed material, rights are reserved by the respective rights holders. Noto Sans JP retains its included SIL Open Font License.
