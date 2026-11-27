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
| `GameCalendar` | Turns play time into in-game days (default 48 min per day, to be confirmed) and raises `OnDayStarted`. |
| `EventBus` | Typed events between modules (e.g. `DomeActivated`), so modules do not reference each other. |
| `ISimulationSystem` | One piece of game logic. `Tick(in TickContext)` is called every tick, in registration order. |
| `Simulation` | Owns the clock, calendar, event bus and systems; `Update`, `Step` (tests) and `Restore` (loading). |

Daily rules (food and water upkeep, starvation, Tools upkeep, grass growth) subscribe to `GameCalendar.OnDayStarted`.

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

One test decides every rule below: is the code easy to understand and easy to change? Code is read far more often than it is written, so it is written for the next reader.

### Naming

| Element | Style | Example |
| --- | --- | --- |
| Namespace | matches the assembly | `Colony.Economy`, tests in `Colony.Tests.Economy` |
| Class, struct, enum | `PascalCase` noun | `GameCalendar`, `ResourceCategory` |
| Interface | `I` + `PascalCase` | `ISimulationSystem` |
| Method | `PascalCase` verb | `AdvanceTo`, `TryRemove` |
| Property | `PascalCase` | `DayLengthSeconds` |
| C# event | `On` + what happened | `OnDayStarted` |
| `EventBus` message (struct) | what happened, past tense | `DomeActivated` |
| Private field | `_camelCase` | `_elapsedSeconds` |
| Parameter, local variable | `camelCase` | `dayLengthSeconds` |
| Constant | `UPPER_SNAKE_CASE` | `MAX_FRAME_SECONDS` |
| Generic type parameter | `T` or `T` + role | `TEvent` |

- A name says what a thing is or does: `pickupDistance`, not `d`; `UpdateHealthBar()`, not `DoStuff()`. Long names are fine.
- No abbreviations or dropped vowels (`distance`, not `dst`). Single letters only for loop counters `i`, `j`, `k`.
- Collections are named after what they hold: `colonistList`, `amountByResource`.
- A name that needs "And" (`MoveAndPlaySound`) means the code should be split in two.
- Units go in the name when they matter: `DayLengthSeconds`, `EnergyPerSecond`.

### Code style

- Always write the access modifier, including `private`.
- Always use braces, even for a one-line `if`. Opening braces go on their own line.
- One declaration per line. A long parameter list puts each parameter on its own line.
- `var` only when the type is visible on the same line (`new`, a cast); otherwise write the type.
- State is exposed through properties with a private setter (`{ get; private set; }`), never through public fields.
- Unity fields shown in the Inspector are `[SerializeField] private`, never public.
- `=>` bodies only for single-expression members; the `?:` operator only to choose between two simple values.
- **C# version:** Unity's C# 9. No file-scoped namespaces, no `record` types (use a `readonly struct` or a `sealed class`).

### Design

- **One job each.** A class has one responsibility and a method does one thing. Split when a second job appears; merge back when splitting made the code harder to follow.
- **Composition over inheritance.** Give objects capabilities through fields and small interfaces; inherit only for genuinely shared logic, and the base class never knows its subclasses.
- **Logic never knows about presentation.** Simulation code does not reference UI, audio, animation or `UnityEngine`. Presentation reads state and subscribes to events.
- **One source of truth.** Each value is stored in one place; everything else reads it or derives it.
- **No magic numbers.** Balance numbers live in data (ScriptableObjects in `Assets/_Project/Data/`); other fixed values are named constants.
- **No strings as identity.** Use references, enums, ScriptableObjects or `nameof`. No Unity tags and no `Find` by name. A string that an API forces on us (Animator parameter, save key) is declared once as a constant.
- **Simple control flow.** Guard clauses instead of deep nesting; at most one loop nested in another; no boolean parameters that switch a method's mode; a method that answers a question does not change state.
- **No static mutable state**, so no singletons and no static events. Fast Enter Play Mode is on by default in Unity 6.6, so statics are not reset between play sessions. If a static is unavoidable, reset it in a `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` method.
- **Events.** Between modules: `EventBus` with small struct messages. Inside a module: C# `event Action<T>`, raised with `?.Invoke`. A listener that can be destroyed before the sender unsubscribes (`OnDisable` or `OnDestroy`).
- **Hot paths** (anything called every tick or frame) avoid LINQ, closures and allocations.

### Errors and validation

- Public constructors and methods validate their arguments and throw `ArgumentOutOfRangeException` or `ArgumentNullException` with `nameof` and a clear message.
- Expected game outcomes (not enough resources, a blocked building spot) are returned, not thrown: a `TryX` method or a result enum.
- Input from outside the game (save files, settings) is validated where it is read; a failure is reported with context, never swallowed.
- "No value" is `null` only where the API says so, or `int?` and similar; never a sentinel such as `-1`.
- Simulation code does not log. Presentation code may; debug-only output goes behind `#if UNITY_EDITOR`.

### Comments

- Comments explain why, never what. If code needs a comment to say what it does, rename or extract until it does not.
- XML doc comments on public types and on members whose purpose the name cannot carry.
- Stale comments and dead code are deleted, not commented out.

## Tests

- Every simulation rule gets Edit Mode tests in `Assets/_Project/Tests/EditMode/<Module>/`.
- Test names follow `Method_Situation_Result`, e.g. `Advance_WhenPaused_RunsNoTicks`.
- Tests follow the same conventions as game code. Each test checks one behaviour, covering normal, boundary and invalid input.
- A bug fix comes with a test that would have caught it.
- Run them from **Window > General > Test Runner > EditMode**.

## Scenes and prefabs

- Scene and prefab files are hard to merge. Only one person edits a given scene at a time; say so in the team chat before you start.
- Prefer prefabs for anything placed in more than one scene, so scene changes stay small.
