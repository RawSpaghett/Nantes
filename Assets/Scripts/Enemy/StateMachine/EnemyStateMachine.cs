using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
//https://youtu.be/RQd44qSaqww?si=OizMMmozHDlTWRr3

public class EnemyStateMachine 
{

    public EnemyState currentState {get;set;}
  
    public void Intialize(EnemyState intialState)
    {
        Debug.Log("State Machine Intialized");
        currentState = intialState;
        currentState.EnterState();
    }

    public void ChangeState(EnemyState newState)
    {
        currentState.ExitState();
        currentState = newState;
        currentState.EnterState();
    }
}
