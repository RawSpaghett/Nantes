using System.Collections.Generic;
using UnityEngine;

namespace NantesGame.UI.Editor
{
    public static class NantesExhaustBuilder
    {
        public static Mesh Create()
        {
            var vertices = new List<Vector3>(); var uv = new List<Vector2>();
            var colors = new List<Color>(); var indices = new List<int>();
            for (int shell = 0; shell < 2; shell++)
            {
                const int rings = 36, sides = 24;
                int start = vertices.Count;
                for (int j = 0; j <= rings; j++) for (int i = 0; i <= sides; i++)
                {
                    float t = (float)j / rings, a = (float)i / sides * Mathf.PI * 2;
                    float radius = (shell == 0 ? .070f : .185f) * Mathf.Pow(1-t, .8f);
                    radius *= 1 + .065f * Mathf.Sin(t*72);
                    vertices.Add(new Vector3(Mathf.Cos(a)*radius, Mathf.Sin(a)*radius, -t*2.8f));
                    uv.Add(new Vector2((float)i/sides, t)); colors.Add(new Color(shell == 0 ? 0 : .4f, 0, 0));
                    if(j<rings && i<sides) Quad(indices, start+j*(sides+1)+i, sides+1);
                }
            }
            for (int ring = 0; ring < 4; ring++)
            {
                const int around=32, tube=8; int start=vertices.Count;
                float radius=.15f-ring*.018f, z=-.075f-ring*.235f;
                for(int j=0;j<=around;j++) for(int i=0;i<=tube;i++)
                {
                    float a=(float)j/around*Mathf.PI*2, b=(float)i/tube*Mathf.PI*2;
                    float r=radius+.014f*Mathf.Cos(b);
                    vertices.Add(new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,z+.014f*Mathf.Sin(b)));
                    uv.Add(new Vector2((float)j/around,ring/4f)); colors.Add(Color.red);
                    if(j<around&&i<tube) Quad(indices,start+j*(tube+1)+i,tube+1);
                }
            }
            var mesh=new Mesh{name="Twin engine plasma shaft and compression rings"};
            mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetColors(colors);mesh.SetTriangles(indices,0);
            mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }

        static void Quad(List<int> indices,int i,int stride)
        { indices.Add(i);indices.Add(i+stride);indices.Add(i+1);indices.Add(i+1);indices.Add(i+stride);indices.Add(i+stride+1); }
    }
}
