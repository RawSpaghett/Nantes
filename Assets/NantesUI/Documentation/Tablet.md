# Tablet

## 1. Attach the screen

`GameplayBuilder` makes a square housing from cubes and a flat screen from a quad. It saves them as `ChestScreen.prefab` and builds the pause controls into `NantesGameUI.prefab`.

When the supermarket in `Prototype_Main` loads, `SceneUI` adds a separate mount on the player's chest. Tab unfolds it from the chest and tilts the view down to read it. Stowing restores the view. The mount follows the body when crouching. The original Scanner object stays unchanged.

## 2. Draw the interface

`TabletDisplay` draws the panels, lines, radar rings, and icons from points. `TabletController` places the text over them. A separate camera draws this interface into a RenderTexture, which is an image Unity updates each frame. That image is placed on the square screen with the tablet glass material.

The tablet boots straight into Scanner. It has Scan and Power buttons and a fixed 25-metre range. There is no Home page or range toggle. The food total stays in the top-right. The flashlight icon, charge bar and status sit along the bottom. `SceneUI` attaches the meter to the tablet canvas, so it appears on the device and stays hidden while off or booting.

## 3. Use the alien font

The original Stray font by MaowCraft1282 is kept unchanged. `TabletFont.Prepare` makes a TextMesh Pro font atlas, which stores the shapes Unity needs to draw the letters.

Labels contain real words such as SCANNER, FOOD, and RANGE, rendered in Stray. Numbers and keyboard hints use Nantes Display. Stray also appears above the pause title. The main-menu logo stays in Nantes Display.

## 4. Draw the shrimp

The shrimp is an original line icon in `TabletDisplay`. It uses lists of points for the curled body, pointed head, antennae, legs, and tail. Short lines divide the body into segments.

Each pair of points becomes a thin strip of triangles. The strips scale with the icon and use the same pale teal as the display. Small versions hide the extra segment lines so they stay readable. The downloaded shrimp SVG was a reference only; it is not imported or recolored.

## 5. Boot and fold away

`ChestTablet` uses Tab to raise or stow the device. Its position and angle ease between those two poses. The housing tucks into the chest and its renderers switch off when stowed, so looking straight down shows no panel. Looking around never opens the tablet.

Boot lasts 1.55 seconds. Shutdown lasts 0.75 seconds. Screen brightness and the startup graphics follow those states. The food value stays in memory while the screen is off.

## 6. Use the controls

While raised, the mouse moves the tablet cursor. Stowing restores mouse look. The pointer is projected onto the screen to find which button it touches.

Space scans while the tablet is open. World interaction is held while using the tablet so selecting a button cannot also collect or throw something. Previous input states return afterward. Escape opens the pause menu.

## 7. Read the scanner

Opening Scanner does not scan. Click Scan or press Space to find food within 25 metres. `ChestTablet` reveals those items through walls for eight seconds, with a 0.2-second fade in and a 0.4-second fade out. Scanning again restarts that timer without blinking the outline off. Closing the tablet leaves the reveal running; reopening does not refresh it. Pausing freezes it.

Food and existing `Enemy` objects within 25 metres become radar dots relative to the player. Food dots, the detected count, and red outlines use the same results. Moving out of range, collecting food, or reaching the eight-second limit removes a result. Moving back into range needs another scan. The outward ring is the scan animation; it does not delay individual dots. Enemy senses and behavior are unchanged.

`GameplayWorldLook` registers CocoCereals, ChocolateBar and Tomato as `FoodScanTarget` objects at runtime. Add new prop names to its prefab list, or add `FoodScanTarget` and assign the red material on its `ObjectOutline`. Change `ChestTablet.foodRevealSeconds` to adjust the duration. Disabled or removed food stops showing up.

## 8. Update food

Aim at food within interaction reach and press E. No scan is needed. `FoodPickup` uses the existing `IInteractable` interface, adds one to the tally, then disables the food and its outline. It also consumes that key press so holding E cannot count the same item twice. Saves store each collected item's scene path and the total. Continue keeps collected food hidden; New Game and Restart bring it back.

Small food props have 12 cm of aiming allowance. The pickup check still needs a clear line to the item, so it cannot reach through walls or shelves. Other object interactions use the original ray.

## 9. Make the sounds

The key and scan sounds are generated in Unity from short sine-wave tones. The key falls from 930 to 540 Hz over 0.07 seconds. The scan rises from 290 to 720 Hz over 0.19 seconds. A short volume envelope softens their start and end. No recorded tablet sound is used.

## 10. Pause and change scenes

Escape freezes gameplay and shows the pause controls over a darkened, blurred capture of the camera. The rows support mouse and keyboard selection. Resume restores the previous controls and sound level.

New Game, Restart, and Main Menu use `ScreenTransition`. It fades the picture and audio out, loads the scene while the screen is covered, then fades back in. Reduced Motion shortens those fades. Menu sound stops when entering the level.

[Controls](Gameplay.md) · [Credits and licenses](Asset-credits.md)
