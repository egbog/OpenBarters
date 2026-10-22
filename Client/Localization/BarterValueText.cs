using System;

namespace OpenBarters.Localization;

public enum BarterValueStatus {
    Required,
    Overpaying,
    EvenTrade
}

public static class BarterValueText {
    public static string Format(BarterValueStatus status, string currency, string price, string? culture) {
        bool russian = culture?.StartsWith("ru", StringComparison.OrdinalIgnoreCase) == true;
        return status switch {
            BarterValueStatus.Required   => $"{(russian ? "Требуется" : "Required")}: {currency} {price}",
            BarterValueStatus.Overpaying => $" {(russian ? "Переплата" : "Overpaying")}: +{currency} {price}",
            BarterValueStatus.EvenTrade  => $" {(russian ? "Равный обмен" : "Even trade")}: {currency} {price}",
            _ => throw new ArgumentOutOfRangeException(nameof(status))
        };
    }
}
