using UnityEngine;

public class Segments : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject monsterHead;
    [SerializeField] private GameObject segmentPrefab;
    private GameObject[] segments;
    private Vector3[] locationArray;
    
    [Header("Creature Settings")]
    [SerializeField] private int segmentAmount = 10; //infinitely scalable
    [SerializeField] private float saveDistance = 2f; //aka follow distance, change this to change distance between segments

    private Vector3 lastSavedPosition;
    private float distanceMoved;
    private Rigidbody rb;

    void Awake()
    {
        segments = new GameObject[segmentAmount]; //size the arrays
        locationArray = new Vector3[segmentAmount + 1]; //leave room for monster transform
        monsterHead = GameObject.FindWithTag("Nantes");
        rb = monsterHead.GetComponent<Rigidbody>(); //grab rigid body
        GameObject folder = new GameObject("SegmentFolder");
    


        for(int i = 0; i < segmentAmount; i++) //spawn the objects and attatch to array
            {
                segments[i] = Instantiate(segmentPrefab,monsterHead.transform.position,monsterHead.transform.rotation); //spawns at same position as head
                segments[i].transform.SetParent(folder.transform);
            }


        for(int i = 0; i < locationArray.Length; i++) //intialize array
            locationArray[i] = monsterHead.transform.position;
        
        lastSavedPosition = monsterHead.transform.position; //get intial position
    }

    void FixedUpdate()
    {
        distanceMoved = Vector3.Distance(monsterHead.transform.position,lastSavedPosition); //add distance every fixedframe

        //Debug.Log($"distance moved: {distanceMoved}");
        
        if(distanceMoved > saveDistance) //basically follow distance
        {
            //Debug.Log("<color = green> If segment fired </color>");
            for(int i = locationArray.Length - 1; i > 0; i--) //constantly shift down the array and input latest monster head data, overwrite data-safe
            {
                locationArray[i] = locationArray[i - 1];
                //Debug.Log($"array {i} = {locationArray[i]}");
            }
            locationArray[0] = monsterHead.transform.position;
            lastSavedPosition = monsterHead.transform.position;
            distanceMoved = 0f;
        }

        for(int i = 0; i < segments.Length; i++) //moves all the segments to their corresponding location at the same velocity of the rb
        {
            Vector3 target = locationArray[i + 1]; //first one follows monster head and so on so forth
            Vector3 direction = target - segments[i].transform.position;
            float distance = direction.magnitude; //length of line between vectors origin and end point
            
            segments[i].transform.position = Vector3.MoveTowards(segments[i].transform.position, target, rb.linearVelocity.magnitude*Time.fixedDeltaTime);
        }
    }

}