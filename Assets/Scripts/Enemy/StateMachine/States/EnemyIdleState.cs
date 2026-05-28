using UnityEngine;

public class EnemyIdleState : EnemyState //still
{
    private float timer;
    private float switchTime = 5f;

    public EnemyIdleState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {
        isPassive = true;
    }
    
    public override void EnterState()
    {
        base.EnterState();
        enemy.rb.linearVelocity = Vector3.zero;
    }
    public override void ExitState()
    {
        base.ExitState();
    }
    public override void FrameUpdate()
    {
        base.FrameUpdate();
        timer += Time.deltaTime;
        if (timer >= switchTime && enemy.StateMachine.currentState.isPassive)
        {
            IdleWanderSwitch();
            timer = 0;
        }
    }

    private void IdleWanderSwitch()
    {
        if (UnityEngine.Random.Range(1,3) == 1) 
        {
                Debug.Log("LateUpdateIdleSwitchTrue");
                enemy.StateMachine.ChangeState(enemy.WanderState);
                return;
        }
        Debug.Log("LateUpdateIdleSwitchFalse");
    }
}
