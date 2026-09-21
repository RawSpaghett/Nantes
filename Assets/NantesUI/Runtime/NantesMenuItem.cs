using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Nantes.UI
{
    public sealed class NantesMenuItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerDownHandler, IPointerUpHandler
    {
        public NantesLineArt outline;
        public TMP_Text label;
        public RectTransform marker;
        public NantesMenu menu;
        bool hovered, selected;
        float amount;
        Button button;
        Vector2 labelPosition;
        RectTransform rect;
        bool pressed;
        Image markerImage;

        void Awake()
        {
            button = GetComponent<Button>();
            labelPosition = label.rectTransform.anchoredPosition;
            rect = (RectTransform)transform;
            markerImage=marker.GetComponent<Image>();
            if(markerImage!=null){markerImage.enabled=false;markerImage.raycastTarget=false;}
        }

        void OnDisable() { hovered = selected = pressed = false; amount = 0; }

        void LateUpdate()
        {
            bool active = button.IsInteractable() && (menu.KeyboardNavigation ? selected : hovered);
            amount = menu.ReducedMotion ? (active ? 1 : 0) : Mathf.MoveTowards(amount, active ? 1 : 0, Time.unscaledDeltaTime * 9);
            outline.reveal = amount;
            bool tendril=menu.selector!=null&&menu.selector.isActiveAndEnabled;
            outline.enabled=!tendril;
            outline.SetVerticesDirty();
            var theme = menu.theme;
            Color idle = theme != null ? theme.muted : new Color(.63f,.69f,.64f);
            Color focus = theme != null ? theme.bone : new Color(.94f,.91f,.82f);
            Color disabled = theme != null ? theme.disabled : new Color(.28f,.33f,.30f);
            label.color = button.IsInteractable() ? Color.Lerp(idle,focus,amount) : disabled;
            if (theme != null) outline.color = new Color(theme.bone.r,theme.bone.g,theme.bone.b,.55f);
            label.rectTransform.anchoredPosition = labelPosition + Vector2.right * (menu.ReducedMotion ? 0 : amount * 9);
            marker.sizeDelta = new Vector2(22, 2);
            if(markerImage!=null)
            {
                markerImage.enabled=!tendril&&button.IsInteractable()&&amount>.03f;
                Color accent=theme!=null?theme.blood:new Color(.5f,.17f,.15f);
                accent.a=amount;markerImage.color=accent;
            }
            rect.localScale = Vector3.Lerp(rect.localScale, Vector3.one * (pressed && !menu.ReducedMotion ? .975f : 1), 1 - Mathf.Exp(-Time.unscaledDeltaTime * 24));
        }

        // Pointer-enter alone must not override keyboard focus.
        public void OnPointerEnter(PointerEventData e) { hovered = true; }
        public void OnPointerExit(PointerEventData e) { hovered = false; }
        public void OnSelect(BaseEventData e) { selected = true; }
        public void OnDeselect(BaseEventData e) { selected = false; }
        public void OnPointerDown(PointerEventData e) { menu.UsePointer(); if (button.IsInteractable()) pressed = true; }
        public void OnPointerUp(PointerEventData e) { pressed = false; }
    }
}
