using System;

namespace SchoolEscape.Puzzles
{
    public enum PuzzleState { InProgress, Solved, Failed }

    public enum AnswerOutcome { Correct, Incorrect, Invalid }

    public readonly struct AnswerResult
    {
        public AnswerOutcome Outcome { get; }
        /// <summary>Explanation after a correct answer, or a message for invalid input.</summary>
        public string Message { get; }

        public AnswerResult(AnswerOutcome outcome, string message = "")
        {
            Outcome = outcome;
            Message = message ?? string.Empty;
        }
    }

    /// <summary>
    /// Runtime state and logic of ONE attempt at a puzzle. Created by PuzzleDefinition.CreateSession().
    /// Knows nothing about UI or the world; communicates through events.
    /// </summary>
    public abstract class PuzzleSession
    {
        protected PuzzleSession(PuzzleDefinition definition) => Definition = definition;

        public PuzzleDefinition Definition { get; }
        public PuzzleState State { get; private set; } = PuzzleState.InProgress;
        public int WrongAttempts { get; protected set; }
        public int HintsUsed { get; protected set; }
        public string FailReason { get; private set; } = string.Empty;

        /// <summary>Wrong answers left before failing, or -1 when unlimited.</summary>
        public int AttemptsRemaining => Definition.MaxWrongAttempts > 0
            ? Math.Max(0, Definition.MaxWrongAttempts - WrongAttempts)
            : -1;

        public event Action Solved;
        public event Action Failed;

        /// <summary>Called by the UI when the time limit runs out.</summary>
        public void Expire() => MarkFailed("De tijd is om!");

        protected void MarkSolved()
        {
            if (State != PuzzleState.InProgress) return;
            State = PuzzleState.Solved;
            Solved?.Invoke();
        }

        protected void MarkFailed(string reason)
        {
            if (State != PuzzleState.InProgress) return;
            State = PuzzleState.Failed;
            FailReason = reason;
            Failed?.Invoke();
        }

        /// <summary>Registers a wrong answer and fails the session when the limit is reached.</summary>
        protected void RegisterWrongAnswer()
        {
            WrongAttempts++;
            if (Definition.MaxWrongAttempts > 0 && WrongAttempts >= Definition.MaxWrongAttempts)
                MarkFailed("Je hebt te veel fouten gemaakt.");
        }
    }
}
