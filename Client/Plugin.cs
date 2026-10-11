/* LICENSE:
 * MIT
 *
 * AUTHOR:
 * egbog
 * */

using OpenBarters.Patches.Panel;
using BepInEx;
using BepInEx.Logging;

namespace OpenBarters;

[BepInPlugin("com.egbog.openbarters", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("EscapeFromTarkov.exe")]
public class Plugin : BaseUnityPlugin {
    public static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("OpenBarters");

    private void Awake() {
        // Plugin startup logic
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        new BuildPanel().Enable();
        new ShowMultiSelectWindow().Enable();
        new CloseMultiSelectWindow().Enable();
        new HandleItemMove().Enable();
        new HandleItemMoveCanAccept().Enable();
        new IsBeingBartered().Enable();
        new UnprepareBarterItem().Enable();
        new GetBarterSum().Enable();
        new CanBuyBarterRequisite().Enable();
        new RefreshSchemeOnPreparedItemsChanged().Enable();
        new HideValidDealWarning().Enable();
    }
}