namespace SchoolEscape.Puzzles
{
    public enum PuzzleCategory { DutchLanguage, Mathematics, Geography, Science, DigitalLiteracy, General }

    public enum PuzzleDifficulty { Easy, Medium, Hard }

    /// <summary>Visual style of the puzzle window.</summary>
    public enum PuzzleSkin { Computer, Blackboard, Paper, Keypad }

    public enum QuestionType { MultipleChoice, Text, Number }

    /// <summary>What a reward means. The puzzle system only announces rewards; Inventory/Progression (Phase 4) react to them.</summary>
    public enum PuzzleRewardKind { InventoryItem, ProgressionFlag, WorldEvent }
}
