using UnityEngine;

namespace SchoolEscape.Player
{
    /// <summary>
    /// Extra movement visuals on the camera: sprint FOV kick, strafe roll and a subtle idle breathing sway.
    /// Place on the Main Camera (child of CameraBob). Only reads from PlayerMotor.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraEffects : MonoBehaviour
    {
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private PlayerSettings settings;
        [SerializeField] private Camera targetCamera;

        [Header("Sprint FOV")]
        [Tooltip("Extra field of view (degrees) at full sprint speed.")]
        [SerializeField, Min(0f)] private float sprintFovBoost = 8f;
        [SerializeField, Min(0.1f)] private float fovSmoothing = 8f;

        [Header("Strafe roll")]
        [Tooltip("Camera tilt in degrees when strafing.")]
        [SerializeField, Min(0f)] private float strafeRollDegrees = 1.5f;
        [SerializeField, Min(0.1f)] private float rollSmoothing = 10f;

        [Header("Idle breathing")]
        [SerializeField, Min(0f)] private float breathingDegrees = 0.15f;
        [SerializeField, Min(0.01f)] private float breathingFrequency = 0.3f;

        private float _baseFov;
        private float _roll;
        private bool _effectsEnabled = true;

        public void SetEffectsEnabled(bool value) => _effectsEnabled = value;

        private void Awake()
        {
            if (targetCamera == null) targetCamera = GetComponent<Camera>();
            if (motor == null || settings == null)
            {
                Debug.LogError("CameraEffects: Motor and Settings must be assigned.", this);
                enabled = false;
                return;
            }
            _baseFov = targetCamera.fieldOfView;
        }

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            if (!_effectsEnabled)
            {
                targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, _baseFov, 1f - Mathf.Exp(-fovSmoothing * dt));
                transform.localRotation = Quaternion.identity;
                _roll = 0f;
                return;
            }

            float speed = motor.HorizontalSpeed;

            // FOV kick
            float sprintAmount = motor.IsSprinting ? Mathf.InverseLerp(settings.WalkSpeed, settings.SprintSpeed, speed) : 0f;
            float targetFov = _baseFov + sprintFovBoost * sprintAmount;
            targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, targetFov, 1f - Mathf.Exp(-fovSmoothing * dt));

            // Strafe roll
            float strafe = motor.transform.InverseTransformDirection(motor.Velocity).x / Mathf.Max(0.01f, settings.WalkSpeed);
            float targetRoll = -Mathf.Clamp(strafe, -1.5f, 1.5f) * strafeRollDegrees;
            _roll = Mathf.Lerp(_roll, targetRoll, 1f - Mathf.Exp(-rollSmoothing * dt));

            // Breathing (fades out while moving)
            float idle = 1f - Mathf.Clamp01(speed / Mathf.Max(0.01f, settings.WalkSpeed));
            float breath = Mathf.Sin(Time.time * Mathf.PI * 2f * breathingFrequency) * breathingDegrees * idle;

            transform.localRotation = Quaternion.Euler(breath, 0f, _roll + breath * 0.5f);
        }
    }
}
