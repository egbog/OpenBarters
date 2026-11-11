using System.Reflection;
using EFT.Communications;
using EFT.InventoryLogic;
using EFT.Trading;
using EFT.UI;
using SPT.Reflection.Patching;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Basket;

public class ControlClickPatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(Assortment).GetMethod("QuickFindTradingAppropriatePlace", BindingFlags.Instance | BindingFlags.Public);
    }

    // ctrl+click a stash item in barter mode: fill a matching requirement first, otherwise put it in the basket
    [PatchPrefix]
    private static bool Prefix(ref bool __result, Item item, Assortment __instance) {
        if (!ToggleActive()) {
            // sell tab or barter grid off. vanilla
            return true;
        }

        Requisite? requisite = FindOpenRequisite(__instance, item);

        // insert the item into the requirement UI element instead of the barter grid
        if (requisite != null) {
            requisite.PrepareItem(item);
            __result = true;
            return false;
        }

        // not accepted or already in the grid
        if (!__instance.CanPrepareItemToSell(item)) {
            __result = false;
            return false;
        }

        GridItemAddress? gridItemAddress = Current!.BarterTableGrid.FindLocationForItem(item);

        if (gridItemAddress != null) {
            Current.PrepareBarterItem(item, gridItemAddress.LocationInGrid, item.StackObjectsCount);

            __result = true;
            return false;
        }

        // grid full
        var noFreeSpaceError = new NoFreeSpaceError(item);
        NotificationManager.DisplayWarningNotification(noFreeSpaceError.GetLocalizedDescription(), ENotificationDurationType.Default);

        __result = false;
        return false;
    }
}