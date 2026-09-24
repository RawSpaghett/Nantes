# World look

The supermarket uses the overcast sky, haze and color grade. Its layout, props and original materials are unchanged.

The sky uses Kloppenheim 07 (Pure Sky), a cloud panorama by Greg Zaal with sky edits by Jarod Guest. It is desaturated and tinted gray-green, with a dusty horizon and a slow moving cloud shadow. Source: https://polyhaven.com/a/kloppenheim_07_puresky — CC0: https://polyhaven.com/license

The atmosphere prefab adds distance haze, softer daylight, ambient light, and light color grading. It has no buildings, parking markings, or extra props.

1. The builder copies `Prototype_Main` into `NantesWorld/Preview/WorldLook`.
2. It disables gameplay in that copy for a free-moving preview camera.
3. It adds the atmosphere and an outline to the existing CocoCereals box.
4. It builds a separate preview with a free-moving camera.

The playable game loads `Prototype_Main` from New Game. `GameplayWorldLook` adds the same atmosphere when that scene loads, disables its old directional light and enables camera color grading. It also adds red outlines to CocoCereals, ChocolateBar and Tomato. These food props do not need pickup scripts for the outline.

The flat dev test scene gets none of these effects. The supermarket scene file and player prefab are unchanged. Run **Tools > Nantes > Update gameplay world look** to rebuild the setup and refresh the playable scene list.

Controls: 1 exterior, 2 maze, 3 food outline, 4 outline through a shelf. Tab compares the original lighting. O toggles the outline in views 3 and 4. Right-click to look, WASD to move, Q/E down/up, Shift faster. F toggles a preview flashlight. H hides the help. Escape releases the mouse; press again to exit.

For a scene test, add `World atmosphere.prefab` to a copy of the level and enable its root. Disable the scene's old directional light while testing. Disabling the prefab restores the previous sky, fog, and ambient settings. Its camera needs URP post-processing enabled for the color grade.

Build with Unity 6000.3.10f1: Tools > Nantes > Build world look preview. The [outline notes](Object-outline.md) cover how to connect the shader to gameplay.
