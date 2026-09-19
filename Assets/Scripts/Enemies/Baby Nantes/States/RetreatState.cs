using UnityEngine;

public class RetreatState: EState<BNantes>
{
    public RetreatState(BNantes enemy, EStateMachine<BNantes> stateMachine) : base(enemy, stateMachine)
    {}
    public override void EnterState()
    {}
    public override void ExitState()
    {}
    public override void FrameUpdate()
    {}
    public override void AnimationTriggerEvent()
    {}
    public override void AudioTriggerEvent()
    {}
}
