# World look

The supermarket uses an overcast sky, haze, worn surfaces, a roof, and ceiling lights. Its layout and existing props stay in place. These additions load at runtime; the original scene file stays unchanged.

The sky uses Kloppenheim 07 (Pure Sky), a cloud panorama by Greg Zaal with sky edits by Jarod Guest. It is desaturated and tinted gray-green, with a dusty horizon and slowly drifting clouds. The cloud animation only changes the sky; it does not project shadows inside the store. Source: https://polyhaven.com/a/kloppenheim_07_puresky — CC0: https://polyhaven.com/license

The atmosphere prefab adds distance haze, softer daylight, ambient light, and light color grading. No extra buildings or parking areas are added.

1. The builder copies `Prototype_Main` into `NantesWorld/Preview/WorldLook`.
2. It disables gameplay in that copy for a free-moving preview camera.
3. It adds the atmosphere and an outline to the existing CocoCereals box.
4. It builds a separate preview with a free-moving camera.

The playable game loads `Prototype_Main` from New Game. `GameplayWorldLook` adds the same atmosphere when that scene loads, disables its old directional light and enables camera color grading. It also registers CocoCereals, ChocolateBar and Tomato for scanning and collection. Their red outlines fade in on a scan and fade out at the end of eight seconds. E collects them through the existing interaction system, with or without a scan.

`SupermarketDetails` replaces the outside plane, floor and wall materials at runtime. The shelves and their textures are kept. It adds a solid roof meeting the walls, beams, and rows of ceiling fixtures. Six lights are steady, six are weak and damaged, and the rest are dead. Each working light casts shadows. A little ambient fill keeps the unlit aisles readable, with the flashlight still needed to see details.

The steady fixtures use intensity 1.25; damaged ones use 0.65. `DamagedFixture` adds a slight unevenness to the light and a brief dip every 11–16 seconds, staggered between lamps. The tube dims with its light. Pause freezes the effect, and Reduced Motion keeps the damaged lamps steady. Ambient scale is 0.20 and stays constant as the camera moves.

The surfaces use ambientCG's Asphalt012, Concrete034 and Tiles074, all CC0. Tiles074 is the worn black-and-white checkerboard. Color maps provide the texture, normal maps add small bumps, and roughness maps control reflections. `WorldSurface.shader` projects them in world space so large meshes do not stretch them. The floor tiles are desaturated; the lower wall gets a green paint band.

The surface shader supports the project's Forward+ lights, including the flashlight and ceiling spots. Its shadow and depth passes are included directly. The roof uses small overlapping sections; a single store-sized mesh lost its shadow when viewed from inside. A temporary copy of the render settings keeps shadows active for 130 metres across the whole store. Leaving the level restores the previous settings.

The flashlight lights the floor and walls as well as the shelves. Its light also casts shadows, so the beam does not shine through them.

Run **Tools > Nantes > Prepare supermarket details** after changing the surface maps. It imports the maps and updates the six materials in `NantesWorld/Resources/WorldSurfaces`. The roof and fixture positions are in `SupermarketDetails`.

The flat dev test scene gets none of these effects. The supermarket scene file is unchanged. Run **Tools > Nantes > Update gameplay world look** to rebuild the setup and refresh the playable scene list.

Controls: 1 exterior, 2 maze, 3 food outline, 4 outline through a shelf. Tab compares the original lighting. O toggles the outline in views 3 and 4. Right-click to look, WASD to move, Q/E down/up, Shift faster. F toggles a preview flashlight. H hides the help. Escape releases the mouse; press again to exit.

For a scene test, add `World atmosphere.prefab` to a copy of the level and enable its root. Disable the scene's old directional light while testing. Disabling the prefab restores the previous sky, fog, and ambient settings. Its camera needs URP post-processing enabled for the color grade.

Build with Unity 6000.3.10f1: Tools > Nantes > Build world look preview. The [outline notes](Object-outline.md) cover how to connect the shader to gameplay.
