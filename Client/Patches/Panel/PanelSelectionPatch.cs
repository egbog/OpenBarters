using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using EFT.InventoryLogic;
using OpenBarters.Controllers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Panel;

public class PanelSelectionPatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(BarterSchemePanel).GetMethod("SelectedItemChangedHandler", BindingFlags.Instance | BindingFlags.Public);
    }

    // runs on every offer selection, builds ui the first time then enforces toggle state
    [PatchPostfix]
    private static void Postfix(BarterSchemePanel __instance, Transform ____requisitesContainer, ScrollRect ____mainPart) {
        // one time setup
        if (BarterTradingTable == null) {
            TraderDealScreen traderDealScreen = __instance.GetComponentInParent<TraderDealScreen>();
            var tradingTable = (TradingTable)AccessTools.Field(typeof(TraderDealScreen), "_tradingTable").GetValue(traderDealScreen);

            SellTab = traderDealScreen._sellTab;

            // placeholder in the window's layout. centering component keeps the grid on the viewport
            var slot = new GameObject("Barter Grid Slot", typeof(RectTransform), typeof(LayoutElement), typeof(BarterGridCentering)) {
                // copy the ui layer so the clone renders
                layer = ____mainPart.content.gameObject.layer
            };

            MainViewport = ____mainPart.viewport;

            // requirements text
            ItemsNeededLabel = ____mainPart.content.Find("BarterPanel/Items Needed From Stash");

            // whole ribbon image + text as child
            // this is hidden via HideValidDealWarningPatch
            Transform? validWarning = ____mainPart.content.Find("BarterPanel/ValidWarning");

            // false = keep ui-local position/scale
            slot.transform.SetParent(____mainPart.content, false);

            BarterGridSlot = (RectTransform)slot.transform;
            SlotLayout     = slot.GetComponent<LayoutElement>();

            BarterTradingTable      = Object.Instantiate(tradingTable, BarterGridSlot, false);
            BarterTradingTable.name = "Barter Panel";

            // stays active, the slot controls visibility
            BarterTradingTable.gameObject.SetActive(true);
            var cloneRt = (RectTransform)BarterTradingTable.transform;

            // top-center point anchors. fixed size, hangs from the slot's top and spills evenly sideways
            cloneRt.anchorMin        = new Vector2(0.5f, 1.0f);
            cloneRt.anchorMax        = new Vector2(0.5f, 1.0f);
            cloneRt.pivot            = new Vector2(0.5f, 1.0f);
            cloneRt.anchoredPosition = Vector2.zero;

            // reflect the grid view for our clone
            BarterTradingTableGridView = (TradingTableGridView)AccessTools.Field(typeof(TradingTable), "_tableGridView")
                                                                          .GetValue(BarterTradingTable);

            // the whole buy window scrolls
            ScrollRect scroll = BarterTradingTableGridView.GetComponentInParent<ScrollRect>(true);
            scroll.enabled = false;
            scroll.verticalScrollbar.gameObject.SetActive(false);
            if (scroll.horizontalScrollbar != null) {
                scroll.horizontalScrollbar.gameObject.SetActive(false);
            }

            BarterTradingTable.transform.Find("Trading Table/Border")?.gameObject.SetActive(false);

            // required value ribbon
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
                    // plain rectangle, color comes from the sprite's tint
                    BackgroundRibbon = background.GetComponent<Image>();
                }

                if (glow != null) {
                    // glow color is baked into the sprite. must stay white
                    BackgroundGlow       = glow.GetComponent<Image>();
                    BackgroundGlow.color = Color.white;

                    // save the red glow for later
                    RedGlowSprite = BackgroundGlow.sprite;

                    // steal sprite for green glow. BasketPricePatch hides glow if not found
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

        // clear before toggle visibility changes: on deselect, this event fires while the toggle is still visible
        // so RefreshSchemePatch re-greys items the old barter had un-greyed
        // an exception from a refresh handler (e.g. pricing) must not skip the toggle update below
        try {
            Current!.ClearBarterItems();
        }
        catch (Exception ex) {
            Plugin.Log.LogError(ex);
        }

        // a barter has at least one non-money req. cash offers don't get the toggle
        bool isBarter =
            TraderAssortment?.CurrentRequisites.Any(req => !CurrencyUtil.TryGetCurrencyType(req.RequiredItem.TemplateId, out _)) ?? false;

        OpenBarterToggle.gameObject.SetActive(hasOffer && isBarter);

        if (hasOffer && isBarter) {
            // restore the player's last choice
            ApplyToggle(OpenBarterToggle.isOn, ____requisitesContainer);
            // second refresh once the toggle is visible. un-greys the new barter's required items
            TraderAssortment?.PreparedItemsChanged.Invoke();
        }
        else {
            // cash offer or no offer
            ApplyToggle(false, ____requisitesContainer);
        }

        // re-evaluate the warning now that the toggle's visibility is settled (it was checked before we set the toggle)
        __instance.UpdateValidDealWarning();
    }
}