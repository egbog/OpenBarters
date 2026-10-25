This repo is a mod for [sp-tushonka](https://sp-mod.com/)

# Open Barters

Trader barters are rigid. They want one exact set of items, and if you're missing a single one you're stuck farming raids or buying it off the flea, all while sitting on a stash full of loot the same trader would gladly take off your hands.

Open Barters turns barters into a value trade instead. Throw whatever the trader is willing to buy into a grid, and once it's worth as much as the original requirements, the deal is yours. The original barter is still there, open bartering is just a toggle.

## Usage
1. Select a barter on the buy tab and tick the toggle in the buy window. The requirements list is replaced with a grid.
2. Put items from your stash into the grid:
   - Drag them in
   - Ctrl+click them
   - Ctrl+drag a stack to choose how many to put in
3. The ribbon above the grid shows how much value is still required, or how much you are overpaying.
4. Once it turns green, hit DEAL!

To remove an item from the grid, Ctrl+click or right-click it.
The grid is cleared when you change offers, switch to the sell tab, close the trader or untick the toggle.
Cash offers are untouched, the toggle only shows up on barters.

## What you can pay with
Anything the trader would buy from you on the sell tab, plus the items the barter originally asks for, even if that trader doesn't normally buy them.
Same rules as selling apply, so containers need to be empty and worn out items are refused. Weapons are accepted with their attachments.

## How value works
- The barter's value is the handbook value of the items it originally asks for, in the trader's currency. Dogtags are multiplied by the required level.
- Matching original requirements are credited at their handbook value, up to the requested quantity. Matching uses the game's barter rules, including dogtag level and side, encoded state, and weapon functionality.
- Everything else, including surplus items of a requested type, is valued at what the trader would pay you for it on the sell tab (condition and loyalty level included).
- Only the missing part of the original requirements receives the configurable substitute price multiplier (default **1.5**).
- A zero or missing dogtag minimum level uses level 1 for valuation, so an unrestricted dogtag barter cannot become unpriced. The game's original level/side matching rules still apply.

For example, a barter has requirements worth 10,000 roubles. You provide matching requirements worth 4,000. With a multiplier of 1.5, substitute items must have a combined sell-tab value of at least `(10,000 - 4,000) * 1.5 = 9,000` roubles. Bringing all the required items incurs no markup.

Matching requirements first reduce the amount that needs substituting. The ribbon shows the remaining substitute cost or overpayment; the price above DEAL shows the sell-tab value of the substitutes and surplus. Bringing all exact requirements leaves zero substitute cost. The same calculation controls the button and is checked again before purchase.

## Configuration

In the **F12** configuration manager, open **OpenBarters > Pricing > Substitute price multiplier** (range **1–10**, default **1.5**). It is also saved in `BepInEx/config/com.egbog.openbarters.cfg`.

- **1**: no additional markup on substitutes; they are still valued at the trader's sell-tab price.
- **1.5**: 50% markup on the missing part.
- **2**: double the value of the missing part.

Changes apply immediately to an open barter. The normal barter with the toggle off and cash offers are unchanged.

## Installation
Extract the zip into your SPT folder (the one with `EscapeFromTarkov.exe`). You should end up with `BepInEx/plugins/OpenBarters/OpenBarters.dll`.
To uninstall, delete the `BepInEx/plugins/OpenBarters` folder.

## Known Issues
Barters that require both money and items will only charge the items. There are none in vanilla, so this only affects modded traders.
New UI text is English only.

## Compatibility
Compatible with SPT 4.1.x
Client only, no server mod required.
Compatible with [UI Fixes](https://github.com/tyfon7/UIFixes):
- Multi-select drags into the grid work, multi-selected stacks go in whole
- "Autoswitch Buy/Sell when Trading" won't switch to the sell tab on barter Ctrl+clicks

## Building
1. Install the [.NET SDK](https://dotnet.microsoft.com/download) (8 or newer).
2. Create a `Client/dependencies` folder and copy these into it:
   - From `<SPT install>/EscapeFromTarkov_Data/Managed`: `Assembly-CSharp.dll`, `Comfort.dll`, `ItemComponent.Types.dll`, `Sirenix.Serialization.dll`, `Unity.TextMeshPro.dll`, `UnityEngine.dll`, `UnityEngine.CoreModule.dll`, `UnityEngine.UI.dll`, `UnityEngine.UIModule.dll`, `UnityEngine.InputLegacyModule.dll`
   - From `<SPT install>/BepInEx/plugins/spt`: `spt-common.dll`, `spt-reflection.dll`
3. (Optional) To deploy straight into your game on every build, create `Client/OpenBartersClient.csproj.user`:
   ```xml
   <Project>
     <PropertyGroup>
       <SptInstallPath>C:\path\to\your\SPT</SptInstallPath>
     </PropertyGroup>
   </Project>
   ```
   Without it, the build still works, it just doesn't copy anything.
4. Build:
   - `dotnet build Client -c Debug` builds and deploys with debug symbols
   - `dotnet build Client -c Release` also packages `Client/dist/OpenBarters-<version>.zip`

Both `Client/dependencies` and the `.user` file are gitignored.

There's also a `Source-Debug` configuration that references a decompiled `Assembly-CSharp` project at `../Decompiled-SPT4.1.6/Assembly-CSharp` instead of the dll, so go-to-definition lands in readable source. It's only for browsing, it doesn't deploy.

### Pricing checks

With the .NET 8 SDK or newer, run `dotnet run --project Tests -c Release`. These checks do not require game DLLs. They cover partial and full substitution, exact-item credit, surplus and partial stacks, purchase quantities, currency rounding, and overlapping requirements.

## Contributing
AI generated code will not be merged. Code you submit must be written and understood by you, for two reasons. First, I will not allow the codebase to become convoluted with complicated code no one understands. Second, this repo must stay aligned with the SPT Forge's stance on AI mods.

Using AI tools to assist in research, planning and debugging is expected and acceptable. However, it is also expected that you explicitly disclose the usage of AI in your PR.

Please reference existing code formatting and naming conventions when adding new code. Generally, Microsoft C# conventions, but with same-line (K&R) braces. A ReSharper-exported `.editorconfig` is included under the `Client/` directory.

## Credits
Includes `MultiSelectInterop.cs` from [UI Fixes](https://github.com/tyfon7/UIFixes) by Tyfon (MIT).

## AI Disclosure
This README was written with the help of AI (Claude).

The mod's code was written by me. AI was used as a research and review tool along the way:
- Digging through the decompiled game and SPT server source to find where to hook in, and explaining how those parts of the game work
- Reviewing my code as I wrote it, catching bugs and checking that it builds
- Suggesting small fixes and short example snippets, which I adapted
- Keeping the project's planning notes and docs up to date

Every patch was written, tested in game and debugged by me.
