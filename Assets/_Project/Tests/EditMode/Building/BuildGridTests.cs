using System;
using Colony.Building;
using NUnit.Framework;

namespace Colony.Tests.Building
{
    public sealed class BuildGridTests
    {
        private const int COLUMN_COUNT = 10;
        private const int ROW_COUNT = 8;

        private BuildGrid _grid;

        [SetUp]
        public void CreateEmptyGrid()
        {
            _grid = new BuildGrid(COLUMN_COUNT, ROW_COUNT);
        }

        [TestCase(0, 1)]
        [TestCase(1, 0)]
        [TestCase(-5, 5)]
        public void Constructor_WithNonPositiveSize_Throws(int columnCount, int rowCount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BuildGrid(columnCount, rowCount));
        }

        [Test]
        public void CheckPlacement_OnFreeCellsInsideGrid_ReturnsAllowed()
        {
            GridArea area = new GridArea(new GridCell(2, 3), columnCount: 2, rowCount: 2);

            Assert.That(_grid.CheckPlacement(area), Is.EqualTo(PlacementResult.Allowed));
        }

        [Test]
        public void CheckPlacement_TouchingTheFarCorner_ReturnsAllowed()
        {
            GridArea area = new GridArea(
                new GridCell(COLUMN_COUNT - 2, ROW_COUNT - 3),
                columnCount: 2,
                rowCount: 3);

            Assert.That(_grid.CheckPlacement(area), Is.EqualTo(PlacementResult.Allowed));
        }

        [Test]
        public void CheckPlacement_CoveringTheWholeGrid_ReturnsAllowed()
        {
            GridArea area = new GridArea(new GridCell(0, 0), COLUMN_COUNT, ROW_COUNT);

            Assert.That(_grid.CheckPlacement(area), Is.EqualTo(PlacementResult.Allowed));
        }

        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        [TestCase(COLUMN_COUNT - 1, 0)]
        [TestCase(0, ROW_COUNT - 1)]
        [TestCase(COLUMN_COUNT, ROW_COUNT)]
        public void CheckPlacement_ReachingPastAnEdge_ReturnsOutsideGrid(int originColumn, int originRow)
        {
            GridArea area = new GridArea(
                new GridCell(originColumn, originRow),
                columnCount: 2,
                rowCount: 2);

            Assert.That(_grid.CheckPlacement(area), Is.EqualTo(PlacementResult.OutsideGrid));
        }

        [Test]
        public void CheckPlacement_OverlappingOneOccupiedCell_ReturnsOccupied()
        {
            _grid.Occupy(new GridArea(new GridCell(4, 4), columnCount: 2, rowCount: 2));
            GridArea overlappingArea = new GridArea(new GridCell(5, 5), columnCount: 3, rowCount: 3);

            Assert.That(_grid.CheckPlacement(overlappingArea), Is.EqualTo(PlacementResult.Occupied));
        }

        [Test]
        public void CheckPlacement_NextToAnOccupiedArea_ReturnsAllowed()
        {
            _grid.Occupy(new GridArea(new GridCell(4, 4), columnCount: 2, rowCount: 2));
            GridArea neighbouringArea = new GridArea(new GridCell(6, 4), columnCount: 2, rowCount: 2);

            Assert.That(_grid.CheckPlacement(neighbouringArea), Is.EqualTo(PlacementResult.Allowed));
        }

        [Test]
        public void CheckPlacement_AskedTwice_DoesNotOccupyAnything()
        {
            GridArea area = new GridArea(new GridCell(1, 1), columnCount: 1, rowCount: 1);

            _grid.CheckPlacement(area);

            Assert.That(_grid.CheckPlacement(area), Is.EqualTo(PlacementResult.Allowed));
        }

        [Test]
        public void Occupy_AnAllowedArea_MarksExactlyItsCells()
        {
            _grid.Occupy(new GridArea(new GridCell(2, 3), columnCount: 2, rowCount: 1));

            Assert.That(_grid.IsOccupied(new GridCell(2, 3)), Is.True);
            Assert.That(_grid.IsOccupied(new GridCell(3, 3)), Is.True);
            Assert.That(_grid.IsOccupied(new GridCell(4, 3)), Is.False);
            Assert.That(_grid.IsOccupied(new GridCell(2, 4)), Is.False);
        }

        [Test]
        public void Occupy_AnOccupiedArea_Throws()
        {
            GridArea area = new GridArea(new GridCell(2, 3), columnCount: 2, rowCount: 2);
            _grid.Occupy(area);

            Assert.Throws<InvalidOperationException>(() => _grid.Occupy(area));
        }

        [Test]
        public void Occupy_AnAreaOutsideTheGrid_Throws()
        {
            GridArea area = new GridArea(new GridCell(-1, 0), columnCount: 2, rowCount: 2);

            Assert.Throws<InvalidOperationException>(() => _grid.Occupy(area));
        }

        [TestCase(-1, 0)]
        [TestCase(0, ROW_COUNT)]
        public void IsOccupied_ForACellOutsideTheGrid_Throws(int column, int row)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _grid.IsOccupied(new GridCell(column, row)));
        }
    }
}
