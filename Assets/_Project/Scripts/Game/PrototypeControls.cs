using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Colony.Game
{
    /// <summary>
    /// The player's controls in the prototype, read through the Input System:
    /// camera pan and zoom, the pointer, placing a building and picking which one.
    /// </summary>
    public sealed class PrototypeControls : IDisposable
    {
        private const string POINTER_POSITION_BINDING = "<Pointer>/position";
        private const string PLACE_BINDING = "<Mouse>/leftButton";
        private const string ZOOM_BINDING = "<Mouse>/scroll/y";
        private const string TWO_AXIS_COMPOSITE = "2DVector";
        private const string COMPOSITE_UP = "Up";
        private const string COMPOSITE_DOWN = "Down";
        private const string COMPOSITE_LEFT = "Left";
        private const string COMPOSITE_RIGHT = "Right";

        private static readonly string[] PICK_BUILDING_BINDINGS =
        {
            "<Keyboard>/1",
            "<Keyboard>/2",
            "<Keyboard>/3",
            "<Keyboard>/4",
            "<Keyboard>/5",
            "<Keyboard>/6",
            "<Keyboard>/7",
            "<Keyboard>/8",
            "<Keyboard>/9",
        };

        private readonly InputAction _pointerPositionAction;
        private readonly InputAction _placeAction;
        private readonly InputAction _zoomAction;
        private readonly InputAction _panAction;
        private readonly InputAction[] _pickBuildingActionArray;

        public PrototypeControls()
        {
            _pointerPositionAction = new InputAction(
                nameof(PointerPosition),
                InputActionType.PassThrough,
                POINTER_POSITION_BINDING);
            _placeAction = new InputAction(
                nameof(WasPlacePressedThisFrame),
                InputActionType.Button,
                PLACE_BINDING);
            _zoomAction = new InputAction(
                nameof(ZoomDirection),
                InputActionType.PassThrough,
                ZOOM_BINDING);
            _panAction = CreatePanAction();
            _pickBuildingActionArray = CreatePickBuildingActions();
        }

        /// <summary>Pointer position on the screen in pixels, origin at the bottom left.</summary>
        public Vector2 PointerPosition => _pointerPositionAction.ReadValue<Vector2>();

        public bool WasPlacePressedThisFrame => _placeAction.WasPressedThisFrame();

        /// <summary>Requested camera movement: X is right, Y is forward, each from -1 to 1.</summary>
        public Vector2 Pan => _panAction.ReadValue<Vector2>();

        /// <summary>
        /// 1 when the wheel was rolled forward this frame, -1 when rolled back, 0 otherwise.
        /// Only the direction is used because the size of one wheel step differs between devices.
        /// </summary>
        public int ZoomDirection => Math.Sign(_zoomAction.ReadValue<float>());

        /// <summary>
        /// Position in the list of buildings of the number key pressed this frame
        /// (key 1 is position 0), or null when none was pressed.
        /// </summary>
        public int? GetBuildingIndexPickedThisFrame()
        {
            for (int i = 0; i < _pickBuildingActionArray.Length; i++)
            {
                if (_pickBuildingActionArray[i].WasPressedThisFrame())
                {
                    return i;
                }
            }

            return null;
        }

        public void Enable()
        {
            _pointerPositionAction.Enable();
            _placeAction.Enable();
            _zoomAction.Enable();
            _panAction.Enable();
            for (int i = 0; i < _pickBuildingActionArray.Length; i++)
            {
                _pickBuildingActionArray[i].Enable();
            }
        }

        public void Dispose()
        {
            _pointerPositionAction.Dispose();
            _placeAction.Dispose();
            _zoomAction.Dispose();
            _panAction.Dispose();
            for (int i = 0; i < _pickBuildingActionArray.Length; i++)
            {
                _pickBuildingActionArray[i].Dispose();
            }
        }

        private static InputAction CreatePanAction()
        {
            InputAction panAction = new InputAction(nameof(Pan), InputActionType.Value);
            panAction.AddCompositeBinding(TWO_AXIS_COMPOSITE)
                .With(COMPOSITE_UP, "<Keyboard>/w")
                .With(COMPOSITE_DOWN, "<Keyboard>/s")
                .With(COMPOSITE_LEFT, "<Keyboard>/a")
                .With(COMPOSITE_RIGHT, "<Keyboard>/d");
            panAction.AddCompositeBinding(TWO_AXIS_COMPOSITE)
                .With(COMPOSITE_UP, "<Keyboard>/upArrow")
                .With(COMPOSITE_DOWN, "<Keyboard>/downArrow")
                .With(COMPOSITE_LEFT, "<Keyboard>/leftArrow")
                .With(COMPOSITE_RIGHT, "<Keyboard>/rightArrow");
            return panAction;
        }

        private static InputAction[] CreatePickBuildingActions()
        {
            InputAction[] pickBuildingActionArray = new InputAction[PICK_BUILDING_BINDINGS.Length];
            for (int i = 0; i < PICK_BUILDING_BINDINGS.Length; i++)
            {
                pickBuildingActionArray[i] = new InputAction(
                    nameof(GetBuildingIndexPickedThisFrame),
                    InputActionType.Button,
                    PICK_BUILDING_BINDINGS[i]);
            }

            return pickBuildingActionArray;
        }
    }
}
