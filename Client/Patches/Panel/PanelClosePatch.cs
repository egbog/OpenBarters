using EFT.UI;
using SPT.Reflection.Patching;
using System.Reflection;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Panel;

public class PanelClosePatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(BarterSchemePanel).GetMethod("Close", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPostfix]
    private static void Postfix() {
        UpdatableToggle? openBarterToggle = OpenBarterToggle;

        if (BarterGridSlot != null) {
            BarterGridSlot.gameObject.SetActive(false);
        }

        if (BarterTradingTableGridView != null) {
            BarterTradingTableGridView.Close();
        }

        if (openBarterToggle != null) {
            openBarterToggle.gameObject.SetActive(false);
        }

        Current?.ClearBarterItems();
    }
}