using UnityEngine;
using UnityEngine.Events;

public class Ears: MonoBehaviour
{
    [SerializeField] private float hearingSensitivity = 100f;
    private IEars parent;

    void Awake()
    {
        parent = GetComponentInParent<IEars>();
    }

    void OnEnable()//Subscribe to relevant actions
    {
        ThrowableObjects.OnLand += Listen;
    }

    void OnDisable()//Unsubscribe
    {
        ThrowableObjects.OnLand -= Listen;
    }

    private void Listen(Vector3 sourcePosition, float loudness)
    {
        float distance = Vector3.Distance(sourcePosition,transform.position);
        float range = loudness * hearingSensitivity;

        if(distance <= range)
        {
            parent.OnNoiseHeard(sourcePosition); //Investigate
        }
    }







}
