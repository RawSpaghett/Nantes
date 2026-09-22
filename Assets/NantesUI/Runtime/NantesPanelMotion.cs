using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace NantesGame.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    [MovedFrom("Nantes.UI")]
    public sealed class NantesPanelMotion : MonoBehaviour
    {
        public NantesMenu menu;
        public float duration = .48f;
        public float distance = 30;
        public float delay;
        RectTransform rect;
        CanvasGroup group;
        Vector2 restingPosition;
        float elapsed;

        void Awake()
        {
            rect = (RectTransform)transform;
            group = GetComponent<CanvasGroup>();
            restingPosition = rect.anchoredPosition;
        }

        void OnEnable() { elapsed = 0; }

        void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            float t = menu != null && menu.ReducedMotion ? 1 : Mathf.Clamp01((elapsed - delay) / Mathf.Max(.01f,duration));
            float eased = 1 - Mathf.Pow(1 - t, 3);
            rect.anchoredPosition = restingPosition + Vector2.right * (distance * (1 - eased));
            group.alpha = Mathf.SmoothStep(0,1,t);
        }
    }
}
