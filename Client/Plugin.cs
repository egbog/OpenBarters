/* LICENSE:
 * MIT
 *
 * AUTHOR:
 * egbog
 * */

using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using OpenBarters.Patches.Basket;
using OpenBarters.Patches.Compat;
using OpenBarters.Patches.ItemState;
using OpenBarters.Patches.Panel;
using OpenBarters.Patches.Pricing;
using OpenBarters.Patches.Submit;

namespace OpenBarters;

[BepInPlugin("com.egbog.openbarters", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.tyfon.uifixes", BepInDependency.DependencyFlags.SoftDependency)]
[BepInProcess("EscapeFromTarkov.exe")]
public class Plugin : BaseUnityPlugin {
    public static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("OpenBarters");

    private void Awake() {
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
        new DealButtonTogglePatch().Enable();
        new DealButtonClickPatch().Enable();
        new ControlClickPatch().Enable();

        if (Chainloader.PluginInfos.ContainsKey("com.tyfon.uifixes")) {
            new UIFixesAutoSwitchPatch().Enable();
        }
    }
}