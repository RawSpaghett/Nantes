# Sounds

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

## Yell

The two shouts come from Brandon Song's CC0 [Male Adventurer RPG pack](https://opengameart.org/content/voice-clip-pack-male-adventurer-rpg): `attackbig0.wav` and `attackbig2.wav`. The edits trim silence, play at 90% speed for a lower voice, filter to 110–6500 Hz, and add a quiet reflection 65 milliseconds later. Short fades prevent clicks. The results are mono 44.1 kHz WAVs.

`PlayerSoundEffects` alternates the clips when `PlayerMovement.Yelled` fires. Y sends the existing loudness-10 hearing event once and has a 2.5-second cooldown. The audio does not send another enemy-hearing event.

## Cranklight

The crank loop combines filtered noise, 14 gear pulses per second, and tones at 112 and 224 Hz. The bulb buzz combines 100, 200, and 600 Hz tones with a little high noise. The ready click is a short noise hit with a 380 Hz tone. The winding-down sound is a falling tone and noise over 0.7 seconds.

Holding R raises the gear loop's pitch as charge builds. Reaching half charge plays the click. The bulb buzz gets louder and higher toward full charge, then fades with the battery. Letting go of R plays the winding-down sound. Pause stops these sounds until gameplay resumes.

The WAVs are in `NantesGame/Resources/GameplayAudio`. [GenerateGameplayAudio.py](../../../Design/Audio/GenerateGameplayAudio.py) recreates them with Python, NumPy, and SciPy. Pass the unpacked voice pack folder to include the yell edits.

## Footsteps

Five concrete steps from Kenney's CC0 [Impact Sounds](https://kenney.nl/assets/impact-sounds) are used without editing. `PlayerFootsteps` picks a different recording for each step, with a small pitch variation.

Steps follow the distance the player actually travels. At full movement speed, walking is about one step every 0.48 seconds, Shift running 0.31, Ctrl slow walking 0.70, and C crouching 0.85. Their volume settings are 0.40, 0.70, 0.14, and 0.035 respectively. Crouching stays faintly audible. These are playback settings, not decibel values.

Standing still, pushing against a wall, and leaving the ground stop new steps. Pause holds the sound. This only adds audio; the existing enemy-hearing events stay unchanged. Adjust the four volume fields in `PlayerFootsteps` to tune the mix.
