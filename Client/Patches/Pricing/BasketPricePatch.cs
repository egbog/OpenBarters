using System.Reflection;
using EFT.InventoryLogic;
using EFT.Trading;
using EFT.UI;
using SPT.Reflection.Patching;
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
            Trader.ItemPrice itemPrice = Current!.CurrentTrader.GetAssortmentPrice((Stash)Current.TraderController.RootItem)
                                                 .GetValueOrDefault();
            currency = Current!.CurrentTrader.Settings.Currency;

            // last basket total shown on the DEAL! button; only refreshed when the price display redraws, so may be stale
            BarterSum = amount = itemPrice.Amount;

            __result = true;

            return false;
        }

        return true;
    }
}