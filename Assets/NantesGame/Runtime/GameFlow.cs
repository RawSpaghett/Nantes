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
            RefreshSave();
            menu.newGameRequested.AddListener(NewGame);menu.continueRequested.AddListener(Continue);menu.quitRequested.AddListener(Quit);
        }
        public void RefreshSave()
        {
            bool available=GameSave.TryRead(out _,out var message);
            menu.SetContinueAvailable(available);menu.continueHint.text=message;
        }
        public void NewGame(){StartNew(font);}
        public static void StartNew(TMP_FontAsset font)
        {
            ScreenTransition.Load(Level,font,()=>{
                var save=FindFirstObjectByType<LevelSave>();
                if(save)save.pause.ShowSaveResult(save.Save(out var message),message);
            });
        }
        public void Continue()
        {
            if(!GameSave.TryRead(out var data,out _)){RefreshSave();return;}
            ScreenTransition.Load(data.scene,font,()=>FindFirstObjectByType<LevelSave>().Restore(data));
        }
        public static void Quit(){
            if(ScreenTransition.Busy)return;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }
        void OnDestroy(){if(menu){menu.newGameRequested.RemoveListener(NewGame);menu.continueRequested.RemoveListener(Continue);menu.quitRequested.RemoveListener(Quit);}}
    }
}
