using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    public Rigidbody rb {get; set;}

    #region State Machine Variables
    public EnemyStateMachine StateMachine{get;set;}
    public EnemyIdleState IdleState{get;set;}
    public EnemyWanderState WanderState{get;set;} // auditory
    public EnemySearchState SearchState{get;set;} // visually
    public EnemyPursueState PursueState{get;set;}
    #endregion

    void Awake()
    {
        StateMachine = new EnemyStateMachine();

        //States here, attackState = new EnemyAttackState
        IdleState = new EnemyIdleState(this, StateMachine);
        WanderState = new EnemyWanderState(this,StateMachine);
        SearchState = new EnemySearchState(this,StateMachine);
        PursueState = new EnemyPursueState(this,StateMachine);
    }

    void Start()
    {
        StateMachine.Intialize(IdleState);
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        StateMachine.currentState.FrameUpdate();
    }
}
