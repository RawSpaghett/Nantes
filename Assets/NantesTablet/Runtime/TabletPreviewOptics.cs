using UnityEngine;

namespace NantesGame.Tablet
{
    [RequireComponent(typeof(Camera))]
    public sealed class TabletPreviewOptics : MonoBehaviour
    {
        public Shader shader;
        Material material;
        void OnEnable(){GetComponent<Camera>().depthTextureMode|=DepthTextureMode.Depth;if(shader)material=new Material(shader);}
        void OnRenderImage(RenderTexture source,RenderTexture destination)
        {
            if(material)Graphics.Blit(source,destination,material);else Graphics.Blit(source,destination);
        }
        void OnDisable(){if(material)Destroy(material);}
    }
}
