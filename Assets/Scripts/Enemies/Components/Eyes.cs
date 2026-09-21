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
                if(Physics.Raycast(transform.position,(other.transform.position - transform.position), out RaycastHit hit, raycastDistance)) //check if other stuff is in the way
                {
                    if(hit.collider == other)
                    {
                        parent.OnSee(other.transform.position); //Pursue
                    }
                }
            }
        
    }
    
}
