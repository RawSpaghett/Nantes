using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerMovementManager sm) : base(sm) { }

    public override void Update() 
    {
        player.rigidBody.linearVelocity /= 2;
    }

    public override void FixedUpdate()
    {        

    }

    public override void EnterState(PlayerMovementManager movementManager) 
    {
        //Debug.Log("Entered Idle state");
    }

    public override void UpdateState(PlayerMovementManager movementManager)
    {
        if (player.MoveInput.magnitude > 0.1f)
        {
            //Debug.Log("Idle UpdateState is called");
            stateMachine.SwitchState(new PlayerWalkingState(stateMachine));
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
