using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private LayerMask ignoreLayer;

    private int layerMask;

    //[SerializeField] private ThrowableObjects throwableObjects;

    private IInteractable currentInteractable;

    // Update is called once per frame
    void Update()
    {
        layerMask = ~ignoreLayer;

        //throw what we're holding before trying to pick up anything else
        if(playerInputHandler.interactTriggered)
        {
            ThrowableObjects heldObject = GetComponentInChildren<ThrowableObjects>();
            if(heldObject != null && heldObject.isHeld)
            {
                heldObject.Throw();
                return;
            }
        }

        CheckForInteractable();

        if(playerInputHandler.interactTriggered && currentInteractable != null)
        {
            Debug.Log("hi");
            currentInteractable.Interact(this);
        }
    }

    void CheckForInteractable()
    {
        currentInteractable = null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if(Physics.Raycast(ray, out RaycastHit hit, interactionDistance,layerMask))
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

            if(interactable != null)
            {
                Debug.Log(interactable);
                currentInteractable = interactable;
            }
        }
        if(currentInteractable == null)
            currentInteractable = NantesGame.Gameplay.FoodPickup.InReach(playerCamera, interactionDistance, layerMask);
    }
}
