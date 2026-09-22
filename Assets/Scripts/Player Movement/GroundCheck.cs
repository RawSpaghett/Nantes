using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public PlayerMovement playerMovement;

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == playerMovement.gameObject)
            return;

            playerMovement.SetGrounded(true);
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject == playerMovement.gameObject)
            return;

        playerMovement.SetGrounded(false);
    }

    void OnTriggerStay(Collider other)
    {
        if(other.gameObject == playerMovement.gameObject)
            return;

        playerMovement.SetGrounded(true);
    }
}
