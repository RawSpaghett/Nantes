using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace NantesGame.UI
{
    // Only the title uses this mask; buttons stay above the ship.
    [DefaultExecutionOrder(100)]
    [MovedFrom("Nantes.UI")]
    public sealed class NantesTitleDepth : MonoBehaviour
    {
        public NantesSpaceMotion motion;
        public NantesLogoAnimator title;
        public int vesselLayer = 29;
        public int depthOverride = -1;
        public bool InFront { get; private set; }
        Camera maskCamera;
        RenderTexture mask;
        Transform[] vesselObjects;
        int[] originalLayers;

        void Awake()
        {
            vesselObjects=motion.saucer.GetComponentsInChildren<Transform>(true);
            originalLayers=new int[vesselObjects.Length];
            for(int i=0;i<vesselObjects.Length;i++){originalLayers[i]=vesselObjects[i].gameObject.layer;vesselObjects[i].gameObject.layer=vesselLayer;}
            var go=new GameObject("Vessel title occlusion camera");
            maskCamera=go.AddComponent<Camera>();maskCamera.enabled=false;
        }

        void LateUpdate()
        {
            float leg=motion.AnimationTime/Mathf.Max(1,motion.flightLegSeconds);
            float phase=Mathf.Repeat(leg,6);
            bool alternateReturn=(Mathf.FloorToInt(leg/6)%2)==0&&phase>3&&phase<4.35f;
            InFront=depthOverride<0?alternateReturn:depthOverride==1;
            Setup(motion.spaceCamera,Mathf.Max(1,Screen.width),Mathf.Max(1,Screen.height));
            maskCamera.enabled=InFront;
            title.SetVesselDepth(mask,InFront);
        }

        void Setup(Camera source,int width,int height)
        {
            if(mask==null||mask.width!=width||mask.height!=height)
            {
                if(mask!=null){mask.Release();Destroy(mask);}
                mask=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32){name="Vessel-only title mask",antiAliasing=2};mask.Create();
            }
            maskCamera.CopyFrom(source);maskCamera.transform.SetPositionAndRotation(source.transform.position,source.transform.rotation);
            maskCamera.rect=new Rect(0,0,1,1);maskCamera.aspect=(float)width/height;
            maskCamera.targetTexture=mask;maskCamera.clearFlags=CameraClearFlags.SolidColor;maskCamera.backgroundColor=Color.clear;
            maskCamera.cullingMask=1<<vesselLayer;maskCamera.depth=source.depth-1;maskCamera.allowHDR=false;maskCamera.allowMSAA=true;
        }

        public void RenderForCapture(Camera source,int width,int height)
        {
            Setup(source,width,height);maskCamera.enabled=false;
            if(InFront)maskCamera.Render();
            title.SetVesselDepth(mask,InFront);
        }

        void OnDestroy()
        {
            if(vesselObjects!=null)for(int i=0;i<vesselObjects.Length;i++)if(vesselObjects[i]!=null)vesselObjects[i].gameObject.layer=originalLayers[i];
            if(maskCamera!=null)Destroy(maskCamera.gameObject);
            if(mask!=null){mask.Release();Destroy(mask);}
        }
    }
}
