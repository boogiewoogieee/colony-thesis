# colony-thesis

*Working title.* A sci-fi economic strategy game about building a colony on a hostile planet: mine deposits, keep colonists fed and supplied, power and expand a protective dome, defend against raids, and slowly turn the barren ground green.

Bachelor's thesis project, Game Development, Collegium Da Vinci, Poznań.

**Status:** early development.

## Team

| Name | Role | GitHub |
| --- | --- | --- |
| Tymoteusz Jaskólski | Programming | [@boogiewoogieee](https://github.com/boogiewoogieee) |
| Weronika Górowska | | [@gorowskaw1-crypto](https://github.com/gorowskaw1-crypto) |
| Jakub Błoński | [@Dexter676](https://github.com/Dexter676) |
| Oliwier Karło | | [@Thr4x1440](https://github.com/Thr4x1440) |

## Requirements

- **Unity 6000.6.7f1** exactly. Install it through Unity Hub. Opening the project in another version changes many files.
- **Git with Git LFS.** Easiest: [GitHub Desktop](https://desktop.github.com/), which includes LFS. Command-line users: install [Git](https://git-scm.com/) and run `git lfs install` once.

## Getting started

1. Clone `https://github.com/boogiewoogieee/colony-thesis` (GitHub Desktop: **File > Clone repository**).
2. Open the folder in Unity Hub (**Add > Add project from disk**) with Unity 6000.6.7f1. The first import takes a few minutes.
3. Set up scene and prefab merging once (see below).

### Merging scenes and prefabs (once per computer)

Run these in a terminal inside the project folder, adjusting the path if Unity is installed elsewhere:

```
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver "'C:/Program Files/Unity/Hub/Editor/6000.6.7f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p %O %B %A %A"
git config merge.unityyamlmerge.recursive binary
```

## How we work

- **`main` is protected.** Nobody pushes to it directly. Every change goes through a pull request that Tymoteusz reviews and merges.
- **One task, one branch.** Create a branch from `main`, commit there, push, and open a pull request.
- **Branch names:**

  | Work | Pattern | Example |
  | --- | --- | --- |
  | Code | `feature/<module>-<thing>` | `feature/economy-production-queue` |
  | Art | `art/<asset>` | `art/command-center-lvl2` |
  | Audio | `audio/<asset>` | `audio/raid-music` |
  | Design and data | `design/<topic>` | `design/recipe-balance` |
  | Fixes | `fix/<thing>` | `fix/dome-segment-regen` |

- **Commit messages** follow [Conventional Commits](https://www.conventionalcommits.org/): `feat(economy): add production queue`, `art(buildings): add silo model`, `fix(dome): restore segment after raid`.
- **Large files** (models, textures, audio, fonts) are stored with Git LFS automatically; nothing to do as long as LFS is installed.
- **Source art** (`.psd`, `.blend`, `.fbx`) cannot be merged. Before editing one, lock it (`git lfs lock <file>`) or tell the team, and unlock it after your pull request is merged.
- **Scenes:** only one person edits a scene at a time. Announce it before you start.
- **Tasks** are tracked in the GitHub project board and issues, labelled `code`, `art`, `audio`, `design`, `bug` or `blocked`.

## Project structure

```
Assets/_Project/
  Scripts/      Game code, one folder and assembly per module (see Docs/Architecture.md)
  Data/         Balance data: buildings, resources, recipes, weather (ScriptableObjects)
  Prefabs/      Buildings, units, effects
  Scenes/       Game scenes
  Art/          Models, Materials, Textures, Shaders
  Audio/        Music and sound effects
  UI/           UI Toolkit layouts (UXML) and styles (USS)
  Tests/        Automated tests
Docs/
  GDD.md            Game design document
  Architecture.md   How the code is organised
  HowItWorks/       Plain-language explanations of every system, with likely defence questions
```

Put new assets inside `Assets/_Project/`. Third-party assets from the Asset Store go in their own folders under `Assets/` and are not edited.

## Documents

- [Game design document](Docs/GDD.md)
- [Code architecture](Docs/Architecture.md)
- [How it works](Docs/HowItWorks/README.md): every system explained in plain language, for the whole team. Read these before the thesis defence.

## License

All rights reserved by the authors until the team decides otherwise.
