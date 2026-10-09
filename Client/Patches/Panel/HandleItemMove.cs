using Diz.LanguageExtensions;
using EFT.InventoryLogic;
using EFT.Trading;
using EFT.UI.DragAndDrop;
using SPT.Reflection.Patching;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Comfort.Common;
using EFT.HandBook;
using EFT.UI;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Panel;

//TODO: split these patches into separate files

public class HandleItemMove : ModulePatch {
    protected static Item CloneForBasket(Item item, LocationInGrid location) {
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

        Item clonedItem = CloneForBasket(itemContext.Item, locationInGrid);

        Current!.TraderController.AddAndRaiseEvents(clonedItem, Current.BarterTableGrid.CreateItemAddress(locationInGrid));

        itemContext.CloseDependentWindows();

        ____traderAssortment.PreparedItemsChanged.Invoke();
        ____traderAssortment.PreparedSumChanged.Invoke();

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
        if (itemContext != null && ____traderAssortment.CanPrepareItemToSell(itemContext.Item)) {
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

public class UnprepareBarterItem : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(Assortment).GetMethod("UnprepareSellItem", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPrefix]
    private static bool Prefix(Assortment __instance, Item item) {
        // skip if item is not in our barter table dictionary
        if (Current?.BarterItems.ContainsKey(item) != true) {
            return true;
        }

        Current!.UnprepareBarterItem(item);

        return false;
    }
}

public class GetBarterSum : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(TraderDealScreen).GetMethod("TryGetPurchasePrice", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPrefix]
    private static bool Prefix(out ECurrencyType currency, out int amount) {
		currency = ECurrencyType.RUB;
        amount   = 0;

		if (OpenBarterToggle && OpenBarterToggle is { isOn: true }) {
            foreach (Item item in Current!.BarterItems.Keys) {
                double totalPrice = item.GetAllItems()
                                           .Sum(subItem => Singleton<Handbook>.Instance.GetBasePrice(subItem.Template._id) *
                                                           subItem.StackObjectsCount);

                amount += (int)totalPrice;
            }

            BarterSum = amount;

            return false;
        }

        return true;
    }
}