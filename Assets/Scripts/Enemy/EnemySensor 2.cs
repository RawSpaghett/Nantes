using Unity.VisualScripting;
using UnityEngine;

public class EnemySensor : MonoBehaviour
{
    //General Variables    
    public Vector3 PlayerGhostPosition{get;private set;}
    public bool playerInSight;
    public Enemy enemy{get;private set;}
    public GameObject player{get;private set;}

    [Header("Sensor Settings")]
    public float detectionRadius = 15f;

    
    void Awake()
    {
        
    }

    void Update()
    {
        if(Vector3.Distance(enemy.gameObject.transform.position,player.transform.position) < detectionRadius)
        {
            //scan for player and then switch enemy.StateMachine.currentState = PursueState; and after a period of time without a visual confirmation switch the target to the players last known position
        }
        //check for audio here
    }

    

}
