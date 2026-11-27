using System;
using System.Collections.Generic;
using Colony.Data;

namespace Colony.Building
{
    /// <summary>
    /// The list of buildings standing on the grid, and the only way to add one.
    /// </summary>
    public sealed class BuildingRegistry
    {
        private readonly BuildGrid _grid;
        private readonly List<PlacedBuilding> _placedBuildingList = new List<PlacedBuilding>();

        public BuildingRegistry(BuildGrid grid)
        {
            _grid = grid ?? throw new ArgumentNullException(nameof(grid));
        }

        /// <summary>Raised after a building has been added to the grid.</summary>
        public event Action<PlacedBuilding> OnBuildingPlaced;

        public IReadOnlyList<PlacedBuilding> PlacedBuildings => _placedBuildingList;

        /// <summary>
        /// Answers whether the building may stand with its lowest corner on the origin cell.
        /// </summary>
        public PlacementResult CheckPlacement(BuildingDefinition definition, GridCell origin)
        {
            return _grid.CheckPlacement(GetFootprintArea(definition, origin));
        }

        /// <summary>
        /// Places the building if its cells are inside the grid and free.
        /// Returns false, changing nothing, when they are not.
        /// </summary>
        public bool TryPlace(BuildingDefinition definition, GridCell origin)
        {
            GridArea area = GetFootprintArea(definition, origin);
            if (_grid.CheckPlacement(area) != PlacementResult.Allowed)
            {
                return false;
            }

            _grid.Occupy(area);
            PlacedBuilding placedBuilding = new PlacedBuilding(definition, area);
            _placedBuildingList.Add(placedBuilding);
            OnBuildingPlaced?.Invoke(placedBuilding);
            return true;
        }

        private static GridArea GetFootprintArea(BuildingDefinition definition, GridCell origin)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            return new GridArea(origin, definition.FootprintColumns, definition.FootprintRows);
        }
    }
}
