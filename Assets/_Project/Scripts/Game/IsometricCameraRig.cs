using UnityEngine;

namespace Colony.Game
{
    /// <summary>
    /// Keeps the camera looking down at a point on the build level from a fixed angle.
    /// The player moves that point (pan) and changes the distance to it (zoom).
    /// </summary>
    public sealed class IsometricCameraRig : MonoBehaviour
    {
        [SerializeField] private float _pitchDegrees = 50f;
        [SerializeField] private float _yawDegrees = 45f;
        [SerializeField] private float _startDistanceMetres = 220f;
        [SerializeField] private float _minDistanceMetres = 60f;
        [SerializeField] private float _maxDistanceMetres = 600f;
        [SerializeField] private float _zoomStepMetres = 30f;
        [SerializeField] private float _panSpeedMetresPerSecond = 150f;

        private PrototypeControls _controls;
        private Vector2 _panLimitMetres;
        private Vector3 _focusPoint;
        private float _distanceMetres;

        /// <param name="controls">Source of pan and zoom input.</param>
        /// <param name="focusHeightMetres">Height of the plane the camera looks at.</param>
        /// <param name="panLimitMetres">
        /// How far from the map centre the looked-at point may move along X and Z.
        /// </param>
        public void Initialize(PrototypeControls controls, float focusHeightMetres, Vector2 panLimitMetres)
        {
            _controls = controls;
            _panLimitMetres = panLimitMetres;
            _focusPoint = new Vector3(0f, focusHeightMetres, 0f);
            _distanceMetres = _startDistanceMetres;
            ApplyToTransform();
        }

        private void Start()
        {
            if (_controls == null)
            {
                Debug.LogError(
                    $"{nameof(IsometricCameraRig)} was not initialised by {nameof(GameBootstrap)}.",
                    this);
                enabled = false;
            }
        }

        // LateUpdate, so the camera moves after everything else has moved this frame.
        private void LateUpdate()
        {
            Zoom(_controls.ZoomDirection);
            Pan(_controls.Pan, Time.unscaledDeltaTime);
            ApplyToTransform();
        }

        private void Zoom(int zoomDirection)
        {
            _distanceMetres = Mathf.Clamp(
                _distanceMetres - (zoomDirection * _zoomStepMetres),
                _minDistanceMetres,
                _maxDistanceMetres);
        }

        private void Pan(Vector2 panInput, float deltaSeconds)
        {
            Quaternion heading = Quaternion.Euler(0f, _yawDegrees, 0f);
            Vector3 direction = heading * new Vector3(panInput.x, 0f, panInput.y);
            Vector3 movedFocusPoint = _focusPoint + (direction * (_panSpeedMetresPerSecond * deltaSeconds));
            _focusPoint = new Vector3(
                Mathf.Clamp(movedFocusPoint.x, -_panLimitMetres.x, _panLimitMetres.x),
                _focusPoint.y,
                Mathf.Clamp(movedFocusPoint.z, -_panLimitMetres.y, _panLimitMetres.y));
        }

        private void ApplyToTransform()
        {
            Quaternion rotation = Quaternion.Euler(_pitchDegrees, _yawDegrees, 0f);
            transform.SetPositionAndRotation(
                _focusPoint - (rotation * Vector3.forward * _distanceMetres),
                rotation);
        }
    }
}
