# How it works

Plain-language explanations of every system, tool and technique in this project, written so every team member can explain the project at the thesis defence, not only the person who built a given part.

The main version is in Polish: [Jak to działa](../pl/README.md). This English version says the same thing, document for document.

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
| 03 | [Simulation core](03-simulation-core.md) | Fixed ticks, clock with pause and speed, calendar and days, messages between modules |
| 05 | [Grid and building placement](05-grid-and-building-placement.md) | Hidden square grid, footprints, snapping, overlap check, one build level for all buildings |

Number 04 (packages and main scene) is reserved for the document that is written with that task.

New documents are added as systems are built (packages and main scene, storage, energy, production, colonists, saving, weather, dome, grass, raids, expeditions, tutorial).

## Glossary

| Term | Meaning |
| --- | --- |
| Assembly | A separately compiled piece of the code. Each of our modules is one assembly. |
| Build level | The single height at which every building stands, whatever the terrain below looks like. |
| Commit | A saved snapshot of changes in git, with a message describing them. |
| Edit Mode test | An automated test that runs inside the Unity Editor without pressing Play. |
| Event | A message one part of the game sends ("the dome was activated") that other parts can react to. |
| Frame | One picture drawn on the screen. Frames take a varying amount of time; ticks do not. |
| Footprint | The rectangle of grid cells a building covers, for example 2 × 2. |
| Game speed | How many times faster than normal the rules run. It changes how many ticks run per real second, not what a tick does. |
| Grid cell | One square of the invisible building grid (10 m × 10 m). |
| In-game day | A fixed number of ticks (28 800, which is 48 minutes of play at normal speed; to be confirmed). Daily rules run when a new one starts. |
| LFS | Git Large File Storage: keeps big binary files (models, textures, audio) outside normal git history. |
| Placeholder | A simple stand-in (a plain box) used until the real model exists. |
| Pull request | A request to merge a branch into `main`, where changes are reviewed first. |
| Ray | An imaginary straight line from the camera through the mouse pointer, used to find what the player points at. |
| ScriptableObject | A Unity asset that stores data (e.g. a building's cost) editable in the Inspector. |
| Simulation | The part of the code that holds the game rules and state, independent of what is drawn on screen. |
| Snapping | Moving a pointed position to the nearest allowed one, here to whole grid cells. |
| Tick | One fixed step of the simulation (0.1 s of game time). |
