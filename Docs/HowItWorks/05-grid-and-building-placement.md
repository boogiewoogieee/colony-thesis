# 05 · Grid and building placement

**In one sentence:** the colony's ground is divided into invisible square cells, every building covers a whole number of them, and all buildings stand at one fixed height called the build level.

**Status:** this document covers the rules (which cells a building covers, where it may stand). How the player sees and controls placement in the scene is added with the placement view.

## Why we need it

Buildings must not overlap (GDD §3), and each one has a fixed size. Checking this with free positions and model shapes would be slow and unpredictable. A grid turns the question "do these two buildings collide?" into "is any of these cells already taken?", which is a simple lookup.

The terrain is not flat, but our buildings are modelled on concrete slabs and piles. So that every model fits everywhere, all buildings stand at exactly the same height. The slab or piles of the model reach down to the uneven ground below.

## How it works

1. **The grid.** The playable area is 100 × 100 cells, each 10 m × 10 m (1 km × 1 km in total). The grid is centred on the middle of the map. Cell (0, 0) is the corner with the lowest X and Z; columns run along X, rows along Z. The grid is never drawn: the player only sees the building preview jump from cell to cell.
2. **Footprint.** Each building type says how many cells it covers, for example 2 columns × 2 rows. This is its footprint. A Silo is 1 × 1, a Residential Building 2 × 2, a Factory 3 × 2 (placeholder sizes).
3. **Snapping.** When the player points at the ground, we look for the footprint whose centre is nearest to the pointer. A 1 × 1 building is centred in the cell under the pointer. A 2 × 2 building is centred on the nearest corner where four cells meet. Each axis is snapped on its own, so a 3 × 2 building is centred in a cell along X and on a cell border along Z.
4. **Checking.** The footprint is checked in two steps: first, are all its cells inside the grid; second, is every one of them free. The answer is one of three values: allowed, outside the grid, occupied.
5. **Placing.** If the answer is "allowed", the cells are marked as taken, the building is added to the list of placed buildings, and an event announces it so the scene can show it.
6. **Build level.** A position on the grid has only two numbers (X and Z). The height is not part of it: it is one value for the whole map, kept in the grid settings. Nothing in the rules can give a building a different height.

Example: the pointer is 1 m east and 1 m north of the map centre. For a Silo (1 × 1) the result is cell (50, 50), whose centre is 5 m east and 5 m north of the map centre. For a Residential Building (2 × 2) the result is cells (49, 49) to (50, 50), centred exactly on the map centre.

### Key classes

| Class | Job |
| --- | --- |
| `BuildGridSettings` (data) | Cell size, number of columns and rows, height of the build level. |
| `BuildingDefinition` (data) | One building type: id, name, footprint in cells. |
| `GridCell` | Address of one cell: column and row. |
| `GridArea` | A rectangle of cells: the footprint of a building at a given place. |
| `GroundPoint` | A position on the build level in metres (X and Z only). |
| `BuildGridGeometry` | Converts metres to cells (snapping) and cells back to metres. |
| `BuildGrid` | Remembers which cells are taken; answers allowed / outside the grid / occupied. |
| `BuildingRegistry` | The list of placed buildings; the only way to add one; raises `OnBuildingPlaced`. |
| `PlacedBuilding` | One building on the grid: its type and its cells. |

All of these except the two data classes are plain C# with no connection to the scene, so they can be tested without pressing Play (see 02).

## Why this approach

| Option | Why not (or why) |
| --- | --- |
| Free placement, collision checked with Unity physics | Positions are unlimited, so buildings end up almost touching or slightly rotated; the check depends on colliders in the scene and cannot be tested without Play Mode. |
| Hexagonal grid (as in *Surviving Mars*) | Fits round domes well, but footprints, model sizes and the maths are harder; our buildings are rectangular. |
| Visible grid lines while building | Decided against by the team: the preview snapping to cells is enough feedback and keeps the view clean. |
| Buildings follow the terrain height | Every model would need to fit every slope; buildings next to each other would stand at different heights. |
| Flattening the terrain under each building | Needs terrain editing at run time and changes the artist's terrain. |
| **Square hidden grid, one build level (chosen)** | Overlap is a table lookup, sizes are whole cells, every model fits everywhere, and the rules are plain code that tests can check. |

Why the cell size and the build level are in a data asset and not in the code: they are design values the team may change (GDD numbers are not final). Changing them is an edit in the Inspector, not a code change.

## Where it lives

- `Assets/_Project/Scripts/Building/`: `GridCell`, `GridArea`, `GroundPoint`, `PlacementResult`, `BuildGrid`, `BuildGridGeometry`, `PlacedBuilding`, `BuildingRegistry`.
- `Assets/_Project/Scripts/Data/`: `BuildGridSettings`, `BuildingDefinition`.
- `Assets/_Project/Data/BuildGridSettings.asset`: the grid values.
- `Assets/_Project/Data/Buildings/`: one asset per building type.
- `Assets/_Project/Tests/EditMode/Building/`: the tests.

## How we know it works

Edit Mode tests (**Window > General > Test Runner > EditMode**), 53 in total:

- `BuildGridGeometryTests`: snapping for odd and even footprints, a pointer exactly on a cell border, a pointer outside the grid, and that the snapped centre is never more than half a cell from the pointer.
- `BuildGridTests`: a footprint touching the grid edge is allowed, one cell past it is refused; overlapping one taken cell is refused; standing next to a building is allowed.
- `BuildingRegistryTests`: placing adds the building and raises the event once; a second building on the same cells is refused and changes nothing.
- `GridAreaTests`: a footprint with zero or negative size is rejected.

To check by hand: open `BuildGridSettings.asset` and a building asset in the Inspector and read the values the rules use.

## Likely questions

**Why is there no visible grid if the game uses one?** The grid is a rule, not a picture. The player feels it because the preview moves in steps of one cell. Drawing lines was a design choice we decided against.

**How do you check that two buildings do not overlap?** Each cell remembers whether it is taken. A new building is allowed only if none of its cells is taken. We never compare buildings with each other.

**Why do all buildings stand at the same height when the terrain is uneven?** The models are built on slabs and piles that bridge the gap to the ground. A single height means every model fits everywhere and neighbouring buildings line up.

**What happens at the edge of the map?** A footprint that reaches past the last column or row gets the answer "outside the grid" and cannot be placed.

**Why does a 2 × 2 building snap to a corner and a 1 × 1 to the middle of a cell?** The centre of an even footprint lies on the line between cells, the centre of an odd one in the middle of a cell. We always pick the footprint whose centre is nearest to the pointer.

**Why are cell size and footprints not written in the code?** They are balance values. In a data asset a designer can change them in the Inspector, and the tests still run with their own fixed numbers.

**Can a placed building be removed, rotated or saved yet?** Not yet. Removal, rotation, build costs and saving are separate later tasks.
