using EFT.UI;
using SPT.Reflection.Patching;
using System.Reflection;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Submit;

public class DealButtonClickPatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(TraderDealScreen).GetMethod("OnDealButtonClickAsync", BindingFlags.Instance | BindingFlags.Public);
    }

    // in barter mode, the deal button buys with the basket instead of vanilla's requisites (vanilla would also block on IsValidDeal)
    [PatchPrefix]
    private static bool Prefix(ref Task __result, TraderDealScreen __instance) {
        if (ToggleActive() && __instance.TradeMode == ETradeMode.Purchase) {
            // must be a Task: the caller runs HandleExceptions() on it
            __result = Current!.PurchaseBarter();
            return false;
        }

        return true;
    }
}