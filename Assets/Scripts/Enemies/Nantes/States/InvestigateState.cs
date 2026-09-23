using UnityEngine;

public class InvestigateState: EState<Nantes>
{
    public InvestigateState(Nantes enemy, EStateMachine<Nantes> stateMachine) : base(enemy, stateMachine)
    {}
    public override void EnterState()
    {
        enemy.stateMachine.currentState.speed = 3f;
        //Debug.Log($"EnterState: {enemy.stateMachine.currentState}");
        enemy.eyes.enabled = true;
        AudioTriggerEvent();
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
    {
       enemy.speakers.PlayOneShot(enemy.Audio[1]);
    }
}
