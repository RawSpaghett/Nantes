using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NantesGame.Gameplay
{
    public sealed class PauseOption : MaskableGraphic,IPointerEnterHandler,IPointerMoveHandler,ISelectHandler
    {
        public GamePause owner;
        public int index;
        public TMP_Text label,number;
        float focus;
        void Update()
        {
            focus=Mathf.Lerp(focus,owner.Focused==index?1:0,1-Mathf.Exp(-Time.unscaledDeltaTime*12));
            label.color=Color.Lerp(new Color(.41f,.49f,.48f),new Color(.84f,.89f,.82f),focus);
            number.color=Color.Lerp(new Color(.19f,.28f,.29f),new Color(.34f,.69f,.65f),focus);
            label.rectTransform.anchoredPosition=new Vector2(66+focus*8,0);SetVerticesDirty();
        }
        public void OnPointerEnter(PointerEventData e){owner.PointerFocus(index);}
        public void OnPointerMove(PointerEventData e){owner.PointerFocus(index);}
        public void OnSelect(BaseEventData e){owner.KeyboardFocus(index);}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();float w=rectTransform.rect.width,h=rectTransform.rect.height;
            Quad(vh,0,-h/2,w,h,new Color(.035f,.1f,.105f,focus*.46f));
            Quad(vh,0,-h/2,w,1,new Color(.23f,.36f,.36f,.21f));
            Color edge=new Color(.39f,.67f,.63f,focus*.8f);
            Quad(vh,0,-h/2,w*focus,1,edge);Quad(vh,0,-11,2,22,edge);
            Quad(vh,w-18,h/2-1,18,1,edge);Quad(vh,w-1,h/2-15,1,15,edge);
            Quad(vh,w-36,-1,17*focus,1,new Color(.77f,.86f,.80f,focus));
        }
        static void Quad(VertexHelper vh,float x,float y,float w,float h,Color c)
        {
            int n=vh.currentVertCount;vh.AddVert(new Vector3(x,y),c,Vector2.zero);vh.AddVert(new Vector3(x+w,y),c,Vector2.zero);vh.AddVert(new Vector3(x+w,y+h),c,Vector2.zero);vh.AddVert(new Vector3(x,y+h),c,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);
        }
    }
}
