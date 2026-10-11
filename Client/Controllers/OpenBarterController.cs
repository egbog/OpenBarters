using Comfort.Common;
using Diz.LanguageExtensions;
using EFT;
using EFT.InventoryLogic;
using EFT.Trading;
using EFT.UI;
using EFT.UI.DragAndDrop;
using UnityEngine;
using UnityEngine.UI;

namespace OpenBarters.Controllers;

public class OpenBarterController {
    public static OpenBarterController? Current { get; private set; }

    public static TradingTable?         BarterTradingTable; // our clone
    public static TradingTableGridView? BarterTradingTableGridView;
    public static UpdatableToggle?      OpenBarterToggle;
    public static Assortment?           TraderAssortment;
    public static RectTransform?        BarterGridSlot;
    public static LayoutElement?        SlotLayout;
    public static RectTransform?        MainViewport;
    public static Transform?            ItemsNeededLabel;

    public ItemController TraderController;

    public Grid BarterTableGrid;

    public        Dictionary<Item, Item> BarterItems = new();
    public        Trader                 CurrentTrader;
    public static int                    BarterSum = 0;

    private OpenBarterController(Trader trader) {
        CurrentTrader = trader;

        ItemFactory itemFactoryClass = Singleton<ItemFactory>.Instance;

        Stash barterStash = itemFactoryClass.CreateFakeStash(null);
        BarterTableGrid = new Grid("barterTable", 7, 10, false, [], barterStash);
        barterStash.Grids[0] = BarterTableGrid;
        TraderController = new ItemController(barterStash, trader.Id, trader.Settings.Nickname.Localized(null), true, EOwnerType.Profile);
    }

    public static void UseTrader(Trader traderToCheck) {
        if (Current == null || Current.CurrentTrader.Id != traderToCheck.Id) {
            Current = new OpenBarterController(traderToCheck);
        }
    }

    public static void ApplyToggle(bool useGrid, Transform requisitesContainer) {
        requisitesContainer.parent.gameObject.SetActive(!useGrid);

        // this takes up space we need for our grid and info
        if (ItemsNeededLabel != null) {
            ItemsNeededLabel.gameObject.SetActive(!useGrid);
        }

        if (BarterGridSlot != null) {
            BarterGridSlot.gameObject.SetActive(useGrid);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)requisitesContainer.parent.parent);
    }

    public void ClearBarterItems() {
        foreach (OperationResult<DiscardResult> operationResult in
                 BarterItems.Select(kvp => ItemManipulator.Discard(kvp.Key, TraderController, false))) {
            ProcessDiscardResult(operationResult);
        }

        BarterItems.Clear();
        TraderAssortment?.PreparedItemsChanged.Invoke();
        TraderAssortment?.PreparedSumChanged.Invoke();
    }

    public void UnprepareBarterItem(Item item) {
        OperationResult<DiscardResult> operationResult = ItemManipulator.Discard(item, TraderController, false);

        ProcessDiscardResult(operationResult);

        BarterItems.Remove(item);
        TraderAssortment?.PreparedItemsChanged.Invoke();
        TraderAssortment?.PreparedSumChanged.Invoke();

        // TODO: use these later for barter item sum
        //this.PreparedSum = this._trader.GetAssortmentPrice(this.SellingStash).GetValueOrDefault();
        //this.PreparedSumChanged.Invoke();
    }

    private bool ProcessDiscardResult(OperationResult<DiscardResult> operationResult) {
        if (operationResult.Succeeded) {
            operationResult.Value.RaiseEvents(TraderController, CommandStatus.Begin);
            operationResult.Value.RaiseEvents(TraderController, CommandStatus.Succeed);
            return true;
        }

        const string text  = "Cannot discard barter item: ";
        Error        error = operationResult.Error;
        Plugin.Log.LogWarning(text + error);
        return false;
    }
}