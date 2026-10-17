using System.Reflection;
using EFT.InventoryLogic;
using EFT.Trading;
using EFT.UI;
using SPT.Reflection.Patching;
using UnityEngine;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Pricing;

public class BasketPricePatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(TraderDealScreen).GetMethod("TryGetPurchasePrice", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPrefix]
    private static bool Prefix(ref bool __result, out ECurrencyType currency, out int amount) {
        currency = ECurrencyType.RUB;
        amount   = 0;

        if (OpenBarterToggle && OpenBarterToggle is { isOn: true }) {
            //Trader.ItemPrice itemPrice = Current!.CurrentTrader.GetAssortmentPrice((Stash)Current.TraderController.RootItem)
            //                                     .GetValueOrDefault();
            currency = Current!.CurrentTrader.Settings.Currency;
            
            amount = Current.GetBasketValue().Amount;

            int requiredValue = Current.GetRequiredValue();

            if (BarterValueLabelText != null) {
                BarterValueLabelText.text =
                    $"Required: {CurrencyUtil.GetCurrencyChar(currency)} {TraderDealScreen.FormatPrice(requiredValue)}";
            }

            if (BackgroundRibbon != null) {
                BackgroundRibbon.color = amount >= requiredValue ? new Color(0.55f, 1.0f, 0.21f, 0.63f) : new Color(0.8f, 0.0f, 0.0f, 0.6f);
            }

            if (BackgroundGlow != null) {
                if (GreenGlowSprite != null) {
                    BackgroundGlow.sprite = amount >= requiredValue ? GreenGlowSprite : RedGlowSprite;
                }
                // if green glow is null, just disable the glow for green and reenable for red
                else {
                    BackgroundGlow.enabled = amount < requiredValue;
                }
            }

            __result = true;

            return false;
        }

        return true;
    }
}