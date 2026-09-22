using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NantesGame.UI
{
    [MovedFrom("Nantes.UI")]
    public sealed class NantesTendrilTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, ISubmitHandler
    {
        public NantesTendrilSelector selector;
        Selectable control;
        void Awake(){control=GetComponent<Selectable>();}
        public void OnPointerEnter(PointerEventData e){if(control.IsInteractable())selector.Hover(control);}
        public void OnPointerExit(PointerEventData e){selector.Leave(control);}
        public void OnPointerDown(PointerEventData e)
        {
            if(e.button==PointerEventData.InputButton.Left&&control.IsInteractable()&&!(control is Button))
                selector.Strike((RectTransform)transform,null);
        }
        public void OnSubmit(BaseEventData e)
        {
            if(control.IsInteractable()&&!(control is Button))selector.Strike((RectTransform)transform,null);
        }
        void OnDisable(){if(selector!=null)selector.Leave(control);}
    }
}
