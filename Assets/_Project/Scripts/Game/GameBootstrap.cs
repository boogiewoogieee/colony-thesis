using Colony.Building;
using Colony.Data;
using UnityEngine;

namespace Colony.Game
{
    /// <summary>
    /// The one place where the game's objects are created and handed to each other.
    /// Scene components never look each other up; they receive what they need from here.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        private const float HALF = 0.5f;

        [SerializeField] private BuildGridSettings _buildGridSettings;
        [SerializeField] private BuildingDefinition[] _placeableBuildingArray;
        [SerializeField] private Camera _camera;
        [SerializeField] private IsometricCameraRig _cameraRig;
        [SerializeField] private BuildingPlacementController _placementController;
        [SerializeField] private PlacedBuildingsView _placedBuildingsView;

        private PrototypeControls _controls;

        private void Awake()
        {
            if (!AreReferencesAssigned())
            {
                Debug.LogError(
                    $"{nameof(GameBootstrap)}: assign every field in the Inspector, including at " +
                    "least one placeable building. The game was not started.",
                    this);
                return;
            }

            BuildGridGeometry geometry = new BuildGridGeometry(
                _buildGridSettings.CellSizeMetres,
                _buildGridSettings.ColumnCount,
                _buildGridSettings.RowCount);
            BuildGrid grid = new BuildGrid(_buildGridSettings.ColumnCount, _buildGridSettings.RowCount);
            BuildingRegistry registry = new BuildingRegistry(grid);
            BuildSite site = new BuildSite(_camera, geometry, _buildGridSettings.BuildLevelHeightMetres);

            _controls = new PrototypeControls();
            _controls.Enable();

            _cameraRig.Initialize(_controls, _buildGridSettings.BuildLevelHeightMetres, GetPanLimitMetres());
            _placedBuildingsView.Initialize(registry, site);
            _placementController.Initialize(_controls, site, registry, _placeableBuildingArray);
        }

        private void OnDestroy()
        {
            _controls?.Dispose();
        }

        private bool AreReferencesAssigned()
        {
            return _buildGridSettings != null
                && _placeableBuildingArray != null
                && _placeableBuildingArray.Length > 0
                && _camera != null
                && _cameraRig != null
                && _placementController != null
                && _placedBuildingsView != null;
        }

        private Vector2 GetPanLimitMetres()
        {
            float halfCellSizeMetres = _buildGridSettings.CellSizeMetres * HALF;
            return new Vector2(
                _buildGridSettings.ColumnCount * halfCellSizeMetres,
                _buildGridSettings.RowCount * halfCellSizeMetres);
        }
    }
}
