using UnityEngine;
using UnityEngine.AI;

public class EnemyWanderState : EnemyState //wandering
{
    public float WanderMinimum = 20f;
    public float WanderMaximum = 30f;
    private float WanderRandom;
    private float wanderSpeed = 3f;
    private Vector3 randomLocation;
    private NavMeshPath randomPath;

    public EnemyWanderState(Enemy enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {
        isPassive = true;
    }

     public override void EnterState()
    {
        base.EnterState();
        speed = wanderSpeed;
        randomLocation = RandomNearbyLocation();
        enemy.Pathfinder(randomLocation);
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
        
        if (enemy.currentCornerIndex < enemy.corners.Length)
        {
            enemy.MoveEnemy();
        }
        if (enemy.currentCornerIndex >= enemy.corners.Length)
        {
            Debug.Log("Wander stopped!");
            enemyStateMachine.ChangeState(enemy.IdleState);
        }
        
    }

    //might run into problems where creature endlessly wanders into walls, or stares into walls, attempt to avoid body collision and clipping
    private Vector3 RandomNearbyLocation() //finds random point on the navmesh
    {
        WanderRandom = Random.Range(WanderMinimum,WanderMaximum + 1f);
        Debug.Log("grabbing nearby location");
        NavMeshHit hit;
        Vector3 randomDirection = Random.insideUnitSphere * WanderRandom; //grabs a random point within a radius
        randomDirection += this.enemy.transform.position; //current position
        Debug.Log($"Random Direction : {randomDirection.ToString()}");

        if (NavMesh.SamplePosition(randomDirection,out hit,WanderRandom,NavMesh.AllAreas)) //snaps to closest navmesh location
        {
            Debug.Log($"hit position : {hit.position.ToString()}");
            return hit.position;
        }

        Debug.Log("<color=red> Navmesh.SamplePosition FAILED </color>");
        return Vector3.zero;
    }
}
