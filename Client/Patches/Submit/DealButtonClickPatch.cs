using EFT.UI;
using SPT.Reflection.Patching;
using System.Reflection;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Submit;

public class DealButtonClickPatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(TraderDealScreen).GetMethod("OnDealButtonClickAsync", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPrefix]
    private static bool Prefix(ref Task __result, TraderDealScreen __instance) {
        if (ToggleActive() && __instance.TradeMode == ETradeMode.Purchase) {
            
            
            __result = Current.PurchaseBarter();
            return false;
        }
        return true;
    }
}