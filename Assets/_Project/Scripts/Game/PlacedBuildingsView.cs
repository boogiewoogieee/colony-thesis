using Colony.Building;
using UnityEngine;

namespace Colony.Game
{
    /// <summary>
    /// Shows every building that is placed on the grid as a box in the scene.
    /// </summary>
    public sealed class PlacedBuildingsView : MonoBehaviour
    {
        [SerializeField] private Color _buildingColor = Color.gray;

        private BuildingRegistry _registry;
        private BuildSite _site;

        public void Initialize(BuildingRegistry registry, BuildSite site)
        {
            _registry = registry;
            _site = site;
            _registry.OnBuildingPlaced += ShowBuilding;
        }

        private void OnDestroy()
        {
            if (_registry != null)
            {
                _registry.OnBuildingPlaced -= ShowBuilding;
            }
        }

        private void ShowBuilding(PlacedBuilding placedBuilding)
        {
            BuildingPlaceholder placeholder = BuildingPlaceholder.Create(
                placedBuilding.Definition.DisplayName,
                transform);
            placeholder.StandOn(
                _site.GetWorldCenter(placedBuilding.Area),
                _site.GetSizeMetres(placedBuilding.Area));
            placeholder.SetColor(_buildingColor);
        }
    }
}
