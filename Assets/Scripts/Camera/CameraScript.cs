using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private Transform scannerTransform;
    [Header("Variables")]
    [SerializeField] private float smoothTime = 0.3f;

    private Vector3 velocity = Vector3.zero;
    private Vector3 cameraRotation;

    [SerializeField] private Transform regularPositionTransform;

    void Update()
    {
        HandleLogic();
    }

    void HandleLogic()
    {
        if(playerInputHandler.inScanner) GoToScanner();

        else if(!playerInputHandler.inScanner) GoToOriginal();
    }

    private void GoToScanner()
    {
        if (Vector3.Distance(transform.position,scannerTransform.position) < 0.01f)
        {
            transform.position = scannerTransform.position;
            transform.rotation = scannerTransform.rotation;
            return;
        }

        cameraRotation = scannerTransform.forward;

        transform.position = Vector3.SmoothDamp(transform.position,scannerTransform.position, ref velocity,smoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation,scannerTransform.rotation, Time.deltaTime/smoothTime);
    }

    private void GoToOriginal()
    {
        if (Vector3.Distance(transform.position,regularPositionTransform.position) < 0.01f)
        {
            transform.position = regularPositionTransform.position;
            transform.rotation = regularPositionTransform.rotation;
            return;
        }

        //offset because idk why the camera wouldn't just spawn where it was supposed to L
        Vector3 targetPosition = regularPositionTransform.position + new Vector3(0f,0.42f,0f);

        transform.position = Vector3.SmoothDamp(transform.position,targetPosition, ref velocity,smoothTime);
        //transform.rotation = Quaternion.Slerp(transform.rotation,regularPositionTransform.rotation, Time.deltaTime/smoothTime);
    }
}
