using Comfort.Common;
using EFT;
using EFT.InventoryLogic;

namespace OpenBarters.Controllers;

public class OpenBarterController {
    private ItemFactory      _itemFactoryClass;
    private Stash        _barterStash;
    public  ItemController TraderController;

    public Grid BarterTableGrid;

    public Dictionary<Item, Item> Items = new();

    public OpenBarterController(string traderId, string traderNickname) {
        _itemFactoryClass = Singleton<ItemFactory>.Instance;

        _barterStash          = _itemFactoryClass.CreateFakeStash(null);
        BarterTableGrid       = new Grid("barterTable", 8, 8, false, [], _barterStash);
        _barterStash.Grids[0] = BarterTableGrid;
        TraderController      = new ItemController(_barterStash, traderId, traderNickname, true, EOwnerType.Profile);
    }
}