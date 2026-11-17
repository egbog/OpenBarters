using EFT.HandBook;
using EFT.UI.Ragfair;
using SPT.Reflection.Patching;
using System.Reflection;
using Comfort.Common;
using static OpenBarters.Controllers.OpenBarterController;

namespace OpenBarters.Patches.Pricing;

public class CacheHandbookPricesPatch : ModulePatch {
    protected override MethodBase GetTargetMethod() {
        return typeof(RagFair).GetMethod("RefreshItemPrices", BindingFlags.Instance | BindingFlags.Public);
    }

    // flea's add-offer window overwrites handbook prices with flea prices; snapshot the originals first
    [PatchPrefix]
    private static void Prefix() {
        if (HandbookCache != Singleton<Handbook>.Instance) {
            // store handbook instance
            HandbookCache = Singleton<Handbook>.Instance;
            
            // reset the price dictionary
            HandbookPrices = new Dictionary<string, float>();

            // store all handbook prices
            foreach (KeyValuePair<string, HandbookNode> node in Singleton<Handbook>.Instance.AllNodes) {
                HandbookPrices.Add(node.Key, node.Value.Data.Price);
            }
        }
    }
}