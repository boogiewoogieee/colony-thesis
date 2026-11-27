# 02 · Code architecture

**In one sentence:** the game's rules run as plain C# code (the simulation), completely separate from the Unity scene that only displays the game and passes player input to it.

**Status:** the module folders and their assembly definitions exist; the simulation itself is being rebuilt from scratch under the conventions in `Docs/Architecture.md`. Parts below that name classes describe the design being built.

## Why we need it

An economic strategy game is mostly rules and numbers: resources, energy, production, colonists, days. If those rules were spread across scene objects (`MonoBehaviour` scripts), they would depend on what is loaded in the scene, on frame rate and on Play Mode, which makes them hard to test, hard to save and easy to break by accident.

## How it works

1. **Two layers.**
   - The **simulation** holds the game state (what is in storage, which buildings exist, what day it is) and all rules. It is plain C#: no `MonoBehaviour`, no `Time.deltaTime`, no scene objects.
   - The **presentation** (scene, models, UI) reads the simulation's state to draw it, and turns player clicks into commands for the simulation ("place a silo here").
2. **One entry point.** `GameBootstrap`, a single component in the scene, creates the simulation when the scene loads and calls `Simulation.Update` once per frame.
3. **Modules.** Each game system is a module in its own folder under `Assets/_Project/Scripts/` (Economy, Population, Building, World, Terraform, Combat, Expedition, Tutorial, Persistence, UI, Game).
4. **Assemblies.** Each module has an assembly definition file (`.asmdef`), which makes Unity compile it separately and lists which other modules it may use. For example `Colony.Economy` may use `Colony.Core` and `Colony.Data`, but not `Colony.Combat`. A forbidden dependency simply does not compile.
5. **Events between modules.** When modules must react to each other without depending on each other, one publishes an event (e.g. "dome activated") and the others subscribe to it (see 03).
6. **Data, not code, for numbers.** Costs, production rates, capacities and durations are stored in ScriptableObject assets under `Assets/_Project/Data/`, so designers can balance the game in the Inspector without touching code.

## Why this approach

| Option | Why not (or why) |
| --- | --- |
| Everything in `MonoBehaviour`s | Quick to start, but rules depend on the scene, run at frame rate, and can only be tested in Play Mode. Saving means collecting state from many objects. |
| One big assembly (Unity's default) | Any script can use any other, so dependencies tangle over time, and every change recompiles everything. |
| Unity DOTS / ECS | Built for huge numbers of objects; our colony has hundreds at most. It would add complexity and learning time without a benefit we need. |
| **Plain C# simulation + thin scene layer (chosen)** | Rules are testable in milliseconds, saving is serializing one set of objects, results do not depend on frame rate, and modules stay independent. |

## Where it lives

- `Assets/_Project/Scripts/<Module>/Colony.<Module>.asmdef`: module boundaries and allowed references.
- `Assets/_Project/Scripts/Game/`: will hold `GameBootstrap`, the link between scene and simulation (not built yet).
- `Docs/Architecture.md`: the full module table and coding conventions.

## How we know it works

- The project compiles: a forbidden reference between modules would be a compile error.
- Every simulation rule will get Edit Mode tests in `Assets/_Project/Tests/EditMode/`, which create a simulation without any scene. There are none yet: the simulation is being rebuilt.

## Likely questions

**Why not just use `Update()` in each building?** Each building would advance at the frame rate, the order of updates would be unpredictable, and the rules could not be tested or saved in one place. One simulation ticking every system in a fixed order avoids all three problems.

**What is an assembly definition?** A file telling Unity to compile a folder as a separate library and which other libraries it may use. It enforces our module boundaries and speeds up recompiling.

**How does the UI know what to show?** It reads the simulation's state and subscribes to its events; it never changes state directly, it sends commands.

**Where would a designer change a building's cost?** In its ScriptableObject asset under `Assets/_Project/Data/`, in the Inspector. No code change.

**Why does the code follow such strict naming and style rules?** Code is read far more often than it is written, and four people must be able to explain it. Fixed rules (listed in `Docs/Architecture.md` > Conventions) mean every file looks the same, so a name alone says what something is: capitals mean a constant (`MAX_FRAME_SECONDS`), a leading underscore means a private field (`_elapsedSeconds`), and `On` at the start means an event (`OnDayStarted`).

**Why are there no global objects ("singletons") that any script can reach?** Unity keeps global (static) values alive between test runs in the Editor, so old state could leak into a new game. Instead, one scene object, `GameBootstrap`, will create the simulation and hand it to whatever needs it.
