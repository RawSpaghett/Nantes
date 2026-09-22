using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace NantesGame.UI
{
    [DefaultExecutionOrder(110)]
    [MovedFrom("Nantes.UI")]
    public sealed class NantesCinematicFrame : MonoBehaviour
    {
        public NantesSpaceMotion motion;
        public Material presentation;
        public Material bloomProcessing;
        public RawImage Frame { get; private set; }
        Material film,filter;
        RenderTexture scene, bloomA, bloomB, previousTarget;
        bool previousHdr;
        Camera displayCamera;

        void OnEnable()
        {
            film=new Material(presentation);
            filter=new Material(bloomProcessing);
            var go=new GameObject("Cinematic world",typeof(RectTransform),typeof(RawImage));
            go.layer=5;go.transform.SetParent(motion.menu.transform,false);go.transform.SetAsFirstSibling();
            Frame=go.GetComponent<RawImage>();Frame.raycastTarget=false;Frame.material=film;
            var rect=Frame.rectTransform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;
            rect.offsetMin=rect.offsetMax=Vector2.zero;
            previousTarget=motion.spaceCamera.targetTexture;previousHdr=motion.spaceCamera.allowHDR;
            // A screen camera is still needed to draw the overlay Canvas.
            var display=new GameObject("Cinematic display camera",typeof(Camera));
            displayCamera=display.GetComponent<Camera>();displayCamera.cullingMask=0;
            displayCamera.clearFlags=CameraClearFlags.SolidColor;displayCamera.backgroundColor=Color.black;
            displayCamera.depth=motion.spaceCamera.depth+10;displayCamera.allowHDR=false;
            displayCamera.allowMSAA=false;displayCamera.targetDisplay=motion.spaceCamera.targetDisplay;
            EnsureTargets(Screen.width,Screen.height);
            Camera.onPostRender+=CameraFinished;
            RenderPipelineManager.endCameraRendering+=PipelineFinished;
        }

        void LateUpdate() { EnsureTargets(Screen.width,Screen.height); }
        void CameraFinished(Camera camera) { if(camera==motion.spaceCamera)FinishFrame(); }
        void PipelineFinished(ScriptableRenderContext context,Camera camera) { CameraFinished(camera); }

        void EnsureTargets(int width,int height)
        {
            width=Mathf.Max(1,width);height=Mathf.Max(1,height);
            if(scene!=null&&scene.width==width&&scene.height==height)return;
            motion.spaceCamera.targetTexture=null;
            Release(ref scene);Release(ref bloomA);Release(ref bloomB);
            var format=SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf)?RenderTextureFormat.ARGBHalf:RenderTextureFormat.ARGB32;
            scene=new RenderTexture(width,height,24,format){name="Nantes HDR world",antiAliasing=2};scene.Create();
            bloomA=new RenderTexture(Mathf.Max(1,width/4),Mathf.Max(1,height/4),0,format){name="Nantes bloom A",filterMode=FilterMode.Bilinear};bloomA.Create();
            bloomB=new RenderTexture(bloomA.width,bloomA.height,0,format){name="Nantes bloom B",filterMode=FilterMode.Bilinear};bloomB.Create();
            motion.spaceCamera.allowHDR=true;motion.spaceCamera.targetTexture=scene;
            Frame.texture=scene;film.SetTexture("_BloomTex",bloomA);
        }

        void FinishFrame()
        {
            if(scene==null)return;
            var previous=RenderTexture.active;
            try
            {
                Graphics.Blit(scene,bloomA,filter,0);
                Graphics.Blit(bloomA,bloomB,filter,1);Graphics.Blit(bloomB,bloomA,filter,2);
                Graphics.Blit(bloomA,bloomB,filter,1);Graphics.Blit(bloomB,bloomA,filter,2);
                film.SetTexture("_BloomTex",bloomA);film.SetTexture("_MainTex",scene);
            }
            finally { RenderTexture.active=previous; }
        }

        public void RenderForCapture(Camera camera,int width,int height)
        {
            EnsureTargets(width,height);
            var target=camera.targetTexture;bool hdr=camera.allowHDR;
            camera.targetTexture=scene;camera.allowHDR=true;
            try { camera.Render();FinishFrame(); }
            finally { camera.targetTexture=target;camera.allowHDR=hdr; }
        }

        void OnDisable()
        {
            Camera.onPostRender-=CameraFinished;RenderPipelineManager.endCameraRendering-=PipelineFinished;
            if(motion!=null&&motion.spaceCamera!=null){motion.spaceCamera.targetTexture=previousTarget;motion.spaceCamera.allowHDR=previousHdr;}
            if(Frame!=null)Destroy(Frame.gameObject);
            if(displayCamera!=null)Destroy(displayCamera.gameObject);
            Release(ref scene);Release(ref bloomA);Release(ref bloomB);
            if(film!=null)Destroy(film);
            if(filter!=null)Destroy(filter);
        }

        static void Release(ref RenderTexture texture)
        { if(texture!=null){texture.Release();Destroy(texture);texture=null;} }
    }
}
