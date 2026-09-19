using UnityEngine;

public class PursueState: EState<Nantes>
{
    public PursueState(Nantes enemy, EStateMachine<Nantes> stateMachine) : base(enemy, stateMachine)
    {}
    public override void EnterState()
    {
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
