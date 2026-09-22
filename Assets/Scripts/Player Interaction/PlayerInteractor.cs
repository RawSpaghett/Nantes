using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private ThrowableObjects throwableObjects;

    private IInteractable currentInteractable;

    // Update is called once per frame
    void Update()
    {
        CheckForInteractable();

        if(playerInputHandler.interactTriggered && currentInteractable != null)
        {
            currentInteractable.Interact(this);
        }
    }

    void CheckForInteractable()
    {
        currentInteractable = null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if(Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if(interactable != null)
            {
                currentInteractable = interactable;
            }
        }
    }
}
