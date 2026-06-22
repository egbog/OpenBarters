using System.Diagnostics.CodeAnalysis;
using OpenBarters.Controllers;
using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace OpenBarters.Patches;

public class ShowMultiSelectWindow : ModulePatch {
    public static OpenBarterController OpenBarter;
    public static TradingTable         BarterTradingTable; // our clone
    public static TradingTableGridView BarterTradingTableGridView;
    public static UpdatableToggle      OpenBarterToggle;

    protected static void ApplyToggle(bool useGrid, Transform requisitesContainer) {
        requisitesContainer.gameObject.SetActive(!useGrid);
        BarterTradingTable.gameObject.SetActive(useGrid);
    }

    //traderAssortmentControllerClass.SelectedItemChanged event
    //EFT.UI.BarterSchemePanel.method_01()
    protected override MethodBase GetTargetMethod() {
        return typeof(BarterSchemePanel).GetMethod("method_1", BindingFlags.Instance | BindingFlags.Public);
    }

    // create and render custom grid view and toggle box
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [PatchPostfix]
    private static void Postfix(BarterSchemePanel __instance,
                                TraderAssortmentControllerClass ___traderAssortmentControllerClass,
                                InventoryController ___inventoryController_0,
                                ref UpdatableToggle ____autoFillRequirements, Transform ____requisitesContainer) {
        TraderClass traderClass = ___traderAssortmentControllerClass.TraderClass;

        OpenBarter ??= new OpenBarterController(traderClass.Settings.Id, traderClass.Settings.Nickname);

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
            OpenBarterToggle.onValueChanged.AddListener(useGrid => ApplyToggle(useGrid, ____requisitesContainer));
        }

        // clone TradingTable
        if (BarterTradingTable == null) {
            TraderDealScreen traderDealScreen = __instance.GetComponentInParent<TraderDealScreen>();
            var tradingTable = (TradingTable)AccessTools.Field(typeof(TraderDealScreen), "_tradingTable")
                                                        .GetValue(traderDealScreen);

            // get parent for a transform under a Canvas
            // parent under BarterSchemePanel.Scroll View
            ScrollRect scrollView = __instance.GetComponentInChildren<ScrollRect>(true); // TODO: not working...

            // parent under BarterSchemePanel's scroll view
            BarterTradingTable      = Object.Instantiate(tradingTable, scrollView.transform, false);
            BarterTradingTable.name = "Barter Panel";
            BarterTradingTable.gameObject.SetActive(true);

            // remove listener from the clone to prevent interference with the original TradingTable
            var clearButton = (DefaultUIButton)AccessTools.Field(typeof(TradingTable), "_clearTableButton")
                                                          .GetValue(BarterTradingTable);
            var cloneHandler = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), BarterTradingTable,
                                                                    AccessTools.Method(typeof(TradingTable),
                                                                        "method_0"));
            clearButton.OnClick.RemoveListener(cloneHandler);

            // reflect the grid view for our clone
            BarterTradingTableGridView = (TradingTableGridView)AccessTools.Field(typeof(TradingTable), "_tableGridView")
                                                                          .GetValue(BarterTradingTable);

            BarterTradingTableGridView.Show(OpenBarter.BarterTableGrid, ___traderAssortmentControllerClass,
                                            ___inventoryController_0, ItemUiContext.Instance);

            ScrollRect scroll = BarterTradingTableGridView.GetComponentInParent<ScrollRect>(true);
            scroll.verticalScrollbar.gameObject.SetActive(true);
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent; // shows on overflow
            scroll.scrollSensitivity           = 30f;

            // scroll bar size
            var sbRt = (RectTransform)scroll.verticalScrollbar.transform;
            sbRt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 315f);
            sbRt.anchoredPosition = new Vector2(10f, 65f); // align with the new grid window

            // reposition
            var rt = (RectTransform)scroll.transform;
            rt.anchoredPosition = new Vector2(0f, -130f);

            Canvas.ForceUpdateCanvases();
            rt.anchorMin = new Vector2(0f,         1f); // was (0,0) → pin bottom-stretch off
            rt.anchorMax = new Vector2(1f,         1f); // horizontal stretch kept, vertical now fixed at top
            rt.pivot     = new Vector2(rt.pivot.x, 1f);

            scroll.verticalScrollbar.value    = 1f;
            scroll.verticalNormalizedPosition = 1f; // 1 = top, 0 = bottom

            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 315f); // visible height in px, 5 rows

            BarterTradingTable.transform.Find("Trading Table/Border")?.gameObject.SetActive(false);
        }

        // enforce toggle state
        bool hasOffer = __instance.Item_0 != null;
        OpenBarterToggle.gameObject.SetActive(hasOffer);
        if (hasOffer) {
            ApplyToggle(OpenBarterToggle.isOn, ____requisitesContainer);
        }
        else {
            BarterTradingTable.gameObject.SetActive(false);
        }
    }
}