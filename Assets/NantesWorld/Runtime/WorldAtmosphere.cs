using UnityEngine;
using UnityEngine.Rendering;

namespace NantesGame.World
{
    public sealed class WorldAtmosphere : MonoBehaviour
    {
        public Material sky;
        public Color fogColor = new Color(.20f, .205f, .18f);
        [Range(0, .05f)] public float fogDensity = .010f;
        public Color skyLight = new Color(.47f, .52f, .50f);
        public Color sideLight = new Color(.36f, .40f, .37f);
        public Color groundLight = new Color(.18f, .17f, .145f);
        Material oldSky;
        Color oldFogColor, oldSkyLight, oldSideLight, oldGroundLight;
        AmbientMode oldAmbientMode;
        SphericalHarmonicsL2 oldProbe, environmentProbe;
        FogMode oldFogMode;
        float oldDensity, oldReflection;
        bool oldFog, applied;
        public float lightScale = 1;

        void OnEnable()
        {
            oldSky = RenderSettings.skybox; oldFog = RenderSettings.fog; oldProbe = RenderSettings.ambientProbe;
            oldFogMode = RenderSettings.fogMode; oldFogColor = RenderSettings.fogColor; oldDensity = RenderSettings.fogDensity;
            oldAmbientMode = RenderSettings.ambientMode; oldSkyLight = RenderSettings.ambientSkyColor;
            oldSideLight = RenderSettings.ambientEquatorColor; oldGroundLight = RenderSettings.ambientGroundColor;
            oldReflection = RenderSettings.reflectionIntensity;
            RenderSettings.skybox = sky; RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = fogColor; RenderSettings.fogDensity = fogDensity;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = skyLight; RenderSettings.ambientEquatorColor = sideLight; RenderSettings.ambientGroundColor = groundLight;
            environmentProbe = new SphericalHarmonicsL2();
            environmentProbe.AddAmbientLight(sideLight.linear);
            environmentProbe.AddDirectionalLight(Vector3.up, skyLight.linear, .5f);
            environmentProbe.AddDirectionalLight(Vector3.down, groundLight.linear, .25f);
            RenderSettings.ambientProbe = environmentProbe;
            RenderSettings.reflectionIntensity = .22f; applied = true;
        }

        void LateUpdate()
        {
            if (!applied) return;
            float colorScale=Mathf.LinearToGammaSpace(lightScale);
            RenderSettings.ambientSkyColor=skyLight*colorScale;
            RenderSettings.ambientEquatorColor=sideLight*colorScale;
            RenderSettings.ambientGroundColor=groundLight*colorScale;
            var probe = environmentProbe;
            for (int rgb = 0; rgb < 3; rgb++)
                for (int coefficient = 0; coefficient < 9; coefficient++) probe[rgb, coefficient] *= lightScale;
            RenderSettings.ambientProbe = probe;
        }

        void OnDisable()
        {
            if (!applied) return;
            RenderSettings.skybox = oldSky; RenderSettings.fog = oldFog; RenderSettings.fogMode = oldFogMode;
            RenderSettings.fogColor = oldFogColor; RenderSettings.fogDensity = oldDensity;
            RenderSettings.ambientMode = oldAmbientMode; RenderSettings.ambientSkyColor = oldSkyLight;
            RenderSettings.ambientEquatorColor = oldSideLight; RenderSettings.ambientGroundColor = oldGroundLight;
            RenderSettings.ambientProbe = oldProbe;
            RenderSettings.reflectionIntensity = oldReflection; applied = false;
        }
    }
}
