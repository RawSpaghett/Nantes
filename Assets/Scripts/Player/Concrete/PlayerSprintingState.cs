using UnityEngine;

public class PlayerSprintingState : PlayerBaseState
{
    public PlayerSprintingState(PlayerMovementManager sm) : base(sm) { }

    public override void Update()
    {

    }

    public override void FixedUpdate()
    {
        Vector3 moveDir =
            player.transform.right * player.MoveInput.x +
            player.transform.forward * player.MoveInput.y;

        player.rigidBody.linearVelocity = moveDir * player.moveSpeed * player.sprintSpeedModifier;
    }

    public override void EnterState(PlayerMovementManager movementManager)
    {
        //Debug.Log("Entered Sprint state");   
    }

    public override void UpdateState(PlayerMovementManager movementManager)
    {
        if (player.MoveInput.magnitude < 0.1f)
        {
            stateMachine.SwitchState(new PlayerIdleState(stateMachine));
        }
        if (!Input.GetKey(KeyCode.LeftShift))
        {
            stateMachine.SwitchState(new PlayerWalkingState(stateMachine));
        }
        if (Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl))
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
