using UnityEngine;

namespace SchoolEscape.Player {
    /// <summary>
    /// Optional sprint stamina. Fully disabled via PlayerSettings.StaminaEnabled.
    /// Driven by PlayerMotor (Tick), has no dependency on it. Place on the Player root.
    /// </summary>
    public sealed class PlayerStamina : MonoBehaviour {
        [SerializeField] private PlayerSettings settings;

        private float _current;
        private float _regenTimer;
        private bool _exhausted;

        public float Current => _current;
        public float Normalized => settings != null && settings.MaxStamina > 0f ? _current / settings.MaxStamina : 1f;
        public bool CanSprint => settings == null || !settings.StaminaEnabled || !_exhausted;

        private void Awake() {
            if (settings == null) {
                Debug.LogError("PlayerStamina: PlayerSettings is not assigned.", this);
                enabled = false;
                return;
            }
            _current = settings.MaxStamina;
        }

        public void Tick(bool isSprinting, float deltaTime) {
            if (settings == null || !settings.StaminaEnabled) return;

            if (isSprinting) {
                _current = Mathf.Max(0f, _current - settings.StaminaDrainPerSecond * deltaTime);
                _regenTimer = settings.StaminaRegenDelay;
                if (_current <= 0f) _exhausted = true;
            } else {
                if (_regenTimer > 0f) _regenTimer -= deltaTime;
                else _current = Mathf.Min(settings.MaxStamina, _current + settings.StaminaRegenPerSecond * deltaTime);

                if (_exhausted && _current >= settings.MinStaminaToSprint) _exhausted = false;
            }
        }
    }
}