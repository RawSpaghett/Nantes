using UnityEngine;

public class PlayerState
{
    protected Player player;
    protected PlayerStateMachine playerStateMachine;

    public PlayerState(Player player, PlayerStateMachine playerStateMachine)
    {
    	this.player = player;
    	playerStateMachine = playerStateMachine;
    }

    public virtual void EnterState() { };
    public virtual void ExitState() { };
    public virtual void FrameUpdate() { };
    public virtual void PhysicsUpdate() { };
    public virtual void AnimationTriggerEvent(Enemy.AnimationTriggerType triggerType) { };

}
