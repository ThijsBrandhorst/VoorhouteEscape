using System;
using System.Collections.Generic;
using UnityEngine;
using SchoolEscape.Player;
using SchoolEscape.UI;

namespace SchoolEscape.Puzzles
{
    /// <summary>
    /// Runs puzzles: creates sessions, shows the window, locks the player, remembers completed
    /// puzzles and announces rewards. One per scene (not a singleton: stations get it via
    /// the Inspector or find it automatically).
    /// </summary>
    public sealed class PuzzleManager : MonoBehaviour
    {
        [SerializeField] private PuzzleWindow window;
        [SerializeField] private PlayerControlLock controlLock;
        [Tooltip("Off = the world keeps running while a puzzle is open (NPCs can still move later).")]
        [SerializeField] private bool freezeTimeWhilePuzzleIsOpen;

        private readonly HashSet<string> _completed = new HashSet<string>();
        private PuzzleSession _session;
        private PuzzleDefinition _definition;
        private bool _timeFrozen;
        private float _previousTimeScale = 1f;

        public bool IsOpen => _session != null;
        public IReadOnlyCollection<string> CompletedPuzzleIds => _completed;

        public event Action<PuzzleDefinition> PuzzleOpened;
        public event Action<PuzzleDefinition> PuzzleCompleted;
        public event Action<PuzzleDefinition, bool> PuzzleClosed;
        /// <summary>Phase 4 (inventory/progression) subscribes here.</summary>
        public event Action<PuzzleReward> RewardGranted;

        public bool IsCompleted(string puzzleId) => !string.IsNullOrEmpty(puzzleId) && _completed.Contains(puzzleId);

        /// <summary>For the future save system.</summary>
        public void RestoreCompleted(IEnumerable<string> ids)
        {
            _completed.Clear();
            if (ids == null) return;
            foreach (string id in ids) _completed.Add(id);
        }

        private void Awake()
        {
            if (window == null) window = FindFirstObjectByType<PuzzleWindow>();
            if (controlLock == null) controlLock = FindFirstObjectByType<PlayerControlLock>();
            if (window == null) Debug.LogError("PuzzleManager: no PuzzleWindow found.", this);
            if (controlLock == null) Debug.LogWarning("PuzzleManager: no PlayerControlLock found; the player will not be locked.", this);
        }

        private void OnEnable()
        {
            if (window == null) return;
            window.Closed += HandleWindowClosed;
            window.RetryRequested += HandleRetry;
        }

        private void OnDisable()
        {
            if (window != null)
            {
                window.Closed -= HandleWindowClosed;
                window.RetryRequested -= HandleRetry;
            }
            if (IsOpen) EndSession(false);
        }

        public bool TryStartPuzzle(PuzzleDefinition definition)
        {
            if (window == null) return false;
            if (definition == null) { Debug.LogError("PuzzleManager: puzzle definition is null.", this); return false; }
            if (IsOpen) return false;
            if (!definition.Validate(out string error))
            {
                Debug.LogError($"PuzzleManager: '{definition.name}' is not valid: {error}", definition);
                return false;
            }

            _definition = definition;
            if (controlLock != null) controlLock.AddLock(this);
            if (freezeTimeWhilePuzzleIsOpen)
            {
                _previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                _timeFrozen = true;
            }

            OpenSession();
            PuzzleOpened?.Invoke(definition);
            return true;
        }

        private void OpenSession()
        {
            if (_session != null) _session.Solved -= HandleSolved;
            _session = _definition.CreateSession();
            _session.Solved += HandleSolved;
            window.Show(_session);
        }

        private void HandleRetry()
        {
            if (_definition != null && IsOpen) OpenSession();
        }

        private void HandleWindowClosed()
        {
            if (IsOpen) EndSession(true);
        }

        private void EndSession(bool notify)
        {
            PuzzleDefinition def = _definition;
            bool solved = _session != null && _session.State == PuzzleState.Solved;
            if (_session != null) _session.Solved -= HandleSolved;
            _session = null;
            _definition = null;

            if (controlLock != null) controlLock.RemoveLock(this);
            if (_timeFrozen)
            {
                Time.timeScale = _previousTimeScale;
                _timeFrozen = false;
            }
            if (notify) PuzzleClosed?.Invoke(def, solved);
        }

        private void HandleSolved()
        {
            PuzzleDefinition def = _definition;
            bool firstTime = _completed.Add(def.PuzzleId);

            if (firstTime || !def.RewardsOnlyOnce)
            {
                foreach (PuzzleReward reward in def.Rewards)
                {
                    if (reward == null) continue;
                    if (reward.EventChannel != null) reward.EventChannel.Raise();
                    RewardGranted?.Invoke(reward);
                }
            }
            PuzzleCompleted?.Invoke(def);
        }
    }
}
