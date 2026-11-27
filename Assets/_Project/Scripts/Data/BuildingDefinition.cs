using UnityEngine;

namespace Colony.Data
{
    /// <summary>
    /// Describes one type of building: its identity and how many grid cells it covers.
    /// </summary>
    [CreateAssetMenu(fileName = "BuildingDefinition", menuName = "Colony/Building Definition")]
    public sealed class BuildingDefinition : ScriptableObject
    {
        private const int MIN_FOOTPRINT_CELLS = 1;

        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField, Min(MIN_FOOTPRINT_CELLS)] private int _footprintColumns = MIN_FOOTPRINT_CELLS;
        [SerializeField, Min(MIN_FOOTPRINT_CELLS)] private int _footprintRows = MIN_FOOTPRINT_CELLS;

        /// <summary>Stable identifier used by save files; never shown to the player.</summary>
        public string Id => _id;

        public string DisplayName => _displayName;

        /// <summary>Number of grid cells the building covers along the X axis.</summary>
        public int FootprintColumns => _footprintColumns;

        /// <summary>Number of grid cells the building covers along the Z axis.</summary>
        public int FootprintRows => _footprintRows;

        internal static BuildingDefinition CreateForTests(
            string id,
            int footprintColumns,
            int footprintRows)
        {
            BuildingDefinition definition = CreateInstance<BuildingDefinition>();
            definition._id = id;
            definition._displayName = id;
            definition._footprintColumns = footprintColumns;
            definition._footprintRows = footprintRows;
            return definition;
        }
    }
}
