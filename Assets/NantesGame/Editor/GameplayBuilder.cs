using System.IO;
using System.Linq;
using NantesGame.Gameplay;
using NantesGame.Tablet;
using NantesGame.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.UI;

public static class GameplayBuilder
{
    const string Root="Assets/NantesGame";
    static TMP_FontAsset Font=>AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/NantesUI/Fonts/NantesDisplay SDF.asset");
    [MenuItem("Tools/Nantes/Prepare game UI")]
    public static void Prepare()
    {
        Directory.CreateDirectory(Root+"/Materials");Directory.CreateDirectory(Root+"/Prefabs");Directory.CreateDirectory(Root+"/Resources");AssetDatabase.Refresh();
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var screen=PrepareScreen();PrepareGameUI(screen);PrepareCredits();PrepareMenu();
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(GameFlow.Menu,true),new EditorBuildSettingsScene(GameFlow.Level,true)};
        AssetDatabase.SaveAssets();Debug.Log("GAME_UI_PREPARE_PASS");
    }
    static Material Mat(string name,string shader,Color color)
    {
        string path=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!m){m=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(m,path);}
        m.color=color;if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.37f);return m;
    }
    static GameObject PrepareScreen()
    {
        var script=AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/NantesTablet/Runtime/TabletController.cs");
        if(script.GetClass()!=typeof(TabletController))throw new System.Exception("Tablet controller script did not import.");
        var root=new GameObject("Chest screen");
        var body=Mat("Tablet casing","Universal Render Pipeline/Lit",new Color(.027f,.035f,.038f));
        Cube("Housing",root.transform,new Vector3(0,-.18f,0),new Vector3(10.3f,.34f,10.3f),body);
        Cube("Rim L",root.transform,new Vector3(-4.86f,.05f,0),new Vector3(.54f,.15f,10.3f),body);
        Cube("Rim R",root.transform,new Vector3(4.86f,.05f,0),new Vector3(.54f,.15f,10.3f),body);
        Cube("Rim top",root.transform,new Vector3(0,.05f,4.86f),new Vector3(9.2f,.15f,.54f),body);
        Cube("Rim bottom",root.transform,new Vector3(0,.05f,-4.86f),new Vector3(9.2f,.15f,.54f),body);
        var screen=GameObject.CreatePrimitive(PrimitiveType.Quad);screen.name="Screen";screen.transform.SetParent(root.transform,false);
        screen.transform.localPosition=new Vector3(0,.05f,0);screen.transform.localRotation=Quaternion.Euler(90,0,0);screen.transform.localScale=Vector3.one*9.16f;
        Object.DestroyImmediate(screen.GetComponent<Collider>());
        var renderer=screen.GetComponent<Renderer>();renderer.sharedMaterial=Mat("Chest display","Nantes/Tablet Glass",Color.white);renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
        var tablet=root.AddComponent<TabletController>();tablet.font=Font;tablet.alienFont=NantesGame.Tablet.Editor.TabletFont.Prepare();tablet.displaySurface=screen.transform;tablet.screenRenderer=renderer;
        tablet.squareDisplay=true;tablet.animateHousing=false;tablet.manageCursor=false;tablet.gazeInput=true;tablet.wakeOnStart=false;tablet.usePreviewKeys=false;
        var serialized=new SerializedObject(tablet);serialized.FindProperty("m_Script").objectReferenceValue=AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/NantesTablet/Runtime/TabletController.cs");serialized.ApplyModifiedPropertiesWithoutUndo();
        root.SetActive(false);
        var prefab=PrefabUtility.SaveAsPrefabAsset(root,Root+"/Prefabs/ChestScreen.prefab");Object.DestroyImmediate(root);return prefab;
    }
    static void Cube(string name,Transform parent,Vector3 position,Vector3 scale,Material mat)
    {var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=mat;Object.DestroyImmediate(go.GetComponent<Collider>());}
    static void PrepareGameUI(GameObject screen)
    {
        var canvas=Canvas("Game interface");var pause=canvas.gameObject.AddComponent<GamePause>();
        var link=canvas.gameObject.AddComponent<SceneUI>();link.pause=pause;link.screenPrefab=screen;
        pause.hint=Text(canvas.transform,"Controls","",new Vector2(0,-502),new Vector2(1780,46),17);
        var aim=new GameObject("Aim",typeof(RectTransform),typeof(Image));aim.transform.SetParent(canvas.transform,false);aim.GetComponent<RectTransform>().sizeDelta=new Vector2(3,3);aim.GetComponent<Image>().color=new Color(.77f,.84f,.78f,.7f);aim.GetComponent<Image>().raycastTarget=false;pause.aim=aim;
        var panel=Panel(canvas.transform,"Pause panel",Color.clear);pause.panel=panel;pause.presentation=panel.AddComponent<CanvasGroup>();
        var backdrop=new GameObject("Dark glass",typeof(RectTransform),typeof(RawImage));backdrop.transform.SetParent(panel.transform,false);
        var dr=backdrop.GetComponent<RectTransform>();dr.anchorMin=Vector2.zero;dr.anchorMax=Vector2.one;dr.offsetMin=dr.offsetMax=Vector2.zero;
        pause.backdrop=backdrop.GetComponent<RawImage>();pause.backdrop.raycastTarget=false;pause.backdrop.material=Mat("Pause backdrop","Nantes/Pause Backdrop",Color.white);
        var brand=Text(panel.transform,"Nantes","NANTES",new Vector2(-430,382),new Vector2(600,40),17);brand.alignment=TextAlignmentOptions.MidlineLeft;brand.characterSpacing=8;brand.color=new Color(.43f,.58f,.55f);
        var alien=Text(panel.transform,"Alien pause","PAUSED",new Vector2(-410,126),new Vector2(660,70),30);alien.font=NantesGame.Tablet.Editor.TabletFont.Prepare();alien.alignment=TextAlignmentOptions.MidlineLeft;alien.color=new Color(.22f,.43f,.42f,.8f);
        var title=Text(panel.transform,"Title","PAUSED",new Vector2(-410,28),new Vector2(660,110),70);title.alignment=TextAlignmentOptions.MidlineLeft;title.characterSpacing=2;
        Rule(panel.transform,"Title accent",new Vector2(-701,-67),new Vector2(58,2),new Color(.32f,.57f,.53f));
        Rule(panel.transform,"Divider",new Vector2(17,0),new Vector2(1,490),new Color(.28f,.42f,.43f,.22f));
        Rule(panel.transform,"Top edge",new Vector2(402,288),new Vector2(604,1),new Color(.28f,.42f,.43f,.28f));
        var help=Text(panel.transform,"Pause controls","ARROWS Navigate     ENTER Select     ESC Resume",new Vector2(-290,-410),new Vector2(880,36),13);help.alignment=TextAlignmentOptions.MidlineLeft;help.color=new Color(.38f,.49f,.48f);
        var options=new GameObject("Options",typeof(RectTransform));options.transform.SetParent(panel.transform,false);pause.options=options.GetComponent<RectTransform>();pause.options.anchoredPosition=new Vector2(402,0);pause.options.sizeDelta=new Vector2(604,500);
        pause.resume=PauseButton(pause,0,"RESUME",159);pause.restart=PauseButton(pause,1,"RESTART",53);
        pause.mainMenu=PauseButton(pause,2,"MAIN MENU",-53);pause.quit=PauseButton(pause,3,"QUIT",-159);
        var events=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));events.transform.SetParent(canvas.transform,false);events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        PrefabUtility.SaveAsPrefabAsset(canvas.gameObject,Root+"/Resources/NantesGameUI.prefab");Object.DestroyImmediate(canvas.gameObject);
    }
    static void PrepareCredits()
    {
        const string path="Assets/NantesUI/Prefabs/NantesMenu.prefab";
        var root=PrefabUtility.LoadPrefabContents(path);
        try
        {
            foreach(var text in root.GetComponentsInChildren<TMP_Text>(true))if(text.name=="Asset credits")
            {
                text.text=NantesCredits.Text;text.fontSize=12;text.enableAutoSizing=false;
                text.rectTransform.sizeDelta=new Vector2(610,230);
            }
            PrefabUtility.SaveAsPrefabAsset(root,path);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
    }
    static void PrepareMenu()
    {
        var scene=EditorSceneManager.OpenScene(GameFlow.Menu);
        foreach(var preview in Object.FindObjectsByType<NantesMenuPreview>(FindObjectsSortMode.None))Object.DestroyImmediate(preview);
        var old=GameObject.Find("Game loading");if(old)Object.DestroyImmediate(old);
        old=GameObject.Find("Menu flow");if(old)Object.DestroyImmediate(old);
        var flow=new GameObject("Menu flow").AddComponent<GameFlow>();flow.menu=Object.FindFirstObjectByType<NantesMenu>();flow.font=Font;
        EditorSceneManager.SaveScene(scene);
    }
    static Canvas Canvas(string name)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var c=go.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=100;
        var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;return c;
    }
    static GameObject Panel(Transform parent,string name,Color color)
    {var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);var r=go.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;go.GetComponent<Image>().color=color;return go;}
    static TMP_Text Text(Transform parent,string name,string text,Vector2 position,Vector2 size,float fontSize)
    {var go=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);var r=go.GetComponent<RectTransform>();r.anchoredPosition=position;r.sizeDelta=size;var t=go.GetComponent<TMP_Text>();t.font=Font;t.fontSize=fontSize;t.text=text;t.alignment=TextAlignmentOptions.Center;t.color=new Color(.77f,.84f,.78f);t.raycastTarget=false;return t;}
    static void Rule(Transform parent,string name,Vector2 position,Vector2 size,Color color)
    {var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);go.GetComponent<RectTransform>().anchoredPosition=position;go.GetComponent<RectTransform>().sizeDelta=size;go.GetComponent<Image>().color=color;go.GetComponent<Image>().raycastTarget=false;}
    static Button PauseButton(GamePause pause,int index,string text,float y)
    {
        var go=new GameObject(text,typeof(RectTransform),typeof(CanvasRenderer),typeof(PauseOption),typeof(Button));go.transform.SetParent(pause.options,false);
        var r=go.GetComponent<RectTransform>();r.pivot=new Vector2(0,.5f);r.anchoredPosition=new Vector2(-302,y);r.sizeDelta=new Vector2(604,82);
        var art=go.GetComponent<PauseOption>();art.owner=pause;art.index=index;
        art.label=Text(go.transform,"Label",text,new Vector2(66,0),new Vector2(470,60),22);art.label.alignment=TextAlignmentOptions.MidlineLeft;art.label.characterSpacing=1;
        art.label.rectTransform.anchorMin=art.label.rectTransform.anchorMax=art.label.rectTransform.pivot=new Vector2(0,.5f);
        art.number=Text(go.transform,"Number",(index+1).ToString("00"),new Vector2(21,0),new Vector2(35,40),12);art.number.alignment=TextAlignmentOptions.MidlineLeft;art.number.rectTransform.anchorMin=art.number.rectTransform.anchorMax=art.number.rectTransform.pivot=new Vector2(0,.5f);
        var b=go.GetComponent<Button>();b.targetGraphic=art;b.transition=Selectable.Transition.None;return b;
    }
    public static void Build()
    {
        Prepare();var args=System.Environment.GetCommandLineArgs();int index=System.Array.IndexOf(args,"-gameOutput");
        var output=index>=0&&index+1<args.Length?args[index+1]:Path.GetFullPath(Path.Combine(Application.dataPath,"../Build/Nantes.exe"));Directory.CreateDirectory(Path.GetDirectoryName(output));
        var options=System.Array.IndexOf(args,"-gameChecks")>=0?BuildOptions.Development:BuildOptions.None;
        var result=BuildPipeline.BuildPlayer(EditorBuildSettings.scenes.Select(s=>s.path).ToArray(),output,BuildTarget.StandaloneWindows64,options);
        if(result.summary.result!=BuildResult.Succeeded)throw new System.Exception("Game build failed: "+result.summary.result);
        Debug.Log("GAME_BUILD_PASS "+output);EditorApplication.Exit(0);
    }
}
