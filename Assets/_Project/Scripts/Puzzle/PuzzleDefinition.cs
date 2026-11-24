using System.Collections.Generic;
using UnityEngine;

namespace SchoolEscape.Puzzles
{
    /// <summary>
    /// Base class for every puzzle's DATA. A definition describes a puzzle and creates the
    /// runtime session that contains the logic. New puzzle types (sequence, matching, ...)
    /// derive from this and provide their own session + view.
    /// </summary>
    public abstract class PuzzleDefinition : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique, stable ID (used for completion state and saving). Defaults to the asset name.")]
        [SerializeField] private string puzzleId = string.Empty;
        [SerializeField] private string title = "Nieuwe opdracht";
        [Tooltip("Shown in the window header, e.g. 'REKENCOMPUTER - GROEP 8'.")]
        [SerializeField] private string windowTitle = "OPDRACHT";
        [SerializeField] private PuzzleCategory category = PuzzleCategory.General;
        [SerializeField] private PuzzleDifficulty difficulty = PuzzleDifficulty.Medium;
        [Tooltip("Look of the puzzle window.")]
        [SerializeField] private PuzzleSkin skin = PuzzleSkin.Computer;

        [Header("Text")]
        [SerializeField, TextArea(2, 4)] private string instructions = string.Empty;
        [SerializeField, TextArea(1, 3)] private string completionMessage = "Goed gedaan! Je hebt de opdracht voltooid.";
        [Tooltip("Text after [E] on a station that uses this puzzle.")]
        [SerializeField] private string stationPrompt = "Opdracht starten";

        [Header("Rules")]
        [Tooltip("Wrong answers allowed before the attempt fails. 0 = unlimited.")]
        [SerializeField, Min(0)] private int maxWrongAttempts;
        [Tooltip("Seconds to finish. 0 = no time limit.")]
        [SerializeField, Min(0f)] private float timeLimitSeconds;
        [Tooltip("When on, rewards are only granted the first time the puzzle is solved.")]
        [SerializeField] private bool rewardsOnlyOnce = true;

        [Header("Rewards")]
        [SerializeField] private List<PuzzleReward> rewards = new List<PuzzleReward>();

        public string PuzzleId => puzzleId;
        public string Title => title;
        public string WindowTitle => windowTitle;
        public PuzzleCategory Category => category;
        public PuzzleDifficulty Difficulty => difficulty;
        public PuzzleSkin Skin => skin;
        public string Instructions => instructions;
        public string CompletionMessage => completionMessage;
        public string StationPrompt => stationPrompt;
        public int MaxWrongAttempts => maxWrongAttempts;
        public float TimeLimitSeconds => timeLimitSeconds;
        public bool RewardsOnlyOnce => rewardsOnlyOnce;
        public IReadOnlyList<PuzzleReward> Rewards => rewards;

        /// <summary>Creates a fresh runtime session (all logic/state lives there, never in the asset).</summary>
        public abstract PuzzleSession CreateSession();

        public virtual bool Validate(out string error)
        {
            if (string.IsNullOrWhiteSpace(puzzleId)) { error = "Puzzle ID is empty."; return false; }
            error = null;
            return true;
        }

        protected virtual void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(puzzleId)) puzzleId = name;
            if (!Validate(out string error)) Debug.LogWarning($"[{name}] {error}", this);
        }

#if UNITY_EDITOR
        public void EditorInitialize(string id, string title, string windowTitle, PuzzleCategory category,
            PuzzleDifficulty difficulty, PuzzleSkin skin, string instructions, string stationPrompt,
            string completionMessage, params PuzzleReward[] rewards)
        {
            puzzleId = id;
            this.title = title;
            this.windowTitle = windowTitle;
            this.category = category;
            this.difficulty = difficulty;
            this.skin = skin;
            this.instructions = instructions;
            this.stationPrompt = stationPrompt;
            this.completionMessage = completionMessage;
            this.rewards = new List<PuzzleReward>(rewards);
        }
#endif
    }
}
