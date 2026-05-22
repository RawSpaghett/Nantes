using UnityEngine;
using System;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/*
Because we only have one enemy, only one script is required
https://docs.unity3d.com/6000.4/Documentation/ScriptReference/AI.NavMesh.html
*/

public class Enemy : MonoBehaviour
{
    [Header("Enemy Variables")]
    public Rigidbody rb {get; set;}

    [Header("Nav Mesh")]
    public NavMeshPath path;
    public Vector3[] corners;
    public int currentCornerIndex;


    public float switchTime = 2.0f;

    [Header("debugging tools")]
    [SerializeField]
    private float debugTimer = 1f;
    [SerializeField]
    private Vector3 velocity;
    [SerializeField]
    private float pathTime;
    

    #region State Machine Variables
    public EnemyStateMachine StateMachine{get;set;}
    public EnemyIdleState IdleState{get;set;}
    public EnemyWanderState WanderState{get;set;} // auditory
    public EnemySearchState SearchState{get;set;} // visually
    public EnemyPursueState PursueState{get;set;}
    #endregion

    #region Awake,Start,Updates
    void Awake()
    {
        path = new NavMeshPath();

        StateMachine = new EnemyStateMachine();
        //States here, attackState = new EnemyAttackState
        IdleState = new EnemyIdleState(this, StateMachine);
        WanderState = new EnemyWanderState(this,StateMachine);
        SearchState = new EnemySearchState(this,StateMachine);
        PursueState = new EnemyPursueState(this,StateMachine);

        rb = GetComponent<Rigidbody>();

        StateMachine.Intialize(IdleState);
        Debug.Log($"<color = green> Monster StateMachine Intialized: {StateMachine.currentState.ToString()} </color");
    }

    void Start()
    {
        velocity = rb.linearVelocity;
    }

    void Update()
    {
        debugTimer += Time.deltaTime;
        if(corners != null && debugTimer >= 1f)
        {
            Debug.Log($"Current Corner Index: {currentCornerIndex.ToString()}");
            Debug.Log($"speed: {StateMachine.currentState.speed}");
            debugTimer = 0f;
        }
        StateMachine.currentState.FrameUpdate();
    }
    #endregion

    public void MoveEnemy() //navigates the mesh from pathfinder
    {
        if(corners == null)
        {
            Debug.Log($"Still finding path: {pathTime}");
            pathTime = Time.time * 1000f; //milliseconds
            return;
        }

        Vector3 targetCorner = corners[currentCornerIndex];
        Vector3 direction = (targetCorner - transform.position).normalized; //keeps direction, drops velocity
        /*
        Debug.Log($"direction:{direction.ToString()}");
        Debug.Log($"speed:{speed.ToString()}");
        */
        direction.y = 0;

        rb.AddForce(direction * StateMachine.currentState.speed,ForceMode.Acceleration);//applies actual speed to object

        transform.forward = Vector3.Slerp(transform.forward, direction, Time.deltaTime * 5f); 

        if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),new Vector3(targetCorner.x, 0, targetCorner.z)) < 1f)
        {
            currentCornerIndex++;
        }
    }

    public void Pathfinder( Vector3 target ) //uses navmesh utilities to find path to target
    {
            
        if (NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path)) // stores resulting path
        {
            corners = path.corners;
            currentCornerIndex = 0;
        }
        else
        {
            Debug.Log("<Color=red>Target path unreachable</Color>");
        }
    }

}
