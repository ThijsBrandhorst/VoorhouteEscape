using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using SchoolEscape.Player;

namespace SchoolEscape.UI
{
    /// <summary>
    /// Esc pause menu with options (mouse sensitivity, volume, head bob, fullscreen, graphics quality).
    /// Settings are saved in PlayerPrefs. Place on "GameSystems".
    /// </summary>
    public sealed class PauseMenu : MonoBehaviour
    {
        private const string SensitivityKey = "opt_sensitivity";
        private const string VolumeKey = "opt_volume";
        private const string HeadBobKey = "opt_headbob";
        private const string FullscreenKey = "opt_fullscreen";
        private const string QualityKey = "opt_quality";

        [SerializeField] private PlayerControlLock controlLock;
        [SerializeField] private PlayerLook look;
        [SerializeField] private HeadBob headBob;
        [SerializeField] private CameraEffects cameraEffects;
        [SerializeField] private int sortingOrder = 100;

        private GameObject _root, _mainPanel, _optionsPanel;
        private TMP_Text _qualityLabel;
        private bool _isOpen;
        private float _previousTimeScale = 1f;
        private UiPalette _p;

        private void Awake()
        {
            if (controlLock == null) controlLock = FindFirstObjectByType<PlayerControlLock>();
            if (look == null) look = FindFirstObjectByType<PlayerLook>();
            if (headBob == null) headBob = FindFirstObjectByType<HeadBob>();
            if (cameraEffects == null) cameraEffects = FindFirstObjectByType<CameraEffects>();
            UiKit.EnsureEventSystem();
            _p = UiPalettes.Menu;
            Build();
            _root.SetActive(false);
        }

        private void Start() => ApplySavedSettings();

        private void OnDestroy()
        {
            if (_isOpen) Time.timeScale = _previousTimeScale;
        }

        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null || !kb.escapeKey.wasPressedThisFrame) return;

            if (_isOpen)
            {
                if (_optionsPanel.activeSelf) ShowMain();
                else Resume();
                return;
            }

            // Ignore Esc while a puzzle is open, or in the same frame one was just closed with Esc.
            if (controlLock == null || controlLock.IsLocked || controlLock.LastUnlockFrame == Time.frameCount) return;
            Open();
        }

        private void Open()
        {
            _isOpen = true;
            controlLock.AddLock(this);
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            _root.SetActive(true);
            ShowMain();
        }

        public void Resume()
        {
            if (!_isOpen) return;
            _isOpen = false;
            Time.timeScale = _previousTimeScale;
            _root.SetActive(false);
            controlLock.RemoveLock(this);
        }

        private void ShowMain()
        {
            _mainPanel.SetActive(true);
            _optionsPanel.SetActive(false);
        }

        private void ShowOptions()
        {
            _mainPanel.SetActive(false);
            _optionsPanel.SetActive(true);
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ---------------------------------------------------------------- Settings

        private void ApplySavedSettings()
        {
            ApplySensitivity(PlayerPrefs.GetFloat(SensitivityKey, 1f));
            AudioListener.volume = PlayerPrefs.GetFloat(VolumeKey, 1f);
            ApplyHeadBob(PlayerPrefs.GetInt(HeadBobKey, 1) == 1);
            if (PlayerPrefs.HasKey(FullscreenKey)) Screen.fullScreen = PlayerPrefs.GetInt(FullscreenKey) == 1;
            if (PlayerPrefs.HasKey(QualityKey)) QualitySettings.SetQualityLevel(PlayerPrefs.GetInt(QualityKey), true);
        }

        private void ApplySensitivity(float value)
        {
            if (look != null) look.SetSensitivityMultiplier(value);
        }

        private void ApplyHeadBob(bool on)
        {
            if (headBob != null) headBob.SetBobEnabled(on);
            if (cameraEffects != null) cameraEffects.SetEffectsEnabled(on);
        }

        private void CycleQuality()
        {
            int count = QualitySettings.names.Length;
            if (count == 0) return;
            int next = (QualitySettings.GetQualityLevel() + 1) % count;
            QualitySettings.SetQualityLevel(next, true);
            PlayerPrefs.SetInt(QualityKey, next);
            RefreshQualityLabel();
        }

        private void RefreshQualityLabel()
        {
            string[] names = QualitySettings.names;
            int level = QualitySettings.GetQualityLevel();
            _qualityLabel.text = "KWALITEIT: " + (level >= 0 && level < names.Length ? names[level].ToUpperInvariant() : "-");
        }

        // ---------------------------------------------------------------- UI

        private void Build()
        {
            Canvas canvas = UiKit.CreateCanvas(transform, "PauseCanvas", sortingOrder);
            _root = canvas.gameObject;

            Image overlay = UiKit.CreatePanel(canvas.transform, "Overlay", new Color(0, 0, 0, 0.75f), false);
            UiKit.Stretch(overlay.rectTransform);

            Image window = UiKit.CreatePanel(overlay.transform, "Window", _p.Window);
            UiKit.Place(window.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820, 760));

            _mainPanel = BuildPanel(window.transform, "Main");
            AddTitle(_mainPanel.transform, "PAUZE");
            AddButton(_mainPanel.transform, "HERVATTEN", Resume);
            AddButton(_mainPanel.transform, "OPTIES", ShowOptions);
            AddButton(_mainPanel.transform, "AFSLUITEN", Quit);

            _optionsPanel = BuildPanel(window.transform, "Options");
            AddTitle(_optionsPanel.transform, "OPTIES");

            float sens = PlayerPrefs.GetFloat(SensitivityKey, 1f);
            Slider sensSlider = AddSliderRow(_optionsPanel.transform, "Muisgevoeligheid", 0.25f, 2.5f, sens);
            sensSlider.onValueChanged.AddListener(v => { ApplySensitivity(v); PlayerPrefs.SetFloat(SensitivityKey, v); });

            Slider volSlider = AddSliderRow(_optionsPanel.transform, "Volume", 0f, 1f, PlayerPrefs.GetFloat(VolumeKey, 1f));
            volSlider.onValueChanged.AddListener(v => { AudioListener.volume = v; PlayerPrefs.SetFloat(VolumeKey, v); });

            Toggle bobToggle = AddToggleRow(_optionsPanel.transform, "Hoofdbeweging", PlayerPrefs.GetInt(HeadBobKey, 1) == 1);
            bobToggle.onValueChanged.AddListener(on => { ApplyHeadBob(on); PlayerPrefs.SetInt(HeadBobKey, on ? 1 : 0); });

            Toggle fsToggle = AddToggleRow(_optionsPanel.transform, "Volledig scherm", Screen.fullScreen);
            fsToggle.onValueChanged.AddListener(on => { Screen.fullScreen = on; PlayerPrefs.SetInt(FullscreenKey, on ? 1 : 0); });

            Button quality = AddButton(_optionsPanel.transform, "", CycleQuality);
            _qualityLabel = UiKit.LabelOf(quality);
            RefreshQualityLabel();

            AddButton(_optionsPanel.transform, "TERUG", ShowMain);
            _optionsPanel.SetActive(false);
        }

        private GameObject BuildPanel(Transform parent, string name)
        {
            RectTransform rt = UiKit.CreateRect(name, parent);
            UiKit.Stretch(rt, 50, 40, 50, 40);
            UiKit.AddVLayout(rt.gameObject, 18, 0, TextAnchor.UpperCenter, true);
            return rt.gameObject;
        }

        private void AddTitle(Transform parent, string text)
        {
            TextMeshProUGUI t = UiKit.CreateText(parent, "Title", text, 64, _p.Accent, TextAlignmentOptions.Center, FontStyles.Bold);
            UiKit.AddLayout(t, prefH: 100);
        }

        private Button AddButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
        {
            Button b = UiKit.CreateButton(parent, "Button", label, 34, _p.Button, _p.ButtonText);
            UiKit.AddLayout(b, prefH: 84);
            b.onClick.AddListener(onClick);
            return b;
        }

        private RectTransform AddRow(Transform parent, string label)
        {
            RectTransform row = UiKit.CreateRect("Row", parent);
            UiKit.AddHLayout(row.gameObject, 20, 0, TextAnchor.MiddleLeft);
            UiKit.AddLayout(row, prefH: 64);
            TextMeshProUGUI t = UiKit.CreateText(row, "Label", label, 32, _p.Text, TextAlignmentOptions.MidlineLeft);
            UiKit.AddLayout(t, prefW: 330, prefH: 64);
            return row;
        }

        private Slider AddSliderRow(Transform parent, string label, float min, float max, float value)
        {
            RectTransform row = AddRow(parent, label);
            Slider s = UiKit.CreateSlider(row, "Slider", _p.InputBg, _p.Accent, _p.Text, min, max, value);
            UiKit.AddLayout(s, prefH: 48, flexW: 1);
            return s;
        }

        private Toggle AddToggleRow(Transform parent, string label, bool isOn)
        {
            RectTransform row = AddRow(parent, label);
            Toggle t = UiKit.CreateToggle(row, "Toggle", _p.InputBg, _p.Accent, isOn);
            UiKit.AddLayout(t, prefW: 52, prefH: 52);
            return t;
        }
    }
}
