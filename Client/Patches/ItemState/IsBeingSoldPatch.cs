using System.Reflection;
using EFT.InventoryLogic;
using EFT.Trading;
using SPT.Reflection.Patching;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.ItemState;

public class IsBeingSoldPatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(Assortment).GetMethod("IsBeingSold", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPostfix]
    private static void Postfix(ref bool __result, Item item) {
        // skip if barter table is not active
        if (OpenBarterToggle == null || !OpenBarterToggle.isOn) {
            return;
        }

        // OR with vanilla's result so items on the real sell table still show as being sold
        __result |= item.GetAllItems().Any(subItem => Current!.BarterTableGrid.Items.Any(x => subItem.Id == x.Id && subItem != x));
    }
}