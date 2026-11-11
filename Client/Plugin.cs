/* LICENSE:
 * MIT
 *
 * AUTHOR:
 * egbog
 * */

using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
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
    public static readonly ManualLogSource    Log = BepInEx.Logging.Logger.CreateLogSource("OpenBarters");
    public static          ConfigEntry<float> SubstitutePriceMultiplier { get; private set; } = null!;
    public static          ConfigEntry<float> MinimumValueFactor        { get; private set; } = null!;

    private void Awake() {
        SubstitutePriceMultiplier = Config.Bind("Pricing",
                                                "Substitute price multiplier",
                                                1.0f,
                                                new
                                                    ConfigDescription("Multiplier for the missing part of a barter paid with substitute items. " +
                                                                      "Required items placed on the requirement slots count at handbook value, up to the requested quantity. " +
                                                                      "Surplus and non-matching items count at the trader's sell-tab price. " +
                                                                      "1 = no additional markup; 1.5 = 50% markup on the substituted part.",
                                                                      new AcceptableValueRange<float>(1f, 5f)));
        SubstitutePriceMultiplier.SettingChanged += (_, _) => {
            if (OpenBarterController.ToggleActive()) {
                OpenBarterController.TraderAssortment?.PreparedSumChanged.Invoke();
            }
        };

        // minimum required value as a fraction of the reward’s handbook value
        MinimumValueFactor = Config.Bind("Pricing",
                                         "Minimum barter value",
                                         0.50f,
                                         new ConfigDescription("Minimum barter value as a percentage of the reward’s handbook value. " +
                                                               "The substitute price multiplier is applied after the minimum. " +
                                                               "This prevents barters from being dirt cheap, even with the global multiplier, due to low valued requirements. " +
                                                               "0 = no minimum value; 0.5 = 50% of handbook value of offered item.",
                                                               new AcceptableValueRange<float>(0f, 1f)));

        MinimumValueFactor.SettingChanged += (_, _) => {
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
        new RequisitePrepareItemPatch().Enable();
        new RequisitePriceLabelPatch().Enable();

        if (Chainloader.PluginInfos.ContainsKey("com.tyfon.uifixes")) {
            new UIFixesAutoSwitchPatch().Enable();
        }
    }
}