using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using SPT.Reflection.Patching;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EFT;
using UnityEngine;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Basket;

public class AcceptItemPatch : ModulePatch {
    // drop onto our grid. put a clone in the basket

    protected override MethodBase GetTargetMethod() {
        return typeof(TradingTableGridView).GetMethod("AcceptItem", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPrefix]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static bool Prefix(TradingTableGridView __instance, ref Task __result, DragItemContext itemContext) {
        // only handle item move for our custom grid view
        if (__instance != BarterTradingTableGridView) {
            return true;
        }

        // cell under the cursor
        LocationInGrid locationInGrid = __instance.CalculateItemLocation(itemContext);
        // the dragged original snaps back to stash
        itemContext.DragCancelled();

        Item item = itemContext.Item;

        // eg. an open inspect window for the dragged item
        itemContext.CloseDependentWindows();

        // open split dialog if being used
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && item.StackObjectsCount > 1) {
            SplitDialog splitDialog = ItemUiContext.Instance.SplitDialog;
            splitDialog.Show("Split".Localized(null).ToUpper(),
                             item.StackObjectsCount,
                             Input.mousePosition,
                             count => {
                                 splitDialog.Hide();
                                 Current!.PrepareBarterItem(item, locationInGrid, count);
                             },
                             splitDialog.Hide);
        }
        else {
            Current!.PrepareBarterItem(item, locationInGrid, item.StackObjectsCount);
        }

        __result = Task.CompletedTask;
        return false;
    }
}