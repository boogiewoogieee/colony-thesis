# Code Architecture

This document describes how the game's code is organised. The design itself is in [GDD.md](GDD.md).

## Core rule: simulation separate from presentation

All game rules (resources, energy, production, colonists, weather, raids, grass growth) run as **plain C# classes** on a fixed tick. They do not derive from `MonoBehaviour`, do not read `Time`, and do not touch scenes.

The scene only:

1. Creates the simulation (`GameBootstrap`).
2. Calls `Simulation.Update(Time.unscaledDeltaTime)` once per frame.
3. Shows the simulation's state (views, UI) and sends player commands to it.

This gives three benefits:

- **Testable:** game rules are covered by fast Edit Mode tests that run without entering Play Mode.
- **Saveable:** the whole game state is one set of plain objects, so saving is serializing that state.
- **Deterministic:** the simulation advances in fixed steps, so frame rate does not change the results.

## Simulation loop

| Piece | Responsibility |
| --- | --- |
| `SimulationClock` | Turns frame time into whole fixed ticks (default 0.1 s); handles pause and game speed; caps long frames at 0.25 s. |
| `GameCalendar` | Turns play time into in-game days (default 48 min per day, to be confirmed) and raises `DayStarted`. |
| `EventBus` | Typed events between modules (e.g. `DomeActivated`), so modules do not reference each other. |
| `ISimulationSystem` | One piece of game logic. `Tick(in TickContext)` is called every tick, in registration order. |
| `Simulation` | Owns the clock, calendar, event bus and systems; `Update`, `Step` (tests) and `Restore` (loading). |

Daily rules (food and water upkeep, starvation, Tools upkeep, grass growth) subscribe to `GameCalendar.DayStarted`.

## Assemblies

Each module lives in `Assets/_Project/Scripts/<Module>/` with its own assembly definition (`Colony.<Module>`). A module may only reference the assemblies listed for it, which keeps dependencies one-directional and compile times short.

| Assembly | Owns | References |
| --- | --- | --- |
| `Colony.Core` | Clock, calendar, event bus, system interface. No UnityEngine. | none |
| `Colony.Data` | ScriptableObject definitions: buildings and grid sizes, resources, recipes, research, weather. | Core |
| `Colony.Economy` | Global storage (items, food, water, energy), silo capacity, production queue, power balance, Tools and building condition. | Core, Data |
| `Colony.Population` | Colonist count, job assignment, daily food budget and starvation, morale, summoning. | Core, Data, Economy |
| `Colony.Building` | Grid, placement, footprints, deposit checks, upgrades to level 2. | Core, Data, Economy |
| `Colony.World` | Dome radius and segments, deposits, weather, day/night. | Core, Data |
| `Colony.Terraform` | Grass growth per area, driven by play time, only under the dome. | Core, World |
| `Colony.Combat` | Raids (after dome activation), enemy tracking, turrets and outposts. | Core, World, Building |
| `Colony.Expedition` | Off-map expeditions: timers, rewards, reports. | Core, Population, Economy |
| `Colony.Tutorial` | Tutorial steps as data, ending with dome construction. | Core, Building, World |
| `Colony.Persistence` | Save and load of every module's state, with a save version number. | all simulation assemblies |
| `Colony.UI` | UI Toolkit screens and HUD. | all gameplay assemblies |
| `Colony.Game` | Scene glue: `GameBootstrap`, views, input. | everything |
| `Colony.Tests.EditMode` | Edit Mode tests for the simulation. | all simulation assemblies |

## Conventions

- **Namespaces** match assemblies: `Colony.Economy`, `Colony.Tests.Economy`.
- **Naming:** `PascalCase` for types, methods and properties; `_camelCase` for private fields; `camelCase` for locals and parameters.
- **Serialized fields:** `[SerializeField] private` fields, never public fields.
- **No static mutable state.** Fast Enter Play Mode is on by default in Unity 6.6, so statics are not reset between play sessions. If a static is unavoidable, reset it in a `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` method.
- **Balance numbers live in data** (ScriptableObjects in `Assets/_Project/Data/`), not in code.
- **Hot paths** (anything called every tick or frame) avoid LINQ, closures and allocations.
- **C# version:** Unity's C# 9. No file-scoped namespaces.

## Tests

- Every simulation rule gets Edit Mode tests in `Assets/_Project/Tests/EditMode/<Module>/`.
- Test names follow `Method_Situation_Result`, e.g. `Advance_WhenPaused_RunsNoTicks`.
- Run them from **Window > General > Test Runner > EditMode**.

## Scenes and prefabs

- Scene and prefab files are hard to merge. Only one person edits a given scene at a time; say so in the team chat before you start.
- Prefer prefabs for anything placed in more than one scene, so scene changes stay small.
