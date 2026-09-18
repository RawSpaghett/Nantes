using UnityEngine;
using System;

public class Touch: MonoBehaviour
{
    //Variables
    private SphereCollider sphereCollider;
    private Enemy parent;
    private event Action OnCollision;

    void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        parent = GetComponentInParent<Enemy>();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            parent.stateMachine.ChangeState(null); //probably pursue
        }
        if(other.tag == "Thrown")
        {
            parent.stateMachine.ChangeState(null); //probably investigate
        }
    }

}
