using UnityEngine;
using UnityEngine.AI;

public class EnemyPursueState : EnemyState
{
    private float pursueSpeed = 15f;
    private float recalculateRate = 0.2f;
    private float recalculateTimer = 0;
    private GameObject targetObj; //grab from search state

    public EnemyPursueState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {
        isPassive = false;
    }
    
     public override void EnterState()
    {
        base.EnterState();
        targetObj = GameObject.Find("player");
        if (targetObj == null)
            Debug.Log("player not found");
        speed = pursueSpeed;
    }
    public override void ExitState()
    {
        base.ExitState();
    }
    public override void FrameUpdate()
    {
        base.FrameUpdate();
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetObj.transform.position,out hit,5f,NavMesh.AllAreas) && recalculateTimer >= recalculateRate)
        {
            enemy.Pathfinder(hit.position);
            recalculateTimer = 0f;
        }
    
        if (enemy.currentCornerIndex < enemy.corners.Length)
        {
            enemy.MoveEnemy();
        }
        else 
            enemy.rb.linearVelocity = Vector3.zero;

        recalculateTimer += Time.deltaTime;
    }


}
