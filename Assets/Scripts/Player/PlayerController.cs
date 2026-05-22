using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class PlayerController : MonoBehaviour
{
    [SerializeField] Transform playerCamera = null;
    [SerializeField] float mouseSensitivity = 5.0f;
    [SerializeField] float moveSpeed = 5.0f;
    [SerializeField] float crouchSpeedModifier = 0.5f;

    [SerializeField] bool lockCursor = true;
    [SerializeField] public Rigidbody rigidBody = null;

    
    [SerializeField] PlayerMovementManager movementStateMachine = null;

    public Vector2 MoveInput;
    public Vector3 velocity;

    float cameraPitch = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        rigidBody = GetComponent<Rigidbody>();
        handStateMachine = GetComponent<PlayerHandManager>();
        movementStateMachine = GetComponent<PlayerMovementManager>();

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMouseLook();
    }

    // FixedUpdate is called in a fixed interval, regardless of framerate
    private void FixedUpdate()
    {
        //UpdateMovement();
        Vector2 MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        movementStateMachine.Update();
    }

    void UpdateMouseLook()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        cameraPitch -= mouseDelta.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -90.0f, 90.0f);

        playerCamera.localEulerAngles = Vector3.right * cameraPitch;
        transform.Rotate(Vector3.up * mouseDelta.x * mouseSensitivity);
    }
}
