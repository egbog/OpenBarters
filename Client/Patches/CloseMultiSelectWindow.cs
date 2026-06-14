using System.Reflection;
using EFT.UI;
using SPT.Reflection.Patching;

namespace OpenBarters.Patches;

public class CloseMultiSelectWindow : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(BarterSchemePanel).GetMethod("Close", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPostfix]
    private static void Postfix() {
        TradingTable    tradingTable     = ShowMultiSelectWindow.BarterTradingTable;
        UpdatableToggle openBarterToggle = ShowMultiSelectWindow.OpenBarterToggle;

        if (tradingTable != null) {
            tradingTable.gameObject.SetActive(false);
        }

        if (openBarterToggle != null) {
            openBarterToggle.gameObject.SetActive(false);
        }
    }
}