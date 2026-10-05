using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace _OpenBarters;

[Injectable]
public sealed class OpenBartersRouter(JsonUtil jsonUtil) : StaticRouter(jsonUtil,
[
    new RouteAction<EmptyRequestData>("/openbarters/populatebarterinfo",
                                      (url, info, sessionId, output, cancellationToken) =>
                                          new ValueTask<string>(jsonUtil.Serialize(OpenBarters.TraderCategories) ?? "{}"))
]);