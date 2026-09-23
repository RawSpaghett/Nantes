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

`GameFlow` connects New Game and Quit. New Game opens `Prototype_Main` through `ScreenTransition`. A save system still needs to connect Continue and call `SetContinueAvailable(true)`.

**Tools > Nantes > Prepare game UI** builds the chest display and pause prefab, connects the menu, and adds both scenes to the build list. `NantesMenuPreview` is removed from the playable scene during this step.

Use an EventSystem with InputSystemUIInputModule. Keep the animated-space root and camera when copying the menu. Layer 29 is used for the ship/title overlap.

The team project uses Unity 6000.3.10f1. The playable UI was checked with URP in 6000.5.10f1. Testing in the team's exact 6000.3.10f1 version is still needed.

[Credits and licenses](Asset-credits.md)
