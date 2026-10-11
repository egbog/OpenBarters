# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Working agreement (read first)

OpenBarters is egbog's mod. The author drives implementation; your role is to **explore and explain
implementation methods, not to write the mod for them**.

- **Do not author original implementation code.** No invented method bodies, no "here is the class you
  need." Locate the right seam, explain the trade-offs, and describe the approach in prose.
- **Code examples only from real sources** — the SPT framework (`../server-csharp`), the official
  `TestMod`, the in-framework "mod example 6.1" referenced by `AbstractPatch`, or this repo's scaffold.
  If you cannot point to where it actually exists, do not show it.
- **Point to where to look; do not walk through how the code works.** Name the file/type/method and give
  signatures and parameter names, but let the author trace the flow and internal logic themselves —
  avoid line-pinned, step-by-step explanations of framework internals. The aim is to help him learn the
  code firsthand, not to summarize it for him.
- **Ground every API claim in source.** Verify a real signature before recommending it; "I don't know /
  not in the source I read" is a valid answer. This mirrors the research-mode the author works in.
- **Machine-local paths live in `LOCAL.md`** (gitignored, per-developer). Read it for deploy targets, the
  framework-solution location, and the decompiled-game path. Never hardcode those here, and do not treat
  `LOCAL.md` as shared project documentation.
- **Verify, do not assume.** Member names, nullability, `virtual`-ness, and load order all change the
  answer here — check them rather than guessing.
- **Use https://db.sp-tarkov.com/search to decipher item ids into item names.

## What this project is

OpenBarters replaces traders' hardcoded barter requirements with **dynamic, value-based bartering**:
instead of fixed required items, the player may hand over **any item that trader would buy** (i.e. could
sell to them on the sell tab) **plus the selected barter's own required items**, as long as their summed
**trader buy value meets-or-exceeds** the original barter's required items' value on the same scale (with an
optional balance multiplier).

Design decisions locked with the author (full rationale in `PLAN.md` → "Design decisions"):
- Accepted items = **whatever the trader buys** — the trader base's `items_buy` / `items_buy_prohibited`.
  Client gate reuses vanilla `Assortment.CanPrepareItemToSell(Item)` (pin-lock, `Trader.GetUserItemPrice` →
  `TraderInfo.CanBuyItem` + price > 0, `!IsBeingSold`). Server enforcement uses `TraderBase.ItemsBuy` /
  `ItemsBuyProhibited` (pattern: `TradeController` → `ItemHelper.IsOfBaseclasses(tpl, ItemsBuy.Category)`).
  *(Supersedes the earlier per-trader ≥10%-specialty allow-list from `traders.md`, decided 2026-10-07.)*
- Value rule = **meet-or-exceed** target X = the **original required items'** value — not the received item's.
  Scale = **trader buy price** (`Trader.GetUserItemPrice` / `GetAssortmentPrice`) for basket and X alike,
  non-currency requirements only (*2026-10-10, was handbook*). The selected barter's required items count as
  buyable/priceable even if the trader normally doesn't buy them.

## Architecture

Two assemblies, two runtimes:

| Project | Target | Runtime | Role |
|---|---|---|---|
| `Server/` (`OpenBartersServer`) | net9.0 | SPT C# server | Rewrite barters at load; validate/enforce purchases |
| `Client/` (`OpenBartersClient`) | netstandard2.1 | BepInEx plugin in the Unity game | Item-selection UI; submit chosen items as `scheme_items` |

**The SPT server framework is a sibling read-only repo at `../server-csharp`** (the author does not
commit to it). The server project is registered in `../server-csharp/server-csharp.slnx` under `/Mods/`
for live debugging. The client's decompiled game reference lives at the sibling `../Decompiled-SPT4.1.16`.

### Server mod seams (all defined in `../server-csharp`)
> **4.1 note:** the framework's library *folders* are now `SPTushonka.*` (namespaces are still
> `SPTarkov.*`), and `DatabaseService.GetTraders()` is no longer present — tables are injected directly as
> DI types (e.g. `TradersTable`, `TemplateTable` under `Models/Spt/Tables/`). Seams below marked *(re-verify for 4.1)* were mapped
> against 4.0 and must be re-checked in source before use.

- **Packaging:** `ModMetadata : AbstractModMetadata` (GUID `com.egbog.openbarters`, `SptVersion ~4.1.0`).
  Canonical pattern: `../server-csharp/Testing/TestMod/TestMod.cs`.
- **Load hook:** implement `IOnLoad`; order with `[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]`
  so the trader DB is already loaded. Trader data → `trader.Assort.BarterScheme` *(re-verify for 4.1: the
  4.0 accessor `DatabaseService.GetTraders()` is gone; `Models/Spt/Tables/TradersTable.cs` is the traders table type)*.
- **DI override:** `[Injectable(TypeOverride = typeof(X))]` swaps a service, but only intercepts
  **virtual** members (see the test mocks under `../server-csharp/Testing/UnitTests/Mock/`).
- **Patching (the seam that matters here):** the purchase path is non-virtual
  (`PaymentService.PayMoney`, `TradeHelper.BuyItem`), so behavior changes go through Harmony via
  `AbstractPatch`/`PatchManager`
  (`../server-csharp/Libraries/SPTushonka.Reflection/Patching/AbstractPatch.cs` — "see mod example 6.1").
  `PatchManager` auto-discovers `AbstractPatch` subclasses in the mod assembly.

### Barter mechanics in the framework (where to explore)

Starting points to trace yourself — not a walkthrough. Read the flow in source; the goal is to
understand it firsthand, so these name the types/methods to follow rather than explaining what each does.

- **Barter data** lives on the trader tables — look at `Trader` / `TraderAssort` and the `barter_scheme`
  dictionary (keyed by assort item id; each value is a list of alternative schemes of
  `BarterScheme { _tpl, count, ... }`). This same structure is what gets sent to the client to render a
  barter.
- **The purchase flow** runs from `TradeHelper.BuyItem(...)` into `PaymentService.PayMoney(...)`. Follow
  how the client's `scheme_items` are consumed, and whether anything reconciles them against the
  trader's `barter_scheme`. That question — does the server validate the payment? — is the crux of this
  mod and is worth answering by reading the code rather than taking it on faith.
- **Value lookups** you will likely use: `HandbookHelper.GetTemplatePrice(MongoId tpl)` and
  `GetTemplatePriceForItems(IEnumerable<Item> items)`.
- **Item category:** `ItemHelper.GetItem(MongoId tpl)` returns `KeyValuePair<bool, TemplateItem?>`
  (key = found); the BSG parent class is `TemplateItem.Parent`.
- **Money vs. barter:** currencies are ordinary tpls — `PaymentHelper.IsMoneyTpl(...)` is how the
  framework tells them apart.

Use go-to-definition / find-references under the `Source-Debug` configuration to follow these threads.

### Server <-> client split (important constraint)
The server can validate and enforce any submission, but **cannot create the item-selection UX** — the
vanilla client only knows how to render a fixed `barter_scheme`. Letting the player freely pick items by
category requires the **client (BepInEx) plugin** to build the selection and submit the chosen instance
ids as `scheme_items`. Those hook points live in the decompiled game (`../Decompiled-SPT4.1.16`), not in
`../server-csharp`. Treat any client-hook claim as unverified until found in that decompiled source.

## Build, debug, deploy

Both projects define three configurations: **Debug**, **Release**, **Source-Debug**. Concrete
machine-local paths — sibling-repo layout, deploy targets, the framework solution, and the decompiled-game
reference — live in **`LOCAL.md`**; consult that file for the actual locations on this machine.

- **Source-Debug** swaps package/assembly references for source `ProjectReference`s so go-to-definition
  and stepping land in real source (framework source for the server, decompiled game for the client).
  `OpenBarters.slnx` only builds under `Source-Debug|*`.
- **Live debugging:** open the framework solution (the server mod is registered there under `/Mods/`),
  run the server, and step into the mod. Solution path in `LOCAL.md`.
- **Build server:** `dotnet build Server/OpenBartersServer.csproj -c Debug` (or `-c Release`).
  `Server/PostBuild.ps1` deploys the build into the server's `user/mods` folder — Debug/Release only;
  Source-Debug intentionally does not deploy. Deploy target in `LOCAL.md`.
- **Build client:** `dotnet build Client/OpenBartersClient.csproj -c Debug`. References the game
  `Assembly-CSharp`, `spt-reflection`, `spt-common`, Unity, and BepInEx 5. `Client/PostBuild.ps1` runs
  after build (deploy target in `LOCAL.md`).
- **Verify a run:** launch the SPT server from the framework solution and watch the server log for the
  mod's `OnLoad` output.

## Current state
Progress lives in `PLAN.md` (source of truth). Structural snapshot:
- **Server** — `Server/OpenBarters.cs`: metadata record + `IOnLoad` (`OnLoadOrder.TraderRegistration + 1`),
  currently running a per-trader root-category tally served by `Server/StaticRouter.cs`
  (`/openbarters/populatebarterinfo`) — left over from the superseded ≥10% design and **unused by the client**.
  Enforcement is Phase 6. Root namespace `_OpenBarters`.
- **Client** — BepInEx plugin; `Client/Plugin.cs` enables every patch. Root namespace `OpenBarters`;
  namespaces follow folders. One patch class per file, file named after the class (`<What>Patch`).
  - `Controllers/` — `OpenBarterController` (`Current` basket instance, basket add/remove/clear, UI statics,
    `ApplyToggle`); `BarterGridCentering` (MonoBehaviour on the grid slot, re-centres the grid in `LateUpdate`).
  - `Patches/Panel/` — `BarterSchemePanel` lifecycle/layout: `PanelShowPatch` (`Show` prefix/postfix:
    per-trader controller, toggle, grid-view `Show` + sizing), `PanelSelectionPatch` (`SelectedItemChangedHandler`:
    one-time clone/slot build, clear + toggle state per offer), `PanelClosePatch` (`Close`),
    `HideValidDealWarningPatch` (`UpdateValidDealWarning`).
  - `Patches/Basket/` — items in/out of the grid: `AcceptItemPatch`, `CanAcceptPatch`
    (`TradingTableGridView`), `UnprepareItemPatch` (`Assortment.UnprepareSellItem`).
  - `Patches/ItemState/` — stash item look/eligibility: `IsBeingSoldPatch`, `CanBuyRequisitePatch`
    (`TraderInfo.CanBuyItem`), `RefreshSchemePatch` (`TradingItemView.CG_NewTradingItemView`).
  - `Patches/Pricing/` — `BasketPricePatch` (`TraderDealScreen.TryGetPurchasePrice` → DEAL! button total).
  - New phases slot in as folders (e.g. `Patches/Submit/` for Phase 5).
- Metadata `Url` points at `github.com/egbog/Open-Barters` while the repo is `OpenBarters` — confirm the
  canonical name/URL before release.
