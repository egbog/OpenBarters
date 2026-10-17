using EFT.InventoryLogic;
using EFT.Trading;
using EFT.UI;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Panel;

public class PanelShowPatch : ModulePatch {
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
        TraderAssortment = traderAssortment;

        // build the toggle button once, clones auto-fill checkbox
        if (OpenBarterToggle == null) {
            OpenBarterToggle      = Object.Instantiate(____autoFillRequirements, __instance.transform.parent, false);
            OpenBarterToggle.name = "OpenBarterToggle";
            RectTransform rt                     = OpenBarterToggle.RectTransform();
            rt.anchorMin          = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f); // center of the parent
            rt.anchoredPosition   = new Vector2(-199, 248);
            OpenBarterToggle.isOn = false;

            // remove auto fill listeners
            OpenBarterToggle.onValueChanged.RemoveAllListeners();

            // add our toggle listener
            OpenBarterToggle.onValueChanged.AddListener(useGrid => {
                ApplyToggle(useGrid, ____requisitesContainer);

                // hide/restore the "missing items" ribbon
                __instance.UpdateValidDealWarning();

                if (!useGrid) {
                    // also fires refresh events
                    Current!.ClearBarterItems();
                }
                else {
                    TraderAssortment?.PreparedItemsChanged.Invoke(); // re-check greying for this barter's required items
                    TraderAssortment?.PreparedSumChanged.Invoke();   // redraw price + ribbon
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