using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI;
using EFT;
using OpenBarters.Controllers;
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
            int basketValue = Current.GetBasketValue().Amount;
            // deal button shows everything paid: basket at trader price + exact requirements at handbook
            amount = basketValue + Current.GetPreparedValue();

            int requiredValue = Current.GetRequiredValue();

            if (BarterValueLabelText != null) {
                string currencyChar = CurrencyUtil.GetCurrencyChar(currency);
                var strings = BarterValueText.GetLocaleStrings(LocalizationManager.Instance?.Culture);
                BarterValueLabelText.text =
                    $"{strings.Required}: {currencyChar} {TraderDealScreen.FormatPrice(requiredValue - basketValue)}";
                if (basketValue > requiredValue) {
                    BarterValueLabelText.text =
                        $"{strings.Overpaying}: +{currencyChar} {TraderDealScreen.FormatPrice(basketValue - requiredValue)}";
                }
                else if (basketValue == requiredValue) {
                    BarterValueLabelText.text = $"{strings.EvenTrade}: {currencyChar} {TraderDealScreen.FormatPrice(0)}";
                }
            }

            // same rule as the deal gate
            bool canTrade = CanTrade(basketValue, requiredValue);
            if (BackgroundRibbon != null) {
                BackgroundRibbon.color = canTrade ? new Color(0.55f, 1.0f, 0.21f, 0.63f) : new Color(0.8f, 0.0f, 0.0f, 0.6f);
            }

            if (BackgroundGlow != null) {
                // glow colour is in the sprite, so swap sprites instead of tinting
                if (GreenGlowSprite != null) {
                    BackgroundGlow.sprite = canTrade ? GreenGlowSprite : RedGlowSprite;
                }
                // if green glow is null, just disable the glow for green and reenable for red
                else {
                    BackgroundGlow.enabled = !canTrade;
                }
            }

            // the return value decides whether the price text is shown
            __result = true;

            return false;
        }

        return true;
    }
}
