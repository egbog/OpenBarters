using Comfort.Common;
using EFT.InventoryLogic;

namespace OpenBarters.Controllers;

public class OpenBarterController {
    private ItemFactoryClass      _itemFactoryClass;
    private StashItemClass        _barterStash;
    public  TraderControllerClass TraderController;

    public StashGridClass BarterTableGrid;

    public Dictionary<Item, Item> Items = new();

    public OpenBarterController(string traderId, string traderNickname) {
        _itemFactoryClass = Singleton<ItemFactoryClass>.Instance;

        _barterStash = _itemFactoryClass.CreateFakeStash(null);
        BarterTableGrid = new StashGridClass("barterTable", 8, 8, false, new ItemFilter[0], _barterStash);
        _barterStash.Grids[0] = BarterTableGrid;
        TraderController = new TraderControllerClass(_barterStash, traderId, traderNickname, true, EOwnerType.Profile);
    }
}