using System;
using Colony.Building;
using NUnit.Framework;

namespace Colony.Tests.Building
{
    public sealed class GridAreaTests
    {
        [Test]
        public void Constructor_WithPositiveSize_ExposesOriginAndExclusiveEnds()
        {
            GridArea area = new GridArea(new GridCell(4, 7), columnCount: 3, rowCount: 2);

            Assert.That(area.Origin, Is.EqualTo(new GridCell(4, 7)));
            Assert.That(area.EndColumn, Is.EqualTo(7));
            Assert.That(area.EndRow, Is.EqualTo(9));
        }

        [Test]
        public void Constructor_WithSingleCell_EndsOneCellPastOrigin()
        {
            GridArea area = new GridArea(new GridCell(0, 0), columnCount: 1, rowCount: 1);

            Assert.That(area.EndColumn, Is.EqualTo(1));
            Assert.That(area.EndRow, Is.EqualTo(1));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_WithNonPositiveColumnCount_Throws(int columnCount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new GridArea(new GridCell(0, 0), columnCount, rowCount: 1));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Constructor_WithNonPositiveRowCount_Throws(int rowCount)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new GridArea(new GridCell(0, 0), columnCount: 1, rowCount));
        }
    }
}
