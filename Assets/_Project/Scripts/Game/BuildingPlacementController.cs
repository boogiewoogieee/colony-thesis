using System.Collections.Generic;
using Colony.Building;
using Colony.Data;
using UnityEngine;

namespace Colony.Game
{
    /// <summary>
    /// Lets the player pick a building, shows a preview that snaps to the grid under the
    /// pointer, and places the building on a click when the spot is allowed.
    /// </summary>
    public sealed class BuildingPlacementController : MonoBehaviour
    {
        private const string PREVIEW_OBJECT_NAME = "Building Preview";

        [SerializeField] private Color _allowedColor = Color.green;
        [SerializeField] private Color _refusedColor = Color.red;

        private PrototypeControls _controls;
        private BuildSite _site;
        private BuildingRegistry _registry;
        private IReadOnlyList<BuildingDefinition> _placeableBuildingList;
        private BuildingDefinition _selectedBuilding;
        private BuildingPlaceholder _preview;

        public void Initialize(
            PrototypeControls controls,
            BuildSite site,
            BuildingRegistry registry,
            IReadOnlyList<BuildingDefinition> placeableBuildingList)
        {
            _controls = controls;
            _site = site;
            _registry = registry;
            _placeableBuildingList = placeableBuildingList;
            _selectedBuilding = placeableBuildingList[0];
            _preview = BuildingPlaceholder.Create(PREVIEW_OBJECT_NAME, transform);
            _preview.Hide();
        }

        private void Start()
        {
            if (_controls == null)
            {
                Debug.LogError(
                    $"{nameof(BuildingPlacementController)} was not initialised by {nameof(GameBootstrap)}.",
                    this);
                enabled = false;
            }
        }

        private void Update()
        {
            SelectPickedBuilding();

            if (!_site.TryGetAreaUnderPointer(_controls.PointerPosition, _selectedBuilding, out GridArea area))
            {
                _preview.Hide();
                return;
            }

            PlacementResult result = _registry.CheckPlacement(_selectedBuilding, area.Origin);
            ShowPreview(area, result);

            if (_controls.WasPlacePressedThisFrame)
            {
                _registry.TryPlace(_selectedBuilding, area.Origin);
            }
        }

        private void SelectPickedBuilding()
        {
            int? pickedIndex = _controls.GetBuildingIndexPickedThisFrame();
            if (pickedIndex.HasValue && pickedIndex.Value < _placeableBuildingList.Count)
            {
                _selectedBuilding = _placeableBuildingList[pickedIndex.Value];
            }
        }

        private void ShowPreview(GridArea area, PlacementResult result)
        {
            _preview.StandOn(_site.GetWorldCenter(area), _site.GetSizeMetres(area));
            _preview.SetColor(result == PlacementResult.Allowed ? _allowedColor : _refusedColor);
            _preview.Show();
        }
    }
}
