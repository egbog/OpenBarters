using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using SPT.Reflection.Patching;
using UnityEngine;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Pricing;

public class BasketPricePatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(TraderDealScreen).GetMethod("TryGetPurchasePrice", BindingFlags.Instance | BindingFlags.Public);
    }

    // Show substitute value after matching exact requirements, and update the remaining-cost ribbon.
    [PatchPrefix]
    private static bool Prefix(ref bool __result, out ECurrencyType currency, out int amount) {
        // out params must be assigned on every path
        currency = ECurrencyType.RUB;
        amount   = 0;

        if (ToggleActive()) {
            // values are in the trader's currency
            currency = Current!.CurrentTrader.Settings.Currency;
            var value = Current.GetBarterValue();
            amount = value.BasketValue;
            int requiredValue = value.RequiredValue;

            if (BarterValueLabelText != null) {
                BarterValueLabelText.text =
                    $"Required: {CurrencyUtil.GetCurrencyChar(currency)} {TraderDealScreen.FormatPrice(requiredValue - amount)}";
                if (amount > requiredValue) {
                    BarterValueLabelText.text =
                        $" Overpaying: +{CurrencyUtil.GetCurrencyChar(currency)} {TraderDealScreen.FormatPrice(amount - requiredValue)}";
                }
                else if (amount == requiredValue) {
                    BarterValueLabelText.text = $" Even trade: {CurrencyUtil.GetCurrencyChar(currency)} {TraderDealScreen.FormatPrice(0)}";
                }
            }

            // Fully matched requirements cost zero substitutes; unpriced/empty requirements stay invalid.
            if (BackgroundRibbon != null) {
                BackgroundRibbon.color = value.CanTrade ? new Color(0.55f, 1.0f, 0.21f, 0.63f) : new Color(0.8f, 0.0f, 0.0f, 0.6f);
            }

            if (BackgroundGlow != null) {
                // glow colour is in the sprite, so swap sprites instead of tinting
                if (GreenGlowSprite != null) {
                    BackgroundGlow.sprite = value.CanTrade ? GreenGlowSprite : RedGlowSprite;
                }
                // if green glow is null, just disable the glow for green and reenable for red
                else {
                    BackgroundGlow.enabled = !value.CanTrade;
                }
            }

            // the return value decides whether the price text is shown
            __result = true;

            return false;
        }

        return true;
    }
}
