using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
//While we are using a single enemy, nantes, it is still good practice to make it scalable
public class EnemyState
{
    protected Enemy enemy;
    protected EnemyStateMachine enemyStateMachine;

    public EnemyState (Enemy enemy, EnemyStateMachine enemyStateMachine) // when declaring a new enemy state, EnemyState state = new EnemyState(enemy,enemyStateMachine)
    {
        this.enemy = enemy;
        this.enemyStateMachine = enemyStateMachine;
    }

    public virtual void EnterState()
    {}

    public virtual void ExitState()
    {}

    public virtual void FrameUpdate()
    {}
    /* public virtual void AnimationTriggerEvent
    {} */

}
