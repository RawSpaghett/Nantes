using UnityEngine;
using System;

public class Touch: MonoBehaviour
{
    //Variables
    private SphereCollider sphereCollider;
    private ITouch parent;

    void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        parent = GetComponentInParent<ITouch>();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            parent.OnTouch(other.transform.position); //probably pursue
        }
        /*
        if(other.tag == "Thrown")
        {
            parent.OnTouch(); //probably investigate
        }
        */
    }

}
