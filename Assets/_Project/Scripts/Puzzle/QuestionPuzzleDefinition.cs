using System.Collections.Generic;
using UnityEngine;

namespace SchoolEscape.Puzzles
{
    /// <summary>
    /// A puzzle made of one or more questions (multiple choice, text answer, number answer; can be mixed).
    /// Create via Create > School Escape > Puzzles > Question Puzzle and fill in the questions in the Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "NewQuestionPuzzle", menuName = "School Escape/Puzzles/Question Puzzle")]
    public sealed class QuestionPuzzleDefinition : PuzzleDefinition
    {
        [Header("Questions")]
        [SerializeField] private bool shuffleQuestions;
        [Tooltip("Shuffle the order of the answers of multiple-choice questions.")]
        [SerializeField] private bool shuffleChoices = true;
        [SerializeField] private List<QuestionData> questions = new List<QuestionData>();

        public bool ShuffleQuestions => shuffleQuestions;
        public bool ShuffleChoices => shuffleChoices;
        public IReadOnlyList<QuestionData> Questions => questions;

        public override PuzzleSession CreateSession() => new QuestionPuzzleSession(this);

        public override bool Validate(out string error)
        {
            if (!base.Validate(out error)) return false;
            if (questions == null || questions.Count == 0) { error = "The puzzle has no questions."; return false; }
            foreach (QuestionData q in questions)
            {
                if (q == null) { error = "The puzzle contains an empty question slot."; return false; }
                if (!q.Validate(out error)) return false;
            }
            error = null;
            return true;
        }

#if UNITY_EDITOR
        public void EditorSetQuestions(IEnumerable<QuestionData> newQuestions) => questions = new List<QuestionData>(newQuestions);
#endif
    }
}
