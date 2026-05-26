using UnityEngine;

public abstract class PlayerBaseState
{
    protected PlayerController player;
    protected PlayerMovementManager stateMachine;

    public PlayerBaseState(PlayerMovementManager sm)
    {
        stateMachine = sm;
        player = sm.player;
    }

    public abstract void Update();

    public abstract void FixedUpdate();

	public abstract void EnterState(PlayerMovementManager movementManager);

    public abstract void UpdateState(PlayerMovementManager movementManager);

    public abstract void ExitState(PlayerMovementManager movementManager);


    public abstract void CheckSwitchState(PlayerMovementManager movementManager);
}
