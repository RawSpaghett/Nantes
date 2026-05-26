using UnityEngine;

public class PlayerHandManager : MonoBehaviour
{

    HandBaseState currentState;
    PlayerController controller;

    EmptyHandState emptyState = new EmptyHandState();
    HoldingItemState holdingItemState = new HoldingItemState();

    void Start()
    {
        // starting state for SM
        currentState = emptyState;

        // "this" is a reference to the context or the instance of the PlayerHandManager
        currentState.EnterState(this);
    }

    void Update()
    {
        currentState.UpdateState(this);
    }

    void SwitchState(HandBaseState state)
    {
        currentState = state;
        state.EnterState(this);
    }
}
