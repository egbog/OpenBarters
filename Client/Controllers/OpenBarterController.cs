using Comfort.Common;
using Diz.LanguageExtensions;
using EFT;
using EFT.HandBook;
using EFT.InventoryLogic;
using EFT.Trading;
using EFT.UI;
using EFT.UI.DragAndDrop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OpenBarters.Controllers;

public class OpenBarterController {
    public static OpenBarterController? Current { get; private set; }

    public static TradingTable?         BarterTradingTable;         // our clone of the sell-tab table
    public static TradingTableGridView? BarterTradingTableGridView; // the clone's grid view (drop target)
    public static UpdatableToggle?      OpenBarterToggle;           // grid switch
    public static Assortment?           TraderAssortment;           // the current trader's assortment, set on every panel Show
    public static RectTransform?        BarterGridSlot;             // layout placeholder holding the clone inside the buy window
    public static LayoutElement?        SlotLayout;                 // reserves the slot's height in the window's layout
    public static RectTransform?        MainViewport;               // buy window's scroll viewport, used to center the grid
    public static Transform?            ItemsNeededLabel;           // "required from your stash" label, hidden in barter grid
    public static Transform?            BarterValueRibbon;          // required value ribbon, clone of ValidWarning
    public static TextMeshProUGUI?      BarterValueLabelText;
    public static Image?                BackgroundRibbon;
    public static Image?                BackgroundGlow;
    public static Sprite?               GreenGlowSprite; // status_green_glow, used to indicate sufficient basket value
    public static Sprite?               RedGlowSprite;   // warning_glow
    public static Tab?     SellTab;

    public ItemController TraderController; // owns the basket's fake stash, not the trader's own controller

    public Grid BarterTableGrid;

    public Dictionary<Item, Item> BarterItems = new(); // basket clone to original stash item
    public Trader                 CurrentTrader;

    private OpenBarterController(Trader trader) {
        CurrentTrader = trader;

        ItemFactory itemFactoryClass = Singleton<ItemFactory>.Instance;

        // fake stash + grid to hold basket clones
        Stash barterStash = itemFactoryClass.CreateFakeStash(null);
        BarterTableGrid = new Grid("barterTable", 7, 10, false, [], barterStash); // 7 columns fit buy window's viewport
        barterStash.Grids[0] = BarterTableGrid;
        TraderController = new ItemController(barterStash, trader.Id, trader.Settings.Nickname.Localized(null), true, EOwnerType.Profile);
    }

    // one basket per trader. rebuild when trader changes
    public static void UseTrader(Trader traderToCheck) {
        if (Current == null || Current.CurrentTrader.Id != traderToCheck.Id) {
            Current = new OpenBarterController(traderToCheck);
        }
    }

    public static void ApplyToggle(bool useGrid, Transform requisitesContainer) {
        requisitesContainer.parent.gameObject.SetActive(!useGrid); // ItemsPanel. requisites list

        // this takes up space we need for our grid and info
        if (ItemsNeededLabel != null) {
            ItemsNeededLabel.gameObject.SetActive(!useGrid);
        }

        if (BarterGridSlot != null) {
            BarterGridSlot.gameObject.SetActive(useGrid);
        }

        if (BarterValueRibbon != null) {
            BarterValueRibbon.gameObject.SetActive(useGrid);
        }

        // rebuild window's content now
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)requisitesContainer.parent.parent);
    }

    // empty basket. modelled on Assortment.ClearSellItems
    public void ClearBarterItems() {
        foreach (OperationResult<DiscardResult> operationResult in
                 BarterItems.Select(kvp => ItemManipulator.Discard(kvp.Key, TraderController, false))) {
            ProcessDiscardResult(operationResult);
        }

        BarterItems.Clear();
        TraderAssortment?.PreparedItemsChanged.Invoke(); // tells stash item views to re-run IsBeingSold, so originals un-grey
        TraderAssortment?.PreparedSumChanged.Invoke();   // price display + ribbon redraw
    }

    // remove one clone from basket. modelled on Assortment.UnprepareSellItem
    public void UnprepareBarterItem(Item item) {
        OperationResult<DiscardResult> operationResult = ItemManipulator.Discard(item, TraderController, false);

        ProcessDiscardResult(operationResult);

        BarterItems.Remove(item);
        TraderAssortment?.PreparedItemsChanged.Invoke(); // tells stash item views to re-run IsBeingSold, so originals un-grey
        TraderAssortment?.PreparedSumChanged.Invoke();   // price display + ribbon redraw
    }

    // copy goes in the basket. original stays in stash until purchase is sent. modelled on PrepareToSell
    public void PrepareBarterItem(Item item, LocationInGrid location, int count) {
		// already in the basket, e.g. our drop and UIFixes' multi-select both placing the dragged item
		if (BarterItems.ContainsValue(item)) {
            return;
        }

        Item clone = item.CloneItemWithSameId();
        clone.OriginalAddress   = item.CurrentAddress;
        clone.StackObjectsCount = count;

        BarterItems.Add(clone, item);

        TraderController.AddAndRaiseEvents(clone, BarterTableGrid.CreateItemAddress(location));

        TraderAssortment?.PreparedItemsChanged.Invoke(); // grey out the original
        TraderAssortment?.PreparedSumChanged.Invoke();   // redraw price + ribbon
    }

    private bool ProcessDiscardResult(OperationResult<DiscardResult> operationResult) {
        if (operationResult.Succeeded) {
            // apply discard to the basket's view
            operationResult.Value.RaiseEvents(TraderController, CommandStatus.Begin);
            operationResult.Value.RaiseEvents(TraderController, CommandStatus.Succeed);
            return true;
        }

        const string text  = "Cannot discard barter item: ";
        Error        error = operationResult.Error;
        Plugin.Log.LogWarning(text + error);
        return false;
    }

    // value the basket must reach. the selected barter's non-money requirements at handbook value, converted to trader currency
    public int GetRequiredValue() {
        var requiredValue = 0;

        if (TraderAssortment == null) {
            return requiredValue;
        }

        double currencyRate = CurrentTrader.CurrencyCourses[CurrencyUtil.GetCurrencyId(CurrentTrader.Settings.Currency)];

        foreach (Requisite req in TraderAssortment.CurrentRequisites) {
            // skip money reqs
            if (CurrencyUtil.TryGetCurrencyType(req.RequiredItem.TemplateId, out _)) {
                continue;
            }

            double basePrice      = Singleton<Handbook>.Instance.GetBasePrice(req.RequiredItem.TemplateId);
            // convert into trader currency
            double convertedPrice = basePrice / currencyRate;

            convertedPrice *= req.RequiredItemsCount;

            // include level of req item in pricing, required for things like dogtags
            if (req.RequiredItem.TryGetItemComponent(out DogtagComponent _)) {
                requiredValue += (int)(convertedPrice * req.Level);
            }
            else {
                requiredValue += (int)convertedPrice;
            }
        }

        return requiredValue;
    }

    // value of the basket at this trader's buy price
    public Trader.ItemPrice GetBasketValue() {
        return CurrentTrader.GetAssortmentPrice((Stash)TraderController.RootItem).GetValueOrDefault();
    }

    // buy the selected item with the basket's original items. modelled on Assortment.Purchase
    public async Task PurchaseBarter() {
        Assortment? assortment = TraderAssortment; // local copy, static can change if trader switches mid request
        if (assortment == null) {
            return;
        }

        if (assortment.TransactionInProgress) {
            Plugin.Log.LogError("Cannot purchase while transaction is in progress");
            return;
        }

        int basketValue   = GetBasketValue().Amount;
        int requiredValue = GetRequiredValue();

        // re check the deal gate. required value 0 means nothing could be priced
        if (basketValue < requiredValue || requiredValue <= 0) {
            Plugin.Log.LogError("Insufficient basket value to complete the barter transaction");
            return;
        }

        // snapshot before await
        Item selectedItem = assortment.SelectedItem;
        int  quantity     = assortment.CurrentQuantity;

        if (selectedItem == null) {
            Plugin.Log.LogError("No item selected for purchase");
            return;
        }

        // lock trade ui until finally
        assortment.TransactionInProgress = true;

        try {
            // pay with the originals
            TradingItemReference[] references = [
                .. BarterItems.Select(pair => new TradingItemReference { Count = pair.Key.StackObjectsCount, Item = pair.Value })
            ];

            // buying last of the stack, deselect. clears basket so it must happen after references are created
            if (selectedItem.StackObjectsCount <= quantity) {
                assortment.SelectItem(null);
            }

            IResult result = await assortment._trading.ConfirmPurchase(CurrentTrader.Id, selectedItem.Id, quantity, 0, references);

            if (result.Succeed) {
                selectedItem.BuyRestrictionCurrent += quantity;
                ClearBarterItems();
                assortment.ClearPreparedItems(); // empty the stash items vanilla set aside for its requisites (auto-fill)
                Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.TradeOperationComplete);

                if (!CurrentTrader.AssortmentLoading) {
                    await assortment.UpdateAssortment(true);
                }
            }
            else {
                int errorCode = result.ErrorCode;

                // Assortment.Purchase keeps the selection for these error codes
                if (errorCode != 214 && errorCode != 1505) {
                    assortment.SelectItem(null);

                    if (EBackendErrorCode.PriceChanged.EqualsToInt(errorCode) && !CurrentTrader.AssortmentLoading) {
                        await assortment.UpdateAssortment(true);
                    }
                }

                Plugin.Log.LogError($"{result.ErrorCode}: {result.Error}");
            }
        }
        catch (Exception e) {
            Plugin.Log.LogError(e.ToString());
        }
        finally {
            assortment.TransactionInProgress = false;
        }
    }

    // barter mode is on and on screen. isOn stays true while the toggle is hidden for cash offers or the sell tab
    public static bool ToggleActive() {
        return OpenBarterToggle && OpenBarterToggle is { isOn: true, gameObject.activeInHierarchy: true };
    }
}