using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NantesGame.World
{
    public sealed class WorldLookPreview : MonoBehaviour
    {
        public GameObject atmosphere;
        public Light[] originalLights;
        public Volume[] originalVolumes;
        public Transform[] views;
        public ObjectOutline[] examples;
        public Camera view;
        public Light flashlight;
        bool[] oldLights, oldVolumes;
        bool look = true, help = true, highlighted = true;
        int currentView;
        Vector2 angles;
        string captureFolder;

        void Start()
        {
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 1;
            RenderSettings.fog = false;
            oldLights = new bool[originalLights.Length]; oldVolumes = new bool[originalVolumes.Length];
            for (int i=0;i<oldLights.Length;i++) oldLights[i]=originalLights[i].enabled;
            for (int i=0;i<oldVolumes.Length;i++) oldVolumes[i]=originalVolumes[i].enabled;
            SetLook(true); SetView(0);
            var args=System.Environment.GetCommandLineArgs(); int index=System.Array.IndexOf(args,"-worldCapture");
            if(index>=0 && index+1<args.Length){captureFolder=args[index+1];help=false;StartCoroutine(CaptureSet());}
        }

        public void SetLook(bool value)
        {
            look=value;
            atmosphere.SetActive(value);
            for(int i=0;i<originalLights.Length;i++) originalLights[i].enabled=!value && oldLights[i];
            for(int i=0;i<originalVolumes.Length;i++) originalVolumes[i].enabled=!value && oldVolumes[i];
        }

        void SetView(int index)
        {
            currentView=index; view.transform.SetPositionAndRotation(views[index].position,views[index].rotation);
            angles=new Vector2(view.transform.eulerAngles.x,view.transform.eulerAngles.y);
            if(angles.x>180)angles.x-=360;
            foreach(var outline in examples)outline.SetHighlighted(highlighted && index>=2);
        }

        void Update()
        {
            if(captureFolder!=null)return;
            var keys=Keyboard.current;var mouse=Mouse.current;if(keys==null)return;
            if(keys.tabKey.wasPressedThisFrame)SetLook(!look);
            if(keys.digit1Key.wasPressedThisFrame)SetView(0);
            if(keys.digit2Key.wasPressedThisFrame)SetView(1);
            if(keys.digit3Key.wasPressedThisFrame)SetView(2);
            if(keys.digit4Key.wasPressedThisFrame)SetView(3);
            if(keys.oKey.wasPressedThisFrame){highlighted=!highlighted;foreach(var outline in examples)outline.SetHighlighted(highlighted && currentView>=2);}
            if(keys.fKey.wasPressedThisFrame)flashlight.enabled=!flashlight.enabled;
            if(keys.hKey.wasPressedThisFrame)help=!help;
            if(keys.f11Key.wasPressedThisFrame)Screen.fullScreen=!Screen.fullScreen;
            if(keys.escapeKey.wasPressedThisFrame)
            {
                if(Cursor.lockState==CursorLockMode.Locked){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
                else Application.Quit();
            }
            if(mouse!=null && mouse.rightButton.wasPressedThisFrame){Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;}
            if(Cursor.lockState!=CursorLockMode.Locked)return;
            angles+=new Vector2(-mouse.delta.y.ReadValue(),mouse.delta.x.ReadValue())*.10f;
            angles.x=Mathf.Clamp(angles.x,-85,85);view.transform.rotation=Quaternion.Euler(angles.x,angles.y,0);
            Vector3 move=new Vector3((keys.dKey.isPressed?1:0)-(keys.aKey.isPressed?1:0),(keys.eKey.isPressed?1:0)-(keys.qKey.isPressed?1:0),(keys.wKey.isPressed?1:0)-(keys.sKey.isPressed?1:0));
            view.transform.position+=view.transform.TransformDirection(Vector3.ClampMagnitude(move,1))*Time.unscaledDeltaTime*(keys.leftShiftKey.isPressed?12:4);
        }

        IEnumerator CaptureSet()
        {
            Directory.CreateDirectory(captureFolder);
            yield return new WaitForSeconds(2);
            for(int i=0;i<views.Length;i++)
            {
                SetView(i);SetLook(false);yield return null;yield return null;Capture(i+"-before");
                SetLook(true);yield return null;yield return null;Capture(i+"-after");
            }
            SetView(2);
            foreach(var outline in examples)outline.SetHighlighted(false);
            yield return null;Capture("outline-off");
            foreach(var outline in examples)outline.SetHighlighted(true);
            yield return null;Capture("outline-on");
            SetView(3);
            foreach(var outline in examples)outline.SetHighlighted(false);
            yield return null;Capture("through-wall-off");
            foreach(var outline in examples)outline.SetHighlighted(true);
            yield return null;Capture("through-wall-on");
            Debug.Log("WORLD_CAPTURE_PASS");Application.Quit();
        }

        void Capture(string name)
        {
            var target=RenderTexture.GetTemporary(1920,1080,24,RenderTextureFormat.ARGB32);
            var request=new UniversalRenderPipeline.SingleCameraRequest{destination=target};
            RenderPipeline.SubmitRenderRequest(view,request);
            var previous=RenderTexture.active;RenderTexture.active=target;
            var texture=new Texture2D(1920,1080,TextureFormat.RGB24,false);
            texture.ReadPixels(new Rect(0,0,1920,1080),0,0);texture.Apply();
            File.WriteAllBytes(Path.Combine(captureFolder,name+".png"),texture.EncodeToPNG());
            RenderTexture.active=previous;RenderTexture.ReleaseTemporary(target);Destroy(texture);
        }

        void OnGUI()
        {
            if(!help)return;
            GUI.color=new Color(.035f,.045f,.05f,.90f);GUI.DrawTexture(new Rect(20,20,850,70),Texture2D.whiteTexture);
            GUI.color=new Color(.82f,.86f,.81f);var style=new GUIStyle(GUI.skin.label){fontSize=15};
            GUI.Label(new Rect(34,28,620,24),look?"NANTES / WORLD LOOK STUDY":"ORIGINAL LIGHTING",style);
            style.fontSize=12;
            GUI.Label(new Rect(34,54,820,24),"1 Exterior   2 Maze   3 Outline   4 Through wall   TAB Compare   O Outline   F Light   H Hide   ESC Exit",style);
            GUI.Label(new Rect(24,Screen.height-34,850,24),"RIGHT CLICK Look   WASD Move   Q/E Down/Up   SHIFT Faster   F11 Fullscreen",style);
            GUI.Label(new Rect(24,Screen.height-58,1050,24),"Sky: Kloppenheim 07 (Pure Sky) / Greg Zaal, Jarod Guest / Poly Haven / CC0",style);
        }
    }
}
