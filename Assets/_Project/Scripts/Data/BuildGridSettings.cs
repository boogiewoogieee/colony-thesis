using UnityEngine;

namespace Colony.Data
{
    /// <summary>
    /// Size of the building grid and the single height at which every building stands.
    /// </summary>
    [CreateAssetMenu(fileName = "BuildGridSettings", menuName = "Colony/Build Grid Settings")]
    public sealed class BuildGridSettings : ScriptableObject
    {
        [SerializeField] private float _cellSizeMetres;
        [SerializeField] private int _columnCount;
        [SerializeField] private int _rowCount;
        [SerializeField] private float _buildLevelHeightMetres;

        /// <summary>Length of one side of a square grid cell.</summary>
        public float CellSizeMetres => _cellSizeMetres;

        /// <summary>Number of cells along the X axis.</summary>
        public int ColumnCount => _columnCount;

        /// <summary>Number of cells along the Z axis.</summary>
        public int RowCount => _rowCount;

        /// <summary>
        /// World height of the floor of every building. Terrain below it is bridged by the
        /// slabs and piles that are part of the building models.
        /// </summary>
        public float BuildLevelHeightMetres => _buildLevelHeightMetres;
    }
}
