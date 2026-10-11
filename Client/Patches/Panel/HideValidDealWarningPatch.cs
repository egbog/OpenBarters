using System.Reflection;
using EFT.UI;
using SPT.Reflection.Patching;
using UnityEngine;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Panel;

public class HideValidDealWarningPatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(BarterSchemePanel).GetMethod("UpdateValidDealWarning", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPostfix]
    private static void Postfix(GameObject ____validSchemeWarning) {
        if (OpenBarterToggle != null && OpenBarterToggle.isOn) {
            ____validSchemeWarning.SetActive(false);
        }
    }
}