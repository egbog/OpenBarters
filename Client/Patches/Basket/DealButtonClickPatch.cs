using EFT.UI;
using SPT.Reflection.Patching;
using System.Reflection;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Basket;

public class DealButtonClickPatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(TraderDealScreen).GetMethod("OnDealButtonClickAsync", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPrefix]
    private static bool Prefix(ref bool __result) {
        return false;
    }
}