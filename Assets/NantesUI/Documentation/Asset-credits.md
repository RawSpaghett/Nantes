# Asset credits

## Earth

“Earth” by Akshat (@shooter24994), Sketchfab. CC BY 4.0.

Source: https://sketchfab.com/3d-models/earth-41fc80d85dfd480281f21b74b2de2faa
License: https://creativecommons.org/licenses/by/4.0/

Converted from GLB to OBJ, resized, and subdivided once. Original UVs and texture are kept. Lighting, rotation, atmosphere, and foreground smoke were added in Unity. [File details](Sources/Earth.json).

## Spaceship

Intergalactic Spaceship Version 2 by Dennis Haupt (3DHaupt).

Source: https://free3d.com/3d-model/intergalactic-spaceships-version-2-blender-292-eevee-359585.html

The creator allows non-commercial, personal/private, and editorial use. Used here for the class prototype. Details are in [the spaceship source note](Sources/Spaceship.md).

Converted the hull to OBJ and centered it. Original textures are kept. The menu adds its own lighting, flight, and exhaust.

## Music

“Universe - Space Sounds” by JuliusH, Pixabay Content License.

Source: https://pixabay.com/music/ambient-universe-space-sounds-3595/
License: https://pixabay.com/service/license-summary/

Repeated into a 25:13 background loop. See [the music source note](Sources/Music.md).

## Earth whoosh

“Simple Whoosh 02” by DRAGON-STUDIO, Pixabay Content License.

Source: https://pixabay.com/sound-effects/film-special-effects-simple-whoosh-02-433006/
License: https://pixabay.com/service/license-summary/

Edited and retimed for the Earth spin. The [source MP3](../ThirdParty/DragonStudio/Simple-Whoosh-02.mp3) is kept with the assets. See [the whoosh source note](Sources/Earth-whoosh.md).

## Menu effects

Ship binding, strain, escape, and selector whip sounds were made for Nantes from generated noise and tones. The sound generator is in [GenerateEncounterAudio.py](../../../Design/Audio/GenerateEncounterAudio.py). [How the sounds were made](Audio.md).

## Font

Nantes Display is a modified Michroma font with an open uppercase A. Copyright 2011 The Michroma Project Authors. SIL Open Font License 1.1.

Source: https://github.com/google/fonts/tree/main/ofl/michroma
License: [SIL Open Font License](Licenses/OFL.txt)

The TextMesh Pro shaders come from Unity uGUI.

Alien lettering on the tablet and pause screen uses **Stray** by **MaowCraft1282**, CC BY-ND 3.0. The original font is unchanged.
Source: https://fontstruct.com/fontstructions/show/2147152
[License](Stray/license.txt) · [Font readme](Stray/readme.txt)

## Tablet icons and sounds

The shrimp, scanner, power, and flashlight icons are drawn in code for Nantes. The shrimp uses an original outline; no downloaded shrimp SVG is included. Tablet key and scan sounds are short tones generated in Unity. [Tablet setup and drawing steps](Tablet.md).

## World sky

Kloppenheim 07 (Pure Sky) by Greg Zaal, with sky edits by Jarod Guest. Poly Haven, CC0.

Source: https://polyhaven.com/a/kloppenheim_07_puresky
License: https://polyhaven.com/license

Used in the world preview with reduced saturation, a gray-green tint, horizon haze, and drifting clouds. [Setup](World-look.md).

## Yell

Voice Clip Pack - Male Adventurer RPG by Brandon Song (wolfwoot), CC0.

Source: https://opengameart.org/content/voice-clip-pack-male-adventurer-rpg

License: https://creativecommons.org/publicdomain/zero/1.0/

`attackbig0.wav` and `attackbig2.wav` were trimmed, slowed to 90% speed, filtered, and given a faint room reflection. [Sound setup](Audio.md).

## Cranklight

The crank loop, bulb buzz, ready click and wind-down were made for Nantes from tones and noise. No downloaded recordings are used for these four sounds. [GenerateGameplayAudio.py](../../../Design/Audio/GenerateGameplayAudio.py) makes the WAVs. [Sound setup](Audio.md).

## Supermarket surfaces

Asphalt012, Concrete034, and Tiles074 by ambientCG, CC0.

- Outside ground: [Asphalt012](https://ambientcg.com/view?id=Asphalt012)
- Walls and roof: [Concrete034](https://ambientcg.com/view?id=Concrete034)
- Worn checkerboard floor: [Tiles074](https://ambientcg.com/view?id=Tiles074)
- License: https://docs.ambientcg.com/license/

Uses their 1K color, normal, and roughness maps. Unity desaturates the tiles and tints the surfaces. The walls get a worn green paint band. The ceiling and light housings are simple Unity shapes.

## Footsteps

Impact Sounds by Kenney, CC0.

Source: https://kenney.nl/assets/impact-sounds

Uses `footstep_concrete_000.ogg` through `004.ogg`, unchanged. Volume, pitch, and step timing change in Unity. [Pack license](Licenses/Kenney-Impact-Sounds.txt).
