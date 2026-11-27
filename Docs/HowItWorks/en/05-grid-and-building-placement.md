# 05 · Grid and building placement

**In one sentence:** the colony's ground is divided into invisible square cells, every building covers a whole number of them, and all buildings stand at one fixed height called the build level.

**Status:** prototype. Buildings are shown as plain boxes, cost nothing, and cannot be removed, rotated or saved yet.

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

### In the scene: pointing, preview and camera

1. **Where the mouse points.** We draw an imaginary straight line (a ray) from the camera through the mouse pointer. We do not ask where this line hits the terrain. We ask where it crosses a flat, horizontal, invisible plane at the height of the build level. This is the whole "height lock": hills and holes in the terrain cannot change the result, and the terrain needs no collision shape for building.
2. **Preview.** The crossing point is snapped to cells (step 3 above), and a preview box is shown there, standing on the build level. It is green when the rules answer "allowed" and red otherwise (outside the grid, or occupied).
3. **Placing.** A left click asks the rules to place the selected building. If they agree, a grey box appears in its place. Number keys 1 to 3 choose the building: Silo, Residential Building, Factory.
4. **Camera.** The camera looks down at a point on the build level from a fixed angle. W, A, S, D or the arrow keys move that point; the mouse wheel changes the distance. The point cannot leave the grid.
5. **Who connects what.** One object in the scene, `GameBootstrap`, creates the grid, the rules, and the controls when the scene starts, and hands them to the camera, the placement controller and the view. No script searches for another one.

| Class (all in `Colony.Game`) | Job |
| --- | --- |
| `GameBootstrap` | Creates everything at scene start and connects the parts. |
| `PrototypeControls` | The keys and mouse buttons of the prototype, read through Unity's Input System. |
| `BuildSite` | The build level in the scene: pointer on the screen → cells, and cells → position in the world. |
| `BuildingPlacementController` | Every frame: pick building, find the cells under the pointer, show the preview, place on click. |
| `BuildingPlaceholder` | A box standing in for a building model: sized to the footprint, 8 m tall. |
| `PlacedBuildingsView` | Listens for `OnBuildingPlaced` and creates a box for each placed building. |
| `IsometricCameraRig` | Camera angle, pan and zoom. |

## Why this approach

| Option | Why not (or why) |
| --- | --- |
| Free placement, collision checked with Unity physics | Positions are unlimited, so buildings end up almost touching or slightly rotated; the check depends on colliders in the scene and cannot be tested without Play Mode. |
| Hexagonal grid (as in *Surviving Mars*) | Fits round domes well, but footprints, model sizes and the maths are harder; our buildings are rectangular. |
| Visible grid lines while building | Decided against by the team: the preview snapping to cells is enough feedback and keeps the view clean. |
| Buildings follow the terrain height | Every model would need to fit every slope; buildings next to each other would stand at different heights. |
| Flattening the terrain under each building | Needs terrain editing at run time and changes the artist's terrain. |
| **Square hidden grid, one build level (chosen)** | Overlap is a table lookup, sizes are whole cells, every model fits everywhere, and the rules are plain code that tests can check. |
| Finding the pointed spot with a physics raycast against the terrain | Needs a collision shape for the whole terrain (expensive for a detailed mesh) and returns the terrain's height, which we do not want. A ray against a flat plane is one small calculation. |
| Cinemachine camera package | Planned for the full camera (T09). Not installed yet; the prototype uses one small script so no package had to be added. |

Why the cell size and the build level are in a data asset and not in the code: they are design values the team may change (GDD numbers are not final). Changing them is an edit in the Inspector, not a code change.

## Where it lives

- `Assets/_Project/Scripts/Building/`: `GridCell`, `GridArea`, `GroundPoint`, `PlacementResult`, `BuildGrid`, `BuildGridGeometry`, `PlacedBuilding`, `BuildingRegistry`.
- `Assets/_Project/Scripts/Data/`: `BuildGridSettings`, `BuildingDefinition`.
- `Assets/_Project/Data/BuildGridSettings.asset`: the grid values.
- `Assets/_Project/Data/Buildings/`: one asset per building type.
- `Assets/_Project/Tests/EditMode/Building/`: the tests.
- `Assets/_Project/Scripts/Game/`: `GameBootstrap`, `PrototypeControls`, `BuildSite`, `BuildingPlacementController`, `BuildingPlaceholder`, `PlacedBuildingsView`, `IsometricCameraRig`.
- `Assets/Scenes/Prototype.unity`: the prototype scene (camera, light, a flat 1 km placeholder ground, `Game Bootstrap`, `Building Placement`, `Placed Buildings`).

## How we know it works

Edit Mode tests (**Window > General > Test Runner > EditMode**), 53 in total:

- `BuildGridGeometryTests`: snapping for odd and even footprints, a pointer exactly on a cell border, a pointer outside the grid, and that the snapped centre is never more than half a cell from the pointer.
- `BuildGridTests`: a footprint touching the grid edge is allowed, one cell past it is refused; overlapping one taken cell is refused; standing next to a building is allowed.
- `BuildingRegistryTests`: placing adds the building and raises the event once; a second building on the same cells is refused and changes nothing.
- `GridAreaTests`: a footprint with zero or negative size is rejected.

The scene part (pointer, preview, camera) has no automated tests; it is checked by hand:

1. Open `Assets/Scenes/Prototype.unity` and press Play.
2. Move the mouse over the ground: a green box follows it in steps of one cell.
3. Left click: a grey box stays. Point at it again: the preview turns red and a click does nothing.
4. Press 2, then 3: the preview changes to 2 × 2 and 3 × 2 cells.
5. Move with W, A, S, D and zoom with the mouse wheel. Move the preview past the edge of the ground: it turns red.
6. Select any placed box in the Hierarchy: its Y position is always 4 (build level 0 plus half of the 8 m box height), wherever it stands.
7. Stop, change `Build Level Height Metres` in `BuildGridSettings.asset` to 5, press Play: every box now stands 5 m higher. Set it back to 0.

## Likely questions

**Why is there no visible grid if the game uses one?** The grid is a rule, not a picture. The player feels it because the preview moves in steps of one cell. Drawing lines was a design choice we decided against.

**How do you check that two buildings do not overlap?** Each cell remembers whether it is taken. A new building is allowed only if none of its cells is taken. We never compare buildings with each other.

**Why do all buildings stand at the same height when the terrain is uneven?** The models are built on slabs and piles that bridge the gap to the ground. A single height means every model fits everywhere and neighbouring buildings line up.

**How does the game know where the mouse points if it ignores the terrain?** It draws a line from the camera through the pointer and calculates where that line crosses a flat plane at the build level. No terrain data is involved.

**What is `GameBootstrap` for?** It is the single place where the parts of the game are created and connected when the scene starts. Because of it, no script needs a global object or a search by name to find another script.

**What happens at the edge of the map?** A footprint that reaches past the last column or row gets the answer "outside the grid" and cannot be placed.

**Why does a 2 × 2 building snap to a corner and a 1 × 1 to the middle of a cell?** The centre of an even footprint lies on the line between cells, the centre of an odd one in the middle of a cell. We always pick the footprint whose centre is nearest to the pointer.

**Why are cell size and footprints not written in the code?** They are balance values. In a data asset a designer can change them in the Inspector, and the tests still run with their own fixed numbers.

**Can a placed building be removed, rotated or saved yet?** Not yet. Removal, rotation, build costs and saving are separate later tasks.
