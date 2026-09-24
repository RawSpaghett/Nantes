using UnityEngine;

namespace NantesGame.World
{
    public sealed class DamagedFixture : MonoBehaviour
    {
        Light lamp;
        Renderer diffuser;
        MaterialPropertyBlock properties;
        float brightness, seed;

        public void Setup(Light source, Renderer surface, int index)
        {
            lamp=source;diffuser=surface;
            brightness=lamp.intensity;seed=index*2.37f;
            properties=new MaterialPropertyBlock();
        }

        void Update()
        {
            if(!lamp || !diffuser) return;
            float level=1;
            if(PlayerPrefs.GetInt("Nantes.UI.ReducedMotion",0)==0)
            {
                float cycle=Mathf.Repeat(Time.time+seed,11+seed%5);
                float dip=(cycle-2)/.24f;
                level=(.92f+.08f*Mathf.PerlinNoise(seed,Time.time*3.7f))*(1-.78f*Mathf.Exp(-dip*dip));
            }
            //The tube and the light dim together when the ballast falters.
            lamp.intensity=brightness*level;
            diffuser.GetPropertyBlock(properties);
            properties.SetColor("_BaseColor",lamp.color*Mathf.Lerp(.04f,.65f,level));
            properties.SetColor("_EmissionColor",lamp.color*(.65f*level));
            diffuser.SetPropertyBlock(properties);
        }
    }
}
