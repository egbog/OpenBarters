using System.Reflection;
using EFT.UI.DragAndDrop;
using SPT.Reflection.Patching;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.ItemState;

public class RefreshSchemePatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(TradingItemView).GetMethod("CG_NewTradingItemView", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPostfix]
    private static void Postfix(TradingItemView __instance, bool ___IsKilled) {
        if (OpenBarterToggle == null || BarterTradingTable == null || Current == null || ___IsKilled) {
            return;
        }

        if (!OpenBarterToggle.gameObject.activeInHierarchy) {
            return;
        }

        __instance.UpdateScheme();
    }
}