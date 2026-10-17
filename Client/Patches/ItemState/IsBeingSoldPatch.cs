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

    // greys out stash originals that are in the basket (same check vanilla uses for its sell table)
    [PatchPostfix]
    private static void Postfix(ref bool __result, Item item) {
        // skip if barter table is not active
        if (!ToggleActive()) {
            return;
        }

        // OR with vanilla's result so items on the real sell table still show as being sold
        // match: a basket item with the same Id that isn't this exact object (i.e. its clone)
        __result |= item.GetAllItems().Any(subItem => Current!.BarterTableGrid.Items.Any(x => subItem.Id == x.Id && subItem != x));
    }
}