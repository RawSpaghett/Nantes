using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class EnemySegmentController : MonoBehaviour
{
    [Header("References")]
    public GameObject monsterHead;
    public Enemy enemyScript;
    public GameObject segmentPrefab;
    public GameObject[] segments;
    public Vector3[] locationArray;
    
    [Header("Creature Settings")]
    public int segmentAmount = 10;
    public float saveDistance = 300f; 
    public float followDistance = 3f;


    private Vector3 lastSavedPosition;
    private float distanceMoved;
    private float speed;


    void Awake()
    {
        segments = new GameObject[segmentAmount]; //size the arrays
        locationArray = new Vector3[segmentAmount + 1]; //leave room for monster transform


        for(int i = 0; i < segmentAmount; i++) //spawn the objects and attatch to array
            segments[i] = Instantiate(segmentPrefab,monsterHead.transform.position,monsterHead.transform.rotation); //spawns at same position as head

        for(int i = 0; i < locationArray.Length; i++) //intialize array
            locationArray[i] = monsterHead.transform.position;
        
        lastSavedPosition = monsterHead.transform.position;
    }

    void FixedUpdate()
    {
        speed = enemyScript.StateMachine.currentState.speed;
        distanceMoved = Vector3.Distance(monsterHead.transform.position,lastSavedPosition);
        
        
            if(distanceMoved > saveDistance)
            {
                for(int i = locationArray.Length - 1; i > 0; i--) //constantly shift down the array and input latest monster head data
                {
                    locationArray[i] = locationArray[i - 1];
                }
                locationArray[0] = monsterHead.transform.position;
                lastSavedPosition = monsterHead.transform.position;
            }

            for(int i = 0; i < segments.Length; i++)
            {
                
                    Vector3 target = locationArray[i]; //first one follows monster head and so on so forth
                    Vector3 direction = target - segments[i].transform.position;
                    float distance = direction.magnitude; //length of line between vectors origin and end point
            
                    segments[i].transform.position = Vector3.MoveTowards(segments[i].transform.position, target, speed);
                
            }

            
    }
}
