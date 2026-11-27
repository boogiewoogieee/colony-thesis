using System;
using Colony.Data;

namespace Colony.Building
{
    /// <summary>
    /// One building standing on the grid: what it is and which cells it covers.
    /// </summary>
    public sealed class PlacedBuilding
    {
        public PlacedBuilding(BuildingDefinition definition, GridArea area)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Area = area;
        }

        public BuildingDefinition Definition { get; }

        public GridArea Area { get; }
    }
}
