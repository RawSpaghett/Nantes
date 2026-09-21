using UnityEngine;

namespace Nantes.UI
{
    [DefaultExecutionOrder(60)]
    public sealed class NantesEncounterAudio : MonoBehaviour
    {
        public NantesSpaceMotion motion;
        public NantesEncounterDirector encounter;
        public AudioClip earthFlick, bind, strain, breakaway;

        // Use animation time for playback and recording.
        public struct Voice
        {
            public AudioClip clip;
            public float position, volume, pitch, pan;
            public bool loop;
        }
        public Voice[] Voices { get; private set; }
        AudioSource[] sources;
        float previousTime=float.NaN, strainPhase;
        int sequence=-1;
        bool paused;

        void Awake()
        {
            Voices=new Voice[4];sources=new AudioSource[4];
            string[] names={"Earth release", "Binding strike", "Engine strain", "Breakaway"};
            for(int i=0;i<sources.Length;i++)
            {
                var go=new GameObject(names[i]);go.transform.SetParent(transform,false);
                var source=go.AddComponent<AudioSource>();sources[i]=source;
                source.playOnAwake=false;source.spatialBlend=0;source.dopplerLevel=0;
            }
        }

        void LateUpdate() { Evaluate(); }

        public void Evaluate()
        {
            if(sources==null)return;
            float now=motion.AnimationTime;
            if(sequence!=encounter.Sequence||(!float.IsNaN(previousTime)&&now<previousTime))
            { sequence=encounter.Sequence;strainPhase=0;previousTime=now;foreach(var source in sources)source.Stop(); }
            float dt=float.IsNaN(previousTime)?0:Mathf.Clamp(now-previousTime,0,.1f);
            previousTime=now;
            bool frozen=motion.menu!=null&&motion.menu.ReducedMotion;
            bool ship=encounter.Active==NantesEncounter.ShipGrab;
            float t=encounter.Elapsed,after=encounter.BreakTime<0?-1:t-encounter.BreakTime;
            float drive=encounter.EscapeThrust;
            float pitch=.72f+drive*.92f;
            float strainVolume=ship&&t>.62f&&after<.45f?
                (.12f+.48f*drive*drive)*Ease(.62f,1.1f,t)*(after<0?1:1-Ease(0,.45f,after)):0;
            if(!frozen&&strainVolume>0)strainPhase+=dt*pitch;
            float vesselPan=Mathf.Clamp((motion.spaceCamera.WorldToViewportPoint(motion.saucer.position).x-.5f)*.9f,-.5f,.5f);
            Voices[0]=State(earthFlick,t-1.8f,encounter.Active==NantesEncounter.EarthFlick?.68f:0,1,-.24f);
            Voices[1]=State(bind,t-.45f,ship?.48f:0,1,vesselPan);
            Voices[2]=State(strain,strainPhase,strainVolume,pitch,vesselPan,true);
            Voices[3]=State(breakaway,after,ship&&encounter.BreakTime>=0?.78f:0,1,vesselPan);
            for(int i=0;i<sources.Length;i++)
            {
                var voice=Voices[i];var source=sources[i];
                if(source.clip!=voice.clip){source.Stop();source.clip=voice.clip;}
                if(voice.clip==null||voice.volume<=0){source.Stop();continue;}
                if(frozen){source.Pause();continue;}
                source.loop=voice.loop;source.volume=voice.volume;source.pitch=voice.pitch;source.panStereo=voice.pan;
                float position=voice.loop?Mathf.Repeat(voice.position,voice.clip.length):voice.position;
                if(paused)source.UnPause();
                // Resync after pauses or clock changes.
                if(!source.isPlaying||Mathf.Abs(source.time-position)>.12f)
                {source.time=Mathf.Clamp(position,0,voice.clip.length-.001f);if(!source.isPlaying)source.Play();}
            }
            paused=frozen;
        }

        static Voice State(AudioClip clip,float position,float volume,float pitch,float pan,bool loop=false)
        {
            if(clip==null||position<0||(!loop&&position>=clip.length))volume=0;
            return new Voice{clip=clip,position=position,volume=volume,pitch=pitch,pan=pan,loop=loop};
        }
        static float Ease(float a,float b,float t){return Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,t));}
        void OnDisable(){if(sources!=null)foreach(var source in sources)if(source!=null)source.Stop();}
        void OnDestroy(){if(sources!=null)foreach(var source in sources)if(source!=null)Destroy(source.gameObject);}
    }
}
