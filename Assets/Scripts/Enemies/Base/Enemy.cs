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

    [Header("Nav Mesh")]
    protected NavMeshPath path;
    protected Vector3[] cornerArray = new Vector3[64]; //pre-allocate for memory
    protected int currentCornerIndex;
    protected int cornerCount;
    protected Quaternion deltaRotation;
    [SerializeField] private float navErrorMargin = 0.5f; //squared
    [SerializeField] private float turnSpeed = 500f;

    [Header("Stats")]
    protected abstract float CurrentSpeed {get;}

    protected virtual void Awake() //base.Awake()
    {
        path = new NavMeshPath();
        rb = GetComponent<Rigidbody>();
        deltaRotation = Quaternion.Euler(0,turnSpeed * Time.fixedDeltaTime,0);
    }

    protected virtual void FixedUpdate()
    {
    }

    public virtual void Move()// Use "Look-ahead" Smoothing, Handle sharp turns, and self-collision
    {
        if(cornerArray == null || currentCornerIndex >= cornerCount)
        {
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            return;
        }

        //Grab positions
        Vector3 currentPosition = new Vector3(transform.position.x,0,transform.position.z);
        Vector3 targetPosition = new Vector3(cornerArray[currentCornerIndex].x,0,cornerArray[currentCornerIndex].z);

        //Get numbers
        Vector3 header = (targetPosition - currentPosition);
        float distance = header.sqrMagnitude;
        Vector3 direction = header.normalized;

        //Look ahead smoothing

        //Sharp turn handling

        Vector3 finalForces = (direction * CurrentSpeed);
        rb.AddForce(finalForces, ForceMode.Force);
        //rotate
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            // MoveRotation is the physics-safe way to rotate a Rigidbody
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * turnSpeed));
        }
        //advance
        if (distance < navErrorMargin * navErrorMargin) //sqr
        {
            currentCornerIndex++;
            Debug.Log($"Current Corner: {currentCornerIndex}");
        }
    }

    protected virtual void PathFinder(Vector3 target)
    {
        //Grabs closest path to a target
        if (NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path)) // stores resulting path
        {
            if(path.status == NavMeshPathStatus.PathComplete || path.status == NavMeshPathStatus.PathPartial)
            {
                cornerCount = path.GetCornersNonAlloc(cornerArray);
                currentCornerIndex = 0; 
            }
        }
        else
        {
            Debug.Log("<Color=red>No Complete OR Partial path found</Color>");
        }
    }
}
