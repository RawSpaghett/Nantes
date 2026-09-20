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

    protected virtual void Awake() //base.Awake()
    {
        path = new NavMeshPath();
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void FixedUpdate()
    {
    }

    protected virtual void Move()// Use "Look-ahead" Smoothing, Handle sharp turns, and self-collision
    {
        

    }

    protected virtual void PathFinder(Vector3 target)
    {
        //Grabs closest path to a target
        if (NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path)) // stores resulting path
        {
            if(path.status == NavMeshPathStatus.PathComplete || path.status == NavMeshPathStatus.PathPartial)
            {
                cornerCount = path.GetCornersNonAlloc(cornerArray);
                currentCornerIndex = 1; //not including self
            }
        }
        else
        {
            Debug.Log("<Color=red>No Complete OR Partial path found</Color>");
        }
    }
}
