using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using EFT;
using OpenBarters.Localization;
using SPT.Reflection.Patching;
using UnityEngine;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Pricing;

public class BasketPricePatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(TraderDealScreen).GetMethod("TryGetPurchasePrice", BindingFlags.Instance | BindingFlags.Public);
    }

    // the price over the deal button comes from here; in barter mode show the basket value instead, and update the ribbon
    [PatchPrefix]
    private static bool Prefix(ref bool __result, out ECurrencyType currency, out int amount) {
        // out params must be assigned on every path
        currency = ECurrencyType.RUB;
        amount   = 0;

        if (ToggleActive()) {
            // values are in the trader's currency
            currency = Current!.CurrentTrader.Settings.Currency;
            amount   = Current.GetBasketValue().Amount;

            int requiredValue = Current.GetRequiredValue();

            if (BarterValueLabelText != null) {
                string currencyChar = CurrencyUtil.GetCurrencyChar(currency);
                var strings = BarterValueText.GetLocaleStrings(LocalizationManager.Instance?.Culture);
                BarterValueLabelText.text =
                    $"{strings.Required}: {currencyChar} {TraderDealScreen.FormatPrice(requiredValue - amount)}";
                if (amount > requiredValue) {
                    BarterValueLabelText.text =
                        $" {strings.Overpaying}: +{currencyChar} {TraderDealScreen.FormatPrice(amount - requiredValue)}";
                }
                else if (amount == requiredValue) {
                    BarterValueLabelText.text = $" {strings.EvenTrade}: {currencyChar} {TraderDealScreen.FormatPrice(0)}";
                }
            }

            // green when the basket meets the required value, red otherwise
            if (BackgroundRibbon != null) {
                BackgroundRibbon.color = amount >= requiredValue ? new Color(0.55f, 1.0f, 0.21f, 0.63f) : new Color(0.8f, 0.0f, 0.0f, 0.6f);
            }

            if (BackgroundGlow != null) {
                // glow colour is in the sprite, so swap sprites instead of tinting
                if (GreenGlowSprite != null) {
                    BackgroundGlow.sprite = amount >= requiredValue ? GreenGlowSprite : RedGlowSprite;
                }
                // if green glow is null, just disable the glow for green and reenable for red
                else {
                    BackgroundGlow.enabled = amount < requiredValue;
                }
            }

            // the return value decides whether the price text is shown
            __result = true;

            return false;
        }

        return true;
    }
}
