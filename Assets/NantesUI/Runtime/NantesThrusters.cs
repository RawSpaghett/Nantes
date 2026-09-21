using UnityEngine;

namespace Nantes.UI
{
    [DefaultExecutionOrder(40)]
    public sealed class NantesThrusters : MonoBehaviour
    {
        public NantesSpaceMotion motion;
        public Transform[] mainJets, brakingJets, sideJets;
        MaterialPropertyBlock properties;
        NantesEncounterDirector encounter;
        float lastTime=float.NaN,flowPhase;
        public float MainDemand { get; private set; }
        public float FlowRate { get; private set; }
        void Awake() { encounter=motion.GetComponent<NantesEncounterDirector>(); }

        void LateUpdate() { Evaluate(motion.AnimationTime); }

        public void Evaluate(float t)
        {
            if (properties == null) properties = new MaterialPropertyBlock();
            bool captive=encounter!=null&&encounter.OverridesThrust;
            MainDemand=captive?encounter.EscapeThrust:Mathf.Pow(motion.Throttle,.55f);
            float speed=Mathf.Clamp01((captive?encounter.VesselSpeed:motion.Velocity.magnitude)/2.5f);
            FlowRate=Mathf.Lerp(1.5f,25,MainDemand)+speed*MainDemand*6;
            if(float.IsNaN(lastTime)||t<lastTime)flowPhase=0;
            else flowPhase+=Mathf.Min(.1f,t-lastTime)*FlowRate;
            lastTime=t;
            float flutter=1+(Mathf.PerlinNoise(flowPhase*.8f,6)-.5f)*Mathf.Lerp(.035f,.22f,MainDemand);
            float escape=encounter!=null?encounter.EscapeThrust:0;
            for(int i=0;i<mainJets.Length;i++)
                SetJet(mainJets[i],captive?encounter.EngineThrust(i):MainDemand,flutter,flowPhase+i*.63f,1);
            SetJets(brakingJets, captive?0:motion.Braking, flutter, flowPhase, .12f);
            for (int i = 0; i < sideJets.Length; i++)
            {
                float demand = Mathf.Max(0, motion.TurnThrust * (i == 0 ? 1 : -1));
                float erratic=Mathf.Clamp01((Mathf.PerlinNoise(t*3.3f,i*17.1f+5)-.42f)*3);
                demand=Mathf.Max(demand,escape*erratic);
                SetJet(sideJets[i], demand, flutter, flowPhase+i*1.7f, .09f);
            }
        }

        public static Color EngineLight(float thrust)
        { return Color.Lerp(new Color(.015f,.20f,.85f),new Color(.64f,.91f,1f),Mathf.SmoothStep(0,1,thrust)); }

        void SetJets(Transform[] jets, float demand, float flutter, float t, float size)
        { foreach (var jet in jets) SetJet(jet, demand, flutter, t, size); }

        void SetJet(Transform jet, float demand, float flutter, float t, float size)
        {
            demand=Mathf.Clamp01(demand);
            var renderer=jet.GetComponent<Renderer>();
            renderer.enabled=demand>.008f;
            if(!renderer.enabled)return;
            float width = size < .5f ? .18f : 1;
            float length=.018f+2.35f*Mathf.Pow(demand,1.35f);
            float aperture=width*Mathf.Lerp(.48f,1.15f,demand);
            jet.localScale = new Vector3(aperture,aperture,size*length*flutter);
            properties.SetFloat("_Thrust", demand);
            properties.SetFloat("_Auxiliary",size<.5f?1:0);
            properties.SetFloat("_JetTime", t);
            renderer.SetPropertyBlock(properties);
        }
    }
}
