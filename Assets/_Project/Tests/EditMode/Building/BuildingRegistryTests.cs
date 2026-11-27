using System;
using Colony.Building;
using Colony.Data;
using NUnit.Framework;

namespace Colony.Tests.Building
{
    public sealed class BuildingRegistryTests
    {
        private const int GRID_SIZE_CELLS = 10;

        private BuildingRegistry _registry;
        private BuildingDefinition _twoByTwoBuilding;
        private int _placedEventCount;

        [SetUp]
        public void CreateEmptyRegistry()
        {
            _registry = new BuildingRegistry(new BuildGrid(GRID_SIZE_CELLS, GRID_SIZE_CELLS));
            _twoByTwoBuilding = BuildingDefinition.CreateForTests(
                "two-by-two",
                footprintColumns: 2,
                footprintRows: 2);
            _placedEventCount = 0;
            _registry.OnBuildingPlaced += CountPlacedEvent;
        }

        [TearDown]
        public void DestroyDefinition()
        {
            UnityEngine.Object.DestroyImmediate(_twoByTwoBuilding);
        }

        [Test]
        public void Constructor_WithoutAGrid_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new BuildingRegistry(null));
        }

        [Test]
        public void TryPlace_OnFreeCells_AddsTheBuildingWithItsFootprint()
        {
            bool wasPlaced = _registry.TryPlace(_twoByTwoBuilding, new GridCell(3, 4));

            Assert.That(wasPlaced, Is.True);
            Assert.That(_registry.PlacedBuildings, Has.Count.EqualTo(1));
            PlacedBuilding placedBuilding = _registry.PlacedBuildings[0];
            Assert.That(placedBuilding.Definition, Is.SameAs(_twoByTwoBuilding));
            Assert.That(placedBuilding.Area.Origin, Is.EqualTo(new GridCell(3, 4)));
            Assert.That(placedBuilding.Area.ColumnCount, Is.EqualTo(2));
            Assert.That(placedBuilding.Area.RowCount, Is.EqualTo(2));
        }

        [Test]
        public void TryPlace_OnFreeCells_RaisesOnBuildingPlacedOnce()
        {
            _registry.TryPlace(_twoByTwoBuilding, new GridCell(3, 4));

            Assert.That(_placedEventCount, Is.EqualTo(1));
        }

        [Test]
        public void TryPlace_OnTheSameCellsTwice_RefusesTheSecondBuilding()
        {
            _registry.TryPlace(_twoByTwoBuilding, new GridCell(3, 4));

            bool wasPlacedAgain = _registry.TryPlace(_twoByTwoBuilding, new GridCell(3, 4));

            Assert.That(wasPlacedAgain, Is.False);
            Assert.That(_registry.PlacedBuildings, Has.Count.EqualTo(1));
            Assert.That(_placedEventCount, Is.EqualTo(1));
        }

        [Test]
        public void TryPlace_PartlyOutsideTheGrid_RefusesAndChangesNothing()
        {
            bool wasPlaced = _registry.TryPlace(
                _twoByTwoBuilding,
                new GridCell(GRID_SIZE_CELLS - 1, 0));

            Assert.That(wasPlaced, Is.False);
            Assert.That(_registry.PlacedBuildings, Is.Empty);
            Assert.That(_placedEventCount, Is.EqualTo(0));
        }

        [Test]
        public void TryPlace_NextToAnotherBuilding_PlacesBoth()
        {
            _registry.TryPlace(_twoByTwoBuilding, new GridCell(0, 0));

            bool wasPlaced = _registry.TryPlace(_twoByTwoBuilding, new GridCell(2, 0));

            Assert.That(wasPlaced, Is.True);
            Assert.That(_registry.PlacedBuildings, Has.Count.EqualTo(2));
        }

        [Test]
        public void TryPlace_WithoutADefinition_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _registry.TryPlace(null, new GridCell(0, 0)));
        }

        [Test]
        public void CheckPlacement_OnFreeCells_ReturnsAllowedWithoutPlacing()
        {
            PlacementResult result = _registry.CheckPlacement(_twoByTwoBuilding, new GridCell(3, 4));

            Assert.That(result, Is.EqualTo(PlacementResult.Allowed));
            Assert.That(_registry.PlacedBuildings, Is.Empty);
        }

        [Test]
        public void CheckPlacement_OverAPlacedBuilding_ReturnsOccupied()
        {
            _registry.TryPlace(_twoByTwoBuilding, new GridCell(3, 4));

            PlacementResult result = _registry.CheckPlacement(_twoByTwoBuilding, new GridCell(4, 5));

            Assert.That(result, Is.EqualTo(PlacementResult.Occupied));
        }

        [Test]
        public void CheckPlacement_PartlyOutsideTheGrid_ReturnsOutsideGrid()
        {
            PlacementResult result = _registry.CheckPlacement(_twoByTwoBuilding, new GridCell(-1, 0));

            Assert.That(result, Is.EqualTo(PlacementResult.OutsideGrid));
        }

        [Test]
        public void CheckPlacement_WithoutADefinition_Throws()
        {
            Assert.Throws<ArgumentNullException>(
                () => _registry.CheckPlacement(null, new GridCell(0, 0)));
        }

        private void CountPlacedEvent(PlacedBuilding placedBuilding)
        {
            _placedEventCount++;
        }
    }
}
