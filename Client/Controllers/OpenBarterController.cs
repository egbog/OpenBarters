using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.Trading;
using EFT.UI;
using EFT.UI.DragAndDrop;
using UnityEngine;

namespace OpenBarters.Controllers;

public class OpenBarterController {
    public static OpenBarterController? Current { get; private set; }
    
    public static TradingTable?         BarterTradingTable; // our clone
    public static TradingTableGridView? BarterTradingTableGridView;
    public static UpdatableToggle?      OpenBarterToggle;
    public static Assortment?           TraderAssortment;

    public  ItemController TraderController;

    public Grid BarterTableGrid;

    public Dictionary<Item, Item> Items = new();
    public Trader                 CurrentTrader;

    private OpenBarterController(Trader trader) {
        CurrentTrader = trader;
        
        ItemFactory itemFactoryClass = Singleton<ItemFactory>.Instance;

        Stash barterStash = itemFactoryClass.CreateFakeStash(null);
        BarterTableGrid       = new Grid("barterTable", 8, 8, false, [], barterStash);
        barterStash.Grids[0] = BarterTableGrid;
        TraderController      = new ItemController(barterStash, trader.Id, trader.Settings.Nickname.Localized(null), true, EOwnerType.Profile);
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
}