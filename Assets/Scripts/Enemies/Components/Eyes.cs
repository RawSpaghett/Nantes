using UnityEngine;

public class Eyes: MonoBehaviour
{
    private BoxCollider collider;
    private Enemy parent;

    void Awake()
    {
        collider = GetComponent<BoxCollider>();
        parent = GetComponentInParent<Enemy>();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            parent.stateMachine.ChangeState(null); //Pursue
        }
    }
    
}
