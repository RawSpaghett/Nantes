using UnityEngine;
using UnityEngine.Events;

public class Ears: MonoBehaviour
{

    private float hearingSensitivity = 1f;
    private Enemy parent;

    void Awake()
    {
        parent = GetComponentInParent<Enemy>();
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
            parent.stateMachine.ChangeState(null); //Investigate
        }
    }







}
