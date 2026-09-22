using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace NantesGame.UI.Editor
{
    public static class NantesPresenceBuilder
    {
        const string Root="Assets/NantesUI/World/";
        public static void Create(NantesSpaceMotion motion)
        {
            var prior=motion.transform.Find("Foreground presence");if(prior!=null)Object.DestroyImmediate(prior.gameObject);
            var root=new GameObject("Foreground presence");root.transform.SetParent(motion.transform,false);
            var animation=root.AddComponent<NantesForegroundPresence>();animation.motion=motion;
            var material=new Material(Shader.Find("Nantes/Space Surface"));
            material.SetColor("_Color",new Color(.072f,.095f,.095f));material.SetFloat("_Ambient",.055f);material.SetFloat("_Exposure",.82f);
            material.SetFloat("_Rim",.48f);material.SetFloat("_RimPower",3.2f);material.SetColor("_RimColor",new Color(.15f,.25f,.28f));
            material.SetFloat("_Specular",.26f);material.SetFloat("_Gloss",36);material.SetFloat("_SurfaceDetail",1);
            material.SetVector("_LightDirection",new Vector4(-.45f,.68f,.1f,0));
            var materialPath=Root+"PresenceShell.mat";var existing=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(existing!=null){EditorUtility.CopySerialized(material,existing);Object.DestroyImmediate(material);material=existing;}else AssetDatabase.CreateAsset(material,materialPath);
            Vector3[][] paths={
                new[]{new Vector3(-8.2f,-5f,-6),new Vector3(-5.2f,-.2f,-6),new Vector3(-.5f,-2f,-5.5f),new Vector3(-2f,.8f,-5)},
                new[]{new Vector3(-7.8f,-5.8f,-7.2f),new Vector3(-2f,-5.5f,-7.5f),new Vector3(2f,-2.5f,-7.6f),new Vector3(.5f,-1.4f,-7.8f)},
                new[]{new Vector3(-9f,.1f,-8),new Vector3(-6.7f,1.9f,-8),new Vector3(-3.4f,-.2f,-7.8f),new Vector3(-4f,-1.5f,-7.5f)}
            };
            animation.limbs=new Transform[paths.Length];
            animation.roots=new Vector3[paths.Length];animation.shoulders=new Vector3[paths.Length];animation.wrists=new Vector3[paths.Length];animation.tips=new Vector3[paths.Length];
            for(int i=0;i<paths.Length;i++){
                var mesh=Limb(paths[i],i);var assetPath=Root+"PresenceLimb"+i+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
                if(old!=null){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);mesh=old;}else AssetDatabase.CreateAsset(mesh,assetPath);
                var go=new GameObject("Chitin silhouette "+i,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root.transform,false);
                go.GetComponent<MeshFilter>().sharedMesh=mesh;var r=go.GetComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;
                animation.limbs[i]=go.transform;
                animation.roots[i]=paths[i][0];animation.shoulders[i]=paths[i][1];animation.wrists[i]=paths[i][2];animation.tips[i]=paths[i][3];
            }
        }
        static Mesh Limb(Vector3[] p,int seed)
        {
            const int rings=NantesForegroundPresence.Rings,sides=NantesForegroundPresence.Sides;var v=new Vector3[(rings+1)*sides];var uv=new Vector2[v.Length];var triangles=new int[rings*sides*6];int k=0;
            NantesForegroundPresence.WriteShape(v,p[0],p[1],p[2],p[3],seed,0);
            for(int j=0;j<=rings;j++){
                for(int i=0;i<sides;i++){
                    int index=j*sides+i;uv[index]=new Vector2((float)i/sides,(float)j/rings);
                    if(j<rings){int next=j*sides+(i+1)%sides;triangles[k++]=index;triangles[k++]=index+sides;triangles[k++]=next;triangles[k++]=next;triangles[k++]=index+sides;triangles[k++]=next+sides;}
                }
            }
            var mesh=new Mesh{name="Tapered chitin silhouette",vertices=v,uv=uv,triangles=triangles};mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
    }
}
