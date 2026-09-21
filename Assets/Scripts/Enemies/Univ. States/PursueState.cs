using UnityEngine;

public class PursueState: EState<Nantes>
{
    public PursueState(Nantes enemy, EStateMachine<Nantes> stateMachine) : base(enemy, stateMachine)
    {}
    public override void EnterState()
    {
        enemy.stateMachine.currentState.speed = 5f;
        Debug.Log($"EnterState: {enemy.stateMachine.currentState}");
        enemy.eyes.enabled = true; //wont crash if already true
    }
    public override void ExitState()
    {}
    public override void FrameUpdate()
    {
        enemy.Move();
    }
    public override void AnimationTriggerEvent()
    {}
    public override void AudioTriggerEvent()
    {}
}
