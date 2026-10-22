using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using EFT.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Compat;

public class UIFixesAutoSwitchPatch : ModulePatch {
    private static bool _wasInteractable;
    private static bool _disabledThisClick; // our prefix turned the sell tab off for the current click

    protected override MethodBase GetTargetMethod() {
        return typeof(TradingItemView).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.Public);
    }

    [HarmonyPriority(Priority.First)]
    [PatchPrefix]
    private static bool Prefix(TradingItemView __instance, PointerEventData.InputButton button, ETradingItemViewType ____itemViewType,
                               bool            ____canSelect) {
        if (button != PointerEventData.InputButton.Left || !ToggleActive() || ____itemViewType == ETradingItemViewType.TradingTable ||
            ____canSelect                               || !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) {
            return true;
        }

        if (SellTab == null) {
            return true;
        }

        // block UIFixes' auto-switch for this click only
        _wasInteractable = SellTab.Interactable;
        SellTab.SetInteractable(false);
        _disabledThisClick = true;
        return true;
    }

    [PatchPostfix]
    private static void Postfix() {
        // restore only what our prefix changed
        if (!_disabledThisClick) {
            return;
        }

        _disabledThisClick = false;

        if (SellTab != null) {
            // restore state after this method & prefixes finish
            SellTab.SetInteractable(_wasInteractable);
        }
    }
}