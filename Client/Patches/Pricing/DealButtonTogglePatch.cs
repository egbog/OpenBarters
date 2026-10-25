using EFT.UI;
using SPT.Reflection.Patching;
using System.Reflection;
using EFT.InventoryLogic;
using EFT.Trading;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Pricing;

public class DealButtonTogglePatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(TraderDealScreen).GetMethod("UpdateDealButton", BindingFlags.Instance | BindingFlags.Public);
    }

    // in barter mode, the deal button follows the basket instead of vanilla's "you have the required items" check
    [PatchPostfix]
    private static void Postfix(TraderDealScreen __instance, DefaultUIButton ____dealButton) {
        if (ToggleActive() && __instance.TradeMode == ETradeMode.Purchase) {
            Trader     trader       = Current!.CurrentTrader;
            Assortment assortment   = trader.CurrentAssortment;
            Item       selectedItem = assortment.SelectedItem;

            // vanilla's purchase conditions minus IsValidDeal
            bool canTrade = !trader.AssortmentLoading && !assortment.TransactionInProgress && selectedItem != null &&
                            (assortment.SelectedItemBuyRestrictionMax > selectedItem.BuyRestrictionCurrent ||
                             selectedItem.BuyRestrictionMax           == 0) && !selectedItem.IsEmptyStack;

            // computed fresh: this runs before the price display updates in OnPriceChanged
            ____dealButton.Interactable = canTrade && Current.GetBarterValue().CanTrade;
        }
    }
}
