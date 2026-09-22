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
                        parent.OnSee(other.transform.position); //Pursue
                    }
                }
            }
    }

/*
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && this.enabled)
            {
                if(Physics.Raycast(transform.position,(other.transform.position - transform.position).normalized, out RaycastHit hit, raycastDistance)) //check if other stuff is in the way
                {
                    Debug.DrawRay(transform.position, (other.transform.position - transform.position).normalized * raycastDistance, Color.red);
                    if(hit.collider == other)
                    {
                        parent.OnSee(other.transform.position); //Pursue
                    }
                }
            }
    }

    void OnTriggerExit(Collider other)
    {
        parent.OnSee(other.transform.position);
    }
    
*/
    
}
