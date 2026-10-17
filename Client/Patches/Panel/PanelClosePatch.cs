using EFT.UI;
using SPT.Reflection.Patching;
using System.Reflection;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Panel;

public class PanelClosePatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(BarterSchemePanel).GetMethod("Close", BindingFlags.Instance | BindingFlags.Public);
    }

    // leaving buy panel (sell tab, closing screen or switching trader). hide ui and empty basket
    [PatchPostfix]
    private static void Postfix() {
        UpdatableToggle? openBarterToggle = OpenBarterToggle;

        if (BarterGridSlot != null) {
            BarterGridSlot.gameObject.SetActive(false);
        }

        // pairs with the Show in PanelShowPatch, so view registrations don't pile up
        if (BarterTradingTableGridView != null) {
            BarterTradingTableGridView.Close();
        }

        if (openBarterToggle != null) {
            openBarterToggle.gameObject.SetActive(false);
        }

        Current?.ClearBarterItems();
    }
}