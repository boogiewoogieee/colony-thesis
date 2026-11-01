# 03 · Simulation core

**In one sentence:** a fixed-step clock turns real time into equal "ticks" of game time, a calendar turns game time into in-game days, and an event bus lets modules notify each other.

## Why we need it

Frame rate varies between computers and from moment to moment. If production advanced by "one step per frame", a faster computer would produce more. Many rules also happen once per in-game day (food and water upkeep, Tools upkeep, grass growth), so the game needs a reliable notion of "a new day started", and it must pause, speed up and survive saving and loading.

## How it works

### Fixed tick (`SimulationClock`)

1. Every frame, Unity reports how much real time passed (e.g. 0.016 s at 60 FPS).
2. The clock adds that time, multiplied by the game speed (×1, ×2, or 0 when paused), to an **accumulator**.
3. While the accumulator holds at least one tick length (0.1 s), the clock takes 0.1 s out of it and counts one tick. Leftover time waits for the next frame.
4. The simulation then runs every game system once per tick, always with exactly 0.1 s of game time.

So at 60 FPS a tick happens roughly every sixth frame, at 30 FPS every third, and the total amount of game time is the same.

Two safety details:

- **Hitch cap.** A single frame counts as at most 0.25 s. If the game freezes for 5 seconds (loading, alt-tab), it does not try to catch up with 50 ticks at once, which would cause a further freeze.
- **Rounding tolerance.** Computers store decimals approximately: 0.06 + 0.06 + 0.08 can come out as 0.19999998 instead of 0.2. Without care the clock would lose a tick here and there. The clock keeps time at double precision, converts the tick length so that 0.1 really is 0.1, and allows a tiny tolerance (one millionth of a tick) when counting. An automated test caught this exact problem during development.

### Days (`GameCalendar`)

- Day length is a setting (currently 48 minutes of play; the GDD's grass growth of 20% per day over 4 hours implies this, still to be confirmed).
- From the total game time, the calendar computes the current day number (starting at 0) and how far through the day we are (0 to 1, used later for day/night and solar panels).
- When game time crosses into a new day, it raises the `DayStarted` event once for every day crossed, in order, so no daily rule is ever skipped, even at high game speed.

### Events (`EventBus`)

- A module publishes a small message, e.g. `DomeActivated`, and every module that subscribed to that message type is called.
- The publisher does not know who listens. This keeps modules independent: the Combat module can start raids when the dome activates without the World module knowing Combat exists.

### Putting it together (`Simulation`)

- Owns the clock, calendar, event bus and the list of game systems.
- `Update(frameTime)`: called every frame by the scene; runs as many ticks as the clock allows.
- For each tick: update the calendar first (so systems see the correct day), then run each system in the order it was registered, so the results are the same on every run.
- `Step(n)`: runs exactly n ticks; used by tests to fast-forward without real time passing.
- `Restore(tick)`: sets the clock and calendar from a saved game without replaying anything.

## Why this approach

| Option | Why not (or why) |
| --- | --- |
| Rules in `Update()` using `Time.deltaTime` | Results depend on frame rate and are not reproducible; tests need Play Mode. |
| Unity's `FixedUpdate` | Fixed step, but tied to physics settings and `MonoBehaviour`s, and it cannot run in Edit Mode tests. |
| Real-world clock (`DateTime.Now`) for days and grass | Breaks pausing, game speed and saving; the GDD timer counts play time, not wall-clock time. |
| Direct references between modules instead of events | Creates a web of dependencies that the assembly rules would forbid anyway. |

## Where it lives

- `Assets/_Project/Scripts/Core/SimulationClock.cs`
- `Assets/_Project/Scripts/Core/GameCalendar.cs`
- `Assets/_Project/Scripts/Core/EventBus.cs`
- `Assets/_Project/Scripts/Core/Simulation.cs` (also defines `TickContext` and `ISimulationSystem`)
- Tests: `Assets/_Project/Tests/EditMode/Core/` (27 tests)

## How we know it works

The tests check, among other things: partial frames add up to whole ticks; 600 frames at 60 FPS give 10 seconds of game time; pausing stops ticks; double speed doubles them; a long freeze is capped; crossing three days raises three events in order; loading a save restores the day without firing events; systems run in registration order. Run them in **Window > General > Test Runner > EditMode**.

## Likely questions

**Why 0.1 s per tick?** Ten updates per game second is plenty for an economy game and cheap to compute. It is a setting and can be changed.

**What happens when the game is paused?** The clock stops accumulating time, so no ticks run and nothing in the simulation changes. The scene keeps rendering.

**How does game speed ×2 work?** Frame time is doubled before it goes into the accumulator, so twice as many ticks run per real second. Each tick still covers exactly 0.1 s of game time, so the rules behave identically.

**If the game runs at 20 FPS, does the colony produce less?** No. Fewer frames means more ticks per frame; game time per real second stays the same.

**How does loading a save avoid replaying the whole game?** The save stores the tick count; `Restore` sets the clock and calendar directly from it and every system loads its own saved state.

**What is an event bus?** A central message board. Modules post typed messages and subscribe to the types they care about, so they can react to each other without referencing each other.
