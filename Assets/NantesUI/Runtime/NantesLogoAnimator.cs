using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI;

namespace NantesGame.UI
{
    [RequireComponent(typeof(RawImage))]
    [MovedFrom("Nantes.UI")]
    public sealed class NantesLogoAnimator : MonoBehaviour
    {
        public NantesMenu menu;
        public bool automatic = true;
        public float AnimationTime { get; private set; }
        public float Corruption { get; private set; }
        Material instance;
        RawImage logo;

        void Awake()
        {
            logo = GetComponent<RawImage>();
            instance = new Material(logo.material);
            logo.material = instance;
        }

        void LateUpdate()
        {
            if (automatic && (menu == null || !menu.ReducedMotion)) AnimationTime += Time.unscaledDeltaTime;
            Evaluate(AnimationTime);
        }

        public void Evaluate(float seconds)
        {
            AnimationTime = seconds;
            float phase = Mathf.Repeat(seconds, 19f);
            Corruption = menu != null && menu.ReducedMotion ? 0 : Mathf.Max(
                Pulse(phase, 3.0f, .50f) * .40f,
                Pulse(phase, 9.4f, .75f) * .68f,
                Pulse(phase, 15.0f, 1.05f));
            if (instance == null) return;
            instance.SetFloat("_LogoTime", seconds);
            instance.SetFloat("_Corruption", Corruption);
            instance.SetFloat("_ReducedMotion", menu != null && menu.ReducedMotion ? 1 : 0);
        }

        static float Pulse(float time, float center, float halfWidth)
        { return Mathf.SmoothStep(0, 1, 1 - Mathf.Abs(time - center) / halfWidth); }

        public void SetVesselDepth(Texture mask, bool inFront)
        {
            if(instance==null)return;
            instance.SetTexture("_VesselMask",mask);
            instance.SetFloat("_VesselInFront",inFront?1:0);
        }

        void OnDestroy() { if (instance != null) Destroy(instance); }
    }
}
