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
    public bool IsLit => flashlight && flashlight.activeSelf && !warmingUp;
    private bool warmingUp;
    Light[] beamLights;
    float[] beamIntensities;

    void Awake()
    {
        beamLights = flashlight.GetComponentsInChildren<Light>(true);
        beamIntensities = new float[beamLights.Length];
        for (int i = 0; i < beamLights.Length; i++)
        {
            beamIntensities[i] = beamLights[i].intensity;
            //keep the beam from shining through shelves and walls
            beamLights[i].shadows = LightShadows.Soft;
            beamLights[i].shadowBias = .015f;
            beamLights[i].shadowNormalBias = .06f;
        }
        flashlight.SetActive(false);
    }

    void LateUpdate()
    {
        //more charge makes the bulb brighter
        float strength = Mathf.Lerp(.12f, 1f, Charge01);
        bool flicker = PlayerPrefs.GetInt("Nantes.UI.ReducedMotion", 0) == 0;
        if(warmingUp)
        {
            strength = Charge01 * .7f;
            if(flicker) strength *= Mathf.Lerp(.3f, 1f, Mathf.PerlinNoise(Time.time * 17f, 4.7f));
        }
        else if(IsLit && Charge01 < .3f && !IsCranking && flicker)
            strength *= 1f - (1f - Charge01 / .3f) * .3f * Mathf.PerlinNoise(Time.time * 13f, 4.7f);
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
            warmingUp = false;
            flashlight.SetActive(true);
        }


        if(playerInputHandler.crankHeld)
        {
            //the bulb sputters while it builds enough charge to stay on
            if(!flashlight.activeSelf) warmingUp = true;
            flashlight.SetActive(true);
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
            if(warmingUp) flashlight.SetActive(false);
            timer -= Time.fixedDeltaTime;

            if(timer <= minTimer)
            {
                timer = minTimer;
                warmingUp = false;

                //flashlightActive = false;
                flashlight.SetActive(false);
                return;
            }
        }
    }
}
