using UnityEngine;
using System;

public class Eyes: MonoBehaviour
{
    private IEyes parent;
    private Transform parentTransform;
    //[SerializeField] private float raycastDistance = 50f;

    void Awake()
    {
        parent = GetComponentInParent<IEyes>();
        parentTransform = (parent as Component).transform;
    }

    void OnTriggerStay(Collider other)
    {
            if(other.CompareTag("Player") && this.enabled)
            {
                parent.activeVision = true;
                parent.OnSee(); //Pursue
                /*
                Vector3 direction = (other.transform.position - parentTransform.position).normalized;
                if(Physics.Raycast(parentTransform.position,direction, out RaycastHit hit, raycastDistance)) //check if other stuff is in the way
                {
                    Debug.DrawRay(parentTransform.position, direction * raycastDistance, Color.red);
                    if(hit.collider == other)
                    {
                        parent.activeVision = true;
                        parent.OnSee(); //Pursue
                    }
                }
                */
            }
    }

    void OnTriggerExit(Collider other)
    {
        parent.activeVision = false;
        parent.playerGhost = other.transform;
    }


    
}
