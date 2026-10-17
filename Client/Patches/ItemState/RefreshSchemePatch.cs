using System.Reflection;
using EFT.UI.DragAndDrop;
using SPT.Reflection.Patching;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.ItemState;

public class RefreshSchemePatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(TradingItemView).GetMethod("CG_NewTradingItemView", BindingFlags.Instance | BindingFlags.Public);
    }

    // runs on PreparedItemsChanged for every trading item view; re-evaluate price/greying so required items
    // un-grey (and re-grey) as the offer or toggle changes
    [PatchPostfix]
    private static void Postfix(TradingItemView __instance, bool ___IsKilled) {
        // ___IsKilled: view returned to the pool
        if (OpenBarterToggle == null || BarterTradingTable == null || Current == null || ___IsKilled) {
            return;
        }

        // only in the buy panel with a barter selected; toggle can be off (toggling off must re-grey)
        if (!OpenBarterToggle.gameObject.activeInHierarchy) {
            return;
        }

        __instance.UpdateScheme();
    }
}