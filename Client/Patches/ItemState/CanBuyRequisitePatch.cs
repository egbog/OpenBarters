using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using OpenBarters.Controllers;
using SPT.Reflection.Patching;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.ItemState;

public class CanBuyRequisitePatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(Profile.TraderInfo).GetMethod("CanBuyItem",
                                                    BindingFlags.Instance | BindingFlags.Public,
                                                    null,
                                                    new[] { typeof(ItemTemplate) },
                                                    null);
    }

	// the trader normally won't buy many barter requirements; count the selected barter's required items as buyable
    // so they can be dropped, priced (GetUserItemPrice), un-greyed and highlighted
	[PatchPostfix]
    private static void Postfix(ref bool __result, Profile.TraderInfo __instance, ItemTemplate itemTemplate) {
        if (OpenBarterToggle == null || BarterTradingTable == null || Current == null) {
            return;
        }

		// only while the grid is shown; otherwise the sell tab would offer cash for these items
		if (!ToggleActive() || !BarterTradingTable.gameObject.activeInHierarchy) {
            return;
        }

		// current trader only, and only templates the selected barter asks for
		if (__instance.Id == Current.CurrentTrader.Id &&
            OpenBarterController.TraderAssortment!.CurrentRequisites.Exists(req => req.RequiredItem.TemplateId == itemTemplate._id)) {
            __result = true;
        }
    }
}