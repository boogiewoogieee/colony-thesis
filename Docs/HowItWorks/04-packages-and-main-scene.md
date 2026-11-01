# 04 · Packages, settings and the main scene

**In one sentence:** the project uses a small, fixed set of Unity packages, reads player input only through the Input System, and starts from one scene, `Main`, whose `GameBootstrap` object creates and runs the simulation.

## Why we need it

Every later system depends on this base. The camera needs Cinemachine, enemies need pathfinding, the controls need the Input System and the tests need the Test Framework. If the packages were added one by one whenever someone needed them, team members would end up with different setups and "it works on my machine" problems. The game also needs one agreed starting point: the scene Unity loads first when the game starts.

## How it works

1. **Packages.** Unity features come as packages, listed in `Packages/manifest.json`. When someone opens the project, Unity downloads exactly those versions, so everyone has the same tools. The ones we rely on:

   | Package | What we use it for |
   | --- | --- |
   | Universal Render Pipeline (URP) | How the game is drawn: lighting, materials, post-processing. |
   | Input System | Keyboard and mouse controls, defined as named actions ("pan camera", "place building") instead of fixed keys. |
   | Test Framework | Running our Edit Mode tests. Built into Unity 6.6, so it does not appear in `manifest.json` itself. |
   | AI Navigation | Pathfinding for creatures during raids: Unity bakes a NavMesh (a map of walkable ground) and moves enemies along it. |
   | Cinemachine | The isometric camera (pan, zoom, rotate). Built into Unity 6.6. |
   | `com.unity.pipeline` | Lets Unity's command-line tools talk to the open Editor (run tests, read the Console). It is not part of the game. |

2. **Input setting.** In Player settings, *Active Input Handling* is set to **Input System Package**. Unity's old input system is switched off, so all controls go through one place.
3. **The main scene.** `Assets/_Project/Scenes/Main.unity` holds three objects: a camera, a light and `GameBootstrap`. It is scene 0 in Build Profiles, so it is the first scene the built game opens.
4. **What `GameBootstrap` does.** When the scene loads, it creates the `Simulation` (see 03). Every frame it passes the real time since the last frame to `Simulation.Update`, which runs as many 0.1 s ticks as that time allows. The scene does nothing else yet; buildings, the camera and the HUD will be added by later tasks.
5. **Unity's sample content removed.** The template's `SampleScene` and its `Assets/Scenes` folder were deleted, so nobody builds on the wrong scene by accident.

## Why this approach

| Option | Why not (or why) |
| --- | --- |
| Add packages when each feature needs them | Team members' projects drift apart, and a missing package shows up as confusing compile errors. |
| Keep both input systems on ("Both") | Two ways to read input invite mixing them; the old one cannot do rebindable named actions. |
| Several scenes (menu, game, tutorial) from the start | Not needed yet. One scene is easier to merge in git; more scenes can be added when the main menu is built. |
| **Fixed package list, Input System only, one main scene (chosen)** | Everyone has the same setup, and there is one obvious place where the game starts. |

## Where it lives

- `Packages/manifest.json`: the package list (with exact versions locked in `Packages/packages-lock.json`).
- `ProjectSettings/ProjectSettings.asset`: Player settings, including Active Input Handling.
- `ProjectSettings/EditorBuildSettings.asset`: the scene list from Build Profiles.
- `Assets/_Project/Scenes/Main.unity`: the main scene.
- `Assets/_Project/Scripts/Game/GameBootstrap.cs`: the component that runs the simulation.

## How we know it works

- All Edit Mode tests pass in **Window > General > Test Runner > EditMode > Run All**.
- Open `Main`, press Play: the Console shows no errors or warnings. The simulation is running but nothing shows it on screen yet; the HUD (task T10) will display the day and time. Checked during setup by reading the tick count while playing: it rose from 0, about 10 ticks per second.
- **File > Build Profiles** lists `Main` as scene 0 and no other scene.

## Likely questions

**Why is the Test Framework not in `manifest.json`?** In Unity 6.6 it ships with the Editor as a built-in package, like Cinemachine. Unity still lists it with its version in `packages-lock.json`.

**Why do we need pathfinding in an economy game?** Raids: creatures must walk from the edge of the map to structures outside the dome and around obstacles. The NavMesh gives them walkable paths without us writing a pathfinding algorithm.

**Why does `GameBootstrap` use unscaled time?** Pause and game speed belong to the simulation clock, not to Unity's `Time.timeScale`. That way pausing the economy does not freeze UI animations.

**What happens if two `GameBootstrap`s are in the scene?** The component forbids two on one object, and the scene is set up with exactly one. Two would create two separate simulations, so the rule is: one per scene.
