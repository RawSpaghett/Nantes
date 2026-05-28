using UnityEngine;

public class EnemySearchState : EnemyState
{
    public EnemySearchState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {
        isPassive = false;
    }
     public override void EnterState()
    {
        base.EnterState();
    }
    public override void ExitState()
    {
        base.ExitState();
    }
    public override void FrameUpdate()
    {
        base.FrameUpdate();
    }
}
