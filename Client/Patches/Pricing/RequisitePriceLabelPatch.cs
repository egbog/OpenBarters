using EFT.InventoryLogic;
using EFT.Trading;
using EFT.UI;
using SPT.Reflection.Patching;
using System.Reflection;
using TMPro;
using UnityEngine;
using static OpenBarters.Controllers.OpenBarterController;
using Object = UnityEngine.Object;

namespace OpenBarters.Patches.Pricing;

public class RequisitePriceLabelPatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(TradingRequisitePanel).GetMethod("UpdateValues", BindingFlags.Instance | BindingFlags.Public);
    }

    // show each requirement slot's prepared handbook value, what the exact items take off the required value
    [PatchPostfix]
    private static void Postfix(TradingRequisitePanel __instance, Requisite ____requisite, TextMeshProUGUI ____count) {
        if (Current == null) {
            return;
        }

        Transform?      existing = __instance.Icon.transform.Find("OpenBarterValue");
        TextMeshProUGUI valueText;

        // create value label if it doesn't exist yet
        if (!existing) {
            valueText       = Object.Instantiate(____count, __instance.Icon.transform);
            valueText.name  = "OpenBarterValue";
            valueText.color = new Color(0.55f, 1.0f, 0.21f, 0.63f);
            RectTransform rt = valueText.GetComponent<RectTransform>();
            rt.anchorMax        = new Vector2(1f,  1f);
            rt.anchorMin        = new Vector2(1f,  1f);
            rt.pivot            = new Vector2(0f,  1f);
            rt.anchoredPosition = new Vector2(30f, 3f);
        }
        else {
            valueText = existing.GetComponent<TextMeshProUGUI>();
        }

        if (valueText != null) {
            valueText.gameObject.SetActive(ToggleActive() && ____requisite.PreparedItemsCount > 0);
            double value = Current.GetUnitValue(____requisite);
            valueText.text = $"+{CurrencyUtil.GetCurrencyChar(Current.CurrentTrader.Settings.Currency)} {TraderDealScreen.FormatPrice(
                 (int)(value * Math.Min(____requisite.PreparedItemsCount, ____requisite.RequiredItemsCount)))}";

            RectTransform rt = valueText.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(valueText.preferredWidth, valueText.preferredHeight);
        }
    }
}