using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Diz.LanguageExtensions;
using EFT.InventoryLogic;
using EFT.Trading;
using EFT.UI.DragAndDrop;
using SPT.Reflection.Patching;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Basket;

public class CanAcceptPatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(TradingTableGridView).GetMethod("CanAccept", BindingFlags.Instance | BindingFlags.Public);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [PatchPrefix]
    private static bool Prefix(TradingTableGridView __instance, ref bool   __result, DragItemContext? itemContext,
                               ref OperationResult  operation,  Assortment ____traderAssortment) {
        // only handle item move for our custom grid view
        if (__instance != BarterTradingTableGridView) {
            return true;
        }

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