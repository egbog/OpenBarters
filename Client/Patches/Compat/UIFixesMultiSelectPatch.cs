using EFT.InventoryLogic;
using EFT.Trading;
using SPT.Reflection.Patching;
using System.Reflection;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Compat;

public class UIFixesMultiSelectPatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(Assortment).GetMethod("PrepareToSell", BindingFlags.Instance | BindingFlags.Public);
    }

    // redirect UIFixes call on PrepareToSell when using multi-select
    [PatchPrefix]
    private static bool Prefix(Item item, LocationInGrid location) {
        // barter mode: anything placed for sale (eg. UIFixes multi-select) goes into the barter grid instead
        if (ToggleActive()) {
            Current!.PrepareBarterItem(item, location, item.StackObjectsCount);
            return false;
        }

        // not in barter mode: item goes to the sell table as normal
        return true;
    }
}