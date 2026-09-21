using UnityEngine;

namespace Nantes.UI
{
    public enum NantesEncounter { None, ShipGrab, EarthFlick }

    [DefaultExecutionOrder(25)]
    public sealed class NantesEncounterDirector : MonoBehaviour
    {
        public NantesSpaceMotion motion;
        public bool randomEncounters = true;
        [Range(0,.1f)] public float chancePerEncounter = .10f;
        public NantesEncounter Active { get; private set; }
        public int Sequence { get; private set; }
        public float Grip { get; private set; }
        public float EscapeThrust { get; private set; }
        public float Elapsed { get; private set; }
        public float SpinSpeed { get; private set; }
        public float BindingWeight { get; private set; }
        public float BreakTime { get; private set; } = -1;
        public bool OverridesThrust => Active==NantesEncounter.ShipGrab && (BreakTime<0||Elapsed<BreakTime+1.4f);
        public float VesselSpeed => shipVelocity.magnitude;
        public float Duration => Active==NantesEncounter.ShipGrab?12f:10f;
        float started, nextOpportunity=18, lastTime=float.NaN, spinAtStart, physicsTime;
        Vector3 holdPosition, shipPosition, shipVelocity, earthFront, earthRight, earthUp;
        Quaternion holdRotation, shipRotation;
        Matrix4x4 releasedFrame;
        bool handedOff, strikeLanded;
        System.Random chance;
        const float Step=1f/120;

        void Awake() { chance=new System.Random(System.Guid.NewGuid().GetHashCode()); }
        void LateUpdate() { Evaluate(motion.AnimationTime); }

        public bool Begin(NantesEncounter encounter)
        {
            if(Active!=NantesEncounter.None||encounter==NantesEncounter.None)return false;
            Sequence++;Active=encounter;started=motion.AnimationTime;Elapsed=0;Grip=EscapeThrust=BindingWeight=SpinSpeed=0;
            shipPosition=motion.saucer.localPosition;shipVelocity=motion.Velocity;
            holdPosition=shipPosition+shipVelocity*.18f;
            holdRotation=shipRotation=motion.saucer.localRotation;
            physicsTime=0;BreakTime=-1;handedOff=strikeLanded=false;
            spinAtStart=motion.PlanetSpinOffset;lastTime=float.NaN;
            earthFront=(motion.spaceCamera.transform.position-motion.planet.position).normalized;
            earthRight=Vector3.ProjectOnPlane(Vector3.right,earthFront).normalized;
            earthUp=Vector3.Cross(earthRight,earthFront).normalized;
            nextOpportunity=started+90;
            return true;
        }

        public void Evaluate(float time)
        {
            if(time==lastTime)return;
            lastTime=time;
            if(Active==NantesEncounter.None&&randomEncounters&&time>=nextOpportunity)
            {
                var p=motion.spaceCamera.WorldToViewportPoint(motion.saucer.position);
                if(p.x>.22f&&p.x<.57f&&p.y>.48f&&p.y<.78f)
                {
                    nextOpportunity=time+60;
                    double roll=chance.NextDouble();
                    if(roll<chancePerEncounter)Begin(NantesEncounter.ShipGrab);
                    else if(roll<chancePerEncounter*2)Begin(NantesEncounter.EarthFlick);
                }
            }
            if(Active==NantesEncounter.None)return;
            Elapsed=Mathf.Max(0,time-started);
            if(Active==NantesEncounter.ShipGrab)ShipGrab();else EarthPull();
            if(Elapsed>=Duration){Active=NantesEncounter.None;Grip=BindingWeight=EscapeThrust=SpinSpeed=0;}
        }

        void ShipGrab()
        {
            while(!handedOff&&physicsTime+Step<=Elapsed)
            {
                physicsTime+=Step;
                StepStruggle(physicsTime,Step);
            }
            float after=BreakTime<0?0:Elapsed-BreakTime;
            Grip=Ease(.53f,.76f,Elapsed)*(BreakTime<0?1:1-Ease(0,.5f,after));
            BindingWeight=Ease(.18f,.55f,Elapsed)*(BreakTime<0?1:1-Ease(.25f,1.6f,after));
            EscapeThrust=BreakTime<0?Drive(Elapsed):Mathf.Lerp(1,Mathf.Pow(motion.Throttle,.55f),Ease(.35f,1.4f,after));
            if(!handedOff)
            {
                motion.saucer.localPosition=shipPosition;
                motion.saucer.localRotation=shipRotation;
                motion.saucerBody.localRotation=Quaternion.identity;
                if(BreakTime>=0&&after>=.8f)
                {
                    // Keep the escape velocity when normal flight resumes.
                    motion.ResumeVessel(shipPosition,shipVelocity,shipRotation);
                    handedOff=true;
                }
            }
        }

        void StepStruggle(float t,float dt)
        {
            float drive=Drive(t);
            float struggle=Ease(.64f,1.25f,t);
            var angles=new Vector3(
                Noise(t*.83f,12)*23+Noise(t*2.2f,7)*6,
                Noise(t*.71f,34)*34+Noise(t*1.9f,4)*9,
                Noise(t*1.10f,56)*43+Noise(t*2.8f,9)*11)*struggle;
            float impact=Mathf.Max(0,t-.62f);
            if(t>.62f)angles+=new Vector3(5,-9,15)*Mathf.Sin(impact*13)*Mathf.Exp(-impact*5);
            if(BreakTime>=0)angles*=1-Ease(0,.65f,t-BreakTime);
            shipRotation=Quaternion.Slerp(shipRotation,holdRotation*Quaternion.Euler(angles),1-Mathf.Exp(-dt*7));
            Vector3 acceleration=shipRotation*Vector3.forward*(.6f+8.8f*drive*drive);
            if(BreakTime<0)
            {
                Vector3 yieldingAnchor=holdPosition+holdRotation*new Vector3(Noise(t*.9f,78)*.33f,Noise(t*.74f,91)*.31f,0)*struggle;
                Vector3 stretch=shipPosition-yieldingAnchor;
                float restraint=Ease(.53f,.76f,t);
                if(!strikeLanded&&t>=.62f)
                {
                    shipVelocity+=holdRotation*new Vector3(.20f,-.85f,-.95f);
                    strikeLanded=true;
                }
                acceleration-=restraint*(stretch*12.5f+shipVelocity*3.8f);
                acceleration+=holdRotation*new Vector3(Noise(t*2.1f,36),Noise(t*1.6f,62),0)*struggle*1.4f;
                if((t>6.8f&&drive>.94f&&stretch.magnitude>.48f)||t>=8.6f)
                {
                    BreakTime=t;
                    releasedFrame=Matrix4x4.TRS(motion.saucer.parent.TransformPoint(shipPosition),motion.saucer.parent.rotation*shipRotation,Vector3.one);
                }
            }
            shipVelocity+=acceleration*dt;
            shipPosition+=shipVelocity*dt;
        }

        float Drive(float t)
        {
            float ramp=.13f+.87f*Ease(1.2f,8.1f,t);
            float surge=.12f*Bump(t,2.15f,.45f)+.10f*Bump(t,3.9f,.66f)+.14f*Bump(t,5.75f,.30f)+.1f*Bump(t,7.15f,.5f);
            return Mathf.Clamp01(ramp+(Noise(t*2.1f,3)*.065f+surge)*Ease(.9f,1.7f,t));
        }

        public float EngineThrust(int engine)
        {
            float mismatch=(1-Ease(.80f,1,EscapeThrust))*.16f;
            return Mathf.Clamp01(EscapeThrust*(1+Noise(Elapsed*2.7f,engine==0?18:49)*mismatch));
        }

        void EarthPull()
        {
            float t=Elapsed;
            Grip=Ease(.35f,1.9f,t)*(1-Ease(3.65f,4.25f,t));
            BindingWeight=Ease(0,1.1f,t)*(1-Ease(3.9f,5.3f,t));
            float pull=EarthPullAngle(t);
            float x=Mathf.InverseLerp(3.7f,9.1f,t);
            float coast=675*(1-Mathf.Pow(1-x,3));
            motion.PlanetSpinOffset=spinAtStart+pull+coast;
            SpinSpeed=t<2.1f?0:t<3.7f?375*Mathf.Pow(Mathf.InverseLerp(2.1f,3.7f,t),4):
                t<9.1f?375*Mathf.Pow(1-x,2):0;
            motion.planet.localRotation=Quaternion.Euler(-12,125+motion.AnimationTime*motion.planetDegreesPerSecond+motion.PlanetSpinOffset,-18);
        }

        static float EarthPullAngle(float t)
        {
            float brace=-7*Ease(1.75f,2.1f,t);
            // Match the release speed to the coast: 375 degrees/sec.
            return brace+120*Mathf.Pow(Mathf.InverseLerp(2.1f,3.7f,t),5);
        }

        public bool BindingPoint(int limb,float u,Vector3 root,Vector3 shoulder,out Vector3 point,out float radius)
        {
            point=Vector3.zero;radius=0;
            if(Active==NantesEncounter.None||limb>1||BindingWeight<=0)return false;
            const float stem=.40f;
            if(u<stem)
            {
                Vector3 entry=Coil(limb,0),tangent=(Coil(limb,.006f)-entry).normalized;
                float reach=Mathf.Min(Vector3.Distance(root,entry)*.25f,3);
                if(Active==NantesEncounter.ShipGrab)
                    shoulder+=motion.saucerBody.TransformVector(new Vector3(limb==0?-1.5f:1.5f,-.7f,0))*Bump(Elapsed,.32f,.30f);
                point=Bezier(root,shoulder,entry-tangent*reach,entry,u/stem);
            }
            else point=Coil(limb,(u-stem)/(1-stem));
            float thickness=Active==NantesEncounter.ShipGrab?.115f:.21f;
            radius=Mathf.Lerp(.19f,thickness,Ease(.1f,.45f,u))*(1-Ease(.90f,1,u))+.003f;
            return true;
        }

        Vector3 Coil(int limb,float s)
        {
            if(Active==NantesEncounter.ShipGrab)
            {
                float after=BreakTime<0?0:Elapsed-BreakTime;
                float close=Ease(.46f+s*.08f,.69f+s*.08f,Elapsed),peel=BreakTime<0?0:Ease(0,1.15f,after);
                float turns=Mathf.Lerp(.36f,1.22f,close)*(1-peel*.9f);
                float angle=-Mathf.PI*.5f+(limb==0?1:-1)*s*turns*Mathf.PI*2;
                float z=(limb==0?.47f:-.53f)+(s-.5f)*.95f;
                float open=(1-close)*1.05f+peel*.72f;
                float rx=z<0?Mathf.Lerp(1.34f,1.24f,z+1):z<.4f?Mathf.Lerp(1.24f,1.10f,z/.4f):Mathf.Lerp(1.10f,.57f,(z-.4f)/.6f);
                Vector3 local=new Vector3(Mathf.Cos(angle)*(rx+open),-.12f+Mathf.Sin(angle)*(.64f+open*.6f),z);
                return BreakTime<0?motion.saucerBody.TransformPoint(local):releasedFrame.MultiplyPoint3x4(local);
            }
            float grow=Ease(.2f,1.9f,Elapsed);
            float angleOnEdge=Mathf.Lerp(163-limb*8,163-limb*8-(139-limb*8)*grow,s)*Mathf.Deg2Rad;
            float facing=limb==0?.39f:.55f;
            Vector3 normal=(earthRight*Mathf.Sin(angleOnEdge)+earthUp*Mathf.Cos(angleOnEdge))*Mathf.Sqrt(1-facing*facing)+earthFront*facing;
            normal=Quaternion.AngleAxis(EarthPullAngle(Mathf.Min(Elapsed,3.7f)),Vector3.up)*normal;
            float recoil=Ease(3.75f,4.8f,Elapsed);
            return motion.planet.position+normal*(12.2f+.16f+recoil*1.6f);
        }

        public static Vector3 Bezier(Vector3 a,Vector3 b,Vector3 c,Vector3 d,float t)
        { float q=1-t;return q*q*q*a+3*q*q*t*b+3*q*t*t*c+t*t*t*d; }
        static float Noise(float t,float seed) { return (Mathf.PerlinNoise(t+seed,seed*.731f)-.5f)*2; }
        static float Bump(float t,float center,float width) { return Ease(0,1,1-Mathf.Abs(t-center)/width); }
        static float Ease(float from,float to,float value) { return Mathf.SmoothStep(0,1,Mathf.InverseLerp(from,to,value)); }
    }
}
