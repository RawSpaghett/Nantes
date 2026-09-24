# Controls

Open Controls from the main menu or Escape menu.

| Key | Action |
| --- | --- |
| WASD | Move |
| Mouse | Look |
| Space | Scan while using the tablet |
| Hold Shift | Sprint |
| Hold Ctrl | Creep / slow walk |
| E | Collect food; pick up or throw other objects |
| Tab | Raise / stow tablet |
| Hold R | Crank the flashlight |
| Hold C | Crouch |
| Y | Yell |
| Escape | Pause / resume; back out of Controls first |
| F11 | Toggle fullscreen during gameplay |

Tab raises the tablet and glances down at Scanner. Tab again stows it completely and restores the view. While open, the mouse controls its cursor. Click Scan or press Space to scan; opening it does not scan automatically. Range stays at 25 metres. Looking down does not open it. E can collect food without scanning it first.

Menus use mouse clicks or arrows and Enter. Escape goes back.

There are two loose boxes near the supermarket entrance. Aim at one and press E to pick it up, then release and press E again to throw. When holding a box, E throws it before checking other items. Pause and the open tablet block pickup and throwing.

Footsteps get faster and louder with Shift. Normal walking uses a steady pace, Ctrl is slower and quieter, and C is almost silent. Steps stop when you stop moving.

Crouch and yell are now connected in the team's player controls. Jump is currently disabled in `PlayerInputHandler`.

Y sends one noise event at loudness 10 through `OnPlayerSound`, plays a short shout, and waits 2.5 seconds before another shout. Enemy `Ears` still use the same hearing event. C uses the team's smoothed crouch: the body changes height at a rate of 3, down to 1.5. Releasing C stands up. Crouch and Ctrl slow walking take priority over Shift. Jump remains disabled.

The main menu fades and slides into Controls. Back glows on hover or keyboard focus in both menus. Reduced Motion skips the slide and fade.

## Flashlight

The timer holds 8 seconds of light. Holding R adds 2 seconds of charge per second. The bulb flickers softly while charging, becomes steady at half charge, and reaches full brightness at full charge. Releasing R below half charge leaves the bulb off; after reaching ready, it runs until empty.

`FlashlightMeter` reads the timer through three read-only properties on `FlashlightScript`. It draws the icon, charge bar and status along the scanner's bottom edge. Rounded ends and soft edges keep the rays smooth at different resolutions. The halfway mark shows the starting threshold. The meter hides when the tablet is off or booting, while paused, and during scene changes. Nothing is drawn on the player's screen.

`FlashlightScript.LateUpdate` scales the existing lights with their charge. Reduced Motion skips startup and low-charge flicker. MAX marks full charge. The gear loop plays while cranking, a click marks ready, and the bulb buzz grows with charge. Releasing the crank plays a short winding-down sound. No overheating was added.

The menu lists are built in `ControlsPanelBuilder`. Run **Tools > Nantes > Update controls and flashlight UI** after changing their text or layout. Input bindings are unchanged. The player prefab uses the team's `crouchHeight: 1.5` and `crouchSpeed: 3` settings.
