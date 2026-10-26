using System;
using System.Collections.Generic;

namespace OpenBarters.Localization;

public sealed class LocaleStrings(string required, string overpaying, string evenTrade) {
    public string Required   { get; } = required;
    public string Overpaying { get; } = overpaying;
    public string EvenTrade  { get; } = evenTrade;
}

public static class BarterValueText {
    private static readonly Dictionary<string, LocaleStrings> Locales = new(StringComparer.OrdinalIgnoreCase) {
        ["en"] = new("Required", "Overpaying", "Even trade"),
        ["ru"] = new("Требуется", "Переплата", "Равный обмен")
    };

    public static LocaleStrings GetLocaleStrings(string? culture) {
        if (string.IsNullOrEmpty(culture)) {
            return Locales["en"];
        }

        if (Locales.TryGetValue(culture, out var strings)) {
            return strings;
        }

        int separator = culture.IndexOf('-');
        if (separator >= 0 && Locales.TryGetValue(culture.Substring(0, separator), out strings)) {
            return strings;
        }

        return Locales["en"];
    }
}
