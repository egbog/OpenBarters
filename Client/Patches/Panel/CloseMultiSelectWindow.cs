using EFT.Trading;
using EFT.UI;
using OpenBarters.Controllers;
using SPT.Reflection.Patching;
using System.Reflection;

namespace OpenBarters.Patches.Panel;

public class CloseMultiSelectWindow : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(BarterSchemePanel).GetMethod("Close", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPostfix]
    private static void Postfix(Assortment? ____traderAssortment) {
        TradingTable?    tradingTable     = OpenBarterController.BarterTradingTable;
        UpdatableToggle? openBarterToggle = OpenBarterController.OpenBarterToggle;

        if (tradingTable != null) {
            tradingTable.gameObject.SetActive(false);
        }

        if (OpenBarterController.BarterTradingTableGridView != null) {
            OpenBarterController.BarterTradingTableGridView.Close();
        }

        if (openBarterToggle != null) {
            openBarterToggle.gameObject.SetActive(false);
        }

        // not a Unity object, safe to use conditional access operator
        ____traderAssortment?.PreparedItemsChanged.Invoke();
    }
}