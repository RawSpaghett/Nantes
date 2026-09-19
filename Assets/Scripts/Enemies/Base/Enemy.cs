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
    public EStateMachine<Enemy> stateMachine {get; set;}

    [Header("Nav Mesh")]
    protected NavMeshPath path;
    protected Vector3[] corners;
    protected int currentCornerIndex;

    protected virtual void Awake() //base.Awake()
    {
        path = new NavMeshPath();
        stateMachine = new EStateMachine<Enemy>();
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void FixedUpdate()
    {}

    protected virtual void Move()
    {}

    protected virtual void PathFinder()
    {}

}
