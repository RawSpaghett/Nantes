# Game UI

New Game opens the team's `Prototype_Main` scene. Escape opens Resume, Restart, Main Menu, and Quit. Restart reloads the scene.

Entering and leaving the level fades the picture and audio. The main-menu logo uses Nantes Display with occasional glitches. Reduced Motion keeps it still.

Look down to extend and boot the square scanner on the player. Look up to shut it down and retract it. Tab unlocks the cursor to use the screen; Tab or right-click returns to looking around. 1 opens Home, 2 opens Scanner, and Space sends a scan while using the scanner.

The food counter starts at zero and can be updated with `TabletController.SetFoodCount`. Food collection still needs a gameplay connection. The scanner reads nearby creatures already present in the scene. An empty scene produces an empty scan.

Tablet labels are real words rendered with the Stray font. Numbers and keyboard hints use the regular UI font. The shrimp counter stays in the top-right.

`SceneUI` attaches the UI when the level loads. It does not change the team's scene, player prefab, or gameplay scripts. The existing movement, flashlight, and creature behavior belong to those systems.
