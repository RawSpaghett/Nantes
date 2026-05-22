// Context for player movement state machine 
using UnityEngine;

public class PlayerMovementManager : MonoBehaviour
{
    public PlayerBaseState currentState;
    public PlayerController player;    
    
    void Start() 
    {
        // give context access to PlayerController
        player = GetComponent<PlayerController>();

        // Player is idle at start
    	currentState = new PlayerIdleState(this);
    	currentState.EnterState(this);
    }

    public void Update()
    {
        currentState.Update();
        currentState.UpdateState(this);
    }

    public void FixedUpdate() {
        currentState.FixedUpdate();
    }

    public void SwitchState(PlayerBaseState state) 
    {
        currentState.ExitState(this);

        currentState = state;

    	currentState.EnterState(this);
    }
}
 