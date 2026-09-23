# How the Nantes menu was made

I handled the menu's look, layout, and interactions. It combines credited models and audio with movement and effects built in Unity. I kept adjusting the previews until the lighting, animation, and controls worked together.

## 1. Earth

The model is Earth by Akshat on Sketchfab, using the 8K version. It was converted for Unity and smoothed so its outline holds up close to the camera.

Slow rotation, directional lighting, and a separate atmospheric layer create the moving planet and thin blue edge. The clouds on Earth are part of its surface image. The drifting smoke in front is a separate effect.

## 2. Spaceship

The ship is Intergalactic Spaceship Version 2 by Dennis Haupt, also known as 3DHaupt. Its model and original textures were imported into Unity.

A flight script moves it along curved routes with changes in height and distance. It calculates acceleration and speed, limits how quickly the ship turns, and points its nose toward its movement. Some routes take it off screen.

## 3. Thrusters and lights

The exhaust uses shaped 3D surfaces and a shader. A shader controls how a surface looks when Unity draws it.

Engine effort changes the exhaust's length, width, brightness, and movement. Low thrust gives a faint blue plume. High thrust creates a longer cyan-blue plume with a nearly white center. The engines also cast blue light onto nearby surfaces.

The effect follows thrust, so a fast-moving ship can coast with very little exhaust. Smaller jets respond to braking and turning.

## 4. Background tentacles

The three tentacles are built in code from rings of points connected into tapered tubes. Moving a curve through each tube bends it like a flexible hose.

Slow waves and uneven movement make the tips curl and search independently. Their bases stay planted, and their tips reach toward the ship when it passes nearby.

## 5. Selector tentacle

A separate fourth tentacle handles button selection. It measures the visible letters, so the wrap fits each word. QUIT gets a smaller loop than NEW GAME.

It comes from the left, passes over the word, curls around the right, and returns underneath. It keeps flowing while selected and takes different curved approaches between options. Keyboard selection takes priority until the mouse moves again.

## 6. Selection whip

Clicking opens the loop, pulls the tip left, sends a bend forward like a rope, strikes the word, and recoils.

The pullback takes about 0.36 seconds. The strike lands around 0.70 seconds, and the option opens around 0.94 seconds. That delay keeps the page visible through the strike and the start of the recoil. The crack sound lands with the strike, and extra clicks cannot trigger duplicate actions during it.

## 7. Ship grab and escape

One script coordinates the ship, tentacles, engines, and sound. Two tentacles snap onto the hull and wrap around it.

The grip acts like a flexible spring while the ship pushes against it. Uneven turns and engine surges create the struggle. As thrust builds, the grip releases, the coils peel away, and the ship carries its escape speed into normal flight. Contact, strain, and escape sounds follow those stages.

## 8. Earth grab and spin

The tentacles wrap around Earth's right side, tighten, and pull across its surface. Earth accelerates during the pull, keeps spinning after release, and gradually slows down.

A shader blurs the surface in the direction of rotation. The outline, tentacles, and buttons stay sharp. The whoosh peaks when the tentacles release, and the planet keeps its new rotation instead of jumping back.

Each eligible check has a 10% ship-grab chance, 10% Earth-spin chance, and 80% chance of neither. A waiting period and a single active event prevent overlap. The selector stays independent.

## 9. Logo and font

The lettering uses Nantes Display, a modified Michroma font with an open uppercase A and wide spacing. [Nantes-Logo-Original.svg](../../../Design/Logo/Nantes-Logo-Original.svg) has editable letter shapes. [Nantes-Logo-Original.psd](../../../Design/Logo/Nantes-Logo-Original.psd) has six pixel layers, one per letter. Unity uses [NantesLogo.png](../Logo/NantesLogo.png).

A shader briefly shifts sections of the title to create stuttering glitches. The glitches build in intensity during a repeating cycle, with quiet gaps so the name stays readable. The source artwork stays clean.

## 10. Stars

Code places 720 small stars across three distances. Their size, brightness, color, and twinkle timing vary.

Blue, white, amber, muted red, and teal stars break up the background. Their different distances work with slight camera movement to give the scene depth.

## 11. Smoke and haze

Transparent surfaces use moving, irregular patterns to create drifting smoke. Several layers overlap at different depths, including wisps in front of Earth.

Different speeds and directions keep the haze moving naturally and help separate the foreground from the distant background.

## 12. Lighting and layering

Earth, the ship, and the tentacles use a shared light direction. Cool highlights, dark shadows, softened bright areas, and slightly darkened screen edges set the mood.

A separate camera draws the ship's silhouette. That shape hides matching parts of the title on selected passes, making the ship appear in front. The buttons stay above the ship and remain readable.

## 13. Music and sound

| Sound | How I made it |
| --- | --- |
| Background music | I repeated Universe - Space Sounds by JuliusH into a 25:13 track, then set it to fade in and loop in Unity. |
| Earth whoosh | I stretched and retimed Simple Whoosh 02 by DRAGON-STUDIO so its loudest part matches the release. |
| Ship binding | I layered a short noise swish, a falling bass tone, and rough low noise for the impact. |
| Ship struggle | I combined filtered noise with two low, wobbling tones in a four-second loop. Its pitch and volume follow engine effort. |
| Ship escape | I mixed a noisy burst with a falling tone, added a slight stereo delay, and timed it to the grip breaking. |
| Selector whip | I combined a filtered swish, a short noise crack, and a low tone. I added a softer pullback swish and timed the crack to the strike. |

The ship and selector sounds were generated using Python and NumPy. [The sound notes](Audio.md) explain the layers, edits, and timing for each one. Master Volume controls music and effects together.

## 14. Buttons and settings

The menu uses Unity's UI controls and TextMesh Pro for lettering. The tentacle provides selection feedback while the buttons handle input.

The volume slider accepts dragging across its full handle height. Reduced Motion stops the ongoing animation and makes selection immediate. Volume and reduced-motion preferences are saved.

Continue stays disabled until a save is available. New Game fades into the team's Prototype_Main scene. Escape opens the pause menu; Restart and Main Menu use the same fade. Navigation, different screen sizes, sound timing, and both events were checked in the standalone preview.

[Menu setup](Menu-setup.md) · [Tablet setup](Tablet.md) · [Asset credits](Asset-credits.md)
