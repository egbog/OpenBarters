using EFT.InventoryLogic;
using EFT.Trading;
using EFT.UI;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Panel;

public class BuildPanel : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(BarterSchemePanel).GetMethod("Show", BindingFlags.Instance | BindingFlags.Public);
    }

    [PatchPrefix]
    private static bool Prefix(BarterSchemePanel   __instance, Assortment traderAssortment, InventoryController inventoryController,
                               ref UpdatableToggle ____autoFillRequirements, Transform ____requisitesContainer) {
        Trader trader = traderAssortment._trader;

        // rebuild our controller for each trader
        UseTrader(trader);

        // this is needed because the listener is created only once and we need to update the assortment for each trader
        // capturing "traderAssortment" instead will only store the first trader's assortment and will not update for subsequent traders
        TraderAssortment = traderAssortment;

        // build the toggle button
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
                TraderAssortment?.PreparedItemsChanged.Invoke();
            });
        }

        return true;
    }

    [PatchPostfix]
    private static void Postfix(Assortment ____traderAssortment, InventoryController ____inventoryController) {
        if (BarterTradingTableGridView != null) {
            BarterTradingTableGridView.Show(Current!.BarterTableGrid,
                                            ____traderAssortment,
                                            ____inventoryController,
                                            ItemUiContext.Instance);
        }
    }
}