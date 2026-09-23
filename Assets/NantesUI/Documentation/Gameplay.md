# Game UI

New Game opens the team's `CreatureDebug` scene and starts a fresh save. Escape opens Resume, Save Game, Restart, Main Menu, and Quit. Escape again resumes. Restart starts the level over and replaces the save.

Save Game keeps your position, facing direction, flashlight charge, tablet food count and range, and the positions of throwable objects and creatures. A held object stays held. Creature behavior starts again when the scene loads; it does not save their current thoughts or chase paths.

Main Menu and Quit save before leaving. If saving fails, the menu stays open and shows the problem. Continue loads the save, including after closing and reopening the game. It stays disabled when there is no usable save.

The file is `save-game.json` in Unity's `Application.persistentDataPath`. The previous good save is kept as `.bak`. A temporary file is written first, then replaces the save. If the main file is damaged, Continue tries the backup.

Entering and leaving the level fades the picture and audio. The main-menu logo uses Nantes Display with occasional glitches. Reduced Motion keeps it still.

Look down to extend and boot the square scanner on the player. Look up to shut it down and retract it. Tab unlocks the cursor to use the screen; Tab or right-click returns to looking around. 1 opens Home, 2 opens Scanner, and Space sends a scan while using the scanner.

The food counter starts at zero and can be updated with `TabletController.SetFoodCount`. Food collection still needs a gameplay connection. The scanner reads nearby creatures already present in the scene. An empty scene produces an empty scan.

Tablet labels are real words rendered with the Stray font. Numbers and keyboard hints use the regular UI font. The shrimp counter stays in the top-right.

`SceneUI` attaches the UI when the level loads. It does not change the team's scene, player prefab, or gameplay scripts. The existing movement, flashlight, and creature behavior belong to those systems.

`LevelSave` reads and restores their existing fields. `link.xml` keeps the private fields available in builds. If those field names change, update the bindings in `LevelSave`. Object IDs use their starting scene paths, before picking them up changes their parents. Major level changes may need a new save version.
