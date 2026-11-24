using System;
using UnityEngine;

namespace SchoolEscape.Player {
    /// <summary>
    /// CharacterController-based movement: walking, sprinting, jumping, gravity,
    /// SphereCast ground check, slopes and stairs. Knows nothing about the camera.
    /// Place on the Player root (do not scale the Player object).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour {
        [SerializeField] private PlayerSettings settings;
        [SerializeField] private PlayerInputReader input;
        [Tooltip("Optional. When assigned, movement stops while controls are locked.")]
        [SerializeField] private PlayerControlLock controlLock;
        [Tooltip("Optional. Leave empty for unlimited sprinting.")]
        [SerializeField] private PlayerStamina stamina;

        private CharacterController _controller;
        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;
        private float _peakFallSpeed;
        private float _groundGap;
        private Vector3 _groundNormal = Vector3.up;

        public bool IsGrounded { get; private set; }
        public bool IsSprinting { get; private set; }
        public float HorizontalSpeed => _horizontalVelocity.magnitude;
        public Vector3 Velocity => new Vector3(_horizontalVelocity.x, _verticalVelocity, _horizontalVelocity.z);

        /// <summary>Raised when a jump starts.</summary>
        public event Action Jumped;
        /// <summary>Raised on landing. Parameter = downward impact speed in m/s.</summary>
        public event Action<float> Landed;

        private void Awake() {
            _controller = GetComponent<CharacterController>();
            if (settings == null || input == null) {
                Debug.LogError("PlayerMotor: Settings and Input must be assigned.", this);
                enabled = false;
            }
        }

        private void Update() {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            bool locked = controlLock != null && controlLock.IsLocked;
            Vector2 move = locked ? Vector2.zero : Vector2.ClampMagnitude(input.Move, 1f);
            bool hasInput = move.sqrMagnitude > 0.0001f;

            // --- Ground state & landing ---
            bool wasGrounded = IsGrounded;
            IsGrounded = CheckGround(wasGrounded);

            if (IsGrounded && !wasGrounded) {
                if (_peakFallSpeed > 0f) Landed?.Invoke(_peakFallSpeed);
                _peakFallSpeed = 0f;
            } else if (!IsGrounded && _verticalVelocity < 0f) {
                _peakFallSpeed = Mathf.Max(_peakFallSpeed, -_verticalVelocity);
            }

            // --- Sprint ---
            IsSprinting = !locked && hasInput && input.SprintHeld && (stamina == null || stamina.CanSprint);
            if (stamina != null) stamina.Tick(IsSprinting, dt);

            // --- Horizontal velocity (smooth accel/decel) ---
            float targetSpeed = IsSprinting ? settings.SprintSpeed : settings.WalkSpeed;
            Vector3 desired = (transform.right * move.x + transform.forward * move.y) * targetSpeed;
            float rate = hasInput ? settings.Acceleration : settings.Deceleration;
            if (!IsGrounded) rate *= settings.AirControl;
            _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, desired, rate * dt);

            // --- Vertical velocity: jump / stick to ground / gravity ---
            if (IsGrounded) {
                if (_verticalVelocity <= 0f) {
                    // Snap down to stairs/slopes when we were already grounded, otherwise just stick.
                    float snap = wasGrounded ? _groundGap / dt : 0f;
                    _verticalVelocity = -Mathf.Max(settings.GroundStickVelocity, snap);
                }

                if (!locked && input.JumpPressed) {
                    _verticalVelocity = Mathf.Sqrt(2f * settings.Gravity * settings.JumpHeight);
                    IsGrounded = false;
                    Jumped?.Invoke();
                }
            } else {
                _verticalVelocity = Mathf.Max(-settings.TerminalVelocity, _verticalVelocity - settings.Gravity * dt);
            }

            // --- Apply movement (follow slope surface so walking downhill doesn't bounce) ---
            Vector3 velocity = _horizontalVelocity;
            if (IsGrounded && _groundNormal != Vector3.up) {
                Vector3 onSlope = Vector3.ProjectOnPlane(velocity, _groundNormal);
                if (onSlope.sqrMagnitude > 1e-6f) velocity = onSlope.normalized * velocity.magnitude;
            }
            velocity.y += _verticalVelocity;

            CollisionFlags flags = _controller.Move(velocity * dt);
            if ((flags & CollisionFlags.Above) != 0 && _verticalVelocity > 0f) _verticalVelocity = 0f;
        }

        /// <summary>SphereCast ground check, independent of the camera.</summary>
        private bool CheckGround(bool wasGrounded) {
            _groundNormal = Vector3.up;
            _groundGap = 0f;

            // Never grounded while moving upward: prevents double jumps.
            if (_verticalVelocity > 0.1f) return false;

            float radius = Mathf.Min(settings.GroundCheckRadius, _controller.radius);
            float inset = _controller.radius - radius;
            float baseOffset = _controller.skinWidth + inset;

            // Extend the check when already grounded so we stay glued to stairs when going down.
            float reach = wasGrounded ? Mathf.Max(settings.GroundCheckDistance, _controller.stepOffset) : settings.GroundCheckDistance;

            Vector3 origin = transform.position + _controller.center
                             + Vector3.down * (_controller.height * 0.5f - _controller.radius);

            if (!Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit,
                    baseOffset + reach, settings.GroundLayers, QueryTriggerInteraction.Ignore))
                return false;

            // Sphere hits on stair edges give sideways normals, so use a ray for the true surface normal.
            Vector3 normal = hit.normal;
            if (Physics.Raycast(hit.point + Vector3.up * 0.05f, Vector3.down, out RaycastHit rayHit, 0.2f,
                    settings.GroundLayers, QueryTriggerInteraction.Ignore))
                normal = rayHit.normal;

            if (Vector3.Angle(normal, Vector3.up) > _controller.slopeLimit + 1f) return false;

            _groundNormal = normal;
            _groundGap = Mathf.Max(0f, hit.distance - baseOffset);
            return true;
        }

        private void OnDrawGizmosSelected() {
            var cc = GetComponent<CharacterController>();
            if (cc == null || settings == null) return;
            float radius = Mathf.Min(settings.GroundCheckRadius, cc.radius);
            Vector3 origin = transform.position + cc.center + Vector3.down * (cc.height * 0.5f - cc.radius);
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(origin + Vector3.down * (cc.skinWidth + cc.radius - radius + settings.GroundCheckDistance), radius);
        }
    }
}