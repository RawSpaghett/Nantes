using UnityEngine;

public class RetreatState: EState<BNantes>
{
    public RetreatState(BNantes enemy, EStateMachine<BNantes> stateMachine) : base(enemy, stateMachine)
    {}
    public override void EnterState()
    {
        Debug.Log($"EnterState: {enemy.stateMachine.currentState}");
        enemy.stateMachine.currentState.speed = 5f;
    }
    public override void ExitState()
    {}
    public override void FrameUpdate()
    {
        enemy.Move();
        if (Vector3.Distance(new Vector3(enemy.transform.position.x, 0, enemy.transform.position.z),new Vector3(enemy.retreatPoint.position.x, 0, enemy.retreatPoint.position.z)) < 3f)
        {
            enemy.rb.linearVelocity = Vector3.MoveTowards(enemy.rb.linearVelocity,Vector3.zero, 2f * Time.fixedDeltaTime); //Current,Target,Max Distance
            enemy.stateMachine.ChangeState(enemy.bPursueState);
        }
    }
    public override void AnimationTriggerEvent()
    {}
    public override void AudioTriggerEvent()
    {}
}
