using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NantesGame.Gameplay
{
    public sealed class GamePause : MonoBehaviour
    {
        public GameObject panel,aim;
        public CanvasGroup presentation;
        public RectTransform options;
        public RawImage backdrop;
        public Button resume,save,restart,mainMenu,quit;
        public TMP_Text hint,saveStatus;
        public LevelSave saving;
        public PlayerInputHandler input;
        public ChestTablet chest;
        public PlayerMovement movement;
        public PlayerInput actions;
        public int Focused { get; private set; }
        public bool IsPaused { get; private set; }
        bool held,movementWasEnabled,actionsWereActive,transitioning;
        float started,fadeVelocity;
        Vector2 pointer,optionsPosition;
        Material backdropMaterial;
        RenderTexture frozenView;
        void Start()
        {
            started=Time.unscaledTime;optionsPosition=options.anchoredPosition;
            if(Mouse.current!=null)pointer=Mouse.current.position.ReadValue();
            backdropMaterial=new Material(backdrop.material);backdrop.material=backdropMaterial;
            presentation.alpha=0;presentation.blocksRaycasts=false;panel.SetActive(false);
            resume.onClick.AddListener(()=>SetPaused(false));restart.onClick.AddListener(Restart);
            save.onClick.AddListener(SaveGame);mainMenu.onClick.AddListener(ReturnToMenu);quit.onClick.AddListener(SaveAndQuit);
            SetTransition(ScreenTransition.Busy);
        }
        void Update()
        {
            float dt=Mathf.Min(Time.unscaledDeltaTime,.05f);
            if(ScreenTransition.Busy){hint.text="";return;}
            if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame||Gamepad.current!=null&&Gamepad.current.startButton.wasPressedThisFrame)SetPaused(!IsPaused);
            if(Keyboard.current!=null&&Keyboard.current.f11Key.wasPressedThisFrame)Screen.fullScreen=!Screen.fullScreen;
            if(panel.activeSelf){
                bool reduced=PlayerPrefs.GetInt("Nantes.UI.ReducedMotion",0)==1;
                presentation.alpha=reduced?(IsPaused?1:0):Mathf.SmoothDamp(presentation.alpha,IsPaused?1:0,ref fadeVelocity,.15f,100,dt);
                options.anchoredPosition=optionsPosition+new Vector2((1-presentation.alpha)*26,0);
                backdropMaterial.SetFloat("_MotionTime",reduced?0:Time.unscaledTime);
                if(!IsPaused&&presentation.alpha<.003f)panel.SetActive(false);
            }
            float master=PlayerPrefs.GetFloat("Nantes.UI.MasterVolume",1);
            AudioListener.volume=Mathf.Lerp(AudioListener.volume,master*(IsPaused?.38f:1),1-Mathf.Exp(-dt*9));
            if(IsPaused){hint.text="";return;}
            if(chest.LookingDown){hint.text=chest.CursorMode?"CLICK  Select     TAB / RIGHT CLICK  Look around":"TAB  Use tablet     1  Home     2  Scanner     SPACE  Scan";return;}
            hint.text=Time.unscaledTime-started<22?"WASD  Move     MOUSE  Look     LOOK DOWN  Tablet     HOLD R  Charge light     ESC  Pause":"";
        }
        public void PointerFocus(int index)
        {
            if(Mouse.current==null)return;
            Vector2 position=Mouse.current.position.ReadValue();
            if((position-pointer).sqrMagnitude<.5f)return;
            pointer=position;Focused=index;
            if(EventSystem.current)EventSystem.current.SetSelectedGameObject(null);
        }
        public void KeyboardFocus(int index){Focused=index;if(Mouse.current!=null)pointer=Mouse.current.position.ReadValue();}
        public void SetPaused(bool value)
        {
            if(ScreenTransition.Busy||IsPaused==value)return;
            IsPaused=value;HoldControls(value);Time.timeScale=value?0:1;
            presentation.blocksRaycasts=value;presentation.interactable=value;if(value){CaptureBackdrop();panel.SetActive(true);}
            if(aim)aim.SetActive(!value);
            Cursor.lockState=value?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=value;
            if(value){Focused=0;if(Mouse.current!=null)pointer=Mouse.current.position.ReadValue();}
            if(EventSystem.current)EventSystem.current.SetSelectedGameObject(value?resume.gameObject:null);
        }
        public void SetTransition(bool value)
        {
            transitioning=value;HoldControls(value||IsPaused);Time.timeScale=value||IsPaused?0:1;
            Cursor.lockState=value||IsPaused?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=!value&&IsPaused;
            if(aim)aim.SetActive(!value&&!IsPaused);
        }
        void CaptureBackdrop()
        {
            if(!frozenView){frozenView=new RenderTexture(640,360,24){name="Pause view",filterMode=FilterMode.Bilinear};frozenView.Create();backdrop.texture=frozenView;}
            if(GraphicsSettings.currentRenderPipeline)RenderPipeline.SubmitRenderRequest(chest.view,new UniversalRenderPipeline.SingleCameraRequest{destination=frozenView});
            else{var previous=chest.view.targetTexture;chest.view.targetTexture=frozenView;chest.view.Render();chest.view.targetTexture=previous;}
        }
        void HoldControls(bool value)
        {
            chest.SetCursorMode(false);chest.GateInput(false);chest.Paused=value;chest.tablet.InputEnabled=!value;
            if(held==value)return;
            if(value){movementWasEnabled=movement.enabled;actionsWereActive=actions.inputIsActive;movement.enabled=false;actions.DeactivateInput();}
            else{movement.enabled=movementWasEnabled;if(actionsWereActive)actions.ActivateInput();}
            input.move=input.look=Vector2.zero;input.interactTriggered=false;input.crankHeld=false;input.sprintHeld=input.walkHeld=false;held=value;
        }
        public void ShowSaveResult(bool success,string message){saveStatus.text=message;saveStatus.color=success?new Color(.5f,.7f,.63f):new Color(.9f,.65f,.5f);}
        public void SaveGame(){if(ScreenTransition.Busy)return;ShowSaveResult(saving.Save(out var message),message);}
        bool SaveBeforeLeaving()
        {
            if(ScreenTransition.Busy)return false;
            bool success=saving.Save(out var message);ShowSaveResult(success,message);return success;
        }
        public void Restart(){GameFlow.StartNew(hint.font);}
        public void ReturnToMenu(){if(SaveBeforeLeaving())ScreenTransition.Load(GameFlow.Menu,hint.font);}
        public void SaveAndQuit(){if(SaveBeforeLeaving())GameFlow.Quit();}
        void OnApplicationFocus(bool focused){if(!focused&&!IsPaused&&!transitioning&&!ScreenTransition.Busy)SetPaused(true);}
        void OnDestroy(){if(backdropMaterial)Destroy(backdropMaterial);if(frozenView){frozenView.Release();Destroy(frozenView);}if(!ScreenTransition.Busy)Time.timeScale=1;}
    }
}
