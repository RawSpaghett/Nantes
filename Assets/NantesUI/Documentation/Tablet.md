# Tablet

## 1. Attach the screen

`GameplayBuilder` makes a square housing from cubes and a flat screen from a quad. It saves them as `ChestScreen.prefab` and builds the pause controls into `NantesGameUI.prefab`.

When `Prototype_Main` loads, `SceneUI` finds the player's existing `Scanner` child and attaches the display there. It adds the UI controls at runtime. The team's player prefab, level, and movement scripts stay unchanged.

## 2. Draw the interface

`TabletDisplay` draws the panels, lines, radar rings, and icons from points. `TabletController` places the text over them. A separate camera draws this interface into a RenderTexture, which is an image Unity updates each frame. That image is placed on the square screen with the tablet glass material.

The screen has Home, Scanner, and Power controls. Home opens the scanner. Scanner has a range toggle and scan button. The food total stays in the top-right.

## 3. Use the alien font

The original Stray font by MaowCraft1282 is kept unchanged. `TabletFont.Prepare` makes a TextMesh Pro font atlas, which stores the shapes Unity needs to draw the letters.

Labels contain real words such as HOME, SCANNER, FOOD, and RANGE, rendered in Stray. Numbers and keyboard hints use Nantes Display. Stray also appears above the pause title. The main-menu logo stays in Nantes Display.

## 4. Draw the shrimp

The shrimp is an original line icon in `TabletDisplay`. It uses lists of points for the curled body, pointed head, antennae, legs, and tail. Short lines divide the body into segments.

Each pair of points becomes a thin strip of triangles. The strips scale with the icon and use the same pale teal as the display. Small versions hide the extra segment lines so they stay readable. The downloaded shrimp SVG was a reference only; it is not imported or recolored.

## 5. Boot and fold away

`ChestTablet` checks the direction of the player's camera. Looking down for 0.2 seconds starts the boot and moves the mount forward. Looking away for 0.35 seconds starts shutdown and folds it back. Different thresholds stop the screen from switching on and off at the edge of the viewing angle.

Boot lasts 1.55 seconds. Shutdown lasts 0.75 seconds. Screen brightness and the startup graphics follow those states. The food value stays in memory while the screen is off.

## 6. Use the controls

Looking down leaves mouse look active. Tab unlocks the pointer for the screen; Tab or right-click returns to mouse look. The pointer is projected onto the screen to find which control it touches.

1 opens Home, 2 opens Scanner, and Space scans while Scanner is open. Jump and Interact are held while using the tablet so a scan does not also jump or use a world object. Their previous states return afterward.

## 7. Read the scanner

Opening Scanner sends a scan. `ChestTablet` gathers existing `Enemy` objects within the selected range and turns their world positions into positions relative to the player. These positions become dots on the radar.

The outward pulse reveals nearby dots first. Contacts update while the scanner is open. The range button switches between 25 and 50 metres. This is a position display, not a change to enemy senses or behavior. It does not test walls or detect food pickups yet.

## 8. Update food

The total starts at zero. `SetFoodCount` sets it and `AddFood` increases it, with a limit of 999. The gameplay pickup code still needs to call one of these methods. Restart resets the total. No inventory or save system is added by the UI.

## 9. Make the sounds

The key and scan sounds are generated in Unity from short sine-wave tones. The key falls from 930 to 540 Hz over 0.07 seconds. The scan rises from 290 to 720 Hz over 0.19 seconds. A short volume envelope softens their start and end. No recorded tablet sound is used.

## 10. Pause and change scenes

Escape freezes gameplay and shows the pause controls over a darkened, blurred capture of the camera. The rows support mouse and keyboard selection. Resume restores the previous controls and sound level.

New Game, Restart, and Main Menu use `ScreenTransition`. It fades the picture and audio out, loads the scene while the screen is covered, then fades back in. Reduced Motion shortens those fades. Menu sound stops when entering the level.

[Controls](Gameplay.md) · [Credits and licenses](Asset-credits.md)
