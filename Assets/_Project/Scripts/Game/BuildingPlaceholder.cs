using UnityEngine;

namespace Colony.Game
{
    /// <summary>
    /// A plain box that stands in for a building model until the real models arrive.
    /// </summary>
    public sealed class BuildingPlaceholder : MonoBehaviour
    {
        private const float HEIGHT_METRES = 8f;

        // Keeps a visible gap between neighbours, so two buildings do not look like one box.
        private const float SIDE_MARGIN_METRES = 0.5f;
        private const float HALF = 0.5f;
        private const string BASE_COLOR_PROPERTY = "_BaseColor";

        private static readonly int BASE_COLOR_PROPERTY_ID = Shader.PropertyToID(BASE_COLOR_PROPERTY);

        private Renderer _renderer;
        private MaterialPropertyBlock _propertyBlock;

        public static BuildingPlaceholder Create(string objectName, Transform parent)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = objectName;
            box.transform.SetParent(parent, worldPositionStays: false);

            // Placement is decided by grid cells, not by physics, so the box needs no collider.
            Destroy(box.GetComponent<Collider>());

            BuildingPlaceholder placeholder = box.AddComponent<BuildingPlaceholder>();
            placeholder._renderer = box.GetComponent<Renderer>();
            placeholder._propertyBlock = new MaterialPropertyBlock();
            return placeholder;
        }

        /// <summary>
        /// Stands the box on the footprint: its floor at the height of the footprint centre.
        /// </summary>
        public void StandOn(Vector3 footprintCenter, Vector2 footprintSizeMetres)
        {
            float doubleMargin = SIDE_MARGIN_METRES + SIDE_MARGIN_METRES;
            transform.localScale = new Vector3(
                footprintSizeMetres.x - doubleMargin,
                HEIGHT_METRES,
                footprintSizeMetres.y - doubleMargin);
            transform.position = footprintCenter + (Vector3.up * (HEIGHT_METRES * HALF));
        }

        public void SetColor(Color color)
        {
            _propertyBlock.SetColor(BASE_COLOR_PROPERTY_ID, color);
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
