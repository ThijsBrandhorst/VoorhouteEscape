using UnityEngine;
using UnityEngine.InputSystem;

namespace SchoolEscape.Player {
    /// <summary>
    /// Wraps the Input System actions for the player. Actions are created in code,
    /// so no Input Action Asset is required. Requires the Input System package.
    /// Place on the Player root.
    /// </summary>
    public sealed class PlayerInputReader : MonoBehaviour {
        private InputAction _move;
        private InputAction _look;
        private InputAction _sprint;
        private InputAction _jump;
        private InputAction _interact;

        public Vector2 Move => _move.ReadValue<Vector2>();
        /// <summary>Mouse delta in pixels this frame (do NOT multiply by deltaTime).</summary>
        public Vector2 LookDelta => _look.ReadValue<Vector2>();
        public bool SprintHeld => _sprint.IsPressed();
        public bool JumpPressed => _jump.WasPressedThisFrame();
        /// <summary>Used by the interaction system in Phase 2.</summary>
        public bool InteractPressed => _interact.WasPressedThisFrame();

        private void Awake() {
            _move = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _move.AddBinding("<Gamepad>/leftStick");

            _look = new InputAction("Look", InputActionType.Value, binding: "<Mouse>/delta", expectedControlType: "Vector2");
            _sprint = new InputAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");
            _jump = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
            _interact = new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");
        }

        private void OnEnable() {
            _move.Enable();
            _look.Enable();
            _sprint.Enable();
            _jump.Enable();
            _interact.Enable();
        }

        private void OnDisable() {
            _move.Disable();
            _look.Disable();
            _sprint.Disable();
            _jump.Disable();
            _interact.Disable();
        }

        private void OnDestroy() {
            _move?.Dispose();
            _look?.Dispose();
            _sprint?.Dispose();
            _jump?.Dispose();
            _interact?.Dispose();
        }
    }
}