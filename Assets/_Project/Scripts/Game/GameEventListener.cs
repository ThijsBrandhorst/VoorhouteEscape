using UnityEngine;
using UnityEngine.Events;

namespace SchoolEscape.Core {
    /// <summary>
    /// Listens to a GameEventChannel and invokes a UnityEvent.
    /// Example: put on a door, assign the "OpenStorageDoor" channel, and hook the response
    /// to your door's open method or to GameObject.SetActive(false) of a blocker.
    /// </summary>
    [AddComponentMenu("School Escape/Game Event Listener")]
    public sealed class GameEventListener : MonoBehaviour {
        [SerializeField] private GameEventChannel channel;
        [SerializeField] private UnityEvent response;

        private void OnEnable() {
            if (channel == null) { Debug.LogWarning("GameEventListener has no channel assigned.", this); return; }
            channel.Raised += OnRaised;
        }

        private void OnDisable() {
            if (channel != null) channel.Raised -= OnRaised;
        }

        private void OnRaised() => response?.Invoke();
    }
}