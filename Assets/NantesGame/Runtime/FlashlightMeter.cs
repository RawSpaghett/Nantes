using TMPro;
using NantesGame.Tablet;
using UnityEngine;
using UnityEngine.UI;

namespace NantesGame.Gameplay
{
    public sealed class FlashlightMeter : MaskableGraphic
    {
        public GamePause pause;
        public TMP_Text status;
        public CanvasGroup visibility;
        TabletController tablet;
        FlashlightScript source;
        float charge;
        Color ink;

        public static void Attach(GamePause pause, TabletController tablet)
        {
            var go = new GameObject("Tablet flashlight", typeof(RectTransform), typeof(CanvasGroup), typeof(FlashlightMeter));
            go.layer = tablet.DisplayRoot.gameObject.layer;
            go.transform.SetParent(tablet.DisplayRoot, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
            rect.anchoredPosition = new Vector2(36, -6);
            rect.sizeDelta = new Vector2(236, 76);
            rect.localScale = Vector3.one * (tablet.squareDisplay ? 1.5f : 1.25f);
            var meter = go.GetComponent<FlashlightMeter>();
            meter.pause = pause; meter.tablet = tablet;
            meter.visibility = go.GetComponent<CanvasGroup>();
            meter.visibility.alpha = 0; meter.visibility.blocksRaycasts = false; meter.visibility.interactable = false;
            meter.raycastTarget = false;
            var text = new GameObject("Flashlight status", typeof(RectTransform), typeof(TextMeshProUGUI));
            text.layer = go.layer; text.transform.SetParent(go.transform, false);
            var labelRect = (RectTransform)text.transform;
            labelRect.anchorMin = labelRect.anchorMax = Vector2.zero; labelRect.pivot = new Vector2(0, .5f);
            labelRect.anchoredPosition = new Vector2(96, 50); labelRect.sizeDelta = new Vector2(140, 24);
            meter.status = text.GetComponent<TMP_Text>();
            meter.status.font = tablet.font; meter.status.fontSize = 14; meter.status.text = "OFF";
            meter.status.alignment = TextAlignmentOptions.MidlineLeft; meter.status.raycastTarget = false;
            meter.status.textWrappingMode = TextWrappingModes.NoWrap;
            tablet.HasFlashlightMeter = true;
        }

        protected override void Start()
        {
            base.Start();
            source = pause.movement.GetComponentInChildren<FlashlightScript>(true);
            raycastTarget = false;
        }

        void Update()
        {
            visibility.alpha = source && tablet && tablet.IsReady && !pause.IsPaused && !ScreenTransition.Busy ? 1 : 0;
            if (!source) return;
            charge = Mathf.MoveTowards(charge, source.Charge01, Time.deltaTime * 2f);
            bool active = source.IsCranking || source.IsLit;
            ink = active ? new Color(.86f, .91f, .89f, .94f) : new Color(.34f, .39f, .39f, .38f);
            status.color = ink;
            status.text = source.IsLit ? (source.Charge01 < .25f && !source.IsCranking ? "LOW CHARGE" : "READY")
                : source.IsCranking ? "CHARGING" : "OFF";
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Line(vh, new Vector2(3, 26), new Vector2(33, 26), 2, ink);
            Line(vh, new Vector2(3, 44), new Vector2(33, 44), 2, ink);
            Line(vh, new Vector2(3, 26), new Vector2(3, 44), 2, ink);
            Line(vh, new Vector2(33, 26), new Vector2(44, 20), 2, ink);
            Line(vh, new Vector2(33, 44), new Vector2(44, 50), 2, ink);
            Line(vh, new Vector2(44, 20), new Vector2(44, 50), 2, ink);
            Line(vh, new Vector2(18, 47), new Vector2(25, 47), 2, ink);
            if (source && (source.IsCranking || source.IsLit))
            {
                for (int i = 0; i < 5; i++)
                {
                    float strength = Mathf.Clamp01(charge * 5 - i);
                    if (strength <= 0) continue;
                    int lane = i == 0 ? 0 : (i % 2 == 1 ? (i + 1) / 2 : -i / 2);
                    var from = new Vector2(54, 35 + lane * 6);
                    var to = new Vector2(70 + charge * 9 - Mathf.Abs(lane) * 2, 35 + lane * 10);
                    var ray = ink; ray.a *= strength;
                    Line(vh, from, to, 2, ray);
                }
            }
            Line(vh, new Vector2(96, 26), new Vector2(228, 26), 5, new Color(.29f, .37f, .37f, .3f));
            if (charge > .001f) Line(vh, new Vector2(96, 26), new Vector2(96 + 132 * charge, 26), 3, ink);
            // Half charge is where the existing flashlight switches on.
            Line(vh, new Vector2(162, 18), new Vector2(162, 21), 1, new Color(.64f, .72f, .69f, .5f));
        }

        void Line(VertexHelper vh, Vector2 a, Vector2 b, float width, Color tint)
        {
            Vector2 origin = rectTransform.rect.min;
            a += origin; b += origin;
            Vector2 direction = (b - a).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x);
            float feather = .85f / (canvas ? canvas.scaleFactor : 1);
            float inner = Mathf.Max(.1f, width * .5f - feather * .5f);
            float outer = width * .5f + feather * .5f;
            var clear = tint; clear.a = 0;
            Quad(vh, a - normal * inner, a + normal * inner, b + normal * inner, b - normal * inner, tint, tint);
            Quad(vh, a + normal * inner, a + normal * outer, b + normal * outer, b + normal * inner, tint, clear);
            Quad(vh, b - normal * inner, b - normal * outer, a - normal * outer, a - normal * inner, tint, clear);
            Cap(vh, a, -direction, normal, inner, outer, tint);
            Cap(vh, b, direction, -normal, inner, outer, tint);
        }

        static void Cap(VertexHelper vh, Vector2 center, Vector2 direction, Vector2 normal, float inner, float outer, Color tint)
        {
            var clear = tint; clear.a = 0;
            for (int i = 0; i < 12; i++)
            {
                float a = -Mathf.PI * .5f + Mathf.PI * i / 12;
                float b = -Mathf.PI * .5f + Mathf.PI * (i + 1) / 12;
                Vector2 from = direction * Mathf.Cos(a) + normal * Mathf.Sin(a);
                Vector2 to = direction * Mathf.Cos(b) + normal * Mathf.Sin(b);
                int n = vh.currentVertCount;
                vh.AddVert(center, tint, Vector2.zero);
                vh.AddVert(center + from * inner, tint, Vector2.zero);
                vh.AddVert(center + to * inner, tint, Vector2.zero);
                vh.AddTriangle(n, n + 1, n + 2);
                Quad(vh, center + from * inner, center + from * outer, center + to * outer, center + to * inner, tint, clear);
            }
        }

        static void Quad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color inner, Color outer)
        {
            int n = vh.currentVertCount;
            vh.AddVert(a, inner, Vector2.zero); vh.AddVert(b, outer, Vector2.zero);
            vh.AddVert(c, outer, Vector2.zero); vh.AddVert(d, inner, Vector2.zero);
            vh.AddTriangle(n, n+1, n+2); vh.AddTriangle(n, n+2, n+3);
        }
    }
}
