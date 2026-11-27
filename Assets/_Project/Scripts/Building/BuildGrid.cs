using System;

namespace Colony.Building
{
    /// <summary>
    /// Remembers which grid cells are covered by a building.
    /// </summary>
    public sealed class BuildGrid
    {
        private const int MIN_CELL_COUNT = 1;

        private readonly bool[,] _isCellOccupied;

        public BuildGrid(int columnCount, int rowCount)
        {
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

            ColumnCount = columnCount;
            RowCount = rowCount;
            _isCellOccupied = new bool[columnCount, rowCount];
        }

        public int ColumnCount { get; }

        public int RowCount { get; }

        public PlacementResult CheckPlacement(GridArea area)
        {
            if (!Contains(area))
            {
                return PlacementResult.OutsideGrid;
            }

            if (HasOccupiedCell(area))
            {
                return PlacementResult.Occupied;
            }

            return PlacementResult.Allowed;
        }

        /// <summary>
        /// Marks every cell of the area as covered. Call <see cref="CheckPlacement"/> first:
        /// occupying an area that is not allowed is a programming error.
        /// </summary>
        public void Occupy(GridArea area)
        {
            PlacementResult result = CheckPlacement(area);
            if (result != PlacementResult.Allowed)
            {
                throw new InvalidOperationException($"Cannot occupy {area}: {result}.");
            }

            for (int column = area.Origin.Column; column < area.EndColumn; column++)
            {
                for (int row = area.Origin.Row; row < area.EndRow; row++)
                {
                    _isCellOccupied[column, row] = true;
                }
            }
        }

        public bool IsOccupied(GridCell cell)
        {
            if (!Contains(cell))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cell),
                    cell,
                    $"The cell is outside the {ColumnCount}x{RowCount} grid.");
            }

            return _isCellOccupied[cell.Column, cell.Row];
        }

        private bool Contains(GridCell cell)
        {
            return cell.Column >= 0
                && cell.Column < ColumnCount
                && cell.Row >= 0
                && cell.Row < RowCount;
        }

        private bool Contains(GridArea area)
        {
            return area.Origin.Column >= 0
                && area.EndColumn <= ColumnCount
                && area.Origin.Row >= 0
                && area.EndRow <= RowCount;
        }

        private bool HasOccupiedCell(GridArea area)
        {
            for (int column = area.Origin.Column; column < area.EndColumn; column++)
            {
                for (int row = area.Origin.Row; row < area.EndRow; row++)
                {
                    if (_isCellOccupied[column, row])
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
