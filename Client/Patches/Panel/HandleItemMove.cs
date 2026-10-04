using Diz.LanguageExtensions;
using EFT.InventoryLogic;
using EFT.Trading;
using EFT.UI.DragAndDrop;
using SPT.Reflection.Patching;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Panel;

public class HandleItemMove : ModulePatch {
    protected static Item CloneForBasket(Item item, LocationInGrid location) {
        Item clone = item.CloneItemWithSameId();
        clone.OriginalAddress = item.CurrentAddress;

        Current!.Items.Add(clone, item);

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

        Item clonedItem = CloneForBasket(itemContext.Item, locationInGrid);

        Current!.TraderController.AddAndRaiseEvents(clonedItem, Current.BarterTableGrid.CreateItemAddress(locationInGrid));

        itemContext.CloseDependentWindows();

        ____traderAssortment.PreparedItemsChanged.Invoke();

        __result = Task.CompletedTask;
        return false;
    }
}

public class HandleItemMoveCanAccept : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(TradingTableGridView).GetMethod("CanAccept", BindingFlags.Instance | BindingFlags.Public);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [PatchPrefix]
    private static bool Prefix(TradingTableGridView __instance, ref bool       __result, DragItemContext? itemContext,
                               ref OperationResult  operation,  ref Assortment ____traderAssortment) {
        // only handle item move for our custom grid view
        if (__instance != BarterTradingTableGridView) {
            return true;
        }

        // TODO: add parent id filters here to only allow certain items to be moved into the barter table grid view
        // dictionary<traderId, list<parentId>> to filter items by parent id for each trader
        if (itemContext != null /* && ___traderAssortment.CanPrepareItemToSell(itemContext.Item)*/) {
            LocationInGrid locationInGrid = __instance.CalculateItemLocation(itemContext);
            operation = ItemManipulator.Move(itemContext.Item,
                                             Current!.BarterTableGrid.CreateItemAddress(locationInGrid),
                                             Current.TraderController,
                                             true);

            __result = operation.Succeeded;
        }
        else {
            operation = default;
            __result  = false;
        }

        return false;
    }
}

public class IsBeingBartered : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(Assortment).GetMethod("IsBeingSold", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPostfix]
    private static void Postfix(Assortment __instance, ref bool __result, Item item) {
        // skip if barter table is not active
        if (OpenBarterToggle == null || !OpenBarterToggle.isOn) {
            return;
        }

        // OR with vanilla's result so items on the real sell table still show as being sold
        __result |= item.GetAllItems().Any(subItem => Current!.BarterTableGrid.Items.Any(x => subItem.Id == x.Id && subItem != x));
    }
}