using System.Reflection;
using EFT.InventoryLogic;
using EFT.Trading;
using SPT.Reflection.Patching;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Basket;

public class UnprepareItemPatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(Assortment).GetMethod("UnprepareSellItem", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPrefix]
    private static bool Prefix(Item item) {
        // skip if item is not in our barter table dictionary
        if (Current?.BarterItems.ContainsKey(item) != true) {
            return true;
        }

        Current!.UnprepareBarterItem(item);

        return false;
    }
}