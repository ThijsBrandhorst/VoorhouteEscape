namespace SchoolEscape.Interaction {
    /// <summary>Contract for everything the player can interact with using E.</summary>
    public interface IInteractable {
        /// <summary>False = the object is visible but cannot be used right now (prompt is shown as unavailable).</summary>
        bool CanInteract(PlayerInteractor interactor);

        /// <summary>Text shown after "[E]", e.g. "Computer gebruiken". When CanInteract is false this is shown as feedback.</summary>
        string GetPrompt(PlayerInteractor interactor);

        /// <summary>Called when the player presses E while CanInteract is true.</summary>
        void Interact(PlayerInteractor interactor);
    }
}