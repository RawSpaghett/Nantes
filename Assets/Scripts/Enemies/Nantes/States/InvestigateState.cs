using UnityEngine;

public class InvestigateState: EState<Nantes>
{
    public InvestigateState(Nantes enemy, EStateMachine<Nantes> stateMachine) : base(enemy, stateMachine)
    {}
    public override void EnterState()
    {
        enemy.eyes.enabled = true;
    }
    public override void ExitState()
    {}
    public override void FrameUpdate()
    {}
    public override void AnimationTriggerEvent()
    {}
    public override void AudioTriggerEvent()
    {}
}
