using System.Reflection;
using EFT.Trading;
using EFT.UI;
using SPT.Reflection.Patching;

namespace OpenBarters.Patches.Panel;

public class CloseMultiSelectWindow : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(BarterSchemePanel).GetMethod("Close", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPostfix]
    private static void Postfix(Assortment ____traderAssortment) {
        TradingTable    tradingTable     = ShowMultiSelectWindow.BarterTradingTable;
        UpdatableToggle openBarterToggle = ShowMultiSelectWindow.OpenBarterToggle;

        if (tradingTable != null) {
            tradingTable.gameObject.SetActive(false);
        }

        if (openBarterToggle != null) {
            openBarterToggle.gameObject.SetActive(false);
        }

		____traderAssortment?.PreparedItemsChanged.Invoke();
	}
}