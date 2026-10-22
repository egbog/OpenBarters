/* LICENSE:
 * MIT
 *
 * AUTHOR:
 * egbog
 * */

using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using BepInEx.Configuration;
using OpenBarters.Controllers;
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
    public static ConfigEntry<float> SubstitutePriceMultiplier { get; private set; } = null!;

    private void Awake() {
        SubstitutePriceMultiplier = Config.Bind("Pricing", "Substitute price multiplier", 1.5f,
            new ConfigDescription("Multiplier for the missing part of a barter paid with substitute items. " +
                                  "Matching required items count at handbook value, up to the requested quantity. " +
                                  "Surplus and non-matching items count at the trader's sell-tab price. " +
                                  "1 = no additional markup; 1.5 = 50% markup on the substituted part.",
                                  new AcceptableValueRange<float>(1f, 10f)));
        SubstitutePriceMultiplier.SettingChanged += (_, _) => {
            if (OpenBarterController.ToggleActive()) {
                OpenBarterController.TraderAssortment?.PreparedSumChanged.Invoke();
            }
        };

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        new PanelShowPatch().Enable();
        new PanelSelectionPatch().Enable();
        new PanelClosePatch().Enable();
        new IsBeingSoldPatch().Enable();
        new UnprepareItemPatch().Enable();
        new BasketPricePatch().Enable();
        new CanBuyRequisitePatch().Enable();
        new RefreshSchemePatch().Enable();
        new HideValidDealWarningPatch().Enable();
        new DealButtonTogglePatch().Enable();
        new DealButtonClickPatch().Enable();
        new ControlClickPatch().Enable();
        new SkipAddListenerOnClonePatch().Enable();
        new PrepareToSellPatch().Enable();

        if (Chainloader.PluginInfos.ContainsKey("com.tyfon.uifixes")) {
            new UIFixesAutoSwitchPatch().Enable();
        }
    }
}
