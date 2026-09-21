using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    public Vector2 move, look;
    public bool sprintHeld;
    public bool interactTriggered;

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    public void OnLookInput(InputAction.CallbackContext context)
    {
        look = context.ReadValue<Vector2>();
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            playerMovement.Jump();
        }
    }

    public void OnSprintInput(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            sprintHeld = true;
        }

        if(context.canceled)
        {
            sprintHeld = false;
        }
    }

    public void OnInteractInput(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            interactTriggered = true;
        }

        if(context.canceled)
        {
            interactTriggered = false;
        }
    }

}
