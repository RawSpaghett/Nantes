using UnityEngine;
using UnityEngine.Events;

public class Ears: MonoBehaviour
{
    private float hearingSensitivity = 1f;
    private IEars parent;

    void Awake()
    {
        parent = GetComponentInParent<IEars>();
    }

    void OnEnable()//Subscribe to relevant actions
    {
        //Throwable.OnLand += Listen;
    }

    void OnDisable()//Unsubscribe
    {
        //Throwable.OnLand -= Listen;
    }

    private void Listen(Vector3 sourcePosition, float loudness)
    {
        float distance = Vector3.Distance(sourcePosition,transform.position);
        float range = loudness * hearingSensitivity;

        if(distance <= range)
        {
            parent.OnNoiseHeard(); //Investigate
        }
    }







}
