namespace OpenBarters.Localization;

public sealed class LocaleStrings(string required, string overpaying, string evenTrade, string split, string enable) {
    public string Required   { get; } = required;
    public string Overpaying { get; } = overpaying;
    public string EvenTrade  { get; } = evenTrade;
    public string Split      { get; } = split;
    public string Enable     { get; } = enable;
}

public static class BarterValueText {
    private static readonly Dictionary<string, LocaleStrings> Locales = new(StringComparer.OrdinalIgnoreCase) {
        ["en"] = new LocaleStrings("Required",  "Overpaying", "Even trade",   "Split", "Toggle open bartering system"),
        ["ru"] = new LocaleStrings("Требуется", "Переплата",  "Равный обмен", "Разделить", "Включить"),
        ["ch"] = new LocaleStrings("需付", "多付", "等价交换", "拆分", "切换开启以物易物系统")
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
