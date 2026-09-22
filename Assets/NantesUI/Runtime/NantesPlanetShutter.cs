using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace NantesGame.UI
{
    [DefaultExecutionOrder(55)]
    [MovedFrom("Nantes.UI")]
    public sealed class NantesPlanetShutter : MonoBehaviour
    {
        public NantesSpaceMotion motion;
        public NantesEncounterDirector encounter;
        public float BlurRadians { get; private set; }
        Renderer surface;
        MaterialPropertyBlock properties;
        void Awake(){surface=motion.planet.GetComponent<Renderer>();properties=new MaterialPropertyBlock();}
        void LateUpdate(){Evaluate();}
        public void Evaluate()
        {
            if(surface==null)return;
            bool frozen=motion.menu!=null&&motion.menu.ReducedMotion;
            BlurRadians=frozen?0:Mathf.Min(.16f,encounter.SpinSpeed*Mathf.Deg2Rad*.024f);
            surface.GetPropertyBlock(properties);
            properties.SetFloat("_AngularBlur",BlurRadians);
            Vector3 axis=motion.planet.InverseTransformDirection(motion.planet.parent.TransformDirection(Vector3.up));
            properties.SetVector("_SpinAxis",axis);
            surface.SetPropertyBlock(properties);
        }
        void OnDisable(){if(surface!=null){surface.GetPropertyBlock(properties);properties.SetFloat("_AngularBlur",0);surface.SetPropertyBlock(properties);}}
    }
}
