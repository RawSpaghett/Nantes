using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class EStateMachine<T> where T : Enemy
{
    public EState<T> currentState {get;set;}
  
    public void Intialize(EState<T> intialState)
    {
        Debug.Log("EStateMachine Intialized");
        currentState = intialState;
        currentState.EnterState();
    }

    public void ChangeState(EState<T> newState)
    {
        currentState.ExitState();
        currentState = newState;
        Debug.Log($"State changed: {newState}");
        currentState.EnterState();
    }
}

