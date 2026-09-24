using UnityEngine;

namespace NantesGame.Gameplay
{
    public sealed class PlayerSoundEffects : MonoBehaviour
    {
        PlayerMovement movement;
        FlashlightScript flashlight;
        AudioSource voice, crank, bulb, clicks;
        AudioClip[] yells;
        AudioClip switchOn, windDown;
        bool wasCranking, wasLit, paused;
        int nextVoice;

        void Start()
        {
            movement = GetComponent<PlayerMovement>();
            flashlight = GetComponentInChildren<FlashlightScript>(true);
            voice = Speaker("Voice"); clicks = Speaker("Flashlight clicks");
            crank = Speaker("Crank gears"); bulb = Speaker("Bulb buzz");
            yells = new[] { Clip("Yell-1"), Clip("Yell-2") };
            switchOn = Clip("Bulb-On"); windDown = Clip("Crank-Down");
            crank.clip = Clip("Crank"); bulb.clip = Clip("Bulb-Buzz");
            crank.loop = bulb.loop = true;
            crank.volume = bulb.volume = 0;
            crank.Play(); bulb.Play();
            movement.Yelled += PlayYell;
        }

        static AudioClip Clip(string name) => Resources.Load<AudioClip>("GameplayAudio/" + name);
        AudioSource Speaker(string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(transform, false);
            var source = child.AddComponent<AudioSource>();
            source.playOnAwake = false; source.spatialBlend = 0;
            return source;
        }

        void PlayYell()
        {
            var clip = yells[nextVoice++ % yells.Length];
            if(clip) voice.PlayOneShot(clip, .8f);
        }

        void Update()
        {
            if(!flashlight) return;
            bool hold = Time.timeScale == 0;
            if(hold != paused)
            {
                foreach(var source in new[] { voice, crank, bulb, clicks })
                    if(hold) source.Pause(); else source.UnPause();
                paused = hold;
            }
            if(hold) return;
            float charge = flashlight.Charge01;
            bool turning = flashlight.IsCranking, lit = flashlight.IsLit;
            if(lit && !wasLit && switchOn) clicks.PlayOneShot(switchOn, .6f);
            if(wasCranking && !turning && windDown) clicks.PlayOneShot(windDown, .55f);
            float blend = 1f - Mathf.Exp(-Time.deltaTime * 12);
            crank.volume = Mathf.Lerp(crank.volume, turning ? .7f : 0, blend);
            crank.pitch = Mathf.Lerp(.9f, 1.45f, charge);
            float hum = lit ? Mathf.Lerp(.25f, .85f, charge * charge) : turning ? charge * .3f : 0;
            bulb.volume = Mathf.Lerp(bulb.volume, hum, blend);
            bulb.pitch = Mathf.Lerp(.78f, 1.25f, charge);
            wasCranking = turning; wasLit = lit;
        }

        void OnDestroy()
        {
            if(movement) movement.Yelled -= PlayYell;
        }
    }
}
