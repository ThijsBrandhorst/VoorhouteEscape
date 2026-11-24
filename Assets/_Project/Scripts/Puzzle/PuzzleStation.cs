using UnityEngine;
using UnityEngine.Events;
using SchoolEscape.Interaction;
using SchoolEscape.Puzzles;

namespace SchoolEscape.World
{
    /// <summary>
    /// Turns ANY object (cube, laptop, blackboard, locker...) into a puzzle terminal.
    /// Add it to the object, drag a PuzzleDefinition into "Puzzle", done.
    /// To use another puzzle: drag a different asset in, or remove this component and add a new one.
    /// The object needs a Collider (added automatically when you add the component in the editor).
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("School Escape/Puzzle Station")]
    public sealed class PuzzleStation : InteractableBase
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [Header("Puzzle")]
        [Tooltip("Unique ID of this physical station (used for saving later).")]
        [SerializeField] private string stationId = string.Empty;
        [Tooltip("The puzzle content. Drag any puzzle asset here.")]
        [SerializeField] private PuzzleDefinition puzzle;
        [Tooltip("Optional. Found automatically in the scene when empty.")]
        [SerializeField] private PuzzleManager manager;
        [Tooltip("Allow playing the puzzle again after it has been solved.")]
        [SerializeField] private bool allowReplay;
        [SerializeField] private string completedPrompt = "Opdracht al voltooid";

        [Header("Feedback")]
        [SerializeField] private AudioClip interactSound;
        [Tooltip("Renderers tinted when the puzzle is solved (works with URP Lit materials).")]
        [SerializeField] private Renderer[] tintRenderers = System.Array.Empty<Renderer>();
        [SerializeField] private Color completedTint = new Color(0.35f, 0.85f, 0.45f, 1f);
        [SerializeField] private GameObject[] activeWhenCompleted = System.Array.Empty<GameObject>();
        [SerializeField] private GameObject[] activeWhenNotCompleted = System.Array.Empty<GameObject>();

        [Header("Events")]
        [SerializeField] private UnityEvent onPuzzleOpened;
        [Tooltip("Fires when the puzzle is solved. Hook up anything: open a door, enable an object, play an animation...")]
        [SerializeField] private UnityEvent onPuzzleSolved;

        private MaterialPropertyBlock _block;

        public string StationId => stationId;
        public PuzzleDefinition Puzzle => puzzle;
        public bool IsCompleted => manager != null && puzzle != null && manager.IsCompleted(puzzle.PuzzleId);

        private void Reset()
        {
            tintRenderers = GetComponentsInChildren<Renderer>();
            if (GetComponentInChildren<Collider>() == null) gameObject.AddComponent<BoxCollider>();
            stationId = gameObject.name;
        }

        private void Awake()
        {
            if (manager == null) manager = FindFirstObjectByType<PuzzleManager>();
            if (manager == null) Debug.LogError($"PuzzleStation '{name}': no PuzzleManager found in the scene.", this);
            if (puzzle == null) Debug.LogWarning($"PuzzleStation '{name}': no puzzle assigned.", this);
            if (GetComponentInChildren<Collider>() == null) Debug.LogWarning($"PuzzleStation '{name}': needs a Collider to be targeted.", this);
        }

        private void OnEnable()
        {
            if (manager != null) manager.PuzzleCompleted += OnPuzzleCompleted;
        }

        private void OnDisable()
        {
            if (manager != null) manager.PuzzleCompleted -= OnPuzzleCompleted;
        }

        private void Start() => ApplyVisuals(IsCompleted);

        public override bool CanInteract(PlayerInteractor interactor)
        {
            if (!base.CanInteract(interactor) || puzzle == null || manager == null) return false;
            if (manager.IsOpen) return false;
            return !IsCompleted || allowReplay;
        }

        public override string GetPrompt(PlayerInteractor interactor)
        {
            if (puzzle == null) return "Geen opdracht ingesteld";
            if (IsCompleted && !allowReplay) return completedPrompt;
            return string.IsNullOrEmpty(prompt) ? puzzle.StationPrompt : prompt;
        }

        public override void Interact(PlayerInteractor interactor)
        {
            if (!manager.TryStartPuzzle(puzzle)) return;
            if (interactSound != null) AudioSource.PlayClipAtPoint(interactSound, transform.position);
            onPuzzleOpened?.Invoke();
        }

        private void OnPuzzleCompleted(PuzzleDefinition completed)
        {
            if (completed != puzzle) return;
            ApplyVisuals(true);
            onPuzzleSolved?.Invoke();
        }

        private void ApplyVisuals(bool completed)
        {
            foreach (Renderer r in tintRenderers)
            {
                if (r == null) continue;
                if (completed)
                {
                    _block ??= new MaterialPropertyBlock();
                    r.GetPropertyBlock(_block);
                    _block.SetColor(BaseColorId, completedTint);
                    _block.SetColor(ColorId, completedTint);
                    r.SetPropertyBlock(_block);
                }
                else
                {
                    r.SetPropertyBlock(null);
                }
            }
            foreach (GameObject go in activeWhenCompleted) if (go != null) go.SetActive(completed);
            foreach (GameObject go in activeWhenNotCompleted) if (go != null) go.SetActive(!completed);
        }
    }
}
