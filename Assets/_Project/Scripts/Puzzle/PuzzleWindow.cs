using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using SchoolEscape.Puzzles;

namespace SchoolEscape.UI
{
    /// <summary>
    /// The puzzle window. Builds its whole UI in code, themed per puzzle skin
    /// (computer, blackboard, paper, keypad). Shows QuestionPuzzleSessions.
    /// Place on the "GameSystems" object. Controlled by PuzzleManager.
    /// </summary>
    public sealed class PuzzleWindow : MonoBehaviour
    {
        private enum Phase { Asking, Answered, Finished }
        private enum ActionMode { None, Check, Next }

        private const int MaxChoices = 6;

        [Header("Behaviour")]
        [SerializeField] private int sortingOrder = 50;
        [Tooltip("Esc closes the puzzle (progress of the current attempt is lost).")]
        [SerializeField] private bool closeWithEscape = true;
        [Tooltip("Seconds the feedback stays visible before the completion screen appears.")]
        [SerializeField, Min(0f)] private float solvedScreenDelay = 1.2f;

        [Header("Texts (Dutch)")]
        [SerializeField] private string correctText = "Goed zo!";
        [SerializeField] private string incorrectText = "Dat klopt nog niet. Probeer het nog eens!";

        [Header("Audio (optional)")]
        [SerializeField] private AudioClip correctClip;
        [SerializeField] private AudioClip incorrectClip;
        [SerializeField] private AudioClip completeClip;
        [SerializeField] private AudioClip failClip;

        public event Action Closed;
        public event Action RetryRequested;
        public bool IsOpen { get; private set; }

        // UI references
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private Image _overlay, _window, _header, _questionImage;
        private TextMeshProUGUI _titleText, _statusText, _instructionText, _questionText, _feedbackText, _hintText;
        private TextMeshProUGUI _resultTitle, _resultBody;
        private Button _closeButton, _hintButton, _actionButton, _resultPrimary, _resultSecondary;
        private readonly Button[] _choiceButtons = new Button[MaxChoices];
        private TMP_InputField _input;
        private GameObject _content, _footer, _resultPanel;
        private AudioSource _audio;

        // State
        private QuestionPuzzleSession _session;
        private UiPalette _palette;
        private Phase _phase;
        private ActionMode _actionMode;
        private int _activeChoices;
        private float _deadline = -1f;
        private int _statusKey = -1;
        private int _answerFrame = -1;
        private readonly StringBuilder _sb = new StringBuilder(128);
        private bool _failedHandled;

        private void Awake()
        {
            UiKit.EnsureEventSystem();
            Build();
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.spatialBlend = 0f;
            _canvas.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------------ Public API

        public void Show(PuzzleSession session)
        {
            if (session is not QuestionPuzzleSession question)
            {
                Debug.LogError($"PuzzleWindow cannot display sessions of type {session?.GetType().Name}. Add a view for it.", this);
                Closed?.Invoke();
                return;
            }

            _session = question;
            _failedHandled = false;
            PuzzleDefinition def = _session.Definition;
            _palette = UiPalettes.For(def.Skin);
            ApplyPalette();

            _titleText.text = def.WindowTitle;
            _instructionText.text = def.Instructions;
            _instructionText.gameObject.SetActive(!string.IsNullOrWhiteSpace(def.Instructions));
            _deadline = def.TimeLimitSeconds > 0f ? Time.unscaledTime + def.TimeLimitSeconds : -1f;
            _statusKey = -1;

            _resultPanel.SetActive(false);
            _content.SetActive(true);
            _footer.SetActive(true);
            _canvas.gameObject.SetActive(true);
            IsOpen = true;

            ShowQuestion();
            StopAllCoroutines();
            StartCoroutine(AnimateIn());
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            StopAllCoroutines();
            _canvas.gameObject.SetActive(false);
            Closed?.Invoke();
        }

        // ------------------------------------------------------------------ Update

        private void Update()
        {
            if (!IsOpen) return;

            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                if (closeWithEscape && kb.escapeKey.wasPressedThisFrame) { Close(); return; }

                if (_phase == Phase.Asking && _session.Current.Type == QuestionType.MultipleChoice)
                {
                    for (int i = 0; i < _activeChoices; i++)
                    {
                        if (kb[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame || kb[(Key)((int)Key.A + i)].wasPressedThisFrame)
                        {
                            OnChoice(i);
                            break;
                        }
                    }
                }
                else if (_phase == Phase.Answered && _actionMode == ActionMode.Next && Time.frameCount != _answerFrame
                         && (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame))
                {
                    OnNext();
                }
            }

            if (_phase != Phase.Finished && _session.State == PuzzleState.InProgress && _deadline > 0f
                && Time.unscaledTime >= _deadline && !_failedHandled)
            {
                _session.Expire();
                ShowFailed();
                return;
            }
            UpdateStatus();
        }

        // ------------------------------------------------------------------ Question flow

        private void ShowQuestion()
        {
            _phase = Phase.Asking;
            QuestionData q = _session.Current;

            _questionText.text = q.Prompt;
            bool hasImage = q.Image != null;
            _questionImage.gameObject.SetActive(hasImage);
            if (hasImage) _questionImage.sprite = q.Image;

            bool multipleChoice = q.Type == QuestionType.MultipleChoice;
            _activeChoices = multipleChoice ? Mathf.Min(_session.ChoiceCount, MaxChoices) : 0;
            for (int i = 0; i < MaxChoices; i++)
            {
                Button b = _choiceButtons[i];
                bool active = i < _activeChoices;
                b.gameObject.SetActive(active);
                if (!active) continue;
                UiKit.LabelOf(b).text = $"{(char)('A' + i)}.   {_session.GetChoice(i)}";
                ((Image)b.targetGraphic).color = _palette.Panel;
                b.interactable = true;
            }

            _input.gameObject.SetActive(!multipleChoice);
            if (!multipleChoice)
            {
                _input.text = string.Empty;
                _input.interactable = true;
                string placeholder = !string.IsNullOrEmpty(q.InputPlaceholder) ? q.InputPlaceholder
                    : q.Type == QuestionType.Number ? "Typ een getal..." : "Typ je antwoord...";
                ((TMP_Text)_input.placeholder).text = placeholder;
                StartCoroutine(FocusInputNextFrame());
            }

            _feedbackText.text = string.Empty;
            _hintText.text = string.Empty;
            _hintButton.gameObject.SetActive(!string.IsNullOrWhiteSpace(q.Hint));
            SetAction(multipleChoice ? ActionMode.None : ActionMode.Check);
            _statusKey = -1;
            UpdateStatus();
        }

        private void OnChoice(int displayIndex)
        {
            if (_phase != Phase.Asking) return;
            AnswerResult result = _session.SubmitChoice(displayIndex);
            Button chosen = _choiceButtons[displayIndex];

            if (result.Outcome == AnswerOutcome.Correct)
            {
                ((Image)chosen.targetGraphic).color = _palette.Good;
                for (int i = 0; i < _activeChoices; i++) _choiceButtons[i].interactable = false;
            }
            else if (result.Outcome == AnswerOutcome.Incorrect)
            {
                ((Image)chosen.targetGraphic).color = _palette.Bad;
                chosen.interactable = false;
            }
            HandleResult(result);
        }

        private void OnCheck()
        {
            if (_phase != Phase.Asking || _session.Current.Type == QuestionType.MultipleChoice) return;
            HandleResult(_session.SubmitText(_input.text));
        }

        private void OnInputSubmit(string _)
        {
            if (_phase == Phase.Asking) OnCheck();
        }

        private void HandleResult(AnswerResult result)
        {
            switch (result.Outcome)
            {
                case AnswerOutcome.Invalid:
                    if (!string.IsNullOrEmpty(result.Message)) SetFeedback(result.Message, _palette.Bad);
                    StartCoroutine(FocusInputNextFrame());
                    return;

                case AnswerOutcome.Incorrect:
                    Play(incorrectClip);
                    if (_session.State == PuzzleState.Failed) { ShowFailed(); return; }
                    string message = incorrectText;
                    if (_session.AttemptsRemaining >= 0) message += $"  (Nog {_session.AttemptsRemaining} pogingen)";
                    SetFeedback(message, _palette.Bad);
                    StartCoroutine(FocusInputNextFrame());
                    return;

                case AnswerOutcome.Correct:
                    Play(correctClip);
                    _phase = Phase.Answered;
                    _answerFrame = Time.frameCount;
                    _input.interactable = false;
                    string good = correctText;
                    if (!string.IsNullOrWhiteSpace(result.Message)) good += "\n" + result.Message;
                    SetFeedback(good, _palette.Good);

                    if (_session.State == PuzzleState.Solved)
                    {
                        SetAction(ActionMode.None);
                        StartCoroutine(ShowSolvedAfterDelay());
                    }
                    else
                    {
                        SetAction(ActionMode.Next);
                    }
                    return;
            }
        }

        private void OnNext()
        {
            if (_phase != Phase.Answered) return;
            _session.Advance();
            ShowQuestion();
        }

        private void OnHint()
        {
            if (_phase != Phase.Asking) return;
            _hintText.text = "Hint: " + _session.UseHint();
        }

        private void OnAction()
        {
            if (_actionMode == ActionMode.Check) OnCheck();
            else if (_actionMode == ActionMode.Next) OnNext();
        }

        // ------------------------------------------------------------------ Result screens

        private IEnumerator ShowSolvedAfterDelay()
        {
            yield return new WaitForSecondsRealtime(solvedScreenDelay);
            if (IsOpen) ShowSolved();
        }

        private void ShowSolved()
        {
            _phase = Phase.Finished;
            _content.SetActive(false);
            _footer.SetActive(false);
            _resultPanel.SetActive(true);

            PuzzleDefinition def = _session.Definition;
            _resultTitle.text = "OPDRACHT VOLTOOID!";
            _resultTitle.color = _palette.Good;

            _sb.Clear();
            _sb.Append(def.CompletionMessage);
            foreach (PuzzleReward reward in def.Rewards)
            {
                if (reward != null && !string.IsNullOrWhiteSpace(reward.DisplayMessage))
                    _sb.Append("\n\n+ ").Append(reward.DisplayMessage);
            }
            _sb.Append("\n\nFouten: ").Append(_session.WrongAttempts).Append("     Hints: ").Append(_session.HintsUsed);
            _resultBody.text = _sb.ToString();

            UiKit.LabelOf(_resultPrimary).text = "DOORGAAN";
            _resultSecondary.gameObject.SetActive(false);
            Play(completeClip);
        }

        private void ShowFailed()
        {
            _failedHandled = true;
            _phase = Phase.Finished;
            StopAllCoroutines();
            _content.SetActive(false);
            _footer.SetActive(false);
            _resultPanel.SetActive(true);

            _resultTitle.text = "HELAAS!";
            _resultTitle.color = _palette.Bad;
            _resultBody.text = _session.FailReason + "\n\nProbeer het nog een keer. Je kunt het!";
            UiKit.LabelOf(_resultPrimary).text = "OPNIEUW PROBEREN";
            _resultSecondary.gameObject.SetActive(true);
            UiKit.LabelOf(_resultSecondary).text = "SLUITEN";
            Play(failClip);
        }

        private void OnResultPrimary()
        {
            if (_session.State == PuzzleState.Failed) RetryRequested?.Invoke();
            else Close();
        }

        // ------------------------------------------------------------------ Helpers

        private void SetAction(ActionMode mode)
        {
            _actionMode = mode;
            _actionButton.gameObject.SetActive(mode != ActionMode.None);
            if (mode == ActionMode.Check) UiKit.LabelOf(_actionButton).text = "CONTROLEREN";
            else if (mode == ActionMode.Next) UiKit.LabelOf(_actionButton).text = _session.HasNext ? "VOLGENDE" : "KLAAR";
        }

        private void SetFeedback(string text, Color color)
        {
            _feedbackText.text = text;
            _feedbackText.color = color;
        }

        private void UpdateStatus()
        {
            int remaining = _deadline > 0f ? Mathf.Max(0, Mathf.CeilToInt(_deadline - Time.unscaledTime)) : -1;
            int attempts = _session.AttemptsRemaining;
            int key = remaining * 100000 + _session.StepIndex * 100 + (attempts + 1);
            if (key == _statusKey) return;
            _statusKey = key;

            _sb.Clear();
            _sb.Append("Vraag ").Append(_session.StepIndex + 1).Append(" van ").Append(_session.StepCount);
            if (remaining >= 0) _sb.Append("   |   ").Append(remaining / 60).Append(':').Append((remaining % 60).ToString("00"));
            if (attempts >= 0) _sb.Append("   |   Pogingen: ").Append(attempts);
            _statusText.text = _sb.ToString();
        }

        private void Play(AudioClip clip)
        {
            if (clip != null) _audio.PlayOneShot(clip);
        }

        private IEnumerator FocusInputNextFrame()
        {
            yield return null;
            if (IsOpen && _phase == Phase.Asking && _input.gameObject.activeInHierarchy)
                _input.ActivateInputField();
        }

        private IEnumerator AnimateIn()
        {
            RectTransform window = _window.rectTransform;
            float time = 0f;
            const float duration = 0.18f;
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(time / duration);
                k = 1f - (1f - k) * (1f - k);
                _canvasGroup.alpha = k;
                window.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, k);
                yield return null;
            }
            _canvasGroup.alpha = 1f;
            window.localScale = Vector3.one;
        }

        private static void Style(Button b, Color bg, Color text)
        {
            ((Image)b.targetGraphic).color = bg;
            UiKit.LabelOf(b).color = text;
        }

        private void ApplyPalette()
        {
            UiPalette p = _palette;
            _overlay.color = p.Overlay;
            _window.color = p.Window;
            _header.color = p.Header;
            _titleText.color = p.HeaderText;
            _statusText.color = p.HeaderText;
            Style(_closeButton, p.Panel, p.Text);
            _instructionText.color = p.TextDim;
            _questionText.color = p.Text;
            _hintText.color = p.TextDim;
            foreach (Button b in _choiceButtons) Style(b, p.Panel, p.Text);
            _input.GetComponent<Image>().color = p.InputBg;
            _input.textComponent.color = p.Text;
            ((TMP_Text)_input.placeholder).color = p.TextDim;
            _input.caretColor = p.Accent;
            Style(_hintButton, p.Panel, p.Text);
            Style(_actionButton, p.Button, p.ButtonText);
            _resultBody.color = p.Text;
            Style(_resultPrimary, p.Button, p.ButtonText);
            Style(_resultSecondary, p.Panel, p.Text);
        }

        // ------------------------------------------------------------------ UI construction

        private void Build()
        {
            _canvas = UiKit.CreateCanvas(transform, "PuzzleCanvas", sortingOrder);
            _canvasGroup = _canvas.gameObject.AddComponent<CanvasGroup>();

            _overlay = UiKit.CreatePanel(_canvas.transform, "Overlay", new Color(0, 0, 0, 0.7f), false);
            UiKit.Stretch(_overlay.rectTransform);

            _window = UiKit.CreatePanel(_overlay.transform, "Window", Color.white);
            UiKit.Place(_window.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1240, 860));

            // Header
            _header = UiKit.CreatePanel(_window.transform, "Header", Color.white);
            UiKit.SetRect(_header.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(16, -106), new Vector2(-16, -16));
            _titleText = UiKit.CreateText(_header.transform, "Title", "", 34, Color.white, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
            UiKit.SetRect(_titleText.rectTransform, new Vector2(0, 0), new Vector2(0.58f, 1), new Vector2(28, 0), Vector2.zero);
            _statusText = UiKit.CreateText(_header.transform, "Status", "", 26, Color.white, TextAlignmentOptions.MidlineRight);
            UiKit.SetRect(_statusText.rectTransform, new Vector2(0.58f, 0), new Vector2(1, 1), Vector2.zero, new Vector2(-96, 0));
            _closeButton = UiKit.CreateButton(_header.transform, "Close", "X", 30, Color.gray, Color.white);
            UiKit.Place((RectTransform)_closeButton.transform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-14, 0), new Vector2(64, 64));
            _closeButton.onClick.AddListener(Close);

            // Content (question area)
            RectTransform content = UiKit.CreateRect("Content", _window.transform);
            _content = content.gameObject;
            UiKit.SetRect(content, Vector2.zero, Vector2.one, new Vector2(44, 130), new Vector2(-44, -124));
            UiKit.AddVLayout(_content, 12, 0, TextAnchor.UpperCenter, true);

            _instructionText = UiKit.CreateText(content, "Instruction", "", 26, Color.white, TextAlignmentOptions.Left, FontStyles.Italic);
            UiKit.AddLayout(_instructionText, prefH: 34);

            _questionImage = UiKit.CreatePanel(content, "QuestionImage", Color.white, false, false);
            _questionImage.preserveAspect = true;
            UiKit.AddLayout(_questionImage, prefH: 200);
            _questionImage.gameObject.SetActive(false);

            _questionText = UiKit.CreateText(content, "Question", "", 40, Color.white, TextAlignmentOptions.TopLeft, FontStyles.Bold);
            _questionText.enableAutoSizing = true;
            _questionText.fontSizeMin = 28;
            _questionText.fontSizeMax = 40;
            UiKit.AddLayout(_questionText, prefH: 110);

            for (int i = 0; i < MaxChoices; i++)
            {
                int captured = i;
                Button b = UiKit.CreateButton(content, $"Choice{i}", "", 30, Color.gray, Color.white);
                TMP_Text label = UiKit.LabelOf(b);
                label.alignment = TextAlignmentOptions.MidlineLeft;
                UiKit.Stretch(label.rectTransform, 28, 4, 16, 4);
                UiKit.AddLayout(b, prefH: 60);
                b.onClick.AddListener(() => OnChoice(captured));
                _choiceButtons[i] = b;
            }

            _input = UiKit.CreateInput(content, "AnswerInput", 36, Color.black, Color.white, Color.gray, "");
            UiKit.AddLayout(_input, prefH: 80);
            _input.onSubmit.AddListener(OnInputSubmit);

            _feedbackText = UiKit.CreateText(content, "Feedback", "", 30, Color.white, TextAlignmentOptions.TopLeft, FontStyles.Bold);
            UiKit.AddLayout(_feedbackText, minH: 70);
            _hintText = UiKit.CreateText(content, "Hint", "", 26, Color.white, TextAlignmentOptions.TopLeft, FontStyles.Italic);
            UiKit.AddLayout(_hintText, minH: 36);

            // Footer
            RectTransform footer = UiKit.CreateRect("Footer", _window.transform);
            _footer = footer.gameObject;
            UiKit.SetRect(footer, Vector2.zero, new Vector2(1, 0), new Vector2(16, 16), new Vector2(-16, 110));
            _hintButton = UiKit.CreateButton(footer, "HintButton", "HINT", 28, Color.gray, Color.white);
            UiKit.Place((RectTransform)_hintButton.transform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(24, 0), new Vector2(240, 72));
            _hintButton.onClick.AddListener(OnHint);
            _actionButton = UiKit.CreateButton(footer, "ActionButton", "CONTROLEREN", 30, Color.gray, Color.white);
            UiKit.Place((RectTransform)_actionButton.transform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-24, 0), new Vector2(380, 72));
            _actionButton.onClick.AddListener(OnAction);

            // Result panel (completion / failure)
            RectTransform result = UiKit.CreateRect("Result", _window.transform);
            _resultPanel = result.gameObject;
            UiKit.SetRect(result, Vector2.zero, Vector2.one, new Vector2(60, 60), new Vector2(-60, -130));
            UiKit.AddVLayout(_resultPanel, 24, 0, TextAnchor.MiddleCenter, false);
            _resultTitle = UiKit.CreateText(result, "ResultTitle", "", 64, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            UiKit.AddLayout(_resultTitle, prefW: 1000, prefH: 90);
            _resultBody = UiKit.CreateText(result, "ResultBody", "", 34, Color.white, TextAlignmentOptions.Center);
            UiKit.AddLayout(_resultBody, prefW: 1000, prefH: 320);
            _resultPrimary = UiKit.CreateButton(result, "ResultPrimary", "DOORGAAN", 34, Color.gray, Color.white);
            UiKit.AddLayout(_resultPrimary, prefW: 520, prefH: 84);
            _resultPrimary.onClick.AddListener(OnResultPrimary);
            _resultSecondary = UiKit.CreateButton(result, "ResultSecondary", "SLUITEN", 28, Color.gray, Color.white);
            UiKit.AddLayout(_resultSecondary, prefW: 520, prefH: 64);
            _resultSecondary.onClick.AddListener(Close);
            _resultPanel.SetActive(false);
        }
    }
}
