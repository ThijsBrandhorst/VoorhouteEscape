using System;
using UnityEngine;
using SchoolEscape.Player;

namespace SchoolEscape.Interaction {
    /// <summary>
    /// Looks through the camera, finds an IInteractable and triggers it with E.
    /// Place on the Player root. Disabled automatically while the player controls are locked
    /// (puzzle open, pause menu...).
    /// </summary>
    public sealed class PlayerInteractor : MonoBehaviour {
        [SerializeField] private PlayerInputReader input;
        [SerializeField] private PlayerControlLock controlLock;
        [Tooltip("The camera the player looks through.")]
        [SerializeField] private Camera viewCamera;
        [SerializeField, Min(0.5f)] private float interactionDistance = 3f;
        [Tooltip("Thickness of the aiming ray. A small value makes thin objects easier to target.")]
        [SerializeField, Range(0f, 0.5f)] private float aimRadius = 0.05f;
        [Tooltip("Layers that can be hit. Exclude the Player layer.")]
        [SerializeField] private LayerMask interactionLayers = ~0;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        private Collider _lastCollider;
        private IInteractable _current;
        private MonoBehaviour _currentBehaviour;

        public IInteractable Current => _current;
        public string CurrentPrompt { get; private set; } = string.Empty;
        public bool CurrentCanInteract { get; private set; }
        public bool IsBlocked => controlLock != null && controlLock.IsLocked;

        /// <summary>Raised when something wants to tell the player something (e.g. "Deze deur is op slot").</summary>
        public event Action<string> FeedbackRequested;

        public void ShowFeedback(string message) {
            if (!string.IsNullOrEmpty(message)) FeedbackRequested?.Invoke(message);
        }

        private void Awake() {
            if (viewCamera == null) viewCamera = Camera.main;
            if (input == null || viewCamera == null) {
                Debug.LogError("PlayerInteractor: Input and View Camera must be assigned.", this);
                enabled = false;
            }
        }

        private void Update() {
            if (IsBlocked) { Clear(); return; }

            Scan();
            if (_current == null) return;

            CurrentCanInteract = _current.CanInteract(this);
            CurrentPrompt = _current.GetPrompt(this) ?? string.Empty;

            if (!input.InteractPressed) return;

            if (CurrentCanInteract) _current.Interact(this);
            else ShowFeedback(CurrentPrompt);
        }

        private void Scan() {
            Transform t = viewCamera.transform;
            if (Physics.SphereCast(t.position, aimRadius, t.forward, out RaycastHit hit, interactionDistance,
                    interactionLayers, triggerInteraction)) {
                bool stale = _current != null && (_currentBehaviour == null || !_currentBehaviour.isActiveAndEnabled);
                if (hit.collider != _lastCollider || stale) {
                    _lastCollider = hit.collider;
                    _current = hit.collider.GetComponentInParent<IInteractable>();
                    _currentBehaviour = _current as MonoBehaviour;
                    if (_currentBehaviour != null && !_currentBehaviour.isActiveAndEnabled) Clear();
                }
            } else {
                Clear();
            }
        }

        private void Clear() {
            _lastCollider = null;
            _current = null;
            _currentBehaviour = null;
            CurrentPrompt = string.Empty;
            CurrentCanInteract = false;
        }
    }
}