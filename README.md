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
- Your items are valued at what the trader would pay you for them, the same price you see on the sell tab (condition and loyalty level included).

Since traders never pay full value, bartering with substitutes always costs a bit more than bringing the exact items. Lore-wise, think of it as the trader marking up the deal for not getting what they actually wanted.

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

## Credits
Includes `MultiSelectInterop.cs` from [UI Fixes](https://github.com/tyfon7/UIFixes) by Tyfon (MIT).
