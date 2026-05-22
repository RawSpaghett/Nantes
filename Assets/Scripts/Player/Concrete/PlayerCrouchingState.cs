using UnityEngine;

public class PlayerCrouchingState : PlayerBaseState
{
    public PlayerCrouchingState(PlayerMovementManager sm) : base(sm) { }
    
    public override void Update() 
    {
        
    }

    public override void FixedUpdate()
    {
        
    }

    public override void EnterState(PlayerMovementManager movementManager) 
    {
        Debug.Log("Entered Crouch state");
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

    }

    public override void CheckSwitchState(PlayerMovementManager movementManager)
    {

    }
}
