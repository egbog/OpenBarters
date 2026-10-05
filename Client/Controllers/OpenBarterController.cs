using Comfort.Common;
using Diz.LanguageExtensions;
using EFT;
using EFT.InventoryLogic;
using EFT.Trading;
using EFT.UI;
using EFT.UI.DragAndDrop;
using Newtonsoft.Json;
using SPT.Common.Http;
using UnityEngine;

namespace OpenBarters.Controllers;

public class OpenBarterController {
    public static OpenBarterController? Current { get; private set; }

    public static TradingTable?         BarterTradingTable; // our clone
    public static TradingTableGridView? BarterTradingTableGridView;
    public static UpdatableToggle?      OpenBarterToggle;
    public static Assortment?           TraderAssortment;

    public ItemController TraderController;

    public Grid BarterTableGrid;

    public        Dictionary<Item, Item>                         Items = new();
    public        Trader                                         CurrentTrader;
    public static Dictionary<MongoID, Dictionary<MongoID, int>>? TraderCategories;

    private OpenBarterController(Trader trader) {
        CurrentTrader = trader;

        ItemFactory itemFactoryClass = Singleton<ItemFactory>.Instance;

        Stash barterStash = itemFactoryClass.CreateFakeStash(null);
        BarterTableGrid = new Grid("barterTable", 8, 8, false, [], barterStash);
        barterStash.Grids[0] = BarterTableGrid;
        TraderController = new ItemController(barterStash, trader.Id, trader.Settings.Nickname.Localized(null), true, EOwnerType.Profile);
    }

    public static void UseTrader(Trader traderToCheck) {
        if (Current == null || Current.CurrentTrader.Id != traderToCheck.Id) {
            Current = new OpenBarterController(traderToCheck);
        }
    }

    public static void ApplyToggle(bool useGrid, Transform requisitesContainer) {
        requisitesContainer.gameObject.SetActive(!useGrid);

        if (BarterTradingTable != null) {
            BarterTradingTable.gameObject.SetActive(useGrid);
        }
    }

    public void ClearBarterItems() {
        foreach (OperationResult<DiscardResult> operationResult in
                 Items.Select(kvp => ItemManipulator.Discard(kvp.Key, TraderController, false))) {
            ProcessDiscardResult(operationResult);
        }

        Items.Clear();
        TraderAssortment?.PreparedItemsChanged.Invoke();

        // TODO: use these later for barter item sum
        //this.PreparedSum = default(Trader.ItemPrice);
        //this.PreparedSumChanged.Invoke();
    }

    public void UnprepareBarterItem(Item item) {
        OperationResult<DiscardResult> operationResult = ItemManipulator.Discard(item, TraderController, false);

        ProcessDiscardResult(operationResult);

        Items.Remove(item);
        TraderAssortment?.PreparedItemsChanged.Invoke();

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

    public bool CanPrepareItemToBarter(Item item) {
        if (item.PinLockState == EItemPinLockState.Locked) {
            return false;
        }

        //Trader.ItemPrice? userItemPrice = this._trader.GetUserItemPrice(item);
        //return userItemPrice.HasValue && userItemPrice.Value.Amount > 0 && !this.IsBeingSold(item);

        if (item.Template.ParentId == null) {
            return false;
        }
        
        Plugin.Log.LogInfo(string.Join(", ", TraderCategories[CurrentTrader.Id].Select(kvp => $"{kvp.Key}: {kvp.Value}")));

        return TraderCategories!.TryGetValue(CurrentTrader.Id, out Dictionary<MongoID, int>? categories) &&
               categories.ContainsKey(item.Template.ParentId.Value);
    }
}