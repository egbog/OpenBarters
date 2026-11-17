using EFT.Trading;
using EFT.UI;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.ItemState;

public class RequisiteClickRemovePatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(TradingRequisitePanel).GetMethod("OnPointerClick", BindingFlags.Instance | BindingFlags.Public);
    }

    // allows ctrl-click to remove items from the requirement slots
    [PatchPostfix]
    private static void Postfix(PointerEventData eventData, Requisite ____requisite) {
        if (ToggleActive() && eventData.button == PointerEventData.InputButton.Left &&
            (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && ____requisite.PreparedItemsCount > 0) {
            ____requisite.RemoveItem(____requisite.PreparedItems[^1].Item);
            TraderAssortment?.PreparedItemsChanged.Invoke(); // un-grey the removed item
            TraderAssortment?.PreparedSumChanged.Invoke();   // redraw price + ribbon
        }
    }
}