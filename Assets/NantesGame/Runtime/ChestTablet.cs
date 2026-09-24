using System.Collections.Generic;
using NantesGame.Tablet;
using NantesGame.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NantesGame.Gameplay
{
    [DefaultExecutionOrder(60)]
    public sealed class ChestTablet : MonoBehaviour
    {
        public TabletController tablet;
        public Transform mount;
        public Camera view;
        public PlayerInputHandler input;
        public PlayerInput actions;
        public Vector3 extendedPosition;
        public Vector3 foldedPosition;
        [Min(0)] public float foodRevealSeconds = 8;
        public bool Raised { get; private set; }
        public bool Paused { get; set; }
        public bool CursorMode { get; private set; }
        float extension,velocity,scanUntil;
        float viewHeight;
        Renderer[] housing;
        InputAction jump,interact;
        InputAction look;
        bool lookWasEnabled;
        bool gated,jumpWasEnabled,interactWasEnabled;
        readonly List<Transform> targets=new List<Transform>();
        readonly List<ContactKind> kinds=new List<ContactKind>();
        readonly List<ScannerContact> contacts=new List<ScannerContact>();

        void Start(){tablet.ScanRequested+=Scan;mount.localPosition=foldedPosition;housing=mount.GetComponentsInChildren<Renderer>(true);SetVisible(false);viewHeight=transform.InverseTransformPoint(view.transform.position).y;jump=actions.actions.FindAction("Jump");interact=actions.actions.FindAction("Interact");look=actions.actions.FindAction("Look");}
        void LateUpdate()
        {
            if(Paused)return;
            var k=Keyboard.current;
            if(k!=null&&k.tabKey.wasPressedThisFrame)SetRaised(!Raised);
            if(Raised&&tablet.State==TabletState.ShuttingDown)SetRaised(false);
            GateInput(Raised);SetCursorMode(Raised);tablet.InputEnabled=Raised;
            extension=Mathf.SmoothDamp(extension,Raised?1:0,ref velocity,.22f);
            SetVisible(extension>.02f);
            float crouchOffset=transform.InverseTransformPoint(view.transform.position).y-viewHeight;
            mount.localPosition=Vector3.Lerp(foldedPosition,extendedPosition,extension)+Vector3.up*crouchOffset;
            mount.localRotation=Quaternion.Euler(Mathf.Lerp(-90,-25,extension),0,0);
            //Glance down at the chest mount while it unfolds.
            float headPitch=Mathf.DeltaAngle(0,view.transform.parent.eulerAngles.x);
            Quaternion glance=Quaternion.Euler(45-headPitch,view.transform.localEulerAngles.y,view.transform.localEulerAngles.z);
            view.transform.localRotation=Quaternion.Slerp(view.transform.localRotation,glance,Mathf.SmoothStep(0,1,extension));
            if(Raised&&tablet.IsReady&&k!=null&&k.spaceKey.wasPressedThisFrame)tablet.RequestScan();
            UpdateContacts();
        }
        void SetVisible(bool value){if(housing==null)return;foreach(var renderer in housing)if(renderer)renderer.enabled=value;}
        public void SetRaised(bool value)
        {
            Raised=value;
            if(value)tablet.PowerOn();else tablet.PowerOff();
            GateInput(value);SetCursorMode(value);
        }
        void Scan(float range)
        {
            targets.Clear();kinds.Clear();
            scanUntil=Time.time+foodRevealSeconds;
            foreach(var food in FindObjectsByType<FoodScanTarget>(FindObjectsSortMode.None))
            {
                if(food.gameObject.scene!=gameObject.scene || !food.isActiveAndEnabled)continue;
                bool inRange=(food.transform.position-transform.position).sqrMagnitude<=range*range;
                food.Reveal(inRange?foodRevealSeconds:0);
                Add(food.transform,ContactKind.Food,range);
            }
            foreach(var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
                if(enemy.gameObject.scene==gameObject.scene)Add(enemy.transform,ContactKind.Movement,range);
            UpdateContacts();
        }
        void Add(Transform target,ContactKind kind,float range){if((target.position-transform.position).sqrMagnitude>range*range)return;targets.Add(target);kinds.Add(kind);}
        void UpdateContacts()
        {
            contacts.Clear();
            for(int i=0;i<targets.Count;i++){
                if(!targets[i]||!targets[i].gameObject.activeInHierarchy)continue;
                if(Time.time>=scanUntil || (targets[i].position-transform.position).sqrMagnitude>tablet.Range*tablet.Range){
                    if(targets[i].TryGetComponent<FoodScanTarget>(out var outside))outside.Reveal(0);
                    targets.RemoveAt(i);kinds.RemoveAt(i--);continue;
                }
                if(kinds[i]==ContactKind.Food && (!targets[i].TryGetComponent<FoodScanTarget>(out var food) || !food.IsRevealed))continue;
                Vector3 p=transform.InverseTransformDirection(targets[i].position-transform.position);
                contacts.Add(new ScannerContact(new Vector2(p.x,p.z),kinds[i]));
            }
            tablet.SetContacts(contacts);
        }
        public void GateInput(bool value)
        {
            if(gated==value)return;
            if(value){jumpWasEnabled=jump!=null&&jump.enabled;interactWasEnabled=interact!=null&&interact.enabled;jump?.Disable();interact?.Disable();input.interactTriggered=false;}
            else{if(jumpWasEnabled)jump?.Enable();if(interactWasEnabled)interact?.Enable();}
            gated=value;
        }
        public void SetCursorMode(bool value)
        {
            if(CursorMode==value)return;
            CursorMode=value;tablet.gazeInput=!value;
            if(value){lookWasEnabled=look!=null&&look.enabled;look?.Disable();input.look=Vector2.zero;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
            else{if(lookWasEnabled)look?.Enable();Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;}
        }
        void OnDisable(){SetCursorMode(false);GateInput(false);SetVisible(false);}
        void OnDestroy(){if(tablet)tablet.ScanRequested-=Scan;}
    }
}
