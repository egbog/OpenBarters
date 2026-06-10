using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services.Mod;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace _OpenBarters;

public record ModMetadata : AbstractModMetadata {
    public override string                                          ModGuid           { get; init; } = "com.egbog.openbarters";
    public override string                                          Name              { get; init; } = "OpenBarters";
    public override string                                          Author            { get; init; } = "egbog";
    public override List<string>?                                   Contributors      { get; init; }
    public override SemanticVersioning.Version                      Version           { get; init; } = new("0.0.1");
    public override SemanticVersioning.Range                        SptVersion        { get; init; } = new("~4.0.0");
    public override List<string>?                                   Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>?   ModDependencies   { get; init; }
    public override string?                                         Url               { get; init; }  = "https://github.com/egbog/Open-Barters";
    public override bool?                                           IsBundleMod       { get; init; }  = false;
    public override string                                          License           { get; init; } = "MIT";
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class OpenBarters(ISptLogger<OpenBarters> logger, CustomItemService customItem) : IOnLoad {
    public static          bool        Debug;
    public static readonly ModMetadata Mod = new();

    public async Task OnLoad() {
        //Debug = config.Debug || logger.IsLogEnabled(LogLevel.Debug);

        await Task.CompletedTask;
    }
}