using UnityEngine;

namespace SchoolEscape.Player {
    /// <summary>
    /// Subtle walking/sprinting head bob plus a spring-based landing dip.
    /// Place on the "CameraBob" object between CameraPivot and the Main Camera.
    /// Its rest local position (normally 0,0,0) is remembered at startup.
    /// </summary>
    public sealed class HeadBob : MonoBehaviour {
        [SerializeField] private PlayerSettings settings;
        [SerializeField] private PlayerMotor motor;

        private Vector3 _restPosition;
        private float _phase;
        private float _blend;
        private float _frequency;
        private float _vertical;
        private float _horizontal;
        private float _landOffset;
        private float _landVelocity;
        private bool _userEnabled = true;
        private float _intensityMultiplier = 1f;

        /// <summary>For the options menu: toggle head bob (and landing dip) entirely.</summary>
        public void SetBobEnabled(bool value) => _userEnabled = value;
        /// <summary>For the options menu: 0..n multiplier on bob amplitude.</summary>
        public void SetIntensityMultiplier(float value) => _intensityMultiplier = Mathf.Max(0f, value);

        private void Awake() {
            if (settings == null || motor == null) {
                Debug.LogError("HeadBob: Settings and Motor must be assigned.", this);
                enabled = false;
                return;
            }
            _restPosition = transform.localPosition;
            _frequency = settings.WalkBobFrequency;
            _vertical = settings.WalkBobVertical;
            _horizontal = settings.WalkBobHorizontal;
        }

        private void OnEnable() => motor.Landed += OnLanded;
        private void OnDisable() {
            if (motor != null) motor.Landed -= OnLanded;
        }

        private bool BobAllowed => _userEnabled && settings.HeadBobEnabled;

        private void OnLanded(float impactSpeed) {
            if (!BobAllowed || !settings.LandingEffectEnabled || impactSpeed < settings.MinLandingImpactSpeed) return;
            impactSpeed = Mathf.Min(impactSpeed, settings.MaxLandingImpactSpeed);
            _landVelocity -= impactSpeed * settings.LandingIntensity;
        }

        private void LateUpdate() {
            float dt = Mathf.Min(Time.deltaTime, 0.033f);
            if (dt <= 0f) return;

            bool allowed = BobAllowed;
            bool moving = motor.IsGrounded && motor.HorizontalSpeed > 0.1f;
            float targetBlend = allowed && moving
                ? Mathf.Clamp01(motor.HorizontalSpeed / Mathf.Max(0.01f, settings.WalkSpeed))
                : 0f;
            _blend = Mathf.MoveTowards(_blend, targetBlend, settings.BobTransitionSpeed * dt);

            // Smoothly change between walk and sprint parameters
            bool sprint = motor.IsSprinting;
            float k = 1f - Mathf.Exp(-settings.BobTransitionSpeed * dt);
            _frequency = Mathf.Lerp(_frequency, sprint ? settings.SprintBobFrequency : settings.WalkBobFrequency, k);
            _vertical = Mathf.Lerp(_vertical, sprint ? settings.SprintBobVertical : settings.WalkBobVertical, k);
            _horizontal = Mathf.Lerp(_horizontal, sprint ? settings.SprintBobHorizontal : settings.WalkBobHorizontal, k);

            // One vertical bob per step; sideways sway completes one cycle per two steps.
            _phase = Mathf.Repeat(_phase + _frequency * 2f * Mathf.PI * dt, 4f * Mathf.PI);
            float intensity = _blend * _intensityMultiplier;
            float x = Mathf.Sin(_phase * 0.5f) * _horizontal * intensity;
            float y = Mathf.Sin(_phase) * _vertical * intensity;

            // Landing spring (semi-implicit Euler)
            if (allowed && settings.LandingEffectEnabled) {
                _landVelocity += (-settings.LandingSpringStiffness * _landOffset - settings.LandingSpringDamping * _landVelocity) * dt;
                _landOffset += _landVelocity * dt;
            } else {
                _landOffset = 0f;
                _landVelocity = 0f;
            }

            transform.localPosition = _restPosition + new Vector3(x, y + _landOffset, 0f);
        }
    }
}