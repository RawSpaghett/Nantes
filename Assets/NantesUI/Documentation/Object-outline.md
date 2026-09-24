# Object outline

The red outline is separate from the object's existing material. It shows through walls and shelves. In the supermarket, `GameplayWorldLook` adds it to CocoCereals, ChocolateBar and Tomato when the scene loads. Creatures, shelves and other objects are not highlighted.

The food names are listed on `NantesWorld/Resources/GameplayWorldLook.prefab`. Update that list when adding more food. Pickup and scanner behavior are unchanged.

1. Add `ObjectOutline` to an object.
2. Assign `NantesWorld/Materials/Red outline` to its Material field.
3. Call `outline.SetHighlighted(true)` to show it, or `false` to hide it.
4. Adjust Width and Color on the component. Width is measured in screen pixels.

The red border stays visible through walls and shelves. The middle stays clear so the wall is still visible inside the border. The original material, colliders, and scripts stay in place.

It masks the object's shape, draws an expanded edge without checking wall depth, then clears the mask. It uses stencil bit 0 during those passes. Its mesh copies skip dynamic occlusion culling so a wall cannot hide the outline.

Readable meshes get averaged normals on the copy so hard corners stay joined. For an imported mesh with gaps at its corners, enable Read/Write on that mesh. Closed static meshes and skinned meshes are supported; thin planes and transparent surfaces need a different treatment. Avoid adding the component on both a parent and its child.

No renderer feature or project settings change is needed. In the preview, 3 shows the food directly and 4 shows it behind a shelf. O switches the outline on and off. The teammate can connect the public method to their own code.
