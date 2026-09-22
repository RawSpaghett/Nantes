using UnityEngine;

public class FlashlightScript : MonoBehaviour
{
    [SerializeField] PlayerInputHandler playerInputHandler;
    [SerializeField] private float timer = 0f;
    [SerializeField] private float stopTimer = 8f;
    [SerializeField] private bool flashlightActive;

    void FixedUpdate()
    {
        if(playerInputHandler.crankHeld && !flashlightActive)
        {
            timer += Time.fixedDeltaTime;

            if(timer >= stopTimer)
            {
                //Debug.Log("Stopped");
                timer = stopTimer;

                flashlightActive = true;
                return;
            }
        }
    }
}
