using System.Collections.Generic;
using NantesGame.Tablet;
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
        public bool LookingDown { get; private set; }
        public bool Paused { get; set; }
        public bool CursorMode { get; private set; }
        float dwell,extension,velocity,nextContactUpdate;
        bool manualOff;
        InputAction jump,interact;
        InputAction look;
        bool lookWasEnabled;
        bool gated,jumpWasEnabled,interactWasEnabled;
        TabletState previousState;
        readonly List<Transform> targets=new List<Transform>();
        readonly List<ContactKind> kinds=new List<ContactKind>();
        readonly List<ScannerContact> contacts=new List<ScannerContact>();

        void Start(){tablet.ScanRequested+=Scan;tablet.SetFoodCount(0);mount.localPosition=foldedPosition;jump=actions.actions.FindAction("Jump");interact=actions.actions.FindAction("Interact");look=actions.actions.FindAction("Look");}
        void LateUpdate()
        {
            tablet.InputEnabled=!Paused&&LookingDown;
            if(Paused)return;
            GateInput(LookingDown&&tablet.HasFocus);
            if(CursorMode&&(!tablet.IsReady||Mouse.current!=null&&Mouse.current.rightButton.wasPressedThisFrame))SetCursorMode(false);
            float downward=Vector3.Dot(view.transform.forward,Vector3.down);
            bool desired=downward>(LookingDown?.47f:.69f);
            if(desired!=LookingDown){dwell+=Time.deltaTime;if(dwell>(desired?.20f:.35f)){LookingDown=desired;dwell=0;if(!desired){manualOff=false;tablet.PowerOff();}else if(!manualOff)tablet.PowerOn();}}
            else dwell=0;
            if(LookingDown&&previousState!=TabletState.ShuttingDown&&tablet.State==TabletState.ShuttingDown)manualOff=true;
            previousState=tablet.State;
            extension=Mathf.SmoothDamp(extension,LookingDown?1:0,ref velocity,.28f);
            mount.localPosition=Vector3.Lerp(foldedPosition,extendedPosition,extension);
            if(tablet.IsReady&&LookingDown){
                var k=Keyboard.current;
                if(k!=null){if(k.tabKey.wasPressedThisFrame)SetCursorMode(!CursorMode);if(k.digit1Key.wasPressedThisFrame)tablet.ShowHome();if(k.digit2Key.wasPressedThisFrame)tablet.ShowScanner();if(k.spaceKey.wasPressedThisFrame&&tablet.State==TabletState.Scanner)tablet.RequestScan();}
            }
            if(tablet.State==TabletState.Scanner&&Time.time>=nextContactUpdate){nextContactUpdate=Time.time+.08f;UpdateContacts();}
        }
        void Scan(float range)
        {
            targets.Clear();kinds.Clear();
            foreach(var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))Add(enemy.transform,ContactKind.Movement,range);
            UpdateContacts();
        }
        void Add(Transform target,ContactKind kind,float range){if((target.position-transform.position).sqrMagnitude>range*range)return;targets.Add(target);kinds.Add(kind);}
        void UpdateContacts()
        {
            contacts.Clear();
            for(int i=0;i<targets.Count;i++){
                if(!targets[i]||!targets[i].gameObject.activeInHierarchy)continue;
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
        void OnDisable(){SetCursorMode(false);GateInput(false);}
        void OnDestroy(){if(tablet)tablet.ScanRequested-=Scan;}
    }
}
