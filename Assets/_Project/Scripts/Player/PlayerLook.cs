using UnityEngine;

namespace SchoolEscape.Player {
    /// <summary>
    /// Mouse look. Rotates the yaw root horizontally and the pitch pivot vertically.
    /// Place on the Player root.
    /// </summary>
    public sealed class PlayerLook : MonoBehaviour {
        [SerializeField] private PlayerSettings settings;
        [SerializeField] private PlayerInputReader input;
        [Tooltip("Optional. Look stops while controls are locked.")]
        [SerializeField] private PlayerControlLock controlLock;
        [Tooltip("Object rotated left/right. Usually the Player root.")]
        [SerializeField] private Transform yawRoot;
        [Tooltip("Object rotated up/down. Usually the CameraPivot child.")]
        [SerializeField] private Transform pitchPivot;

        private float _pitch;
        private float _sensitivityMultiplier = 1f;
        private bool _wasLocked;

        private void Awake() {
            if (yawRoot == null) yawRoot = transform;
            if (settings == null || input == null || pitchPivot == null) {
                Debug.LogError("PlayerLook: Settings, Input and Pitch Pivot must be assigned.", this);
                enabled = false;
            }
        }

        /// <summary>For the options menu. 1 = default sensitivity.</summary>
        public void SetSensitivityMultiplier(float multiplier) => _sensitivityMultiplier = Mathf.Max(0f, multiplier);

        private void Update() {
            if (controlLock != null && controlLock.IsLocked) {
                _wasLocked = true;
                return;
            }
            if (_wasLocked) // discard the mouse jump accumulated while the cursor was free
            {
                _wasLocked = false;
                return;
            }
            if (Cursor.lockState != CursorLockMode.Locked) return;

            Vector2 delta = input.LookDelta;
            float yaw = delta.x * settings.HorizontalSensitivity * _sensitivityMultiplier;
            float pitchDelta = delta.y * settings.VerticalSensitivity * _sensitivityMultiplier;
            if (!settings.InvertY) pitchDelta = -pitchDelta;

            yawRoot.Rotate(0f, yaw, 0f, Space.Self);
            _pitch = Mathf.Clamp(_pitch + pitchDelta, -settings.MaxLookAngle, settings.MaxLookAngle);
            pitchPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }
    }
}