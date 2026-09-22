# Menu setup

Open [NantesMenuPreview.unity](../Scenes/NantesMenuPreview.unity) and press Play. You can also use **Tools > Nantes UI > Open menu preview**.

Use the mouse or arrows and Enter. Escape goes back. Keyboard focus stays selected until the mouse moves.

Settings has Master Volume, Fullscreen, and Reduced Motion. Continue stays disabled until a save is available. The selector wraps each word, pulls back, then whips it before opening the next page.

The ship grab and Earth spin each have a 10% chance per eligible check. They cannot overlap. The selector works separately from both events.

## Editing

Edit [NantesMenu.prefab](../Prefabs/NantesMenu.prefab) for layout and controls. [NantesTheme.asset](../NantesTheme.asset) holds the UI colors.

The editable logo files are in [Design/Logo](../../../Design/Logo). See [logo editing](Logo-editing.md) for exporting them to Unity.

**Tools > Nantes UI > Apply horror art direction** resets the menu to the preset layout and effects. Edit the prefab directly to keep your own changes.

## Gameplay

Menu scripts use `NantesGame.UI`; editor tools use `NantesGame.UI.Editor`. These names avoid the monster's `Nantes` class.

Connect `newGameRequested`, `continueRequested`, and `quitRequested` to the game's scene and save flow. Call `SetContinueAvailable(true)` when a save exists.

`NantesMenuPreview` handles the standalone preview. New Game currently shows a notice. Replace that component when hooking up gameplay.

Use an EventSystem with InputSystemUIInputModule. Keep the animated-space root and camera when copying the menu. Layer 29 is used for the ship/title overlap.

The team project uses Unity 6000.3.10f1. The standalone was checked in 6000.5.10f1; the team's URP setup still needs testing.

[Credits and licenses](Asset-credits.md)
