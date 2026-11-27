using System;
using Colony.Building;
using Colony.Data;
using UnityEngine;

namespace Colony.Game
{
    /// <summary>
    /// The build level as it lies in the scene: a flat, horizontal plane at one height.
    /// Translates between what the camera sees and the cells of the building grid.
    /// </summary>
    public sealed class BuildSite
    {
        private readonly Camera _camera;
        private readonly BuildGridGeometry _geometry;
        private readonly float _buildLevelHeightMetres;
        private readonly Plane _buildLevelPlane;

        public BuildSite(Camera camera, BuildGridGeometry geometry, float buildLevelHeightMetres)
        {
            _camera = camera != null ? camera : throw new ArgumentNullException(nameof(camera));
            _geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
            _buildLevelHeightMetres = buildLevelHeightMetres;
            _buildLevelPlane = new Plane(Vector3.up, new Vector3(0f, buildLevelHeightMetres, 0f));
        }

        /// <summary>
        /// Finds the cells the building would cover if centred under the pointer.
        /// The pointer is projected onto the build level, never onto the terrain, so the
        /// shape of the ground cannot change the result. Returns false when the pointer
        /// does not point at the build level (for example at the sky).
        /// </summary>
        public bool TryGetAreaUnderPointer(
            Vector2 pointerPosition,
            BuildingDefinition definition,
            out GridArea area)
        {
            Ray pointerRay = _camera.ScreenPointToRay(pointerPosition);
            if (!_buildLevelPlane.Raycast(pointerRay, out float distanceAlongRay))
            {
                area = default;
                return false;
            }

            Vector3 pointOnBuildLevel = pointerRay.GetPoint(distanceAlongRay);
            GroundPoint groundPoint = new GroundPoint(pointOnBuildLevel.x, pointOnBuildLevel.z);
            area = _geometry.GetAreaCenteredAt(
                groundPoint,
                definition.FootprintColumns,
                definition.FootprintRows);
            return true;
        }

        /// <summary>World position of the centre of the area, at the build level.</summary>
        public Vector3 GetWorldCenter(GridArea area)
        {
            GroundPoint center = _geometry.GetCenter(area);
            return new Vector3(center.X, _buildLevelHeightMetres, center.Z);
        }

        /// <summary>Width (X) and depth (Y of the result, Z in the world) of the area in metres.</summary>
        public Vector2 GetSizeMetres(GridArea area)
        {
            return new Vector2(
                area.ColumnCount * _geometry.CellSizeMetres,
                area.RowCount * _geometry.CellSizeMetres);
        }
    }
}
