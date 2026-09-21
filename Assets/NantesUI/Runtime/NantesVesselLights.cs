using UnityEngine;

namespace Nantes.UI
{
    [DefaultExecutionOrder(35)]
    public sealed class NantesVesselLights : MonoBehaviour
    {
        public NantesSpaceMotion motion;
        public Renderer[] apertures;
        MaterialPropertyBlock properties;
        NantesEncounterDirector encounter;
        void Start(){encounter=motion.GetComponent<NantesEncounterDirector>();}
        void LateUpdate()
        {
            if (properties == null) properties = new MaterialPropertyBlock();

            float throttle=encounter!=null&&encounter.OverridesThrust?encounter.EscapeThrust:Mathf.Pow(motion.Throttle,.55f);
            float strength=.055f+.10f*throttle;
            for(int i=0;i<apertures.Length;i++)
            {
                properties.SetFloat("_MappedEmission",strength*2.4f);
                properties.SetColor("_EmissionTint",new Color(.08f,.72f,1f));
                properties.SetVector("_EnginePosition",motion.saucerBody.TransformPoint(new Vector3(0,0,-2.25f)));
                properties.SetColor("_EngineColor",NantesThrusters.EngineLight(throttle)*throttle*1.8f);
                apertures[i].SetPropertyBlock(properties);
            }
        }
    }
}
