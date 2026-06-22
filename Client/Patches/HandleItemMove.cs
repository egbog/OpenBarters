using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using SPT.Reflection.Patching;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace OpenBarters.Patches;

public class HandleItemMove : ModulePatch {
    protected static Item CloneForBasket(Item item, LocationInGrid location) {
        Item clone = item.CloneItemWithSameId();
        clone.OriginalAddress = item.CurrentAddress;

        ShowMultiSelectWindow.OpenBarter.Items.Add(clone, item);

        return clone;
    }

    protected override MethodBase GetTargetMethod() {
        return typeof(TradingTableGridView).GetMethod("AcceptItem", BindingFlags.Instance | BindingFlags.Public);
    }

    // create and render custom grid view
    [PatchPrefix]
    private static bool Prefix(TradingTableGridView __instance, ref Task __result, ItemContextClass itemContext) {
        // only handle item move for our custom grid view
        if (__instance != ShowMultiSelectWindow.BarterTradingTableGridView) {
            return true;
        }

        LocationInGrid locationInGrid = __instance.CalculateItemLocation(itemContext);
        itemContext.DragCancelled();

        Item clonedItem = CloneForBasket(itemContext.Item, locationInGrid);

        ShowMultiSelectWindow.OpenBarter.TraderController.AddAndRaiseEvents(clonedItem,
            ShowMultiSelectWindow.OpenBarter.BarterTableGrid.CreateItemAddress(locationInGrid));

        itemContext.CloseDependentWindows();

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
    private static bool Prefix(TradingTableGridView __instance, ref bool __result, ItemContextClass itemContext,
                               ref GStruct153       operation, ref TraderAssortmentControllerClass ___traderAssortmentControllerClass) {
        // only handle item move for our custom grid view
        if (__instance != ShowMultiSelectWindow.BarterTradingTableGridView) {
            return true;
        }

        if (itemContext != null/* && ___traderAssortmentControllerClass.CanPrepareItemToSell(itemContext.Item)*/) {
            LocationInGrid locationInGrid = __instance.CalculateItemLocation(itemContext);
            operation = InteractionsHandlerClass.Move(itemContext.Item,
                                                      ShowMultiSelectWindow.OpenBarter.BarterTableGrid
                                                                           .CreateItemAddress(locationInGrid),
                                                      ShowMultiSelectWindow.OpenBarter.TraderController,
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