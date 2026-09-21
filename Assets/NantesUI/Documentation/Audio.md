# Menu sounds

## Background music

I used [Universe - Space Sounds by JuliusH](https://pixabay.com/music/ambient-universe-space-sounds-3595/) as the source and repeated it into a 25-minute background track. The finished OGG runs 25:13. In Unity, I set it to stream, fade in over three seconds, and loop when it reaches the end.

## Earth spin

I edited [Simple Whoosh 02 by DRAGON-STUDIO](https://pixabay.com/sound-effects/film-special-effects-simple-whoosh-02-433006/). I stretched the roughly four-second source into a 7.3-second effect and moved its loudest part to 1.9 seconds into the clip. The sound starts 1.8 seconds into the encounter, putting the peak at the planet's release at 3.7 seconds. I added a short fade-in and a 0.65-second fade-out for the tail.

File: [Earth-Flick.wav](../Audio/Earth-Flick.wav).

## Ship binding

I built this from generated noise and a low tone using Python and NumPy. I filtered the noise to shape a short, airy swish, then layered a pitch-dropping bass thud with rougher low noise for the contact. The swish peaks at 0.12 seconds and the impact starts at 0.17 seconds. Quick fades stop the clip from clicking at either end.

The finished sound is 0.95 seconds long. It starts 0.45 seconds into the encounter, so the thud lands at 0.62 seconds as the tentacles catch the hull.

File: [Ship-Bind.wav](../Audio/Ship-Bind.wav).

## Ship struggle

I combined filtered noise with two low tones at 94 Hz and 188 Hz. The left and right channels use different noise so the sound has some width. I added small pitch wobbles and two overlapping volume pulses to make the engines sound uneven and under pressure.

I made it a four-second loop. In Unity, its pitch and volume follow engine effort: harder thrust makes it louder and higher, including when the ship is trapped and barely moving. It fades as the grip releases.

File: [Ship-Strain-Loop.wav](../Audio/Ship-Strain-Loop.wav).

## Ship escape

I layered a broad burst of filtered noise with a tone that drops from about 560 Hz toward 210 Hz. It rises to full volume in 0.025 seconds, then decays with a short fade at the end. I delayed the right channel by about seven milliseconds and made it slightly quieter to spread the sound out.

The finished burst is 2.3 seconds long and plays when the ship breaks free.

File: [Ship-Breakaway.wav](../Audio/Ship-Breakaway.wav).

## Selector whip

I started with white noise and filtered it into an airy swish. I let more high frequencies through near the strike, added a very short noise crack, and layered a low, falling tone underneath it. A half-millisecond delay on the right channel adds a little width.

I then added a softer swish for the pullback and shifted the forward strike 0.39 seconds later. The final clip lasts 0.94 seconds: the pullback swish peaks around 0.22 seconds, and the crack lands at 0.70 seconds when the tip hits the selected word. Short fades keep the start and end clean.

File: [Selector-Whip.wav](../Audio/Selector-Whip.wav).

All five effects are stereo, 48 kHz, 16-bit WAVs. [GenerateEncounterAudio.py](../../../Design/Audio/GenerateEncounterAudio.py) recreates them using Python, NumPy, imageio-ffmpeg, and the [source whoosh MP3](../ThirdParty/DragonStudio/Simple-Whoosh-02.mp3). The ship and selector effects use generated noise and tones rather than recordings.

Master Volume controls music and effects together. Reduced Motion pauses encounter audio. [Asset credits](Asset-credits.md).
