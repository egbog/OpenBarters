using EFT.Trading;
using EFT.UI;
using SPT.Reflection.Patching;
using System.Reflection;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Panel;

public class CloseMultiSelectWindow : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(BarterSchemePanel).GetMethod("Close", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPostfix]
    private static void Postfix(Assortment? ____traderAssortment) {
        TradingTable?    tradingTable     = BarterTradingTable;
        UpdatableToggle? openBarterToggle = OpenBarterToggle;

        if (tradingTable != null) {
            tradingTable.gameObject.SetActive(false);
        }

        if (BarterTradingTableGridView != null) {
            BarterTradingTableGridView.Close();
        }

        if (openBarterToggle != null) {
            openBarterToggle.gameObject.SetActive(false);
        }

        // not a Unity object, safe to use conditional access operator
        //____traderAssortment?.PreparedItemsChanged.Invoke(); // already called in ClearBarterItems()

        Current?.ClearBarterItems();
    }
}