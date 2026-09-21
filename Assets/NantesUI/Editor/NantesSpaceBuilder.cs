using UnityEngine;

namespace Nantes.UI.Editor
{
    public static class NantesSpaceBuilder
    {
        public static Mesh Stars()
        {
            const int count = 720;
            var random = new System.Random(4725);
            var v = new Vector3[count * 4]; var uv = new Vector2[v.Length];
            var phases = new Vector2[v.Length]; var colors = new Color[v.Length]; var tris = new int[count * 6];
            for (int i = 0; i < count; i++)
            {
                float z = i % 3 == 0 ? 35 : i % 3 == 1 ? 65 : 105;
                float depth = (z + 25) / 25;
                Vector3 p = new Vector3(((float)random.NextDouble() - .5f) * 40 * depth, ((float)random.NextDouble() - .5f) * 14 * depth, z);
                bool bright=i%19==0;
                float size = Mathf.Lerp(.012f, bright?.046f:.029f, (float)random.NextDouble()) * depth;
                float alpha = Mathf.Lerp(.085f, bright?.52f:.30f, (float)random.NextDouble());
                var phase = new Vector2((float)random.NextDouble() * 6.28f, .35f + (float)random.NextDouble() * .5f);
                float hue=(float)random.NextDouble();
                Color tint=hue<.30f?new Color(.50f,.69f,1):hue<.60f?new Color(.81f,.89f,1):
                    hue<.83f?new Color(1,.76f,.46f):hue<.94f?new Color(1,.47f,.35f):new Color(.56f,.91f,.84f);
                tint.a=alpha;
                for (int q = 0; q < 4; q++)
                {
                    Vector2 corner = new Vector2(q == 0 || q == 3 ? -1 : 1, q < 2 ? -1 : 1);
                    v[i*4+q] = p + (Vector3)(corner * size);
                    uv[i*4+q] = corner; phases[i*4+q] = phase;
                    colors[i*4+q] = tint;
                }
                int t = i * 6, k = i * 4;
                tris[t] = k; tris[t+1] = k+1; tris[t+2] = k+2;
                tris[t+3] = k; tris[t+4] = k+2; tris[t+5] = k+3;
            }
            var mesh = new Mesh { name = "Three depths of cool and warm stars", vertices = v, uv = uv, uv2 = phases, colors = colors, triangles = tris };
            mesh.RecalculateBounds(); return mesh;
        }
    }
}
