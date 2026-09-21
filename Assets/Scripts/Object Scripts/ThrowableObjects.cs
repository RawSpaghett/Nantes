using UnityEngine;

public class ThrowableObjects : MonoBehaviour, IInteractable
{
    public bool isHeld = false;

    [SerializeField] private Transform playerHoldPosition;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private float throwPower = 15f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(playerInputHandler.interactTriggered && isHeld)
        {
            Throw();
        }
    }

    public void Interact(PlayerInteractor playerInteractor)
    {
        if(isHeld)
            return;

        else
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;

            isHeld = true;

            transform.position = playerHoldPosition.position;
            transform.SetParent(playerHoldPosition);

            playerInputHandler.interactTriggered = false;
        }
    }

    private void Throw()
    {
        rb.isKinematic = false;
        rb.detectCollisions = true;

        transform.SetParent(null, true);

        rb.AddForce(playerHoldPosition.forward * throwPower, ForceMode.Impulse);

        isHeld = false;
    }
}
