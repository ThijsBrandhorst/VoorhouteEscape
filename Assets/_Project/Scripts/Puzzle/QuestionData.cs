using System;
using System.Collections.Generic;
using UnityEngine;

namespace SchoolEscape.Puzzles {
    /// <summary>One question inside a QuestionPuzzleDefinition. Pure data, no logic.</summary>
    [Serializable]
    public sealed class QuestionData {
        [SerializeField] private QuestionType type = QuestionType.MultipleChoice;
        [SerializeField, TextArea(2, 6)] private string prompt = string.Empty;
        [Tooltip("Optional picture shown with the question (map, flag, figure...).")]
        [SerializeField] private Sprite image;

        [Header("Multiple choice")]
        [SerializeField] private string[] choices = Array.Empty<string>();
        [Tooltip("Index (starting at 0) of the correct choice in the list above.")]
        [SerializeField, Min(0)] private int correctChoiceIndex;
        [Tooltip("Never shuffle this question's choices (for 'A en B' / 'alles hierboven' style answers).")]
        [SerializeField] private bool keepChoiceOrder;

        [Header("Text answer")]
        [Tooltip("Every accepted spelling. Extra spaces are ignored.")]
        [SerializeField] private string[] acceptedAnswers = Array.Empty<string>();
        [SerializeField] private bool caseSensitive;

        [Header("Number answer")]
        [Tooltip("Accepts 12,5 and 12.5, €, % and thousands dots.")]
        [SerializeField] private double numberAnswer;
        [Tooltip("Allowed difference (0 = exact).")]
        [SerializeField] private double tolerance;

        [Header("Help")]
        [Tooltip("Grey text in the answer box, e.g. 'Antwoord in euro'.")]
        [SerializeField] private string inputPlaceholder = string.Empty;
        [SerializeField, TextArea(1, 3)] private string hint = string.Empty;
        [Tooltip("Shown after a correct answer.")]
        [SerializeField, TextArea(1, 4)] private string explanation = string.Empty;

        public QuestionType Type => type;
        public string Prompt => prompt;
        public Sprite Image => image;
        public IReadOnlyList<string> Choices => choices;
        public int CorrectChoiceIndex => correctChoiceIndex;
        public bool KeepChoiceOrder => keepChoiceOrder;
        public IReadOnlyList<string> AcceptedAnswers => acceptedAnswers;
        public bool CaseSensitive => caseSensitive;
        public double NumberAnswer => numberAnswer;
        public double Tolerance => tolerance;
        public string InputPlaceholder => inputPlaceholder;
        public string Hint => hint;
        public string Explanation => explanation;

        public bool Validate(out string error) {
            if (string.IsNullOrWhiteSpace(prompt)) { error = "A question has an empty prompt."; return false; }
            switch (type) {
                case QuestionType.MultipleChoice:
                    if (choices == null || choices.Length < 2) { error = $"'{Short()}' needs at least 2 choices."; return false; }
                    if (correctChoiceIndex < 0 || correctChoiceIndex >= choices.Length) { error = $"'{Short()}' has a correct index outside the choices."; return false; }
                    break;
                case QuestionType.Text:
                    if (acceptedAnswers == null || acceptedAnswers.Length == 0) { error = $"'{Short()}' has no accepted answers."; return false; }
                    break;
            }
            error = null;
            return true;
        }

        private string Short() => prompt.Length > 40 ? prompt.Substring(0, 40) + "..." : prompt;

        // ---- Factories (used by the sample-content creator and by tests/tools) ----

        public static QuestionData ChoiceQuestion(string prompt, int correctIndex, string explanation, string hint, params string[] choices)
            => new QuestionData { type = QuestionType.MultipleChoice, prompt = prompt, correctChoiceIndex = correctIndex, explanation = explanation, hint = hint, choices = choices };

        public static QuestionData TextQuestion(string prompt, string explanation, string hint, params string[] accepted)
            => new QuestionData { type = QuestionType.Text, prompt = prompt, explanation = explanation, hint = hint, acceptedAnswers = accepted };

        public static QuestionData NumberQuestion(string prompt, double answer, string explanation, string hint, string placeholder = "")
            => new QuestionData { type = QuestionType.Number, prompt = prompt, numberAnswer = answer, explanation = explanation, hint = hint, inputPlaceholder = placeholder };
    }
}