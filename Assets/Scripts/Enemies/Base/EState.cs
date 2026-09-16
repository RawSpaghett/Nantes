using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public class EnemyState
{
    protected Enemy enemy;
    protected EStateMachine enemyStateMachine;

    public EnemyState (Enemy enemy, EStateMachine enemyStateMachine) // when declaring a new enemy state, EnemyState state = new EnemyState(enemy,enemyStateMachine)
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
    public virtual void AnimationTriggerEvent()
    {}

    public virtual void AudioTriggerEvenet()
    {}

}