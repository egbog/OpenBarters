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
    [PatchPostfix]
    private static void Postfix(BarterSchemePanel __instance,
                                TraderAssortmentControllerClass ___traderAssortmentControllerClass,
                                InventoryController ___inventoryController_0,
                                ref UpdatableToggle ____autoFillRequirements, Transform ____requisitesContainer) {
        TraderClass traderClass = ___traderAssortmentControllerClass.TraderClass;

        OpenBarter ??= new OpenBarterController(traderClass.Settings.Id, traderClass.Settings.Nickname);

        if (OpenBarterToggle == null) {
            OpenBarterToggle = Object.Instantiate(____autoFillRequirements, __instance.transform.parent, false);
            RectTransform rt                     = OpenBarterToggle.RectTransform();
            rt.anchorMin          = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f); // center of the parent
            rt.anchoredPosition   = new Vector2(-199, 248);
            OpenBarterToggle.isOn = false;
            OpenBarterToggle.onValueChanged.RemoveAllListeners();
            OpenBarterToggle.onValueChanged.AddListener(useGrid => ApplyToggle(useGrid, ____requisitesContainer));
            //Plugin.Log.LogError(openBarterToggle.graphic.mainTexture.name);
        }

        TraderDealScreen traderDealScreen = __instance.GetComponentInParent<TraderDealScreen>();
        var tradingTable = (TradingTable)AccessTools.Field(typeof(TraderDealScreen), "_tradingTable")
                                                    .GetValue(traderDealScreen);

        // clone TradingTable
        if (BarterTradingTable == null) {
            // get parent for a transform under a Canvas
            BarterTradingTable = Object.Instantiate(tradingTable, tradingTable.transform.parent, false);
            BarterTradingTable.gameObject.SetActive(true);

            // remove listener from the clone to prevent interference with the original TradingTable
            var clearButton = ((DefaultUIButton)AccessTools.Field(typeof(TradingTable), "_clearTableButton").GetValue(BarterTradingTable));
            var cloneHandler = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), BarterTradingTable,
                                                                    AccessTools.Method(typeof(TradingTable),
                                                                        "method_0"));
            clearButton.OnClick.RemoveListener(cloneHandler);


            BarterTradingTableGridView = (TradingTableGridView)AccessTools.Field(typeof(TradingTable), "_tableGridView")
                                                                    .GetValue(BarterTradingTable);

            BarterTradingTableGridView.Show(OpenBarter.BarterTableGrid, ___traderAssortmentControllerClass,
                                      ___inventoryController_0, ItemUiContext.Instance);

            // reposition
            ScrollRect scrollRect = BarterTradingTableGridView.GetComponentInParent<ScrollRect>();
            var        rt         = (RectTransform)scrollRect.transform;
            rt.anchoredPosition = new Vector2(0, -150);
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

        //var transform = (Transform)AccessTools.Field(typeof(BarterSchemePanel), "_requisitesContainer").GetValue(__instance);

        //ItemUiContext itemUiContext = ItemUiContext.Instance;
        //tradingTable.Show(traderClass.CurrentAssortment, ___inventoryController_0, itemUiContext);

        //return false; // skip original method
    }
}