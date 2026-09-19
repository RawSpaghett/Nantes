using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public class EState<T> where T : Enemy //can be any child of enemy
{
    protected T enemy;
    protected EStateMachine<T> enemyStateMachine;

    public EState (T enemy, EStateMachine<T> enemyStateMachine) // when declaring a new enemy state, EnemyState state = new EnemyState(enemy,enemyStateMachine)
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

    public virtual void AudioTriggerEvent()
    {}

}