# Game UI

New Game opens the supermarket in `Prototype_Main` at its existing PlayerHolder spawn and starts a fresh save. Escape opens Resume, Save Game, Controls, Restart, Main Menu, and Quit. Escape again resumes. Restart starts the level over and replaces the save.

The supermarket uses the overcast sky and atmosphere. Tablet scans reveal food through walls for eight seconds. The dev test scene is unchanged.

Gameplay has no control prompts or crosshair. The flashlight meter is inside the tablet scanner. Controls are listed in the main menu and Escape menu; Escape backs out of Controls before resuming.

The flashlight icon is dim grey when off and white while cranking. Its rays and bar fill with charge. The bulb flickers during startup, steadies at READY, and reaches full brightness at MAX. Light and buzz fade as charge drains. Reduced Motion disables the flicker. There is no overheating system.

Save Game keeps your position, facing direction, flashlight charge, collected food, tablet tally, and the positions of throwable objects and creatures. A held object stays held. Creature behavior starts again when the scene loads; it does not save their current thoughts or chase paths.

Main Menu and Quit save before leaving. If saving fails, the menu stays open and shows the problem. Continue loads the save, including after closing and reopening the game. It stays disabled when there is no usable save.

Saves must match the supermarket scene. Old dev-scene saves are not loaded into it.

The file is `save-game.json` in Unity's `Application.persistentDataPath`. The previous good save is kept as `.bak`. A temporary file is written first, then replaces the save. If the main file is damaged, Continue tries the backup.

Entering and leaving the level fades the picture and audio. The main-menu logo uses Nantes Display with occasional glitches. Reduced Motion keeps it still.

Tab raises the tablet and opens Scanner. Tab again stows it. Use the cursor to select Scan or press Space. Range stays at 25 metres. Looking down does not open the tablet.

E collects the food you are aiming at, removes it from the world and radar, and adds one to the tablet tally. No scan is needed to collect food. Saves keep collected food gone. The scanner shows food and creatures within 25 metres. Click Scan or press Space to scan; opening the tablet does not scan. Food dots, the detected count and outlines use the same results for eight seconds, or until collected or out of range.

Tablet labels are real words rendered with the Stray font. Numbers and keyboard hints use the regular UI font. The shrimp counter stays in the top-right.

`SceneUI` attaches the UI and player sounds when the level loads. Crouch uses the team's smoothed code and its matching player prefab settings. Other small edits gate yell presses and cooldown and scale flashlight brightness. Creature behavior is unchanged.

`LevelSave` reads and restores their existing fields. `link.xml` keeps the private fields available in builds. If those field names change, update the bindings in `LevelSave`. Object IDs use their starting scene paths, before picking them up changes their parents. Major level changes may need a new save version.

## Throwables

`Prototype_Main` had no objects with `ThrowableObjects` attached. The existing boxes were static scenery. Two loose boxes now sit near the entrance; the maze boxes and layout are unchanged. Both use the existing Grocery Store Pack Lite Box prefab, with changes saved only on those scene instances.

Each loose box uses the Throwable layer, a convex Mesh Collider, a Rigidbody, and `ThrowableObjects`. Static flags are cleared. The body starts kinematic so it stays put until used. Keep Rigidbody interpolation set to None so parenting it to the hand does not fight the physics pose. Its hold position points to the player's `ObjectHolder`, and its input reference points to `PlayerInputHandler`. Pickup also finds these references from the interacting player, so newly placed copies can work without manual wiring. Keep the scene references assigned for restoring a held object from a save.

`PlayerInteractor` checks for a held box before handling another interaction. `ThrowableObjects.Throw()` consumes that E press, detaches the box, restores world collisions and applies the existing impulse. It ignores the thrower's collider so the box does not hit the player on release. This prevents a throw and another pickup sharing one press. Impact noise still uses `OnLand` at loudness 5. Saves keep each box's position and held state; Restart returns both to their starting spots.

Checked in a Windows build: pickup, turning while holding, throwing, picking up again, food collection without scanning, pause and tablet blocking, saving a held box, Continue, and Restart. Crank and buzz volumes were checked at full charge and while draining. No runtime errors were reported.
