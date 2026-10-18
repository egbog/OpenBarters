using EFT.InventoryLogic;
using EFT.Trading;
using SPT.Reflection.Patching;
using System.Reflection;
using EFT;
using EFT.UI;
using UIFixesInterop;
using UnityEngine;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Basket;

public class PrepareToSellPatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(Assortment).GetMethod("PrepareToSell", BindingFlags.Instance | BindingFlags.Public);
    }

	// all placements into our grid land here (drops via TradingTableGridView.AcceptItem, UIFixes multi-select)
	[PatchPrefix]
    private static bool Prefix(Item item, LocationInGrid location) {
        // barter mode: anything placed for sale (e.g. UIFixes multi-select) goes into the barter grid instead
        if (ToggleActive()) {
            // open split dialog if being used
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && item.StackObjectsCount > 1 && MultiSelect.Count <= 1) {
                SplitDialog splitDialog = ItemUiContext.Instance.SplitDialog;
                splitDialog.Show("Split".Localized(null).ToUpper(),
                                 item.StackObjectsCount,
                                 Input.mousePosition,
                                 count => {
                                     splitDialog.Hide();
                                     Current!.PrepareBarterItem(item, location, count);
                                 },
                                 splitDialog.Hide);
            }
            else {
                Current!.PrepareBarterItem(item, location, item.StackObjectsCount);
            }

            return false;
        }

        // not in barter mode: item goes to the sell table as normal
        return true;
    }
}