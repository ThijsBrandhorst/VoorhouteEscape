using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SchoolEscape.Interaction;

namespace SchoolEscape.UI
{
    /// <summary>Crosshair, "[E] ..." interaction prompt and short feedback toasts. Place on "GameSystems".</summary>
    public sealed class GameHud : MonoBehaviour
    {
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private int sortingOrder = 10;
        [SerializeField] private Color promptColor = Color.white;
        [SerializeField] private Color unavailableColor = new Color(1f, 0.82f, 0.5f, 1f);
        [SerializeField, Min(0.5f)] private float toastSeconds = 2.5f;

        private GameObject _crosshair, _promptRoot;
        private TextMeshProUGUI _promptText, _toastText;
        private CanvasGroup _toastGroup;
        private float _toastUntil;
        private string _lastPrompt = string.Empty;
        private bool _lastCan;

        private void Awake()
        {
            if (interactor == null) interactor = FindFirstObjectByType<PlayerInteractor>();
            Build();
        }

        private void OnEnable()
        {
            if (interactor != null) interactor.FeedbackRequested += ShowToast;
        }

        private void OnDisable()
        {
            if (interactor != null) interactor.FeedbackRequested -= ShowToast;
        }

        private void ShowToast(string message)
        {
            _toastText.text = message;
            _toastUntil = Time.unscaledTime + toastSeconds;
            _toastGroup.alpha = 1f;
            _toastGroup.gameObject.SetActive(true);
        }

        private void Update()
        {
            bool show = interactor != null && !interactor.IsBlocked;
            _crosshair.SetActive(show);

            string prompt = show ? interactor.CurrentPrompt : string.Empty;
            bool can = show && interactor.CurrentCanInteract;
            if (prompt != _lastPrompt || can != _lastCan)
            {
                _lastPrompt = prompt;
                _lastCan = can;
                _promptText.text = can ? "[E]  " + prompt : prompt;
                _promptText.color = can ? promptColor : unavailableColor;
                _promptRoot.SetActive(prompt.Length > 0);
            }

            if (_toastGroup.gameObject.activeSelf)
            {
                float left = _toastUntil - Time.unscaledTime;
                if (left <= 0f) _toastGroup.gameObject.SetActive(false);
                else _toastGroup.alpha = Mathf.Clamp01(left / 0.4f);
            }
        }

        private void Build()
        {
            Canvas canvas = UiKit.CreateCanvas(transform, "HudCanvas", sortingOrder);
            canvas.GetComponent<GraphicRaycaster>().enabled = false;

            Image dot = UiKit.CreatePanel(canvas.transform, "Crosshair", new Color(1, 1, 1, 0.85f), true, false);
            UiKit.Place(dot.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(10, 10));
            _crosshair = dot.gameObject;

            _promptRoot = CreatePill(canvas.transform, "Prompt", new Vector2(0.5f, 0f), new Vector2(0, 190), 36, out _promptText);
            _promptRoot.SetActive(false);

            _toastGroup = CreatePill(canvas.transform, "Toast", new Vector2(0.5f, 1f), new Vector2(0, -130), 32, out _toastText)
                .AddComponent<CanvasGroup>();
            _toastGroup.gameObject.SetActive(false);
        }

        private static GameObject CreatePill(Transform parent, string name, Vector2 anchor, Vector2 pos, float fontSize, out TextMeshProUGUI text)
        {
            Image bg = UiKit.CreatePanel(parent, name, new Color(0, 0, 0, 0.62f), true, false);
            UiKit.Place(bg.rectTransform, anchor, anchor, pos, new Vector2(100, 60));
            UiKit.AddHLayout(bg.gameObject, 0, 0, TextAnchor.MiddleCenter).padding = new RectOffset(32, 32, 14, 14);
            var fitter = bg.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            text = UiKit.CreateText(bg.transform, "Text", "", fontSize, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            text.textWrappingMode = TextWrappingModes.NoWrap;
            return bg.gameObject;
        }
    }
}
