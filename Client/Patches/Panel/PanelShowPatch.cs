using Comfort.Common;
using EFT;
using EFT.HandBook;
using EFT.InventoryLogic;
using EFT.Trading;
using EFT.UI;
using EFT.UI.Ragfair;
using OpenBarters.Controllers;
using OpenBarters.Localization;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Panel;

public class PanelShowPatch : ModulePatch {
    private static bool _barterIconSet;

    protected override MethodBase GetTargetMethod() {
        return typeof(BarterSchemePanel).GetMethod("Show", BindingFlags.Instance | BindingFlags.Public);
    }

    // set state before first Show call
    [PatchPrefix]
    private static bool Prefix(BarterSchemePanel __instance, Assortment traderAssortment, UpdatableToggle ____autoFillRequirements,
                               Transform         ____requisitesContainer) {
        Trader trader = traderAssortment._trader;

        // rebuild our basket for each trader
        UseTrader(trader);

        // the toggle listener is creating once, so it reads this static instead of capturing the first trader's assortment
        OpenBarterController.TraderAssortment = traderAssortment;

        // build the toggle button once, clones auto-fill checkbox
        if (OpenBarterToggle == null) {
            OpenBarterToggle      = Object.Instantiate(____autoFillRequirements, __instance.transform.parent, false);
            OpenBarterToggle.name = "OpenBarterToggle";
            _barterIconSet        = false;
            RectTransform rt                     = OpenBarterToggle.RectTransform();
            rt.anchorMin          = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f); // center of the parent
            rt.anchoredPosition   = new Vector2(-199, 248);
            OpenBarterToggle.isOn = false;

            HoverTooltipArea tooltip = OpenBarterToggle.GetComponentInChildren<HoverTooltipArea>(true);
            tooltip?.SetMessageText(BarterValueText.GetLocaleStrings(LocalizationManager.Instance?.Culture).Enable, true);

            // remove auto fill listeners
            OpenBarterToggle.onValueChanged.RemoveAllListeners();

            // add our toggle listener
            OpenBarterToggle.onValueChanged.AddListener(useGrid => {
                ApplyToggle(useGrid, ____requisitesContainer);

                // re-run each slot's UpdateValues so the value labels follow the toggle
                foreach (Requisite req in OpenBarterController.TraderAssortment?.CurrentRequisites ?? []) {
                    req.PreparedChanged.Invoke();
                }

                if (!useGrid) {
                    OpenBarterController.TraderAssortment?.ClearPreparedItems();
                    // also fires refresh events
                    Current!.ClearBarterItems();
                }
                else {
                    OpenBarterController.TraderAssortment?.PreparedItemsChanged
                                        .Invoke();                                      // re-check greying for this barter's required items
                    OpenBarterController.TraderAssortment?.PreparedSumChanged.Invoke(); // redraw price + ribbon
                }
                
                // hide/restore the "missing items" ribbon
                __instance.UpdateValidDealWarning();
            });
        }

        if (!_barterIconSet && RagFair.IconsLoader != null &&
            Singleton<Handbook>.Instance.AllNodes.TryGetValue("5b47574386f77428ca22b33e", out HandbookNode node) &&
            !string.IsNullOrEmpty(node.Data.Icon)) {
            string icon = node.Data.Icon;

            RagFair.IconsLoader.GetIcon(icon,
                                        sprite => {
                                            // runs when the icon is loaded, or immediately if cached
                                            if (sprite != null && OpenBarterToggle != null) {
                                                foreach (Image img in OpenBarterToggle.GetComponentsInChildren<Image>(true)) {
                                                    if (img.name == "Icon") {
                                                        img.sprite = sprite;
                                                    }
                                                }

                                                _barterIconSet = true;
                                            }
                                        });
        }

        return true;
    }

    // clone exists by now
    [PatchPostfix]
    private static void Postfix(Assortment ____traderAssortment, InventoryController ____inventoryController) {
        if (BarterTradingTableGridView != null) {
            // reshow every open so the grid view uses the current trader's grid + assortment
            BarterTradingTableGridView.Show(Current!.BarterTableGrid,
                                            ____traderAssortment,
                                            ____inventoryController,
                                            ItemUiContext.Instance);

            var     cloneRt = (RectTransform)BarterTradingTable!.transform;
            var     areaRt  = (RectTransform)BarterTradingTableGridView.GetComponentInParent<ScrollRect>(true).transform;
            Vector2 grid    = ((RectTransform)BarterTradingTableGridView.transform).rect.size;

            // the Scroll Area is inset from the clone's edges; add that inset so the whole grid fits inside it
            Vector2 inset = cloneRt.rect.size - areaRt.rect.size;

            cloneRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, grid.x + inset.x);
            cloneRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,   grid.y + inset.y);

            // reserve the same height in the window's layout
            SlotLayout!.preferredHeight = grid.y + inset.y;
        }
    }
}