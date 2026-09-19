using UnityEngine;
using System;

public class IdleState: EState<Nantes>
{
    public IdleState(Nantes enemy, EStateMachine<Nantes> stateMachine) : base(enemy, stateMachine)
    {}
    public override void EnterState()
    {
        //Abrupt halt, can be changed to a smooth deccelleration later
        enemy.rb.linearVelocity = Vector3.zero;
        enemy.rb.angularVelocity = Vector3.zero;
        enemy.eyes.enabled = false;
    }
    public override void FrameUpdate()
    {}
    public override void AnimationTriggerEvent()
    {}
    public override void AudioTriggerEvent()
    {}
}
