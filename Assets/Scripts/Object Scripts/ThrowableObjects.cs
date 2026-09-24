using UnityEngine;
using System;

public class ThrowableObjects : MonoBehaviour, IInteractable
{
    public bool isHeld = false;

    [Header("References")]
    [SerializeField] private Transform playerHoldPosition;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [Header("Variables")]
    [SerializeField] private float throwPower = 15f;
    [SerializeField] private float loudness = 5f;

    private Rigidbody rb;

    public static Action<Vector3, float> OnLand;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Interact(PlayerInteractor playerInteractor)
    {
        if(isHeld)
            return;

        else
        {
            //use the player picking this up, including newly placed prefab copies
            playerInputHandler = playerInteractor.GetComponent<PlayerInputHandler>();
            if(playerHoldPosition == null || !playerHoldPosition.IsChildOf(playerInteractor.transform))
            {
                playerHoldPosition = null;
                foreach(Transform child in playerInteractor.GetComponentsInChildren<Transform>())
                    if(child.name == "ObjectHolder") playerHoldPosition = child;
            }
            if(playerHoldPosition == null || playerInputHandler == null) return;

            rb.isKinematic = true;
            rb.detectCollisions = false;

            isHeld = true;

            transform.position = playerHoldPosition.position;
            transform.SetParent(playerHoldPosition);

            playerInputHandler.interactTriggered = false;
        }
    }

    public void Throw()
    {
        if(!isHeld || playerHoldPosition == null || playerInputHandler == null) return;

        //one press should not throw and immediately pick the object back up
        playerInputHandler.interactTriggered = false;

        //keep the box from hitting the player as it leaves their hand
        foreach(Collider itemCollider in GetComponentsInChildren<Collider>())
            foreach(Collider playerCollider in playerInputHandler.GetComponents<Collider>())
                Physics.IgnoreCollision(itemCollider, playerCollider);

        rb.isKinematic = false;
        rb.detectCollisions = true;

        transform.SetParent(null, true);

        rb.AddForce(playerHoldPosition.forward * throwPower, ForceMode.Impulse);

        isHeld = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("whatup "+ gameObject.transform.position);

        OnLand?.Invoke(gameObject.transform.position, loudness);
    }
}
