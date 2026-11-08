using System.Reflection;
using EFT.Trading;
using SPT.Reflection.Patching;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.ItemState;

// stash views only refresh on PreparedItemsChanged; requisite prepares don't raise it
public class RequisitePrepareItemPatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(Requisite).GetMethod("PrepareItem", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPostfix]
    private static void Postfix() {
        if (ToggleActive()) {
            TraderAssortment?.PreparedItemsChanged.Invoke();
            TraderAssortment?.PreparedSumChanged.Invoke(); // redraw price + ribbon
        }
    }
}