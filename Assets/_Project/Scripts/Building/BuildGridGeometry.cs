using System;

namespace Colony.Building
{
    /// <summary>
    /// Converts between positions in metres and grid cells. The grid is centred on the
    /// origin of the build level, so cell (0, 0) lies in the corner with the lowest X and Z.
    /// </summary>
    public sealed class BuildGridGeometry
    {
        private const int MIN_CELL_COUNT = 1;
        private const float HALF = 0.5f;

        private readonly int _columnCount;
        private readonly int _rowCount;

        public BuildGridGeometry(float cellSizeMetres, int columnCount, int rowCount)
        {
            if (!IsPositiveFiniteNumber(cellSizeMetres))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cellSizeMetres),
                    cellSizeMetres,
                    "The cell size must be a positive number of metres.");
            }

            if (columnCount < MIN_CELL_COUNT)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(columnCount),
                    columnCount,
                    "The grid must have at least one column.");
            }

            if (rowCount < MIN_CELL_COUNT)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(rowCount),
                    rowCount,
                    "The grid must have at least one row.");
            }

            CellSizeMetres = cellSizeMetres;
            _columnCount = columnCount;
            _rowCount = rowCount;
        }

        public float CellSizeMetres { get; }

        /// <summary>
        /// Returns the footprint of the given size whose centre is nearest to the point.
        /// The result may lie partly or fully outside the grid; <see cref="BuildGrid"/> decides
        /// whether it is allowed.
        /// </summary>
        public GridArea GetAreaCenteredAt(GroundPoint point, int columnCount, int rowCount)
        {
            int originColumn = GetNearestOriginIndex(point.X, columnCount, _columnCount);
            int originRow = GetNearestOriginIndex(point.Z, rowCount, _rowCount);
            return new GridArea(new GridCell(originColumn, originRow), columnCount, rowCount);
        }

        /// <summary>Returns the centre of the area in metres.</summary>
        public GroundPoint GetCenter(GridArea area)
        {
            float x = GetCenterMetres(area.Origin.Column, area.ColumnCount, _columnCount);
            float z = GetCenterMetres(area.Origin.Row, area.RowCount, _rowCount);
            return new GroundPoint(x, z);
        }

        private static bool IsPositiveFiniteNumber(float value)
        {
            return value > 0f && !float.IsInfinity(value);
        }

        private int GetNearestOriginIndex(float metresFromGridCenter, int footprintCells, int gridCells)
        {
            float cellsFromGridEdge = (metresFromGridCenter / CellSizeMetres) + (gridCells * HALF);
            float originInCells = cellsFromGridEdge - (footprintCells * HALF);

            // Floor(value + half) rounds a tie upward every time; Math.Round would round ties
            // to the even number and make the footprint jump unevenly at cell borders.
            return (int)MathF.Floor(originInCells + HALF);
        }

        private float GetCenterMetres(int originIndex, int footprintCells, int gridCells)
        {
            float centerInCells = originIndex + (footprintCells * HALF);
            return (centerInCells - (gridCells * HALF)) * CellSizeMetres;
        }
    }
}
