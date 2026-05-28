using UnityEngine;

public class PlayerWalkingState : PlayerBaseState
{
    public PlayerWalkingState(PlayerMovementManager sm) : base(sm) { }

    public override void EnterState(PlayerMovementManager movementManager) 
    {
        //Debug.Log("Entered Walk state");
    }

    public override void Update() {
        
    }

    public override void FixedUpdate()
    {
        Vector3 moveDir = 
            player.transform.right * player.MoveInput.x + 
            player.transform.forward * player.MoveInput.y;

        player.rigidBody.linearVelocity = moveDir * 25f;

    }

    public override void UpdateState(PlayerMovementManager movementManager)
    {

        if (player.MoveInput.magnitude < 0.1f)
        {
            stateMachine.SwitchState(new PlayerIdleState(stateMachine));
        }
        if (Input.GetKey(KeyCode.C))
        {
            stateMachine.SwitchState(new PlayerCrouchingState(stateMachine));
        }
    }

    public override void ExitState(PlayerMovementManager movementManager)
    {

    }

    public override void CheckSwitchState(PlayerMovementManager movementManager)
    {

    }
}
