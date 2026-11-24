using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace SchoolEscape.UI
{
    /// <summary>
    /// Small helper set for building uGUI in code (canvases, panels, text, buttons, input, slider, toggle).
    /// Requires TextMeshPro essentials (Window > TextMeshPro > Import TMP Essential Resources).
    /// </summary>
    public static class UiKit
    {
        private static Sprite _rounded;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _rounded = null;

        public static Sprite RoundedSprite
        {
            get
            {
                if (_rounded == null) _rounded = BuildRoundedSprite(16, 48);
                return _rounded;
            }
        }

        private static Sprite BuildRoundedSprite(int radius, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave
            };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float cx = Mathf.Clamp(x + 0.5f, radius, size - radius);
                    float cy = Mathf.Clamp(y + 0.5f, radius, size - radius);
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                    byte a = (byte)(Mathf.Clamp01(radius - d + 0.5f) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, a);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }

        // ---------- Canvas / EventSystem ----------

        public static Canvas CreateCanvas(Transform parent, string name, int sortingOrder)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem));
            var module = go.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }

        // ---------- Layout helpers ----------

        public static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static void SetRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        public static void Stretch(RectTransform rt, float left = 0, float bottom = 0, float right = 0, float top = 0)
            => SetRect(rt, Vector2.zero, Vector2.one, new Vector2(left, bottom), new Vector2(-right, -top));

        /// <summary>Fixed-size placement relative to an anchor point.</summary>
        public static void Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
        }

        public static LayoutElement AddLayout(Component c, float minW = -1, float minH = -1, float prefW = -1, float prefH = -1, float flexW = -1, float flexH = -1)
        {
            if (!c.TryGetComponent(out LayoutElement le)) le = c.gameObject.AddComponent<LayoutElement>();
            le.minWidth = minW; le.minHeight = minH; le.preferredWidth = prefW; le.preferredHeight = prefH;
            le.flexibleWidth = flexW; le.flexibleHeight = flexH;
            return le;
        }

        public static VerticalLayoutGroup AddVLayout(GameObject go, float spacing, int padding, TextAnchor align, bool expandWidth)
        {
            var v = go.AddComponent<VerticalLayoutGroup>();
            v.spacing = spacing;
            v.padding = new RectOffset(padding, padding, padding, padding);
            v.childAlignment = align;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = expandWidth;
            v.childForceExpandHeight = false;
            return v;
        }

        public static HorizontalLayoutGroup AddHLayout(GameObject go, float spacing, int padding, TextAnchor align)
        {
            var h = go.AddComponent<HorizontalLayoutGroup>();
            h.spacing = spacing;
            h.padding = new RectOffset(padding, padding, padding, padding);
            h.childAlignment = align;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = false;
            return h;
        }

        // ---------- Widgets ----------

        public static Image CreatePanel(Transform parent, string name, Color color, bool rounded = true, bool raycast = true)
        {
            RectTransform rt = CreateRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = raycast;
            if (rounded)
            {
                img.sprite = RoundedSprite;
                img.type = Image.Type.Sliced;
            }
            return img;
        }

        public static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size, Color color,
            TextAlignmentOptions align = TextAlignmentOptions.Left, FontStyles style = FontStyles.Normal)
        {
            RectTransform rt = CreateRect(name, parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.fontStyle = style;
            t.raycastTarget = false;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.overflowMode = TextOverflowModes.Overflow;
            return t;
        }

        public static Button CreateButton(Transform parent, string name, string label, float fontSize, Color bg, Color textColor)
        {
            Image img = CreatePanel(parent, name, bg);
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.ColorTint;
            ColorBlock c = btn.colors;
            c.normalColor = new Color(0.93f, 0.93f, 0.93f, 1f);
            c.highlightedColor = Color.white;
            c.selectedColor = Color.white;
            c.pressedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
            c.disabledColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            c.fadeDuration = 0.08f;
            btn.colors = c;

            TextMeshProUGUI txt = CreateText(img.transform, "Label", label, fontSize, textColor, TextAlignmentOptions.Center, FontStyles.Bold);
            Stretch(txt.rectTransform, 16, 4, 16, 4);
            return btn;
        }

        public static TMP_Text LabelOf(Button b) => b.GetComponentInChildren<TMP_Text>(true);

        public static TMP_InputField CreateInput(Transform parent, string name, float fontSize, Color bg, Color text, Color placeholderColor, string placeholder)
        {
            Image root = CreatePanel(parent, name, bg);
            root.gameObject.SetActive(false); // configure first, then enable

            RectTransform viewport = CreateRect("Text Area", root.transform);
            Stretch(viewport, 24, 8, 24, 8);
            viewport.gameObject.AddComponent<RectMask2D>();

            TextMeshProUGUI txt = CreateText(viewport, "Text", "", fontSize, text, TextAlignmentOptions.MidlineLeft);
            txt.textWrappingMode = TextWrappingModes.NoWrap;
            Stretch(txt.rectTransform);
            TextMeshProUGUI ph = CreateText(viewport, "Placeholder", placeholder, fontSize, placeholderColor, TextAlignmentOptions.MidlineLeft, FontStyles.Italic);
            ph.textWrappingMode = TextWrappingModes.NoWrap;
            Stretch(ph.rectTransform);

            var input = root.gameObject.AddComponent<TMP_InputField>();
            input.textViewport = viewport;
            input.textComponent = txt;
            input.placeholder = ph;
            input.targetGraphic = root;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.characterLimit = 60;
            input.customCaretColor = true;
            input.caretColor = text;
            input.caretWidth = 3;
            input.onFocusSelectAll = false;
            root.gameObject.SetActive(true);
            return input;
        }

        public static Slider CreateSlider(Transform parent, string name, Color track, Color fill, Color handle, float min, float max, float value)
        {
            RectTransform root = CreateRect(name, parent);
            var hit = root.gameObject.AddComponent<Image>();
            hit.color = Color.clear;

            Image bg = CreatePanel(root, "Background", track);
            SetRect(bg.rectTransform, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0, -8), new Vector2(0, 8));

            RectTransform fillArea = CreateRect("Fill Area", root);
            SetRect(fillArea, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0, -8), new Vector2(0, 8));
            Image fillImg = CreatePanel(fillArea, "Fill", fill, true, false);
            SetRect(fillImg.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            RectTransform slideArea = CreateRect("Handle Slide Area", root);
            SetRect(slideArea, Vector2.zero, Vector2.one, new Vector2(18, 0), new Vector2(-18, 0));
            Image handleImg = CreatePanel(slideArea, "Handle", handle);
            handleImg.rectTransform.anchorMin = new Vector2(0, 0.5f);
            handleImg.rectTransform.anchorMax = new Vector2(0, 0.5f);
            handleImg.rectTransform.sizeDelta = new Vector2(36, 36);

            var slider = root.gameObject.AddComponent<Slider>();
            slider.fillRect = fillImg.rectTransform;
            slider.handleRect = handleImg.rectTransform;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            return slider;
        }

        public static Toggle CreateToggle(Transform parent, string name, Color box, Color check, bool isOn)
        {
            Image bg = CreatePanel(parent, name, box);
            Image mark = CreatePanel(bg.transform, "Check", check, true, false);
            SetRect(mark.rectTransform, Vector2.zero, Vector2.one, new Vector2(10, 10), new Vector2(-10, -10));
            var t = bg.gameObject.AddComponent<Toggle>();
            t.targetGraphic = bg;
            t.graphic = mark;
            t.isOn = isOn;
            return t;
        }
    }
}
