using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NantesGame.Tablet
{
    public sealed class TabletController : MonoBehaviour
    {
        public TMP_FontAsset font;
        public TMP_FontAsset alienFont;
        public Transform displaySurface;
        public Camera viewCamera;
        public Renderer screenRenderer;
        public Renderer statusLight;
        public bool usePreviewKeys = true;
        public bool wakeOnStart = true;
        public bool squareDisplay;
        public bool animateHousing = true;
        public bool manageCursor = true;
        public bool gazeInput;
        public bool InputEnabled { get; set; } = true;
        public Transform DisplayRoot => screenUI ? screenUI.transform : null;
        public bool HasFlashlightMeter { get; set; }
        public event Action<bool> FocusChanged;
        public event Action<float> ScanRequested;
        public event Action<int> FoodChanged;
        public TabletState State { get; private set; } = TabletState.Off;
        public int FoodCount { get; private set; }
        public float Range { get; private set; } = 25;
        public float Clock => Time.unscaledTime;
        public float Transition => Mathf.Clamp01((Clock-stateStarted)/transitionDuration);
        public float ScanAge => Clock-scanStarted;
        public int Hover { get; private set; } = -1;
        public IReadOnlyList<ScannerContact> Contacts => contacts;
        public bool HasFocus => State != TabletState.Off && State != TabletState.ShuttingDown;
        public bool IsReady => State==TabletState.Home || State==TabletState.Scanner;
        public float DisplayPower { get; private set; }
        readonly List<ScannerContact> contacts = new List<ScannerContact>();
        TabletDisplay graphic;
        RenderTexture screenTexture;
        Material screenMaterial;
        Material ledMaterial;
        Camera screenCamera;
        GameObject screenUI;
        CanvasGroup screenGroup;
        TMP_Text foodTop,rangeText,contactsText,pulseText,homeIndex,clockText;
        TMP_Text tabletTitle,pageTitle,statusText,bootText;
        readonly List<TMP_Text> homeLabels=new List<TMP_Text>();
        readonly List<TMP_Text> scannerLabels=new List<TMP_Text>();
        float stateStarted,transitionDuration,scanStarted=-100,raise,raiseVelocity,impulse;
        Vector3 basePosition,baseEuler;
        CursorLockMode previousLock;
        bool previousCursor,ownsFocus;
        int previousMask;
        AudioSource speaker;
        AudioClip clickSound,scanSound;
        Vector2 previousPointer;
        bool pointerMode=true;
        int keyboardFocus=3;

        void Awake()
        {
            basePosition=transform.localPosition;baseEuler=transform.localEulerAngles;
            if(!viewCamera)viewCamera=Camera.main;
            if(viewCamera){previousMask=viewCamera.cullingMask;viewCamera.cullingMask&=~(1<<30);}
            screenMaterial=new Material(screenRenderer.sharedMaterial);screenRenderer.sharedMaterial=screenMaterial;
            if(statusLight){ledMaterial=new Material(statusLight.sharedMaterial);statusLight.sharedMaterial=ledMaterial;}
            CreateDisplay();
            speaker=gameObject.AddComponent<AudioSource>();speaker.playOnAwake=false;speaker.spatialBlend=0;speaker.volume=.13f;
            clickSound=Tone("Tablet key",.07f,930,540);scanSound=Tone("Scanner pulse",.19f,290,720);
            if(Mouse.current!=null)previousPointer=Mouse.current.position.ReadValue();
        }

        void Start() { if(wakeOnStart)PowerOn(); }

        void Update()
        {
            float dt=Mathf.Min(Time.unscaledDeltaTime,.05f);
            if(InputEnabled){if(usePreviewKeys)ReadKeys();ReadPointer();}
            else Hover=-1;
            if(State==TabletState.Booting && Transition>=1)ChangeState(TabletState.Home,0);
            if(State==TabletState.ShuttingDown && Transition>=1)ChangeState(TabletState.Off,0);
            float target=State==TabletState.Off?0:State==TabletState.ShuttingDown?1-Mathf.SmoothStep(0,1,Transition):1;
            raise=Mathf.SmoothDamp(raise,target,ref raiseVelocity,.22f,100,dt);
            impulse=Mathf.MoveTowards(impulse,0,dt*1.5f);
            float motion=Clock;
            Vector2 pointer=Mouse.current!=null?Mouse.current.position.ReadValue():new Vector2(Screen.width/2f,Screen.height/2f);
            Vector2 normalized=new Vector2(pointer.x/Mathf.Max(1,Screen.width)-.5f,pointer.y/Mathf.Max(1,Screen.height)-.5f);
            if(animateHousing)transform.localPosition=basePosition+new Vector3(Mathf.Sin(motion*.51f)*.016f,-(1-raise)*5+Mathf.Sin(motion*.87f)*.012f,impulse*.035f);
            Quaternion q=Quaternion.Euler(baseEuler+new Vector3((1-raise)*24+Mathf.Sin(motion*.71f)*.22f-normalized.y*.7f,Mathf.Sin(motion*.39f)*.28f+normalized.x*.85f,Mathf.Sin(motion*.43f)*.16f));
            if(animateHousing)transform.localRotation=Quaternion.Slerp(transform.localRotation,q,dt*5);
            DisplayPower=State==TabletState.Off?0:State==TabletState.Booting?Mathf.SmoothStep(0,1,Transition*3):State==TabletState.ShuttingDown?1-Mathf.SmoothStep(0,1,Transition):1;
            screenMaterial.SetFloat("_Power",DisplayPower);
            if(ledMaterial){Color c=Color.Lerp(new Color(.18f,.075f,.018f),new Color(.05f,.7f,.6f),DisplayPower);ledMaterial.SetColor("_EmissionColor",c);}
            UpdateLabels();graphic.SetVerticesDirty();
        }

        public void SetFoodCount(int value)
        {
            int next=Mathf.Clamp(value,0,999);
            if(next==FoodCount)return;
            FoodCount=next;FoodChanged?.Invoke(FoodCount);
        }
        public void AddFood(int amount=1) { SetFoodCount(FoodCount+amount); }
        public void SetContacts(IEnumerable<ScannerContact> values)
        {
            contacts.Clear();
            if(values==null)return;
            foreach(var contact in values){if(contacts.Count>=64)break;if(float.IsNaN(contact.position.x)||float.IsNaN(contact.position.y)||float.IsInfinity(contact.position.x)||float.IsInfinity(contact.position.y))continue;contacts.Add(contact);}
        }
        public void SetRange(float metres){Range=Mathf.Clamp(metres,5,100);}
        public float ContactVisibility(ScannerContact contact)
        {
            float delay=contact.position.magnitude/Range*1.8f;
            if(ScanAge<delay)return 0;
            return Mathf.Clamp01((ScanAge-delay)*5)*Mathf.Clamp01(1-(ScanAge-5)/8)*contact.confidence;
        }
        public void PowerOn()
        {
            if(State!=TabletState.Off && State!=TabletState.ShuttingDown)return;
            AcquireFocus();ChangeState(TabletState.Booting,1.55f);Click();
        }
        public void PowerOff()
        {
            if(State==TabletState.Off || State==TabletState.ShuttingDown)return;
            ChangeState(TabletState.ShuttingDown,.75f);ReleaseFocus();Click();
        }
        public void TogglePower(){if(State==TabletState.Off||State==TabletState.ShuttingDown)PowerOn();else PowerOff();}
        public void ShowHome(){if(!IsReady)return;ChangeState(TabletState.Home,0);keyboardFocus=3;Click();}
        public void ShowScanner(){if(!IsReady)return;ChangeState(TabletState.Scanner,0);keyboardFocus=4;RequestScan();}
        public void RequestScan()
        {
            if(State!=TabletState.Scanner || ScanAge<1.85f)return;
            scanStarted=Clock;ScanRequested?.Invoke(Range);impulse=.4f;
            if(speaker && scanSound)speaker.PlayOneShot(scanSound,.7f);
        }
        public void Activate(int action)
        {
            if(!IsReady)return;
            if(action==0)ShowHome();
            else if(action==1||action==3)ShowScanner();
            else if(action==2)PowerOff();
            else if(action==4)RequestScan();
            else if(action==5){SetRange(Range<40?50:25);scanStarted=-100;RequestScan();}
        }
        public void RenderDisplay(){Canvas.ForceUpdateCanvases();if(screenCamera)screenCamera.Render();}
        void ChangeState(TabletState state,float duration){State=state;stateStarted=Clock;transitionDuration=Mathf.Max(.001f,duration);Hover=-1;}
        void AcquireFocus(){if(ownsFocus)return;previousLock=Cursor.lockState;previousCursor=Cursor.visible;if(manageCursor){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}ownsFocus=true;FocusChanged?.Invoke(true);}
        void ReleaseFocus(){if(!ownsFocus)return;if(manageCursor){Cursor.lockState=previousLock;Cursor.visible=previousCursor;}ownsFocus=false;FocusChanged?.Invoke(false);}
        void Click(){impulse=1;if(speaker && clickSound)speaker.PlayOneShot(clickSound);}

        void ReadKeys()
        {
            var k=Keyboard.current;if(k==null)return;
            if(k.tabKey.wasPressedThisFrame)TogglePower();
            if(k.f11Key.wasPressedThisFrame)Screen.fullScreen=!Screen.fullScreen;
            if(!IsReady)return;
            if(k.escapeKey.wasPressedThisFrame){if(State==TabletState.Scanner)ShowHome();else PowerOff();}
            if(k.digit1Key.wasPressedThisFrame)ShowHome();
            if(k.digit2Key.wasPressedThisFrame)ShowScanner();
            if(k.spaceKey.wasPressedThisFrame && State==TabletState.Scanner)RequestScan();
            if(k.upArrowKey.wasPressedThisFrame || k.downArrowKey.wasPressedThisFrame){pointerMode=false;int[] options=State==TabletState.Home?new[]{3,1,2}:new[]{4,5,0,2};int index=Array.IndexOf(options,keyboardFocus);index=(index+(k.upArrowKey.wasPressedThisFrame?-1:1)+options.Length)%options.Length;keyboardFocus=options[index];Hover=keyboardFocus;}
            if(k.enterKey.wasPressedThisFrame){pointerMode=false;Hover=keyboardFocus;Activate(keyboardFocus);}
        }

        void ReadPointer()
        {
            if(viewCamera==null||(!gazeInput&&Mouse.current==null))return;
            Vector2 p=gazeInput?new Vector2(Screen.width*.5f,Screen.height*.5f):Mouse.current.position.ReadValue();
            if(gazeInput)pointerMode=true;
            if((p-previousPointer).sqrMagnitude>1)pointerMode=true;
            previousPointer=p;
            if(!IsReady){Hover=-1;return;}
            if(pointerMode){Hover=TryGetDisplayPoint(p,out var uv)?ActionAt(uv):-1;}
            bool press=Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame;
            press|=gazeInput&&Keyboard.current!=null&&Keyboard.current.eKey.wasPressedThisFrame;
            press|=gazeInput&&Gamepad.current!=null&&Gamepad.current.buttonSouth.wasPressedThisFrame;
            if(press){pointerMode=true;if(TryGetDisplayPoint(p,out var uv)){int action=ActionAt(uv);if(action>=0)Activate(action);}else {
                Ray ray=viewCamera.ScreenPointToRay(p);
                if(Physics.Raycast(ray,out var hit,30)){
                    var key=hit.collider.GetComponent<TabletHardwareKey>();
                    if(key && key.tablet==this){key.Press();if(key.action==2)TogglePower();else Activate(key.action);}
                }
            }}
        }

        public bool TryGetDisplayPoint(Vector2 screenPoint,out Vector2 point)
        {
            point=default;if(!viewCamera||!displaySurface)return false;
            Ray ray=viewCamera.ScreenPointToRay(screenPoint);
            Plane plane=new Plane(displaySurface.forward,displaySurface.position);
            if(!plane.Raycast(ray,out float distance))return false;
            Vector3 local=displaySurface.InverseTransformPoint(ray.GetPoint(distance));
            point=new Vector2((local.x+.5f)*(squareDisplay?1024:1440),(.5f-local.y)*(squareDisplay?1024:840));
            return Mathf.Abs(local.x)<=.5f && Mathf.Abs(local.y)<=.5f;
        }
        public int ActionAt(Vector2 point)
        {
            if(!IsReady)return -1;
            if(squareDisplay){
                if(new Rect(36,132,100,104).Contains(point))return 0;
                if(new Rect(36,255,100,104).Contains(point))return 1;
                if(new Rect(36,782,100,104).Contains(point))return 2;
                if(State==TabletState.Home&&new Rect(174,136,804,770).Contains(point))return 3;
                if(State==TabletState.Scanner){if(new Rect(550,815,414,96).Contains(point))return 4;if(new Rect(188,815,305,96).Contains(point))return 5;}
                return -1;
            }
            if(new Rect(44,135,96,98).Contains(point))return 0;
            if(new Rect(44,258,96,98).Contains(point))return 1;
            if(new Rect(44,634,96,98).Contains(point))return 2;
            if(State==TabletState.Home && new Rect(190,132,1207,568).Contains(point))return 3;
            if(State==TabletState.Scanner){if(new Rect(986,557,404,74).Contains(point))return 4;if(new Rect(736,697,156,50).Contains(point))return 5;}
            return -1;
        }

        void CreateDisplay()
        {
            screenTexture=new RenderTexture(squareDisplay?1536:1920,squareDisplay?1536:1120,24,RenderTextureFormat.ARGB32){name="Tablet display",antiAliasing=2};screenTexture.Create();
            screenMaterial.SetTexture("_MainTex",screenTexture);
            var go=new GameObject("Tablet display camera");screenCamera=go.AddComponent<Camera>();
            go.transform.position=new Vector3(0,-1000,-10);screenCamera.orthographic=true;screenCamera.orthographicSize=4.2f;screenCamera.nearClipPlane=.1f;screenCamera.farClipPlane=20;screenCamera.cullingMask=1<<30;screenCamera.clearFlags=CameraClearFlags.SolidColor;screenCamera.backgroundColor=Color.black;screenCamera.targetTexture=screenTexture;screenCamera.allowHDR=false;screenCamera.allowMSAA=true;
            screenUI=new GameObject("Tablet display canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasGroup));screenUI.layer=30;
            screenUI.transform.position=new Vector3(0,-1000,0);screenUI.transform.localScale=Vector3.one*.01f;
            var canvas=screenUI.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=screenCamera;
            ((RectTransform)screenUI.transform).sizeDelta=new Vector2(1440,840);
            screenGroup=screenUI.GetComponent<CanvasGroup>();
            var drawing=new GameObject("Display graphics",typeof(RectTransform),typeof(CanvasRenderer),typeof(TabletDisplay));drawing.layer=30;drawing.transform.SetParent(screenUI.transform,false);
            var rect=(RectTransform)drawing.transform;rect.sizeDelta=new Vector2(1440,840);graphic=drawing.GetComponent<TabletDisplay>();graphic.tablet=this;graphic.raycastTarget=false;
            foodTop=Label("Persistent food count",1330,24,78,46,24,TabletDisplay.Ink);
            homeIndex=Label("Module number",1282,150,84,40,24,TabletDisplay.Cyan);homeIndex.text="01";
            rangeText=Label("Scan range",746,704,72,46,19,TabletDisplay.Ink);
            contactsText=Label("Food contacts",990,270,340,100,66,TabletDisplay.Amber);
            pulseText=Label("Pulse status",1262,654,130,40,17,TabletDisplay.Cyan);
            clockText=Label("Clock",742,780,208,42,19,TabletDisplay.Muted);
            if(squareDisplay){
                screenCamera.orthographicSize=5.12f;
                ((RectTransform)screenUI.transform).sizeDelta=new Vector2(1024,1024);
                rect.sizeDelta=new Vector2(1024,1024);
                Place(foodTop,874,26,112,54,32);Place(homeIndex,875,159,76,40,24);
                Place(rangeText,230,855,120,44,28);Place(contactsText,853,138,105,58,36);
                Place(pulseText,808,934,148,48,24);Place(clockText,568,944,240,40,20);
            }
            CreateAlienLabels();
        }
        void CreateAlienLabels()
        {
            if(!alienFont)alienFont=Resources.Load<TMP_FontAsset>("Stray SDF");
            tabletTitle=AlienLabel("NANTES",44,28,300,52,32,TabletDisplay.Ink);
            pageTitle=AlienLabel("HOME",squareDisplay?382:296,36,240,36,20,TabletDisplay.Muted);
            statusText=AlienLabel("READY",squareDisplay?44:445,squareDisplay?947:784,380,32,19,TabletDisplay.Muted);
            bootText=AlienLabel("BOOTING",squareDisplay?212:420,squareDisplay?636:521,600,48,26,TabletDisplay.Ink);
            bootText.alignment=TextAlignmentOptions.Center;
            if(squareDisplay){
                AlienLabel("SCANNER",212,146,570,48,29,TabletDisplay.Ink,homeLabels);
                AlienLabel("OPEN SCANNER",213,848,560,48,24,TabletDisplay.Muted,homeLabels);
                AlienLabel("SCANNER",194,128,460,56,31,TabletDisplay.Ink,scannerLabels);
                AlienLabel("FOOD",717,146,90,36,18,TabletDisplay.Muted,scannerLabels);
                AlienLabel("RANGE",230,822,160,34,16,TabletDisplay.Muted,scannerLabels);
                AlienLabel("SCAN",646,835,286,56,31,TabletDisplay.Ink,scannerLabels);
            }else{
                AlienLabel("SCANNER",224,142,600,48,29,TabletDisplay.Ink,homeLabels);
                AlienLabel("OPEN SCANNER",225,643,600,48,24,TabletDisplay.Muted,homeLabels);
                AlienLabel("SCANNER",200,114,500,48,28,TabletDisplay.Ink,scannerLabels);
                AlienLabel("CONTACTS",1090,117,300,40,22,TabletDisplay.Muted,scannerLabels);
                AlienLabel("FOOD",1044,194,346,42,24,TabletDisplay.Muted,scannerLabels);
                AlienLabel("MOVEMENT",1044,355,346,42,24,TabletDisplay.Muted,scannerLabels);
                AlienLabel("SCAN",1082,570,274,48,29,TabletDisplay.Ink,scannerLabels);
                AlienLabel("SIGNAL",992,649,250,42,21,TabletDisplay.Muted,scannerLabels);
            }
        }
        TMP_Text AlienLabel(string text,float x,float y,float w,float h,float size,Color color,List<TMP_Text> page=null)
        {
            var label=Label(text,x,y,w,h,size,color);label.font=alienFont?alienFont:font;label.text=text;
            page?.Add(label);return label;
        }
        static void Place(TMP_Text label,float x,float y,float w,float h,float size){var r=label.rectTransform;r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);label.enableAutoSizing=false;label.fontSize=size;}
        TMP_Text Label(string name,float x,float y,float w,float h,float size,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI));go.layer=30;go.transform.SetParent(screenUI.transform,false);
            RectTransform rect=(RectTransform)go.transform;rect.anchorMin=rect.anchorMax=new Vector2(0,1);rect.pivot=new Vector2(0,1);rect.anchoredPosition=new Vector2(x,-y);rect.sizeDelta=new Vector2(w,h);
            var label=go.GetComponent<TextMeshProUGUI>();label.font=font;label.fontSize=size;label.color=color;label.raycastTarget=false;label.textWrappingMode=TextWrappingModes.NoWrap;label.overflowMode=TextOverflowModes.Truncate;label.alignment=TextAlignmentOptions.MidlineLeft;return label;
        }
        void UpdateLabels()
        {
            bool ready=IsReady;
            homeIndex.gameObject.SetActive(State==TabletState.Home);
            rangeText.gameObject.SetActive(State==TabletState.Scanner);contactsText.gameObject.SetActive(State==TabletState.Scanner);pulseText.gameObject.SetActive(State==TabletState.Scanner);
            foodTop.gameObject.SetActive(ready);clockText.gameObject.SetActive(ready);
            foodTop.text=FoodCount.ToString("000");
            tabletTitle.gameObject.SetActive(ready);pageTitle.gameObject.SetActive(ready);statusText.gameObject.SetActive(ready&&!HasFlashlightMeter);
            pageTitle.text=State==TabletState.Scanner?"SCANNER":"HOME";
            statusText.text=State==TabletState.Scanner&&ScanAge<1.8f?"SCANNING":"READY";
            bootText.gameObject.SetActive(State==TabletState.Booting||State==TabletState.ShuttingDown);
            bootText.text=State==TabletState.ShuttingDown?"SHUTTING DOWN":"BOOTING";
            foreach(var label in homeLabels)label.gameObject.SetActive(State==TabletState.Home);
            foreach(var label in scannerLabels)label.gameObject.SetActive(State==TabletState.Scanner);
            rangeText.text=Range.ToString("00");
            int found=0;foreach(var c in contacts)if(c.kind==ContactKind.Food && c.position.magnitude<=Range && ContactVisibility(c)>.1f)found++;
            contactsText.text=found.ToString("00");
            pulseText.text=ScanAge<1.8f?(Mathf.Clamp01(ScanAge/1.8f)*100).ToString("000")+"%":"100%";
            clockText.text=TimeSpan.FromSeconds(Clock).ToString(@"hh\:mm\:ss");
        }
        static AudioClip Tone(string name,float duration,float first,float last)
        {
            int rate=24000,n=(int)(rate*duration);float[] data=new float[n];float phase=0;
            for(int i=0;i<n;i++){float u=(float)i/n;phase+=Mathf.Lerp(first,last,u)*Mathf.PI*2/rate;float env=Mathf.Sin(Mathf.PI*u)*Mathf.Exp(-u*5);data[i]=Mathf.Sin(phase)*env*.22f;}
            var clip=AudioClip.Create(name,n,1,rate,false);clip.SetData(data,0);return clip;
        }
        void OnEnable(){if(screenCamera)screenCamera.enabled=true;if(HasFocus)AcquireFocus();}
        void OnDisable(){ReleaseFocus();if(screenCamera)screenCamera.enabled=false;}
        void OnDestroy()
        {
            ReleaseFocus();if(viewCamera)viewCamera.cullingMask=previousMask;
            if(screenTexture){screenTexture.Release();Destroy(screenTexture);}
            if(screenCamera)Destroy(screenCamera.gameObject);if(screenUI)Destroy(screenUI);
            if(screenMaterial)Destroy(screenMaterial);if(ledMaterial)Destroy(ledMaterial);
            if(clickSound)Destroy(clickSound);if(scanSound)Destroy(scanSound);
        }
    }
}
