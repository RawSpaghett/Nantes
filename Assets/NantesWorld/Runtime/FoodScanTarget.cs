using UnityEngine;

namespace NantesGame.World
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ObjectOutline))]
    public sealed class FoodScanTarget : MonoBehaviour
    {
        ObjectOutline outline;
        float visibleUntil;
        float revealedAt, startingOpacity;
        public bool IsRevealed => isActiveAndEnabled && Time.time < visibleUntil;

        void Awake()
        {
            outline = GetComponent<ObjectOutline>();
            outline.SetHighlighted(false);
        }

        public void Reveal(float seconds)
        {
            if (!IsRevealed) outline.opacity = 0;
            startingOpacity = outline.opacity;
            revealedAt = Time.time;
            visibleUntil = Time.time + Mathf.Max(0, seconds);
            if (seconds <= 0) outline.opacity = 0;
            outline.SetHighlighted(IsRevealed);
        }

        void Update()
        {
            if (!IsRevealed)
            {
                outline.opacity = 0;
                if (outline.highlighted) outline.SetHighlighted(false);
                return;
            }
            //Keep both fades inside the scan timer.
            float fadeIn = Mathf.SmoothStep(startingOpacity, 1, (Time.time - revealedAt) / .2f);
            float fadeOut = Mathf.SmoothStep(0, 1, (visibleUntil - Time.time) / .4f);
            outline.opacity = Mathf.Min(fadeIn, fadeOut);
        }

        void OnDisable()
        {
            visibleUntil = 0;
            if (outline)
            {
                outline.opacity = 0;
                outline.SetHighlighted(false);
            }
        }
    }
}
