using UnityEngine;

public class PlayerCrouchingState : PlayerBaseState
{
    public PlayerCrouchingState(PlayerMovementManager sm) : base(sm) { }
    
    public override void Update() 
    {
        
    }

    public override void FixedUpdate()
    {
        Vector3 moveDir =
            player.transform.right * player.MoveInput.x +
            player.transform.forward * player.MoveInput.y;

        player.rigidBody.linearVelocity = moveDir * 5f * player.crouchSpeedModifier;
    }

    public override void EnterState(PlayerMovementManager movementManager)
    {
        //Debug.Log("Entered Crouch state");
        player.playerCamera.position -= Vector3.up * 0.4f;
    }

    public override void UpdateState(PlayerMovementManager movementManager)
    {
        if(!(Input.GetKey(KeyCode.C)))
        {
            stateMachine.SwitchState(new PlayerWalkingState(stateMachine));
        }
    }

    public override void ExitState(PlayerMovementManager movementManager)
    {
        player.playerCamera.position += Vector3.up * 0.4f;
    }

    public override void CheckSwitchState(PlayerMovementManager movementManager)
    {

    }
}
