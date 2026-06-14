/* LICENSE:
 * MIT
 *
 * AUTHOR:
 * egbog
 * */

using OpenBarters.Patches;
using BepInEx;
using BepInEx.Logging;

namespace OpenBarters;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInProcess("EscapeFromTarkov.exe")]
public class Plugin : BaseUnityPlugin {
    public static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("OpenBarters");

    private void Awake() {
        // Plugin startup logic
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        new ShowMultiSelectWindow().Enable();
        new CloseMultiSelectWindow ().Enable();
        new HandleItemMove().Enable();
        new HandleItemMoveCanAccept().Enable();
    }
}