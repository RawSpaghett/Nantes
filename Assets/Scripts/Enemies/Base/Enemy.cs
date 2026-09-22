using UnityEngine;
using System;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//https://docs.unity3d.com/6000.4/Documentation/ScriptReference/AI.NavMesh.html

public abstract class Enemy: MonoBehaviour
{
    [Header("Enemy Components")]
    public Rigidbody rb;
    public Transform playerLocation;

    #region Navmesh
    protected NavMeshPath path;
    protected Vector3[] cornerArray;
    protected int currentCornerIndex;
    protected int cornerCount;
    #endregion

    [Header("Stats")]
    [SerializeField] private float navErrorMargin = 3f; //squared
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private float maxSpeed = 5f;

    //callbacks
    protected abstract float CurrentSpeed {get;}

    protected virtual void Awake() //base.Awake()
    {
        path = new NavMeshPath();
        rb = GetComponent<Rigidbody>();
        playerLocation = GameObject.FindWithTag("Player").transform;
    }

    protected virtual void FixedUpdate()
    {
        
    }

    public virtual void Move()// Use "Look-ahead" Smoothing, Handle sharp turns, and self-collision
    {
        if(cornerArray == null || currentCornerIndex >= cornerArray.Length) return;
        Vector3 targetCorner = cornerArray[currentCornerIndex];
        Vector3 direction = (targetCorner - transform.position).normalized; //keeps direction, drops velocity
        /*
        Debug.Log($"direction:{direction.ToString()}");
        Debug.Log($"speed:{speed.ToString()}");
        */
        direction.y = 0;

        rb.AddForce(direction * CurrentSpeed,ForceMode.VelocityChange);//applies actual speed to object
        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, CurrentSpeed); //clamp 

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * turnSpeed));
        }

        if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),new Vector3(targetCorner.x, 0, targetCorner.z)) < navErrorMargin)
        {
            currentCornerIndex++;
            Debug.Log($"current corner {currentCornerIndex}");
        }
    }

    public virtual void PathFinder(Vector3 target)
    {
        //Debug.Log("<Color=green>Pathfinder called</Color>");
        //Grabs closest path to a target
        if (NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path)) // stores resulting path
        {
            if(path.status == NavMeshPathStatus.PathComplete || path.status == NavMeshPathStatus.PathPartial)
            {
                cornerArray = path.corners;
                currentCornerIndex = 1; 
            }
        }
        else
        {
            Debug.Log("<Color=red>No Complete OR Partial path found</Color>");
        }
    }
}
