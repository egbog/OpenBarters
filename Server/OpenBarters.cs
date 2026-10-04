using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Services.Modding.Custom;

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

[Injectable(TypePriority = OnLoadOrder.Preload)]
public class OpenBarters(ISptLogger<OpenBarters> logger, CustomItemService customItem) : IOnLoad {
    public static          bool        Debug;
    public static readonly ModMetadata Mod = new();

    public Task OnLoadAsync(CancellationToken cancellationToken) {
        //Debug = config.Debug || logger.IsLogEnabled(LogLevel.Debug);

        return Task.CompletedTask;
    }
}