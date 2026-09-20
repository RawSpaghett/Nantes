using UnityEngine;

public class Eyes: MonoBehaviour
{
    private BoxCollider collider;
    private IEyes parent;

    void Awake()
    {
        collider = GetComponent<BoxCollider>();
        parent = GetComponentInParent<IEyes>();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            parent.OnSee(); //Pursue
        }
    }
    
}
