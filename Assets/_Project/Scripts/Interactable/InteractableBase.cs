using UnityEngine;

namespace SchoolEscape.Interaction {
    /// <summary>Convenience base class for interactables (doors, notes, stations...).</summary>
    public abstract class InteractableBase : MonoBehaviour, IInteractable {
        [Tooltip("Text shown after [E]. Leave empty to use the default of the specific interactable.")]
        [SerializeField] protected string prompt = string.Empty;
        [Tooltip("When disabled the object cannot be interacted with.")]
        [SerializeField] private bool interactionEnabled = true;

        public void SetInteractionEnabled(bool value) => interactionEnabled = value;

        public virtual bool CanInteract(PlayerInteractor interactor) => interactionEnabled;
        public virtual string GetPrompt(PlayerInteractor interactor) => string.IsNullOrEmpty(prompt) ? "Gebruiken" : prompt;
        public abstract void Interact(PlayerInteractor interactor);
    }
}