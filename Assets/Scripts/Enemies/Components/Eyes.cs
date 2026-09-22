using UnityEngine;
using System;

public class Eyes: MonoBehaviour
{
    private IEyes parent;
    [SerializeField] private float raycastDistance = 50f;

    void Awake()
    {
        parent = GetComponentInParent<IEyes>();
    }

    void OnTriggerStay(Collider other)
    {
            if(other.CompareTag("Player") && this.enabled)
            {
                if(Physics.Raycast(transform.position,(other.transform.position - transform.position).normalized, out RaycastHit hit, raycastDistance)) //check if other stuff is in the way
                {
                    Debug.DrawRay(transform.position, (other.transform.position - transform.position).normalized * raycastDistance, Color.red);
                    if(hit.collider == other)
                    {
                        parent.activeVision = true;
                        parent.OnSee(); //Pursue
                    }
                }
            }
    }

    void OnTriggerExit(Collider other)
    {
        parent.activeVision = false;
        parent.playerGhost = other.transform;
    }


    
}
