using System;
using System.Collections.Generic;

namespace OpenBarters.Localization;

public sealed class LocaleStrings {
    public string Required { get; }
    public string Overpaying { get; }
    public string EvenTrade { get; }

    public LocaleStrings(string required, string overpaying, string evenTrade) {
        Required = required;
        Overpaying = overpaying;
        EvenTrade = evenTrade;
    }
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
