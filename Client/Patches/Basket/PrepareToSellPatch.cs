using EFT.InventoryLogic;
using EFT.Trading;
using SPT.Reflection.Patching;
using System.Reflection;
using EFT;
using EFT.UI;
using OpenBarters.Localization;
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
    private static bool Prefix(Assortment __instance, Item item, LocationInGrid location) {
        // barter mode: exact items fill their slot first, everything else goes into the barter grid
        if (ToggleActive()) {
            Requisite? requisite = FindOpenRequisite(__instance, item);

            // send exact req to the requirement slot instead of the barter grid
            if (requisite != null) {
                requisite.PrepareItem(item);
                return false;
            }

            // open split dialog if being used
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && item.StackObjectsCount > 1 &&
                MultiSelect.Count                                                                                   <= 1) {
                string splitText = BarterValueText.GetLocaleStrings(LocalizationManager.Instance?.Culture).Split;

                SplitDialog splitDialog = ItemUiContext.Instance.SplitDialog;
                splitDialog.Show(splitText.ToUpper(),
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