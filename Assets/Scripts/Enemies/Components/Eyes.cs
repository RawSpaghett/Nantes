using UnityEngine;
using System;

public class Eyes: MonoBehaviour
{
    private BoxCollider collider;
    private IEyes parent;
    [SerializeField] private float raycastDistance = 20f;

    void Awake()
    {
        collider = GetComponent<BoxCollider>();
        parent = GetComponentInParent<IEyes>();
    }

    void OnTriggerEnter(Collider other)
    {
        if(Physics.Raycast(transform.position,(transform.position - other.transform.position))) //check if other stuff is in the way
        {
            if(other.tag == "Player")
            {
                parent.OnSee(other.transform.position); //Pursue
            }
        }
    }
    
}
