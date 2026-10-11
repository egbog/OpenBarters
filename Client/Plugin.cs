/* LICENSE:
 * MIT
 *
 * AUTHOR:
 * egbog
 * */

using OpenBarters.Patches.Panel;
using BepInEx;
using BepInEx.Logging;
using OpenBarters.Patches.Basket;
using OpenBarters.Patches.ItemState;
using OpenBarters.Patches.Pricing;

namespace OpenBarters;

[BepInPlugin("com.egbog.openbarters", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("EscapeFromTarkov.exe")]
public class Plugin : BaseUnityPlugin {
    public static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("OpenBarters");

    private void Awake() {
        // Plugin startup logic
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        new PanelShowPatch().Enable();
        new PanelSelectionPatch().Enable();
        new PanelClosePatch().Enable();
        new AcceptItemPatch().Enable();
        new CanAcceptPatch().Enable();
        new IsBeingSoldPatch().Enable();
        new UnprepareItemPatch().Enable();
        new BasketPricePatch().Enable();
        new CanBuyRequisitePatch().Enable();
        new RefreshSchemePatch().Enable();
        new HideValidDealWarningPatch().Enable();
    }
}