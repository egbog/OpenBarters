using EFT;

namespace OpenBarters.Controllers;

// handbook prices for PriceCalculator, from the snapshot when the flea has overwritten them
public class SnapshotPriceSource : IBasePriceSource {
    public double GetBasePrice(MongoID itemId) {
        return OpenBarterController.CheckHandbookValue(itemId);
    }
}