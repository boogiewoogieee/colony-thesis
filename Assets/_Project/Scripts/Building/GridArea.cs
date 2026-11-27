using System;

namespace Colony.Building
{
    /// <summary>
    /// A rectangle of grid cells: the cells one building covers.
    /// </summary>
    public readonly struct GridArea
    {
        private const int MIN_CELL_COUNT = 1;

        public GridArea(GridCell origin, int columnCount, int rowCount)
        {
            if (columnCount < MIN_CELL_COUNT)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(columnCount),
                    columnCount,
                    "An area must cover at least one column.");
            }

            if (rowCount < MIN_CELL_COUNT)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(rowCount),
                    rowCount,
                    "An area must cover at least one row.");
            }

            Origin = origin;
            ColumnCount = columnCount;
            RowCount = rowCount;
        }

        /// <summary>The covered cell with the lowest column and row.</summary>
        public GridCell Origin { get; }

        public int ColumnCount { get; }

        public int RowCount { get; }

        /// <summary>First column past the area (exclusive end).</summary>
        public int EndColumn => Origin.Column + ColumnCount;

        /// <summary>First row past the area (exclusive end).</summary>
        public int EndRow => Origin.Row + RowCount;

        public override string ToString()
        {
            return $"{ColumnCount}x{RowCount} cells at {Origin}";
        }
    }
}
