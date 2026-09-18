using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class EStateMachine
{
    public EnemyState currentState {get;set;}
  
    public void Intialize(EnemyState intialState)
    {
        Debug.Log("EStateMachine Intialized");
        currentState = intialState;
        currentState.EnterState();
    }

    public void ChangeState(EnemyState newState)
    {
        currentState.ExitState();
        currentState = newState;
        Debug.Log($"State changed: {newState}");
        currentState.EnterState();
    }
}

