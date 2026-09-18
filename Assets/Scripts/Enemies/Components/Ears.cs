using UnityEngine;
public class Ears: MonoBehaviour
{

    private float hearingSensitivity = 1f;
    private Enemy parent;

    void Awake()
    {
        parent = GetComponentInParent<Enemy>();
    }

    void OnEnable()
    {
        
    }

    void OnDisable()
    {
        
    }

    private void Listen(Vector3 sourcePosition, float loudness)
    {
        float distance = Vector3.Distance(sourcePosition,transform.position);
        float range = loudness * hearingSensitivity;

        if(distance <= range)
        {
            




        }
    }







}
