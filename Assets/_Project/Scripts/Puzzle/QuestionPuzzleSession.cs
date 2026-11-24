using System;
using System.Collections.Generic;
using UnityEngine;

namespace SchoolEscape.Puzzles
{
    /// <summary>Logic for a QuestionPuzzleDefinition: step through questions, validate answers.</summary>
    public sealed class QuestionPuzzleSession : PuzzleSession
    {
        private readonly QuestionPuzzleDefinition _definition;
        private readonly List<QuestionData> _questions = new List<QuestionData>();
        private int[] _choiceOrder = Array.Empty<int>();
        private int _index;
        private bool _currentSolved;

        public QuestionPuzzleSession(QuestionPuzzleDefinition definition) : base(definition)
        {
            _definition = definition;
            foreach (QuestionData q in definition.Questions)
                if (q != null) _questions.Add(q);

            if (definition.ShuffleQuestions) Shuffle(_questions);
            if (_questions.Count > 0) PrepareQuestion();
        }

        public int StepIndex => _index;
        public int StepCount => _questions.Count;
        public QuestionData Current => _questions[_index];
        public bool HasNext => _index < _questions.Count - 1;
        public int ChoiceCount => _choiceOrder.Length;

        public string GetChoice(int displayIndex) => Current.Choices[_choiceOrder[displayIndex]];

        public string UseHint()
        {
            HintsUsed++;
            return Current.Hint;
        }

        /// <summary>Moves to the next question after a correct answer.</summary>
        public void Advance()
        {
            if (State != PuzzleState.InProgress || !_currentSolved || !HasNext) return;
            _index++;
            _currentSolved = false;
            PrepareQuestion();
        }

        public AnswerResult SubmitChoice(int displayIndex)
        {
            if (!CanAnswer(QuestionType.MultipleChoice) || displayIndex < 0 || displayIndex >= ChoiceCount)
                return new AnswerResult(AnswerOutcome.Invalid);

            return Evaluate(_choiceOrder[displayIndex] == Current.CorrectChoiceIndex);
        }

        /// <summary>Submits a typed answer (for Text and Number questions).</summary>
        public AnswerResult SubmitText(string input)
        {
            if (State != PuzzleState.InProgress || _currentSolved || Current.Type == QuestionType.MultipleChoice)
                return new AnswerResult(AnswerOutcome.Invalid);

            if (string.IsNullOrWhiteSpace(input))
                return new AnswerResult(AnswerOutcome.Invalid, "Vul eerst een antwoord in.");

            bool correct;
            if (Current.Type == QuestionType.Number)
            {
                if (!AnswerValidator.TryParseNumber(input, out double value))
                    return new AnswerResult(AnswerOutcome.Invalid, "Vul een getal in.");
                correct = AnswerValidator.NumbersMatch(value, Current.NumberAnswer, Current.Tolerance);
            }
            else
            {
                correct = AnswerValidator.MatchesAny(input, Current.AcceptedAnswers, Current.CaseSensitive);
            }
            return Evaluate(correct);
        }

        private bool CanAnswer(QuestionType type)
            => State == PuzzleState.InProgress && !_currentSolved && Current.Type == type;

        private AnswerResult Evaluate(bool correct)
        {
            if (correct)
            {
                _currentSolved = true;
                string explanation = Current.Explanation;
                if (!HasNext) MarkSolved();
                return new AnswerResult(AnswerOutcome.Correct, explanation);
            }

            RegisterWrongAnswer();
            return new AnswerResult(AnswerOutcome.Incorrect);
        }

        private void PrepareQuestion()
        {
            QuestionData q = Current;
            if (q.Type != QuestionType.MultipleChoice)
            {
                _choiceOrder = Array.Empty<int>();
                return;
            }

            _choiceOrder = new int[q.Choices.Count];
            for (int i = 0; i < _choiceOrder.Length; i++) _choiceOrder[i] = i;
            if (_definition.ShuffleChoices && !q.KeepChoiceOrder) Shuffle(_choiceOrder);
        }

        private static void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
