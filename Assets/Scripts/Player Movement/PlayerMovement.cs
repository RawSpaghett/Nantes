using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject camHolder;

    [Header ("Variables")]
    public float sensitivity;
    [SerializeField] private float speed, maxForce, jumpForce, sprintSpeed, slowWalkSpeed;
    private float speedHolder;

    private float lookRotation;

    public bool grounded;

    public static Action<Vector3, float> OnPlayerSound;

    private void FixedUpdate()
    {
        SpeedCheck();
        Move();
    }

    private void LateUpdate()
    {
        Look();
    }

    private void Move()
    {
        //find the target velocity
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 targetVelocity = new Vector3(playerInputHandler.move.x,0,playerInputHandler.move.y);

        targetVelocity *= speed;

        //align direction
        targetVelocity = transform.TransformDirection(targetVelocity);

        //calculate forces
        Vector3 velocityChange = targetVelocity - currentVelocity;
        velocityChange = new Vector3(velocityChange.x, 0, velocityChange.z);

        //limits forces on player
        velocityChange = Vector3.ClampMagnitude(velocityChange, maxForce);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
        OnPlayerSound?.Invoke(gameObject.transform.position, speed);
    }

    private void Look()
    {
        if(playerInputHandler.inScanner) return;

        //turn
        transform.Rotate(Vector3.up * playerInputHandler.look.x * sensitivity);

        //look up and down
        lookRotation += (-playerInputHandler.look.y * sensitivity);
        lookRotation = Mathf.Clamp(lookRotation,-90f,90f);
        camHolder.transform.eulerAngles = new Vector3(lookRotation, camHolder.transform.eulerAngles.y,camHolder.transform.eulerAngles.z);
    }

    public void Jump()
    {
        Vector3 jumpForces = Vector3.zero;

        if(grounded)
        {
            jumpForces = Vector3.up * jumpForce;
        }

        rb.AddForce(jumpForces, ForceMode.VelocityChange);
    }

    public void SetGrounded(bool state)
    {
        grounded = state;
    }

    private void SpeedCheck()
    {
        if(playerInputHandler.inScanner)
        {
            speed = 0;
            return;
        }

        else
        {
            speed = speedHolder;
        }

        if(playerInputHandler.sprintHeld)
            speed = sprintSpeed;

        else if(playerInputHandler.walkHeld)
            speed = slowWalkSpeed;

        else
            speed = speedHolder;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        speedHolder = speed;
    }
}

