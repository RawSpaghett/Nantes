using UnityEngine;
using System;

public class IdleState: EState<Nantes>
{
    private float decelSpeed = 10f;
    public IdleState(Nantes enemy, EStateMachine<Nantes> stateMachine) : base(enemy, stateMachine)
    {}
    public override void EnterState()
    {
        /* abrupt halt code, in case frameupdate doesnt work well
        enemy.rb.linearVelocity = Vector3.zero;
        enemy.rb.angularVelocity = Vector3.zero;
        */
        Debug.Log($"EnterState: {enemy.stateMachine.currentState}");
        enemy.eyes.enabled = false;
    }
    public override void FrameUpdate() //deccelleration 
    {
        enemy.rb.linearVelocity = Vector3.MoveTowards(enemy.rb.linearVelocity,Vector3.zero, decelSpeed * Time.fixedDeltaTime); //Current,Target,Max Distance
    }
    public override void AnimationTriggerEvent()
    {}
    public override void AudioTriggerEvent()
    {}
}
