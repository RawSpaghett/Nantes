using UnityEngine;

public class FlashlightScript : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private GameObject flashlight;
    [SerializeField] private float timer = 0f;
    [SerializeField] private float maxTimer = 8f;
    [SerializeField] private float minTimer = 0f;
    //[SerializeField] private bool flashlightActive;

    void Awake()
    {
        flashlight.SetActive(false);
    }

    void FixedUpdate()
    {
        if(timer >= maxTimer/2)
        {
            flashlight.SetActive(true);
        }


        if(playerInputHandler.crankHeld)
        {
            timer += Time.fixedDeltaTime;

            if(timer >= maxTimer)
            {
                //Debug.Log("Stopped");
                timer = maxTimer;
                //flashlightActive = true;
                return;
            }
        }

        if(!playerInputHandler.crankHeld)
        {
            timer -= Time.fixedDeltaTime;

            if(timer <= minTimer)
            {
                timer = minTimer;

                //flashlightActive = false;
                flashlight.SetActive(false);
                return;
            }
        }
    }
}
