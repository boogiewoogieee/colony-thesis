using System;
using Colony.Building;
using NUnit.Framework;

namespace Colony.Tests.Building
{
    public sealed class BuildGridGeometryTests
    {
        private const float CELL_SIZE_METRES = 10f;
        private const int COLUMN_COUNT = 100;
        private const int ROW_COUNT = 100;
        private const float TOLERANCE_METRES = 0.0001f;

        private BuildGridGeometry _geometry;

        [SetUp]
        public void CreateGeometry()
        {
            _geometry = new BuildGridGeometry(CELL_SIZE_METRES, COLUMN_COUNT, ROW_COUNT);
        }

        [TestCase(0f)]
        [TestCase(-10f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Constructor_WithInvalidCellSize_Throws(float cellSizeMetres)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new BuildGridGeometry(cellSizeMetres, COLUMN_COUNT, ROW_COUNT));
        }

        [TestCase(0, 1)]
        [TestCase(1, 0)]
        public void Constructor_WithNonPositiveGridSize_Throws(int columnCount, int rowCount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new BuildGridGeometry(CELL_SIZE_METRES, columnCount, rowCount));
        }

        [Test]
        public void GetAreaCenteredAt_SingleCellJustPastTheGridCenter_ReturnsTheMiddleCell()
        {
            GroundPoint point = new GroundPoint(1f, 1f);

            GridArea area = _geometry.GetAreaCenteredAt(point, columnCount: 1, rowCount: 1);

            Assert.That(area.Origin, Is.EqualTo(new GridCell(50, 50)));
        }

        [Test]
        public void GetAreaCenteredAt_SingleCellJustBeforeTheGridCenter_ReturnsThePreviousCell()
        {
            GroundPoint point = new GroundPoint(-1f, -1f);

            GridArea area = _geometry.GetAreaCenteredAt(point, columnCount: 1, rowCount: 1);

            Assert.That(area.Origin, Is.EqualTo(new GridCell(49, 49)));
        }

        [Test]
        public void GetAreaCenteredAt_SingleCellAtTheLowCorner_ReturnsCellZero()
        {
            GroundPoint point = new GroundPoint(-499f, -499f);

            GridArea area = _geometry.GetAreaCenteredAt(point, columnCount: 1, rowCount: 1);

            Assert.That(area.Origin, Is.EqualTo(new GridCell(0, 0)));
        }

        [Test]
        public void GetAreaCenteredAt_SingleCellExactlyOnACellBorder_ReturnsTheHigherCell()
        {
            GroundPoint point = new GroundPoint(10f, 20f);

            GridArea area = _geometry.GetAreaCenteredAt(point, columnCount: 1, rowCount: 1);

            Assert.That(area.Origin, Is.EqualTo(new GridCell(51, 52)));
        }

        [Test]
        public void GetAreaCenteredAt_EvenFootprintNearACellCorner_SurroundsThatCorner()
        {
            GroundPoint point = new GroundPoint(1f, 1f);

            GridArea area = _geometry.GetAreaCenteredAt(point, columnCount: 2, rowCount: 2);

            Assert.That(area.Origin, Is.EqualTo(new GridCell(49, 49)));
        }

        [Test]
        public void GetAreaCenteredAt_MixedFootprint_SnapsEachAxisOnItsOwn()
        {
            GroundPoint point = new GroundPoint(5f, 1f);

            GridArea area = _geometry.GetAreaCenteredAt(point, columnCount: 3, rowCount: 2);

            Assert.That(area.Origin, Is.EqualTo(new GridCell(49, 49)));
        }

        [Test]
        public void GetAreaCenteredAt_PointBeyondTheGrid_ReturnsCellsOutsideTheGrid()
        {
            GroundPoint point = new GroundPoint(-505f, 505f);

            GridArea area = _geometry.GetAreaCenteredAt(point, columnCount: 1, rowCount: 1);

            Assert.That(area.Origin, Is.EqualTo(new GridCell(-1, 100)));
        }

        [Test]
        public void GetAreaCenteredAt_KeepsTheRequestedFootprintSize()
        {
            GridArea area = _geometry.GetAreaCenteredAt(
                new GroundPoint(0f, 0f),
                columnCount: 3,
                rowCount: 2);

            Assert.That(area.ColumnCount, Is.EqualTo(3));
            Assert.That(area.RowCount, Is.EqualTo(2));
        }

        [Test]
        public void GetCenter_OfTheLowCornerCell_IsHalfACellInsideTheGridEdge()
        {
            GridArea area = new GridArea(new GridCell(0, 0), columnCount: 1, rowCount: 1);

            GroundPoint center = _geometry.GetCenter(area);

            Assert.That(center.X, Is.EqualTo(-495f).Within(TOLERANCE_METRES));
            Assert.That(center.Z, Is.EqualTo(-495f).Within(TOLERANCE_METRES));
        }

        [Test]
        public void GetCenter_OfAnEvenFootprintAroundTheGridCenter_IsTheGridCenter()
        {
            GridArea area = new GridArea(new GridCell(49, 49), columnCount: 2, rowCount: 2);

            GroundPoint center = _geometry.GetCenter(area);

            Assert.That(center.X, Is.EqualTo(0f).Within(TOLERANCE_METRES));
            Assert.That(center.Z, Is.EqualTo(0f).Within(TOLERANCE_METRES));
        }

        [Test]
        public void GetCenter_OfASnappedArea_IsWithinHalfACellOfThePointer()
        {
            GroundPoint point = new GroundPoint(123.4f, -77.7f);
            GridArea area = _geometry.GetAreaCenteredAt(point, columnCount: 3, rowCount: 2);

            GroundPoint center = _geometry.GetCenter(area);

            float halfCellMetres = CELL_SIZE_METRES / 2f;
            Assert.That(Math.Abs(center.X - point.X), Is.LessThanOrEqualTo(halfCellMetres));
            Assert.That(Math.Abs(center.Z - point.Z), Is.LessThanOrEqualTo(halfCellMetres));
        }
    }
}
