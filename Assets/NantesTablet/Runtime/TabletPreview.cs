using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

namespace NantesGame.Tablet
{
    public sealed class TabletPreview : MonoBehaviour
    {
        public TabletController tablet;
        public TMP_FontAsset font;
        TMP_Text hints,context;
        GameObject overlay;
        bool checking;
        readonly ScannerContact[] samples={
            new ScannerContact(new Vector2(-8,13),ContactKind.Food),
            new ScannerContact(new Vector2(14,5),ContactKind.Food),
            new ScannerContact(new Vector2(4,-17),ContactKind.Food,.78f),
            new ScannerContact(new Vector2(-18,-9),ContactKind.Unknown,.7f),
            new ScannerContact(new Vector2(29,8),ContactKind.Food),
            new ScannerContact(new Vector2(11,19),ContactKind.Movement,.85f)
        };
        void Awake()
        {
            tablet.ScanRequested+=SampleScan;
            tablet.SetFoodCount(3);
            var canvasGO=new GameObject("Preview controls",typeof(Canvas));overlay=canvasGO;canvasGO.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasGO.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
            hints=OverlayLabel(canvasGO.transform,"Controls",17,new Color(.47f,.55f,.53f),-38);
            context=OverlayLabel(canvasGO.transform,"Context",15,new Color(.64f,.73f,.68f),-72);
            string[] args=Environment.GetCommandLineArgs();
            for(int i=0;i<args.Length-1;i++)if(args[i]=="-tabletQA"){checking=true;StartCoroutine(Check(args[i+1]));}
        }
        void OnDestroy(){if(tablet)tablet.ScanRequested-=SampleScan;if(overlay)Destroy(overlay);}
        void SampleScan(float range){tablet.SetContacts(samples);}
        void Update()
        {
            var keyboard=Keyboard.current;
            if(!checking && keyboard!=null && keyboard.fKey.wasPressedThisFrame && tablet.IsReady){tablet.AddFood(keyboard.leftShiftKey.isPressed?-1:1);}
            if(!hints)return;
            hints.text=tablet.HasFocus?"TAB  Lower device     SPACE  Scan     F  Food +1     F11  Window":"TAB  Raise device     F11  Window";
            if(tablet.IsReady)context.text=tablet.Hover==1?"Scanner":tablet.Hover==2?"Power down":tablet.Hover==4?"Send pulse":"";
            else context.text="";
        }
        TMP_Text OverlayLabel(Transform parent,string name,int size,Color color,float bottom)
        {
            var obj=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI));obj.transform.SetParent(parent,false);
            var rect=(RectTransform)obj.transform;rect.anchorMin=new Vector2(0,0);rect.anchorMax=new Vector2(1,0);rect.pivot=new Vector2(.5f,0);rect.anchoredPosition=new Vector2(0,-bottom);rect.sizeDelta=new Vector2(-40,30);
            var label=obj.GetComponent<TextMeshProUGUI>();label.font=font;label.fontSize=size;label.color=color;label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;return label;
        }

        IEnumerator Check(string directory)
        {
            Directory.CreateDirectory(directory);
            var failures=new List<string>();
            void Assert(bool condition,string message){Debug.Log((condition?"PASS ":"FAIL ")+message);if(!condition)failures.Add(message);}
            yield return new WaitForSecondsRealtime(3.2f);
            Assert(tablet.State==TabletState.Scanner,"Boot reaches scanner");
            Assert(tablet.FoodCount==3,"Initial sample food count");
            Assert(tablet.GetType().Namespace=="NantesGame.Tablet","Separate namespace");
            Assert(tablet.ActionAt(new Vector2(1200,595))==4,"Scan button target");
            var foodLabel=GameObject.Find("Food count").GetComponent<TMP_Text>();foodLabel.ForceMeshUpdate();
            int visible=0;for(int c=0;c<foodLabel.textInfo.characterCount;c++)if(foodLabel.textInfo.characterInfo[c].isVisible)visible++;
            Assert(visible==3,"All three food digits render");
            yield return Capture(directory,"01-scanner");
            tablet.AddFood(2);Assert(tablet.FoodCount==5,"Food update");
            tablet.SetFoodCount(-9);Assert(tablet.FoodCount==0,"Food never negative");
            tablet.SetFoodCount(1500);Assert(tablet.FoodCount==999,"Food fits three digits");tablet.SetFoodCount(3);
            var originalMouse=Mouse.current;
            var mouse=InputSystem.AddDevice<Mouse>();
            Vector2 screenPoint=Project(new Vector2(1200,595));
            Assert(tablet.TryGetDisplayPoint(screenPoint,out var mapped) && (mapped-new Vector2(1200,595)).magnitude<1,"Angled glass maps pointer correctly");
            InputSystem.QueueStateEvent(mouse,new MouseState{position=screenPoint});yield return null;yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=screenPoint,buttons=1});yield return null;yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=screenPoint});yield return null;
            Assert(tablet.State==TabletState.Scanner&&tablet.ScanAge<.2f,"Pointer starts a scan");
            Assert(tablet.Contacts.Count==6,"Scan provider receives request");
            Assert(tablet.ContactVisibility(samples[0])<.1f,"Contacts wait for pulse");
            yield return new WaitForSecondsRealtime(2.1f);
            Assert(tablet.ContactVisibility(samples[0])>.8f,"Pulse reveals contacts");
            Assert(tablet.FoodCount==3,"Food persists while scanning");
            tablet.Activate(5);Assert(tablet.Range==25,"Removed range action does nothing");
            tablet.SetRange(50);Assert(tablet.Range==25,"Old range values stay at twenty-five");
            yield return new WaitForSecondsRealtime(2.1f);
            InputSystem.QueueStateEvent(mouse,new MouseState{position=Vector2.zero});yield return null;
            yield return Capture(directory,"02-scanner");
            var keys=InputSystem.AddDevice<Keyboard>();
            InputSystem.QueueStateEvent(keys,new KeyboardState(Key.Space));yield return null;yield return null;
            InputSystem.QueueStateEvent(keys,new KeyboardState());yield return null;
            Assert(tablet.ScanAge<.2f,"Space starts a scan");
            tablet.PowerOff();yield return new WaitForSecondsRealtime(1.4f);
            Assert(tablet.State==TabletState.Off && tablet.DisplayPower==0,"Shutdown completes");
            yield return Capture(directory,"03-lowered");
            tablet.PowerOn();yield return new WaitForSecondsRealtime(.72f);yield return Capture(directory,"04-boot");
            yield return new WaitForSecondsRealtime(1.3f);Assert(tablet.State==TabletState.Scanner&&tablet.FoodCount==3,"Wake preserves count");
            tablet.SetContacts(null);Assert(tablet.Contacts.Count==0,"Empty scan data accepted");
            InputSystem.RemoveDevice(mouse);InputSystem.RemoveDevice(keys);
            File.WriteAllText(Path.Combine(directory,"checks.txt"),failures.Count==0?"All tablet checks passed.\n":string.Join("\n",failures));
            Debug.Log(failures.Count==0?"TABLET_RUNTIME_PASS":"TABLET_RUNTIME_FAIL");
            Application.Quit(failures.Count==0?0:1);
        }
        Vector2 Project(Vector2 point)
        {
            Vector3 local=new Vector3(point.x/1440-.5f,.5f-point.y/840,0);
            return tablet.viewCamera.WorldToScreenPoint(tablet.displaySurface.TransformPoint(local));
        }
        IEnumerator Capture(string directory,string name)
        {
            yield return new WaitForEndOfFrame();
            tablet.RenderDisplay();
            var camera=tablet.viewCamera;var previous=camera.targetTexture;var active=RenderTexture.active;
            int width=Screen.width,height=Screen.height;
            var target=new RenderTexture(width,height,24){antiAliasing=4};target.Create();camera.targetTexture=target;camera.Render();RenderTexture.active=target;
            var frame=new Texture2D(width,height,TextureFormat.RGB24,false);frame.ReadPixels(new Rect(0,0,width,height),0,0);frame.Apply();
            File.WriteAllBytes(Path.Combine(directory,name+".png"),frame.EncodeToPNG());
            camera.targetTexture=previous;RenderTexture.active=active;target.Release();Destroy(target);Destroy(frame);
            yield return null;
        }
    }
}
