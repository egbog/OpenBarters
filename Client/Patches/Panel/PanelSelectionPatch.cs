using System.Diagnostics.CodeAnalysis;
using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using EFT.InventoryLogic;
using EFT.Trading;
using OpenBarters.Controllers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Panel;

public class PanelSelectionPatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(BarterSchemePanel).GetMethod("SelectedItemChangedHandler", BindingFlags.Instance | BindingFlags.Public);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [PatchPostfix]
    private static void Postfix(BarterSchemePanel __instance, Transform ____requisitesContainer, ScrollRect ____mainPart,
                                GameObject ____validSchemeWarning) {
        // clone TradingTable
        if (BarterTradingTable == null) {
            TraderDealScreen traderDealScreen = __instance.GetComponentInParent<TraderDealScreen>();
            var tradingTable = (TradingTable)AccessTools.Field(typeof(TraderDealScreen), "_tradingTable").GetValue(traderDealScreen);

            var slot = new GameObject("Barter Grid Slot", typeof(RectTransform), typeof(LayoutElement), typeof(BarterGridCentering)) {
                layer = ____mainPart.content.gameObject.layer
            };

            MainViewport = ____mainPart.viewport;

            // requirements text
            ItemsNeededLabel = ____mainPart.content.Find("BarterPanel/Items Needed From Stash");

            // whole ribbon image + text as child
            // this is hidden via HideValidDealWarningPatch
            Transform? validWarning = ____mainPart.content.Find("BarterPanel/ValidWarning");

            slot.transform.SetParent(____mainPart.content, false);

            BarterGridSlot = (RectTransform)slot.transform;
            SlotLayout     = slot.GetComponent<LayoutElement>();

            BarterTradingTable      = Object.Instantiate(tradingTable, BarterGridSlot, false);
            BarterTradingTable.name = "Barter Panel";
            BarterTradingTable.gameObject.SetActive(true);
            var cloneRt = (RectTransform)BarterTradingTable.transform;

            cloneRt.anchorMin        = new Vector2(0.5f, 1.0f);
            cloneRt.anchorMax        = new Vector2(0.5f, 1.0f);
            cloneRt.pivot            = new Vector2(0.5f, 1.0f);
            cloneRt.anchoredPosition = Vector2.zero;

            // remove listener from the clone to prevent interference with the original TradingTable
            var clearButton = (DefaultUIButton)AccessTools.Field(typeof(TradingTable), "_clearTableButton").GetValue(BarterTradingTable);
            var cloneHandler = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction),
                                                                    BarterTradingTable,
                                                                    AccessTools.Method(typeof(TradingTable), "CG_Awake"));
            clearButton.OnClick.RemoveListener(cloneHandler);

            // reflect the grid view for our clone
            BarterTradingTableGridView = (TradingTableGridView)AccessTools.Field(typeof(TradingTable), "_tableGridView")
                                                                          .GetValue(BarterTradingTable);

            ScrollRect scroll = BarterTradingTableGridView.GetComponentInParent<ScrollRect>(true);
            scroll.enabled = false;
            scroll.verticalScrollbar.gameObject.SetActive(false);
            if (scroll.horizontalScrollbar != null) {
                scroll.horizontalScrollbar.gameObject.SetActive(false);
            }

            BarterTradingTable.transform.Find("Trading Table/Border")?.gameObject.SetActive(false);

            // barter value ribbon label
            if (validWarning != null) {
                BarterValueRibbon = Object.Instantiate(validWarning, validWarning.parent, false);

                BarterValueLabelText = BarterValueRibbon.GetComponentInChildren<TextMeshProUGUI>(true);

                // the cloned text carries a LocalizedText that rewrites it from its locale key on enable/language change
                // disable it so our "Required: …" text sticks
                if (BarterValueLabelText != null) {
                    if (BarterValueLabelText.TryGetComponent(out LocalizedText localText)) {
                        localText.enabled = false;
                    }

                    BarterValueLabelText.fontSize = 20;
                }

                Transform background = BarterValueRibbon.Find("Background");
                Transform glow       = BarterValueRibbon.Find("Glow");

                if (background != null) {
                    BackgroundRibbon = background.GetComponent<Image>();
                }

                if (glow != null) {
                    BackgroundGlow       = glow.GetComponent<Image>();
                    BackgroundGlow.color = Color.white;

                    // save the red glow for later
                    RedGlowSprite = BackgroundGlow.sprite;

                    // steal sprite for green glow
                    foreach (Sprite sprite in Resources.FindObjectsOfTypeAll<Sprite>()) {
                        if (sprite.name == "status_green_glow") {
                            GreenGlowSprite = sprite;
                            break;
                        }
                    }
                }

                // enable clone and set sibling index to be after the original ValidWarning
                BarterValueRibbon.gameObject.SetActive(true);
                BarterValueRibbon.SetSiblingIndex(validWarning.GetSiblingIndex() + 1);
            }
        }

        if (OpenBarterToggle == null) {
            return;
        }

        // enforce toggle state
        bool hasOffer = __instance.SelectedItem != null;

        // clear before toggle visibility changes: on deselect, this event fires while the toggle is still
        // visible, so RefreshSchemePatch re-greys items the old barter had un-greyed
        Current!.ClearBarterItems();

        bool isBarter =
            TraderAssortment?.CurrentRequisites.Any(req => !CurrencyUtil.TryGetCurrencyType(req.RequiredItem.TemplateId, out _)) ?? false;

        OpenBarterToggle.gameObject.SetActive(hasOffer && isBarter);

        // second refresh once the toggle is visible: selecting an offer from none fires the clear above while
        // the toggle is still hidden, so un-grey the new barter's required items now
        if (hasOffer && isBarter) {
            ApplyToggle(OpenBarterToggle.isOn, ____requisitesContainer);
            TraderAssortment?.PreparedItemsChanged.Invoke();
        }
        else if (hasOffer) {
            ApplyToggle(false, ____requisitesContainer);
        }
        else {
            BarterGridSlot.gameObject.SetActive(false);
        }

        __instance.UpdateValidDealWarning();
    }
}