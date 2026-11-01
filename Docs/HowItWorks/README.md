# How it works

Plain-language explanations of every system, tool and technique in this project, written so every team member can explain the project at the thesis defence, not only the person who built a given part.

Each document answers the same questions, in this order:

1. **In one sentence:** what it is.
2. **Why we need it:** the problem it solves in our game.
3. **How it works:** step by step, without code first, then the key classes.
4. **Why this approach:** the alternatives we considered and why we did not choose them.
5. **Where it lives:** files and classes.
6. **How we know it works:** tests and how to check it by hand.
7. **Likely questions:** questions a reviewer might ask, with short answers.

## Contents

| # | Topic | Covers |
| --- | --- | --- |
| 01 | [Repository and teamwork](01-repository-and-teamwork.md) | Git, Git LFS, branches, pull requests, protected `main`, meta files, merging scenes |
| 02 | [Code architecture](02-code-architecture.md) | Simulation separate from presentation, assemblies, data in ScriptableObjects |
| 03 | [Simulation core](03-simulation-core.md) | Fixed tick, game clock, calendar and days, events between modules |
| 04 | [Packages, settings and the main scene](04-packages-and-main-scene.md) | Unity packages we use, Input System setting, the `Main` scene and `GameBootstrap` |

New documents are added as systems are built (storage, energy, grid building, production, colonists, saving, weather, dome, grass, raids, expeditions, tutorial).

## Glossary

| Term | Meaning |
| --- | --- |
| Assembly | A separately compiled piece of the code. Each of our modules is one assembly. |
| Commit | A saved snapshot of changes in git, with a message describing them. |
| Build Profiles | Unity's window (File > Build Profiles) that lists which scenes go into the built game and in what order. Scene 0 opens first. |
| Edit Mode test | An automated test that runs inside the Unity Editor without pressing Play. |
| Event | A message one part of the game sends ("the dome was activated") that other parts can react to. |
| LFS | Git Large File Storage: keeps big binary files (models, textures, audio) outside normal git history. |
| NavMesh | A map of the walkable ground that Unity bakes from the level; characters use it to find paths around obstacles. |
| Package | A Unity feature (rendering, input, pathfinding) installed through the Package Manager and listed in `Packages/manifest.json`. |
| Play Mode | Running the game inside the Editor by pressing Play. |
| Scene | A Unity file holding a level or screen: the objects in it and their settings. |
| Pull request | A request to merge a branch into `main`, where changes are reviewed first. |
| ScriptableObject | A Unity asset that stores data (e.g. a building's cost) editable in the Inspector. |
| Simulation | The part of the code that holds the game rules and state, independent of what is drawn on screen. |
| Tick | One fixed step of the simulation (0.1 s of game time). |
