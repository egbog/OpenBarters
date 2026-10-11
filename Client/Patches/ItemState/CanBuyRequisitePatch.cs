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

    [PatchPostfix]
    private static void Postfix(ref bool __result, Profile.TraderInfo __instance, ItemTemplate itemTemplate) {
        if (OpenBarterToggle == null || BarterTradingTable == null || Current == null) {
            return;
        }

        if (!(OpenBarterToggle.gameObject.activeInHierarchy && OpenBarterToggle.isOn) || !BarterTradingTable.gameObject.activeInHierarchy) {
            return;
        }

        if (__instance.Id == Current.CurrentTrader.Id &&
            OpenBarterController.TraderAssortment!.CurrentRequisites.Exists(req => req.RequiredItem.TemplateId == itemTemplate._id)) {
            __result = true;
        }
    }
}