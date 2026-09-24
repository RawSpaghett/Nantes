# Controls

Open Controls from the main menu or Escape menu.

| Key | Action |
| --- | --- |
| WASD | Move |
| Mouse | Look |
| Space | Jump |
| Hold Shift | Sprint |
| Hold Ctrl | Creep / slow walk |
| E | Pick up; press again to throw |
| Hold R | Crank the flashlight |
| C | Crouch (planned) |
| Y | Yell (planned) |
| Escape | Pause / resume; back out of Controls first |
| F11 | Toggle fullscreen during gameplay |

Look down to boot the tablet and look up to stow it. Tab switches between the tablet cursor and mouse look; right-click also releases the cursor. Click a tablet option or look at it and press E. 1 opens Home, 2 opens Scanner, and Space scans on the scanner screen. The range and power buttons work on the tablet itself.

Menus use mouse clicks or arrows and Enter. Escape goes back.

C and Y are listed for crouch and yell. The team still needs to connect those actions.

The main menu fades and slides into Controls. Back glows on hover or keyboard focus in both menus. Reduced Motion skips the slide and fade.

## Flashlight

The existing timer holds 8 seconds of light. Holding R adds 2 seconds of charge per second. It switches on at half charge and runs until empty. Those rules have not changed.

`FlashlightMeter` reads the timer through three read-only properties on `FlashlightScript`. It draws the icon, charge bar and status in the bottom-right. Rounded ends and soft edges keep the icon's rays smooth at different resolutions. The bar's small halfway mark shows the starting threshold. The meter hides while paused or changing scenes.

`FlashlightScript.LateUpdate` scales the lights' original intensity during the last 30% of charge. A small noise variation gives the low beam a flicker. Cranking restores normal brightness; Reduced Motion skips the flicker. No overheating or cooldown was added.

The menu lists are built in `ControlsPanelBuilder`. Run **Tools > Nantes > Update controls and flashlight UI** after changing their text or layout. The player prefab and input bindings are unchanged.
