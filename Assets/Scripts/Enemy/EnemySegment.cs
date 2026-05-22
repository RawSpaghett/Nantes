using Unity.VisualScripting;
using UnityEngine;

public class EnemySegmentController : MonoBehaviour
{
    [Header("References")]
    public GameObject monsterHead;
    public Enemy enemyScript;
    public GameObject segmentPrefab;
    public GameObject[] segments;
    
    [Header("Creature Settings")]
    public int segmentAmount = 4;
    public float followDistance;

    private float speed;


    void Awake()
    {
        segments = new GameObject[segmentAmount]; //size the array

        for(int i = 0; i < segmentAmount; i++) //spawn the objects and attatch to array
            segments[i] = Instantiate(segmentPrefab,monsterHead.transform.position,monsterHead.transform.rotation); //spawns at same position as head

    }

    void Update()
    {
        speed = enemyScript.StateMachine.currentState.speed;
        for(int i=0; i< segments.Length; i++)
        {
            Transform target = (i == 0) ? monsterHead.transform : segments[i - 1].transform; //if [0] follow monster head, else follow next segment
            Vector3 direction = target.position - segments[i].transform.position;
            float distance = direction.magnitude; //length of line between vectors origin and end point

            if(distance > followDistance)
            {
                Vector3 nextPos = target.position - (direction.normalized * followDistance);

                segments[i].transform.position = Vector3.Lerp(segments[i].transform.position, nextPos, speed);
            }

        }
    }
}
