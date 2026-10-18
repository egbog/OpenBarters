using EFT.UI;
using SPT.Reflection.Patching;
using System.Reflection;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Panel;

public class SkipAddListenerOnClonePatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(TradingTable).GetMethod("Awake", BindingFlags.Instance | BindingFlags.Public);
    }

    // the clone shares the real clear button; its Awake would add a listener that throws (clone has no assortment)
    [PatchPrefix]
    private static bool Prefix(TradingTable __instance) {
        return __instance != BarterTradingTable; // skip Awake only for our clone
    }
}