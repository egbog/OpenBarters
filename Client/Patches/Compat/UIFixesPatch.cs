using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using EFT.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Compat;

public class UIFixesPatch : ModulePatch {
    private static bool _wasInteractable;
    private static Tab? _sellTab; // set only while a barter ctrl+click is in progress

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

        TraderDealScreen screen = __instance.GetComponentInParent<TraderDealScreen>();
        if (screen == null) {
            return true;
        }

		// block UIFixes' auto-switch for this click only
		_sellTab = screen._sellTab;
        _wasInteractable = _sellTab.Interactable;
        _sellTab.SetInteractable(false);
        return true;
    }

    [PatchPostfix]
    private static void Postfix() {
        if (_sellTab == null) {
            return;
        }

        // restore state after this method & prefixes finish
        _sellTab.SetInteractable(_wasInteractable);
        _sellTab = null;
    }
}