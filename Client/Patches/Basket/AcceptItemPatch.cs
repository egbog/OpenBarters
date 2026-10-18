using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EFT.InventoryLogic;
using EFT.Trading;
using EFT.UI.DragAndDrop;
using SPT.Reflection.Patching;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Basket;

public class AcceptItemPatch : ModulePatch {
    // copy goes in the basket. original stays in stash until purchase is sent
    
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

        Current!.PrepareBarterItem(itemContext.Item, locationInGrid);

        // eg. an open inspect window for the dragged item
        itemContext.CloseDependentWindows();

        __result = Task.CompletedTask;
        return false;
    }
}