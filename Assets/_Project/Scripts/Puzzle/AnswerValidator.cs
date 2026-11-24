using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SchoolEscape.Puzzles {
    /// <summary>Pure answer-checking helpers (no Unity dependencies, easy to unit test).</summary>
    public static class AnswerValidator {
        private static readonly Regex DotThousands = new Regex(@"^-?\d{1,3}(\.\d{3})+$", RegexOptions.Compiled);

        /// <summary>Trims, collapses spaces, unifies apostrophes, optionally lower-cases. Keeps é, ë, ï, etc.</summary>
        public static string NormalizeText(string input, bool caseSensitive) {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            var sb = new StringBuilder(input.Length);
            bool lastWasSpace = true;
            foreach (char c in input.Normalize(NormalizationForm.FormC)) {
                char ch = c;
                if (ch == '\u2019' || ch == '\u2018') ch = '\'';
                if (char.IsWhiteSpace(ch)) {
                    if (!lastWasSpace) sb.Append(' ');
                    lastWasSpace = true;
                    continue;
                }
                sb.Append(caseSensitive ? ch : char.ToLowerInvariant(ch));
                lastWasSpace = false;
            }
            return sb.ToString().TrimEnd(' ', '.', '!');
        }

        public static bool MatchesAny(string input, IReadOnlyList<string> accepted, bool caseSensitive) {
            if (accepted == null) return false;
            string given = NormalizeText(input, caseSensitive);
            if (given.Length == 0) return false;

            for (int i = 0; i < accepted.Count; i++) {
                if (string.IsNullOrWhiteSpace(accepted[i])) continue;
                if (string.Equals(given, NormalizeText(accepted[i], caseSensitive), StringComparison.Ordinal)) return true;
            }
            return false;
        }

        /// <summary>
        /// Parses Dutch-style numbers: "12,5", "12.5", "1.250", "1.250,75", "€ 30", "25%".
        /// </summary>
        public static bool TryParseNumber(string input, out double value) {
            value = 0;
            if (string.IsNullOrWhiteSpace(input)) return false;

            string s = input.Trim().Replace("€", "").Replace("%", "").Replace(" ", "").Replace("\u00A0", "");
            if (s.Length == 0) return false;

            bool hasComma = s.IndexOf(',') >= 0;
            bool hasDot = s.IndexOf('.') >= 0;

            if (hasComma && hasDot) {
                // The last separator is the decimal one.
                if (s.LastIndexOf(',') > s.LastIndexOf('.')) s = s.Replace(".", "").Replace(',', '.');
                else s = s.Replace(",", "");
            } else if (hasComma) {
                int first = s.IndexOf(',');
                s = first == s.LastIndexOf(',') ? s.Replace(',', '.') : s.Replace(",", "");
            } else if (hasDot && DotThousands.IsMatch(s)) {
                s = s.Replace(".", "");
            }

            return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        public static bool NumbersMatch(double given, double expected, double tolerance)
            => Math.Abs(given - expected) <= Math.Max(0.0, tolerance) + 1e-9;
    }
}