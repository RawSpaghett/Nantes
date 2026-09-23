using System.Linq;
using NantesGame.Tablet;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace NantesGame.Gameplay
{
    public sealed class SceneUI : MonoBehaviour
    {
        public GameObject screenPrefab;
        public GamePause pause;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register(){SceneManager.sceneLoaded-=Loaded;SceneManager.sceneLoaded+=Loaded;}
        static void Loaded(Scene scene,LoadSceneMode mode)
        {
            if(scene.path!=GameFlow.Level||FindFirstObjectByType<SceneUI>())return;
            var prefab=Resources.Load<GameObject>("NantesGameUI");if(prefab)Instantiate(prefab);
        }
        void Awake()
        {
            var player=FindFirstObjectByType<PlayerMovement>();
            if(!player){Debug.LogError("The level has no player for the tablet UI.");return;}
            var mount=player.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name=="Scanner");
            if(!mount){Debug.LogError("The player has no Scanner surface.");return;}
            var view=player.GetComponentInChildren<Camera>();
            var display=Instantiate(screenPrefab,mount,false);
            var tablet=display.GetComponent<TabletController>();tablet.viewCamera=view;
            var chest=player.gameObject.AddComponent<ChestTablet>();chest.tablet=tablet;chest.mount=mount;chest.view=view;
            chest.input=player.GetComponent<PlayerInputHandler>();chest.actions=player.GetComponent<PlayerInput>();
            chest.extendedPosition=mount.localPosition+new Vector3(0,0,.18f);chest.foldedPosition=mount.localPosition+new Vector3(0,-.07f,-.44f);
            pause.input=chest.input;pause.chest=chest;pause.movement=player;pause.actions=chest.actions;
            display.SetActive(true);
        }
    }
}
