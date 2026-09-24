using UnityEngine;

public class FlashlightScript : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private GameObject flashlight;
    [SerializeField] private float timer = 0f;
    [SerializeField] private float maxTimer = 8f;
    [SerializeField] private float minTimer = 0f;
    //[SerializeField] private bool flashlightActive;

    // The meter reads these without changing the charge timer.
    public float Charge01 => Mathf.InverseLerp(minTimer, maxTimer, timer);
    public bool IsCranking => playerInputHandler && playerInputHandler.crankHeld;
    public bool IsLit => flashlight && flashlight.activeSelf;
    Light[] beamLights;
    float[] beamIntensities;

    void Awake()
    {
        beamLights = flashlight.GetComponentsInChildren<Light>(true);
        beamIntensities = new float[beamLights.Length];
        for (int i = 0; i < beamLights.Length; i++) beamIntensities[i] = beamLights[i].intensity;
        flashlight.SetActive(false);
    }

    void LateUpdate()
    {
        // Only the last part of the charge dims and sputters. Cranking steadies it.
        float strength = 1f;
        if (IsLit && !IsCranking)
        {
            float low = 1f - Mathf.SmoothStep(0f, 1f, Charge01 / .3f);
            strength = Mathf.Lerp(1f, .18f, low);
            if (PlayerPrefs.GetInt("Nantes.UI.ReducedMotion", 0) == 0)
                strength *= 1f - low * .2f * Mathf.PerlinNoise(Time.time * 13f, 4.7f);
        }
        for (int i = 0; i < beamLights.Length; i++)
            if (beamLights[i]) beamLights[i].intensity = beamIntensities[i] * strength;
    }

    void OnDisable()
    {
        if (beamLights == null) return;
        for (int i = 0; i < beamLights.Length; i++)
            if (beamLights[i]) beamLights[i].intensity = beamIntensities[i];
    }

    void FixedUpdate()
    {
        if(timer >= maxTimer/2)
        {
            flashlight.SetActive(true);
        }


        if(playerInputHandler.crankHeld)
        {
            timer += (Time.fixedDeltaTime*2);

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
