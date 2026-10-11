using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EFT.InventoryLogic;
using EFT.Trading;
using EFT.UI.DragAndDrop;
using SPT.Reflection.Patching;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Basket;

public class AcceptItemPatch : ModulePatch {
    protected static Item CloneForBasket(Item item) {
        Item clone = item.CloneItemWithSameId();
        clone.OriginalAddress = item.CurrentAddress;

        Current!.BarterItems.Add(clone, item);

        return clone;
    }

    protected override MethodBase GetTargetMethod() {
        return typeof(TradingTableGridView).GetMethod("AcceptItem", BindingFlags.Instance | BindingFlags.Public);
    }

    // create and render custom grid view
    [PatchPrefix]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static bool Prefix(TradingTableGridView __instance, ref Task __result, DragItemContext itemContext,
                               Assortment           ____traderAssortment) {
        // only handle item move for our custom grid view
        if (__instance != BarterTradingTableGridView) {
            return true;
        }

        LocationInGrid locationInGrid = __instance.CalculateItemLocation(itemContext);
        itemContext.DragCancelled();

        Item clonedItem = CloneForBasket(itemContext.Item);

        Current!.TraderController.AddAndRaiseEvents(clonedItem, Current.BarterTableGrid.CreateItemAddress(locationInGrid));

        itemContext.CloseDependentWindows();

        ____traderAssortment.PreparedItemsChanged.Invoke();
        ____traderAssortment.PreparedSumChanged.Invoke();

        __result = Task.CompletedTask;
        return false;
    }
}