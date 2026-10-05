using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace _OpenBarters;

public record ModMetadata : IModMetadata {
    public string                                        ModGuid           { get; init; } = "com.egbog.openbarters";
    public string                                        Name              { get; init; } = "OpenBarters";
    public string                                        Author            { get; init; } = "egbog";
    public List<string>?                                 Contributors      { get; init; }
    public SemanticVersioning.Version                    Version           { get; init; } = new("0.0.1");
    public SemanticVersioning.Range                      SptVersion        { get; init; } = new("~4.1.0");
    public List<string>?                                 Incompatibilities { get; init; }
    public Dictionary<string, SemanticVersioning.Range>? ModDependencies   { get; init; }
    public string?                                       Url               { get; init; } = "https://github.com/egbog/Open-Barters";
    public string                                        License           { get; init; } = "MIT";
    public bool                                          HasPrepatcher     { get; init; } = false;
}

[Injectable(TypePriority = OnLoadOrder.TraderRegistration)]
public class OpenBarters(ISptLogger<OpenBarters> logger, TradersTable tradersTable, TemplateTable templateTable) : IOnLoad {
    public static          bool                                          Debug;
    public static readonly ModMetadata                                   Mod              = new();
    public static          Dictionary<MongoId, Dictionary<MongoId, int>> TraderCategories = new();

	public Task OnLoadAsync(CancellationToken cancellationToken) {
        //Debug = config.Debug || logger.IsLogEnabled(LogLevel.Debug);

        HashSet<MongoId> barterCategories = [
            BaseClasses.BUILDING_MATERIAL, BaseClasses.ELECTRONICS, BaseClasses.BATTERY, BaseClasses.LUBRICANT,
            BaseClasses.MEDICAL_SUPPLIES, BaseClasses.TOOL, BaseClasses.JEWELRY, BaseClasses.HOUSEHOLD_GOODS, BaseClasses.OTHER
        ];

        foreach (Trader trader in tradersTable.Values) {
            Dictionary<MongoId, int> categories = [];

            foreach (BarterScheme requirement in trader.Assort.BarterScheme.Values.Where(alternatives => alternatives.Count != 0)
                                                       .SelectMany(alternatives => alternatives[0])) {
                if (!templateTable.Items.TryGetValue(requirement.Template, out TemplateItem? template)) {
                    continue;
                }

                if (barterCategories.Contains(template.Parent)) {
                    if (!categories.TryAdd(template.Parent, 1)) {
                        categories[template.Parent]++;
                    }
                }
            }

            // only add if the trader has a barter available
            if (categories.Count > 0) {
                TraderCategories[trader.Base.Id] = categories;
            }
        }

        return Task.CompletedTask;
    }
}