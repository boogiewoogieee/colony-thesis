# 03 · Simulation core

**In one sentence:** the game's rules advance in equal small steps called ticks (ten per second of game time), driven by one clock, with a calendar that counts in-game days and a message board that lets modules react to each other.

**Status:** the loop exists and is tested. It is not connected to the scene yet, and no game rules (energy, production, upkeep) run on it yet; those are added by later tasks.

## Why we need it

A computer does not draw frames at a steady pace: one frame may take 5 ms, the next 40 ms. If rules such as "a solar panel makes 2 energy per second" were calculated once per frame, a fast computer and a slow one would get slightly different results, and a test could never predict the outcome exactly.

We also need one place that answers "what day is it?", because several rules happen once per day (food and water upkeep, grass growth), and the player can pause the game or speed it up.

## How it works

1. **Ticks.** The simulation never advances by "however long the frame took". It advances only in whole ticks of 0.1 s of game time. Every rule is written as "what happens in one tick".
2. **The clock turns frames into ticks.** Each frame, the scene tells the simulation how much real time passed. The clock adds it to what was left over from earlier frames and counts how many whole ticks fit. Example: frames of 0.04 s, 0.04 s and 0.04 s give 0, 0 and then 1 tick, with 0.02 s kept for later.
3. **Frame cap.** If one frame takes very long (the game froze for 3 seconds while loading something), the clock counts only 0.25 s of it. Otherwise the game would suddenly run 30 ticks at once and the player would see everything jump.
4. **Pause and speed.** While paused, the clock ignores frame time, so no ticks run. Speed multiplies the frame time before it is counted: at speed 2, a 0.1 s frame gives 2 ticks. Pause and speed are separate: speed must be a positive number.
5. **The calendar counts days in ticks.** A day lasts 28 800 ticks (48 minutes of play at normal speed; this length is still to be confirmed). The day is simply the tick count divided by 28 800. When a tick crosses into a new day, the calendar raises `OnDayStarted` once. If several days were crossed at once, it raises it once for each, in order.
6. **One tick, in order.** For every tick: first the calendar is moved (so daily rules run at the start of the day's first tick), then every system is ticked, always in the order the systems were added.
7. **Systems.** A system is one piece of game logic, for example "power balance". It implements `ISimulationSystem` and receives a `TickContext` each tick: the tick number, the tick length in seconds, the day, and how far the day has progressed (0 at its start, close to 1 at its end; solar panels will use this for day and night).
8. **Messages between modules.** The `EventBus` is a message board. A module publishes a message, for example `DomeActivated`, and every module that subscribed to that kind of message is called. The publisher does not know who listens, so modules do not have to reference each other.
9. **Loading a save.** `Simulation.Restore(tickCount)` sets the clock and calendar to a saved tick count. It runs no ticks and raises no day events, because those days already happened before the game was saved.

### Key classes

| Class | Job |
| --- | --- |
| `SimulationClock` | Frame time → whole ticks; pause, speed, frame cap; total tick count. |
| `GameCalendar` | Tick count → day and progress through the day; `OnDayStarted`. |
| `EventBus` | Subscribe to, unsubscribe from and publish messages (small structs). |
| `ISimulationSystem` | The interface every piece of game logic implements: `Tick(in TickContext)`. |
| `TickContext` | The information given to each system on each tick. |
| `Simulation` | Owns the clock, the calendar, the event bus and the list of systems. `Update` (called once per frame by the scene), `Step` (run exactly N ticks, for tests), `Restore` (loading), `AddSystem`. |

Other modules may read the clock and calendar, pause, change speed and subscribe to `OnDayStarted`. The methods that move time forward are only usable inside this module, so time can only advance through `Simulation`, which guarantees the systems are ticked.

## Why this approach

| Option | Why not (or why) |
| --- | --- |
| Calculate rules every frame with the frame's length | Results depend on the frame rate; tests cannot predict exact values; a long frame makes one huge step. |
| Unity's `FixedUpdate` | It is also a fixed step, but it belongs to scene objects and physics, cannot be run without Play Mode, and its order between objects is hard to control. |
| Calendar counted in seconds with fractions | Fractions such as 0.1 cannot be stored exactly by a computer, so after many additions a day could start one tick early or late. Whole ticks are exact. |
| Modules calling each other directly | Every module would have to know the others; our assembly rules forbid it and changes would spread everywhere. |
| C# events on a global static object | Global state survives between play sessions in the Editor and is hard to test. The event bus is an ordinary object owned by the simulation. |
| **Fixed ticks, calendar in ticks, event bus (chosen)** | Same result on every computer, exact day boundaries, rules testable with `Step(n)`, modules independent. |

## Where it lives

- `Assets/_Project/Scripts/Core/`: `SimulationClock`, `GameCalendar`, `EventBus`, `ISimulationSystem`, `TickContext`, `Simulation`.
- `Assets/_Project/Tests/EditMode/Core/`: the tests.
- This module uses no Unity code at all (its assembly definition forbids it), which is what lets it run in tests without a scene.

## How we know it works

Edit Mode tests (**Window > General > Test Runner > EditMode**), 83 for this module:

- `SimulationClockTests`: a frame shorter than a tick runs nothing and is carried over; a long frame is capped at 0.25 s; speed doubles or halves the ticks; nothing runs while paused, and the partial tick from before the pause is kept; loading forgets the partial tick; invalid values are rejected.
- `GameCalendarTests`: the last tick of a day is still that day and the next tick starts the new one; crossing three days at once announces days 1, 2, 3 in order; loading announces nothing; the calendar refuses to go backwards.
- `EventBusTests`: a message reaches every subscriber of its kind, in subscription order, and nobody else; unsubscribing works, even from inside a handler.
- `SimulationTests`: systems tick in the order they were added; the day is announced before the systems tick; the same total time in 4 long frames or 40 short frames gives the same 10 ticks; loading sets the tick and day without running anything.

To check by hand: run the tests in the Test Runner. There is nothing to see in the scene until the simulation is connected to it.

## Likely questions

**What is a tick and why 0.1 s?** A tick is one step of the game rules. Ten per second is fine enough that resources and energy change smoothly for the player, and coarse enough to cost almost nothing to calculate. The value is one constant and can be changed.

**Does the game run slower on a slow computer?** No. Fewer frames are drawn, but each frame then covers more time and runs more ticks. Only when a frame takes longer than 0.25 s is the extra time dropped, to avoid a sudden jump.

**How does game speed work?** The time of each frame is multiplied by the speed before it is turned into ticks. At speed 2 twice as many ticks run per real second; each tick still covers 0.1 s of game time, so the rules behave identically.

**How do you know which day it is?** The tick count divided by the number of ticks in a day (28 800). The remainder tells how far the day has progressed.

**Why count days in ticks and not in seconds?** Whole numbers are exact. A decimal such as 0.1 is stored only approximately, so adding it thousands of times drifts by tiny amounts, enough to put a day boundary on the wrong tick.

**What happens to the day events when a save is loaded?** Nothing is raised. The loaded tick count already includes those days; raising them again would charge upkeep twice.

**How can production react to the dome being activated without knowing the dome code?** The dome module publishes a `DomeActivated` message on the event bus; any module that subscribed to that message is called. Neither side references the other.

**In what order do things happen within one tick?** First the calendar (and with it the daily rules, on the first tick of a day), then each system in the order it was added to the simulation. The order never changes, so results are repeatable.

**Why is `Step` there if the game uses `Update`?** `Step(n)` runs exactly n ticks regardless of real time, pause or speed. Tests use it to say "after 100 ticks, storage must hold exactly this much".
