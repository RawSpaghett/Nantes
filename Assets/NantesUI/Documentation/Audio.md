# Menu sounds

Clips are in [Audio](../Audio):

- `Earth-Flick.wav`: edited DRAGON-STUDIO whoosh, timed to the Earth release.
- `Ship-Bind.wav`: ship contact.
- `Ship-Strain-Loop.wav`: engine strain; pitch and volume follow thrust.
- `Ship-Breakaway.wav`: escape burst.
- `Selector-Whip.wav`: backswing and forward crack.

Master Volume controls all sounds. Reduced Motion pauses encounter audio.

[GenerateEncounterAudio.py](../../../Design/Audio/GenerateEncounterAudio.py) rebuilds the four encounter clips. It needs Python, numpy, imageio-ffmpeg, and the [source MP3](../ThirdParty/DragonStudio/Simple-Whoosh-02.mp3).

[Music details](Sources/Music.md) and [asset credits](Asset-credits.md).
