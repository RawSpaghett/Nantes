using NantesGame.World;
using UnityEngine;

namespace NantesGame.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class FoodPickup : MonoBehaviour, IInteractable
    {
        public string SaveId { get; private set; }
        public bool Collected { get; private set; }
        static readonly RaycastHit[] nearby = new RaycastHit[24];

        public static FoodPickup InReach(Camera view,float distance,int mask)
        {
            //A little aiming room for small food props. Walls still block pickup.
            var ray=new Ray(view.transform.position,view.transform.forward);
            int count=Physics.SphereCastNonAlloc(ray,.12f,nearby,distance,mask,QueryTriggerInteraction.Ignore);
            FoodPickup closest=null;
            float nearest=distance;
            for(int i=0;i<count;i++)
            {
                var food=nearby[i].collider.GetComponentInParent<FoodPickup>();
                if(!food || food.Collected) continue;
                Vector3 direction=nearby[i].collider.bounds.center-ray.origin;
                if(direction.magnitude>nearest) continue;
                if(!Physics.Raycast(ray.origin,direction.normalized,out var hit,direction.magnitude+.02f,mask,QueryTriggerInteraction.Ignore)) continue;
                if(hit.collider.GetComponentInParent<FoodPickup>()!=food) continue;
                closest=food;nearest=direction.magnitude;
            }
            return closest;
        }

        void Awake()
        {
            SaveId = name + "[" + transform.GetSiblingIndex() + "]";
            for(var parent = transform.parent; parent; parent = parent.parent)
                SaveId = parent.name + "[" + parent.GetSiblingIndex() + "]/" + SaveId;
        }

        public void Interact(PlayerInteractor player)
        {
            if(Collected) return;
            var chest = player.GetComponent<ChestTablet>();
            if(!chest || chest.Paused || chest.Raised) return;
            chest.input.interactTriggered = false;
            chest.tablet.AddFood();
            SetCollected();
        }

        public void SetCollected()
        {
            Collected = true;
            if(TryGetComponent<FoodScanTarget>(out var target)) target.Reveal(0);
            gameObject.SetActive(false);
        }
    }
}
