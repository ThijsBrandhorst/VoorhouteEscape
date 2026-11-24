using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SchoolEscape.Player {
    /// <summary>
    /// Counted lock for player controls. While at least one owner holds a lock,
    /// movement and look are disabled and the cursor is free.
    /// Puzzle UIs, menus and cutscenes call AddLock(this) / RemoveLock(this).
    /// Place on the Player root.
    /// </summary>
    public sealed class PlayerControlLock : MonoBehaviour {
        [Tooltip("Lock and hide the cursor when gameplay starts.")]
        [SerializeField] private bool lockCursorOnStart = true;

        private readonly HashSet<object> _owners = new HashSet<object>();

        /// <summary>True while controls are locked (cursor is free).</summary>
        public bool IsLocked => _owners.Count > 0;

        /// <summary>Frame in which the last lock was released. Lets the pause menu ignore an Esc press that just closed a puzzle.</summary>
        public int LastUnlockFrame { get; private set; } = -1;

        /// <summary>Raised when locked state changes. Parameter = IsLocked.</summary>
        public event Action<bool> LockStateChanged;

        private void Start() {
            if (lockCursorOnStart) ApplyCursor();
        }

        public void AddLock(object owner) {
            if (owner == null) {
                Debug.LogError("PlayerControlLock.AddLock called with a null owner.", this);
                return;
            }
            if (_owners.Add(owner) && _owners.Count == 1) {
                ApplyCursor();
                LockStateChanged?.Invoke(true);
            }
        }

        public void RemoveLock(object owner) {
            if (owner != null && _owners.Remove(owner) && _owners.Count == 0) {
                LastUnlockFrame = Time.frameCount;
                ApplyCursor();
                LockStateChanged?.Invoke(false);
            }
        }

        private void ApplyCursor() {
            bool free = IsLocked;
            Cursor.lockState = free ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = free;
        }

        private void OnApplicationFocus(bool hasFocus) {
            if (hasFocus) ApplyCursor();
        }

        private void Update() {
            // Re-capture the cursor with a click after Esc / alt-tab in the editor.
            if (!IsLocked && Cursor.lockState != CursorLockMode.Locked
                && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) {
                ApplyCursor();
            }
        }
    }
}