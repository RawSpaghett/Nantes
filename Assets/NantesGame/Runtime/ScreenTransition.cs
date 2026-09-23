using System.Collections;
using NantesGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NantesGame.Gameplay
{
    public sealed class ScreenTransition : MonoBehaviour
    {
        static ScreenTransition current;
        public static bool Busy=>current!=null;
        public static float Cover=>current?current.cover.alpha:0;
        CanvasGroup cover;
        TMP_Text status;
        float volume,audioLevel;
        bool reduced;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetState(){current=null;}
        public static void Load(string scene,TMP_FontAsset font)
        {
            if(Busy)return;
            if(!Application.CanStreamedLevelBeLoaded(scene)){Debug.LogError("Scene is missing from the build: "+scene);return;}
            var root=new GameObject("Scene transition",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            DontDestroyOnLoad(root);current=root.AddComponent<ScreenTransition>();current.Create(font);current.StartCoroutine(current.Run(scene));
        }
        void Create(TMP_FontAsset font)
        {
            volume=PlayerPrefs.GetFloat("Nantes.UI.MasterVolume",1);audioLevel=AudioListener.volume;reduced=PlayerPrefs.GetInt("Nantes.UI.ReducedMotion",0)==1;
            var canvas=GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=32000;
            var scaler=GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
            var veil=new GameObject("Fade",typeof(RectTransform),typeof(Image),typeof(CanvasGroup));veil.transform.SetParent(transform,false);
            var rect=veil.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            veil.GetComponent<Image>().color=new Color(.003f,.007f,.01f,1);cover=veil.GetComponent<CanvasGroup>();cover.alpha=0;
            var text=new GameObject("Loading status",typeof(RectTransform),typeof(TextMeshProUGUI));text.transform.SetParent(veil.transform,false);
            var tr=text.GetComponent<RectTransform>();tr.anchorMin=tr.anchorMax=new Vector2(.5f,0);tr.anchoredPosition=new Vector2(0,88);tr.sizeDelta=new Vector2(600,40);
            status=text.GetComponent<TextMeshProUGUI>();status.font=font;status.fontSize=15;status.characterSpacing=5;status.alignment=TextAlignmentOptions.Center;status.raycastTarget=false;status.text="";status.color=new Color(.38f,.5f,.48f);
        }
        IEnumerator Run(string scene)
        {
            var sourceEvents=EventSystem.current;if(sourceEvents)sourceEvents.enabled=false;
            var sourcePause=FindFirstObjectByType<GamePause>();if(sourcePause)sourcePause.SetTransition(true);
            float startingVolume=AudioListener.volume;
            yield return Fade(0,1,reduced?.16f:.8f,startingVolume,0);
            var operation=SceneManager.LoadSceneAsync(scene,LoadSceneMode.Single);operation.allowSceneActivation=false;
            float began=Time.unscaledTime;
            while(operation.progress<.9f){if(Time.unscaledTime-began>.65f)status.text="LOADING";yield return null;}
            operation.allowSceneActivation=true;
            while(!operation.isDone)yield return null;
            var destinationPause=FindFirstObjectByType<GamePause>();if(destinationPause)destinationPause.SetTransition(true);
            var destinationEvents=EventSystem.current;if(destinationEvents)destinationEvents.enabled=false;
            var logo=FindFirstObjectByType<NantesLogoAnimator>();if(logo){logo.automatic=false;logo.Evaluate(0);}
            AudioListener.volume=0;status.text="";
            yield return null;yield return null;
            yield return Fade(1,0,reduced?.2f:1f,0,volume);
            if(destinationPause)destinationPause.SetTransition(false);
            if(destinationEvents)destinationEvents.enabled=true;
            if(logo)logo.automatic=true;
            audioLevel=volume;AudioListener.volume=volume;current=null;Destroy(gameObject);
        }
        IEnumerator Fade(float from,float to,float duration,float fromVolume,float toVolume)
        {
            float t=0;
            while(t<duration){t+=Time.unscaledDeltaTime;float p=Mathf.SmoothStep(0,1,Mathf.Clamp01(t/duration));cover.alpha=Mathf.Lerp(from,to,p);audioLevel=Mathf.Lerp(fromVolume,toVolume,p);AudioListener.volume=audioLevel;yield return null;}
            cover.alpha=to;audioLevel=toVolume;AudioListener.volume=toVolume;
        }
        void LateUpdate(){AudioListener.volume=audioLevel;}
        void OnDestroy(){if(current==this){current=null;AudioListener.volume=volume;Time.timeScale=1;}}
    }
}
