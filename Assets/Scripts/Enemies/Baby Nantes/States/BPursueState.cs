using UnityEngine;

public class BPursueState: EState<BNantes>
{
    public BPursueState(BNantes enemy, EStateMachine<BNantes> stateMachine) : base(enemy, stateMachine)
    {}
    public override void EnterState()
    {
        enemy.stateMachine.currentState.speed = 1f;
    }
    public override void ExitState()
    {}
    public override void FrameUpdate()
    {
        enemy.PathFinder(enemy.playerLocation.position);
        enemy.Move();
    }
    public override void AnimationTriggerEvent()
    {}
    public override void AudioTriggerEvent()
    {}
}
