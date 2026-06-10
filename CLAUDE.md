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
instead of fixed required items, the player may hand over any items whose **BSG `_parent` class**
matches the original barter's category, as long as their summed **handbook value meets-or-exceeds** the
received item's handbook value (with an optional balance multiplier).

Design decisions locked with the author:
- Category source = **BSG item parent class** (`TemplateItem.Parent`), inherited from the vanilla
  barter's required item(s). No curated category lists.
- Value rule = **meet-or-exceed** the received item's handbook value (`HandbookHelper.GetTemplatePrice`).

## Architecture

Two assemblies, two runtimes:

| Project | Target | Runtime | Role |
|---|---|---|---|
| `Server/` (`OpenBartersServer`) | net9.0 | SPT C# server | Rewrite barters at load; validate/enforce purchases |
| `Client/` (`OpenBartersClient`) | netstandard2.1 | BepInEx plugin in the Unity game | Item-selection UI; submit chosen items as `scheme_items` |

**The SPT server framework is a sibling read-only repo at `../server-csharp`** (the author does not
commit to it). The server project is registered in `../server-csharp/server-csharp.slnx` under `/Mods/`
for live debugging. The client's decompiled game reference lives at the sibling `../Decompiled-SPT4.0.0`.

### Server mod seams (all defined in `../server-csharp`)
- **Packaging:** `ModMetadata : AbstractModMetadata` (GUID `com.egbog.openbarters`, `SptVersion ~4.0.0`).
  Canonical pattern: `../server-csharp/Testing/TestMod/TestMod.cs`.
- **Load hook:** implement `IOnLoad`; order with `[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]`
  so the trader DB is already loaded. Reach trader data via `DatabaseService.GetTraders()`
  (`../server-csharp/Libraries/SPTarkov.Server.Core/Services/DatabaseService.cs:153`) →
  `trader.Assort.BarterScheme`.
- **DI override:** `[Injectable(TypeOverride = typeof(X))]` swaps a service, but only intercepts
  **virtual** members (see the test mocks under `../server-csharp/Testing/UnitTests/Mock/`).
- **Patching (the seam that matters here):** the purchase path is non-virtual
  (`PaymentService.PayMoney`, `TradeHelper.BuyItem`), so behavior changes go through Harmony via
  `AbstractPatch`/`PatchManager`
  (`../server-csharp/Libraries/SPTarkov.Reflection/Patching/AbstractPatch.cs` — "see mod example 6.1").
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
ids as `scheme_items`. Those hook points live in the decompiled game (`../Decompiled-SPT4.0.0`), not in
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
  `Assembly-CSharp`, `spt-reflection`, `spt-common`, Unity, and BepInEx 5. No `Client/PostBuild.ps1`
  exists yet — the client deploy step is not wired up (intended target noted in `LOCAL.md`).
- **Verify a run:** launch the SPT server from the framework solution and watch the server log for the
  mod's `OnLoad` output.

## Current scaffold state
- `Server/OpenBarters.cs`: metadata record + an empty `OnLoad` (`IOnLoad`). The constructor currently
  injects `CustomItemService` — a leftover from the template; the barter-editing seam uses
  `DatabaseService` instead (see above).
- `Client/OpenBarters.cs`: mirror scaffold; not yet a BepInEx plugin entry point.
- Both projects use the root namespace `_OpenBarters`.
- Metadata `Url` points at `github.com/egbog/Open-Barters` while the repo is `OpenBarters` — confirm the
  canonical name/URL before release.
