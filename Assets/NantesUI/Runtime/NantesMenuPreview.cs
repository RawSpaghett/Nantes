using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using TMPro;

namespace NantesGame.UI
{
    // Preview scene only; keep this off the UI prefab.
    [MovedFrom("Nantes.UI")]
    public sealed class NantesMenuPreview : MonoBehaviour
    {
        public NantesMenu menu;
        public TMP_Text previewNote;

        void Start()
        {
#if !UNITY_EDITOR
            if (menu.applyPreferences)
            {
                var display = Screen.currentResolution;
                Screen.SetResolution(display.width, display.height, FullScreenMode.FullScreenWindow);
                menu.fullscreenToggle.SetIsOnWithoutNotify(true);
            }
#endif
            menu.newGameRequested.AddListener(PreviewNewGame);
            menu.quitRequested.AddListener(QuitPreview);
        }

        void PreviewNewGame()
        {
            previewNote.text = "MENU PREVIEW  /  Gameplay coming soon";
        }

        void QuitPreview()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void OnDestroy()
        {
            if (menu == null) return;
            menu.newGameRequested.RemoveListener(PreviewNewGame);
            menu.quitRequested.RemoveListener(QuitPreview);
        }
    }
}
