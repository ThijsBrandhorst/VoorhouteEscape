using UnityEngine;
using SchoolEscape.Puzzles;

namespace SchoolEscape.UI
{
    /// <summary>Color set of a window skin.</summary>
    public readonly struct UiPalette
    {
        public readonly Color Overlay, Window, Header, HeaderText, Panel, Accent, Text, TextDim, Button, ButtonText, Good, Bad, InputBg;

        public UiPalette(string window, string header, string headerText, string panel, string accent, string text,
            string textDim, string button, string buttonText, string good, string bad, string inputBg)
        {
            Overlay = new Color(0f, 0f, 0f, 0.72f);
            Window = Hex(window); Header = Hex(header); HeaderText = Hex(headerText); Panel = Hex(panel);
            Accent = Hex(accent); Text = Hex(text); TextDim = Hex(textDim); Button = Hex(button);
            ButtonText = Hex(buttonText); Good = Hex(good); Bad = Hex(bad); InputBg = Hex(inputBg);
        }

        private static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out Color c) ? c : Color.magenta;
    }

    public static class UiPalettes
    {
        public static UiPalette For(PuzzleSkin skin)
        {
            switch (skin)
            {
                case PuzzleSkin.Blackboard:
                    return new UiPalette("#1F3B30", "#6B4423", "#FFF8E7", "#2C4E40", "#F2E27A", "#F4F1E6",
                        "#BFD1C6", "#8A5A2B", "#FFF8E7", "#8BE28B", "#FF8A80", "#17302A");
                case PuzzleSkin.Paper:
                    return new UiPalette("#F6EFDC", "#B9854A", "#FFFFFF", "#EADFC4", "#C0392B", "#3B2A1A",
                        "#7A6650", "#B9854A", "#FFFFFF", "#2E9E5B", "#C0392B", "#FFFDF5");
                case PuzzleSkin.Keypad:
                    return new UiPalette("#23262B", "#D9822B", "#1A1A1A", "#33373D", "#FFB347", "#F2F2F2",
                        "#A0A5AB", "#D9822B", "#1A1A1A", "#5FD38D", "#FF6B6B", "#15171A");
                default: // Computer
                    return new UiPalette("#101A2E", "#1B4F8F", "#FFFFFF", "#1C2C4A", "#4FD1FF", "#EAF4FF",
                        "#9DB4CF", "#2A7DE1", "#FFFFFF", "#3FCF76", "#F0596B", "#0B1220");
            }
        }

        public static UiPalette Menu => new UiPalette("#1E1B3A", "#5B3FD1", "#FFFFFF", "#2D2956", "#B9A8FF", "#F1EEFF",
            "#B4AED8", "#5B3FD1", "#FFFFFF", "#3FCF76", "#F0596B", "#15132B");
    }
}
