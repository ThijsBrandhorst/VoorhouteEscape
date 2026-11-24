using System;
using UnityEngine;
using SchoolEscape.Core;

namespace SchoolEscape.Puzzles
{
    /// <summary>
    /// One reward granted when a puzzle is solved. Purely data: the PuzzleManager raises
    /// RewardGranted (for inventory/progression systems) and the optional GameEventChannel.
    /// </summary>
    [Serializable]
    public sealed class PuzzleReward
    {
        [SerializeField] private PuzzleRewardKind kind = PuzzleRewardKind.InventoryItem;
        [Tooltip("Item ID, progression flag ID or free identifier, depending on the kind.")]
        [SerializeField] private string targetId = string.Empty;
        [SerializeField, Min(1)] private int amount = 1;
        [Tooltip("Optional: raised when the reward is granted. Hook a GameEventListener to it (open a door, enable an object...).")]
        [SerializeField] private GameEventChannel eventChannel;
        [Tooltip("Shown on the completion screen, e.g. 'Je hebt de sleutel van het klaslokaal gevonden!'")]
        [SerializeField, TextArea(1, 3)] private string displayMessage = string.Empty;

        public PuzzleReward() { }

        public PuzzleReward(PuzzleRewardKind kind, string targetId, string displayMessage, int amount = 1, GameEventChannel channel = null)
        {
            this.kind = kind;
            this.targetId = targetId;
            this.displayMessage = displayMessage;
            this.amount = amount;
            eventChannel = channel;
        }

        public PuzzleRewardKind Kind => kind;
        public string TargetId => targetId;
        public int Amount => amount;
        public GameEventChannel EventChannel => eventChannel;
        public string DisplayMessage => displayMessage;
    }
}
