using NantesGame.UI;
using TMPro;
using UnityEngine;

namespace NantesGame.Gameplay
{
    public sealed class GameFlow : MonoBehaviour
    {
        public const string Level="Assets/Boilerplate/Scenes/Prototype_Main.unity";
        public const string Menu="Assets/NantesUI/Scenes/NantesMenuPreview.unity";
        public NantesMenu menu;
        public TMP_FontAsset font;
        public bool IsLoading=>ScreenTransition.Busy;
        void Start()
        {
            Time.timeScale=1;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
            menu.SetContinueAvailable(false);
            menu.newGameRequested.AddListener(NewGame);menu.quitRequested.AddListener(Quit);
        }
        public void NewGame(){ScreenTransition.Load(Level,font);}
        public static void Quit(){
            if(ScreenTransition.Busy)return;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }
        void OnDestroy(){if(menu){menu.newGameRequested.RemoveListener(NewGame);menu.quitRequested.RemoveListener(Quit);}}
    }
}
