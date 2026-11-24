using System;
using UnityEngine;

namespace SchoolEscape.Core {
    /// <summary>
    /// A ScriptableObject "event wire". Puzzles raise it as a reward; any number of
    /// GameEventListener components (doors, lights, cutscenes...) can react to it.
    /// Create via Create > School Escape > Core > Game Event Channel.
    /// </summary>
    [CreateAssetMenu(fileName = "GameEvent", menuName = "School Escape/Core/Game Event Channel")]
    public sealed class GameEventChannel : ScriptableObject {
        public event Action Raised;

        public void Raise() => Raised?.Invoke();
    }
}