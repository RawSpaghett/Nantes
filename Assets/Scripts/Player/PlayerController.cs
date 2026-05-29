using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class PlayerController : MonoBehaviour
{

    [SerializeField] float mouseSensitivity = 5.0f;
    [SerializeField] public float moveSpeed = 25.0f;
    [SerializeField] bool lockCursor = true;

    [SerializeField] public float crouchSpeedModifier = 0.5f;
    [SerializeField] public float sprintSpeedModifier = 1.3f;

    [SerializeField] public Transform playerCamera = null;
    [SerializeField] public Rigidbody rigidBody { get; private set; }

    [SerializeField] public PlayerMovementManager movementStateMachine { get; private set; }

    public Vector2 MoveInput { get; private set; }

    float cameraPitch = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        rigidBody = GetComponent<Rigidbody>();
        //handStateMachine = GetComponent<PlayerHandManager>();
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
        MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        movementStateMachine.Update();
    }

    // FixedUpdate is called in a fixed interval, regardless of framerate
    private void FixedUpdate()
    {
        movementStateMachine.FixedUpdate();
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
