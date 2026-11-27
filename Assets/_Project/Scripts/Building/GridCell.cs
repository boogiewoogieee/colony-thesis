using System;

namespace Colony.Building
{
    /// <summary>
    /// Address of one square of the building grid. Cells outside the grid are representable,
    /// so a footprint hanging over the edge can be described and then refused.
    /// </summary>
    public readonly struct GridCell : IEquatable<GridCell>
    {
        public GridCell(int column, int row)
        {
            Column = column;
            Row = row;
        }

        /// <summary>Index along the X axis, starting at 0 on the grid's low edge.</summary>
        public int Column { get; }

        /// <summary>Index along the Z axis, starting at 0 on the grid's low edge.</summary>
        public int Row { get; }

        public static bool operator ==(GridCell left, GridCell right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GridCell left, GridCell right)
        {
            return !left.Equals(right);
        }

        public bool Equals(GridCell other)
        {
            return Column == other.Column && Row == other.Row;
        }

        public override bool Equals(object obj)
        {
            return obj is GridCell other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Column, Row);
        }

        public override string ToString()
        {
            return $"({Column}, {Row})";
        }
    }
}
