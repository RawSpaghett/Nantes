using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NantesGame.UI
{
    public sealed class ControlsBackGlow : MaskableGraphic, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        public TMP_Text label;
        Button button;
        bool hovered, selected, pointerMode;
        float amount;

        protected override void Awake() { base.Awake(); button = GetComponent<Button>(); }
        protected override void OnDisable()
        {
            base.OnDisable();
            hovered = selected = false;
            amount = 0;
        }

        void Update()
        {
            if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > .01f) pointerMode = true;
            var keys = Keyboard.current;
            if (keys != null && (keys.upArrowKey.wasPressedThisFrame || keys.downArrowKey.wasPressedThisFrame || keys.enterKey.wasPressedThisFrame)) pointerMode = false;
            var pad = Gamepad.current;
            if (pad != null && (pad.dpad.ReadValue().sqrMagnitude > .1f || pad.buttonSouth.wasPressedThisFrame)) pointerMode = false;
            bool active = button && button.IsInteractable() && (pointerMode ? hovered : selected);
            float target = active ? 1 : 0;
            amount = PlayerPrefs.GetInt("Nantes.UI.ReducedMotion", 0) == 1 ? target
                : Mathf.MoveTowards(amount, target, Time.unscaledDeltaTime * 8);
            if (label) label.color = Color.Lerp(new Color(.8f, .87f, .82f), new Color(.96f, 1, .96f), amount);
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = rectTransform.rect;
            Color fill = Color.Lerp(new Color(.035f, .065f, .07f), new Color(.10f, .22f, .21f), amount);
            Quad(vh, new Vector2(r.xMin, r.yMin), new Vector2(r.xMin, r.yMax),
                new Vector2(r.xMax, r.yMax), new Vector2(r.xMax, r.yMin), fill, fill);
            for (int i = 0; i < 12; i++)
            {
                float fade = 1 - i / 12f;
                var tint = new Color(.48f, .85f, .77f, amount * .23f * fade * fade);
                Frame(vh, r, i + 1, tint);
            }
            Frame(vh, r, 0, new Color(.64f, .93f, .84f, .1f + amount * .6f));
        }

        static void Frame(VertexHelper vh, Rect r, float expand, Color tint)
        {
            r.xMin -= expand; r.xMax += expand; r.yMin -= expand; r.yMax += expand;
            Quad(vh, new Vector2(r.xMin, r.yMin), new Vector2(r.xMin, r.yMin + 1), new Vector2(r.xMax, r.yMin + 1), new Vector2(r.xMax, r.yMin), tint, tint);
            Quad(vh, new Vector2(r.xMin, r.yMax - 1), new Vector2(r.xMin, r.yMax), new Vector2(r.xMax, r.yMax), new Vector2(r.xMax, r.yMax - 1), tint, tint);
            Quad(vh, new Vector2(r.xMin, r.yMin + 1), new Vector2(r.xMin, r.yMax - 1), new Vector2(r.xMin + 1, r.yMax - 1), new Vector2(r.xMin + 1, r.yMin + 1), tint, tint);
            Quad(vh, new Vector2(r.xMax - 1, r.yMin + 1), new Vector2(r.xMax - 1, r.yMax - 1), new Vector2(r.xMax, r.yMax - 1), new Vector2(r.xMax, r.yMin + 1), tint, tint);
        }

        static void Quad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color inner, Color outer)
        {
            int n = vh.currentVertCount;
            vh.AddVert(a, inner, Vector2.zero); vh.AddVert(b, inner, Vector2.zero);
            vh.AddVert(c, outer, Vector2.zero); vh.AddVert(d, outer, Vector2.zero);
            vh.AddTriangle(n, n + 1, n + 2); vh.AddTriangle(n, n + 2, n + 3);
        }

        public void OnPointerEnter(PointerEventData e)
        {
            hovered = true; pointerMode = true;
            var menu = GetComponentInParent<NantesMenu>();
            if (menu) menu.UsePointer();
        }
        public void OnPointerExit(PointerEventData e) { hovered = false; }
        public void OnSelect(BaseEventData e) { selected = true; pointerMode = false; }
        public void OnDeselect(BaseEventData e) { selected = false; }
    }
}
