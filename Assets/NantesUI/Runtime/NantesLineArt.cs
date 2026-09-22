using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI;

namespace NantesGame.UI
{
    [MovedFrom("Nantes.UI")]
    public sealed class NantesLineArt : MaskableGraphic
    {
        public enum Drawing { Wordmark, Orbit, Selection }
        public Drawing drawing;
        [Range(0, 1)] public float reveal = 1;

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            DrawSelection(mesh);
        }

        void Line(VertexHelper mesh, Vector2 a, Vector2 b, float thickness, Color tint)
        {
            Vector2 unit = new Vector2(-(b - a).y, (b - a).x).normalized;
            Vector2 normal = unit * thickness * .5f;
            int i = mesh.currentVertCount;
            mesh.AddVert(a - normal, tint, Vector2.zero);
            mesh.AddVert(a + normal, tint, Vector2.zero);
            mesh.AddVert(b + normal, tint, Vector2.zero);
            mesh.AddVert(b - normal, tint, Vector2.zero);
            mesh.AddTriangle(i, i + 1, i + 2);
            mesh.AddTriangle(i, i + 2, i + 3);
            Color clear = tint; clear.a = 0;
            Vector2 fringe = normal + unit * .65f;
            i = mesh.currentVertCount;
            mesh.AddVert(a - fringe, clear, Vector2.zero);
            mesh.AddVert(a - normal, tint, Vector2.zero);
            mesh.AddVert(b - normal, tint, Vector2.zero);
            mesh.AddVert(b - fringe, clear, Vector2.zero);
            mesh.AddTriangle(i, i + 1, i + 2);
            mesh.AddTriangle(i, i + 2, i + 3);
            i = mesh.currentVertCount;
            mesh.AddVert(a + normal, tint, Vector2.zero);
            mesh.AddVert(a + fringe, clear, Vector2.zero);
            mesh.AddVert(b + fringe, clear, Vector2.zero);
            mesh.AddVert(b + normal, tint, Vector2.zero);
            mesh.AddTriangle(i, i + 1, i + 2);
            mesh.AddTriangle(i, i + 2, i + 3);
        }

        void DrawSelection(VertexHelper m)
        {
            Rect r = rectTransform.rect;
            if (reveal <= 0) return;
            Color c = color; c.a *= Mathf.Lerp(.25f, 1, reveal);
            float y=r.center.y;
            Line(m,new Vector2(r.xMin+9,y+7*reveal),new Vector2(r.xMin+16,y),.85f,c);
            Line(m,new Vector2(r.xMin+16,y),new Vector2(r.xMin+9,y-7*reveal),.85f,c);
            float end=Mathf.Lerp(r.xMin+40,r.xMax-54,reveal);
            Color faint=c;faint.a*=.26f;
            Line(m,new Vector2(r.xMin+40,r.yMin+6),new Vector2(end,r.yMin+6),.7f,faint);
            Line(m,new Vector2(end+5,r.yMin+6),new Vector2(end+18,r.yMin+6),.7f,c);
        }
    }
}
