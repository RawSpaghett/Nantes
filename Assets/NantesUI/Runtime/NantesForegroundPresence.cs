using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace NantesGame.UI
{
    [DefaultExecutionOrder(50)]
    [MovedFrom("Nantes.UI")]
    public sealed class NantesForegroundPresence : MonoBehaviour
    {
        public NantesSpaceMotion motion;
        public Transform[] limbs;
        public Vector3[] roots, shoulders, wrists, tips;
        public float ReachAmount { get; private set; }
        public const int Rings = 128, Sides = 12;
        Mesh[] meshes;
        Vector3[][] vertices;
        Vector3[][] centers;
        float[][] radii;
        Vector3[] currentTips;
        NantesEncounterDirector encounter;
        MaterialPropertyBlock lightProperties;

        void Awake()
        {
            meshes=new Mesh[limbs.Length];vertices=new Vector3[limbs.Length][];currentTips=new Vector3[limbs.Length];
            centers=new Vector3[limbs.Length][];radii=new float[limbs.Length][];
            encounter=motion.GetComponent<NantesEncounterDirector>();
            for(int i=0;i<limbs.Length;i++)
            {
                meshes[i]=Instantiate(limbs[i].GetComponent<MeshFilter>().sharedMesh);
                meshes[i].MarkDynamic();vertices[i]=meshes[i].vertices;
                centers[i]=new Vector3[Rings+1];radii[i]=new float[Rings+1];
                limbs[i].GetComponent<MeshFilter>().sharedMesh=meshes[i];
            }
        }

        void LateUpdate()
        { Evaluate(motion.AnimationTime); }

        public Vector3 TipPosition(int index) { return currentTips[index]; }
        public Vector3 CenterPoint(int limb,float fraction) { return transform.TransformPoint(centers[limb][Mathf.RoundToInt(fraction*Rings)]); }

        public void Evaluate(float t)
        {
            if(meshes==null)return;
            if(lightProperties==null)lightProperties=new MaterialPropertyBlock();
            float engine=encounter!=null&&encounter.OverridesThrust?encounter.EscapeThrust:Mathf.Pow(motion.Throttle,.55f);
            lightProperties.SetVector("_EnginePosition",motion.saucerBody.TransformPoint(new Vector3(0,0,-2.25f)));
            lightProperties.SetColor("_EngineColor",NantesThrusters.EngineLight(engine)*engine*1.5f);
            ReachAmount=0;
            for(int i=0;i<limbs.Length;i++)
            {
                float phase=t*.31f+i*2.1f;
                float attention=Mathf.SmoothStep(0,1,Mathf.PerlinNoise(t*.11f+i*7.3f,4.6f));
                Vector3 curl=new Vector3(
                    Mathf.Sin(phase)*.92f+(Mathf.PerlinNoise(t*.23f,i*5.7f)-.5f)*1.35f,
                    Mathf.Cos(phase*.83f)*.72f+Mathf.Sin(t*.81f+i*1.4f)*.22f,
                    Mathf.Sin(phase*.7f)*.38f);
                Vector3 restingTip=tips[i]+curl;
                // Aim slightly behind the moving ship.
                Vector3 ship=motion.saucer.localPosition-motion.Velocity*.7f;
                float depth=(tips[i].z+25)/Mathf.Max(5,ship.z+25);
                Vector3 target=new Vector3(ship.x*depth,ship.y*depth,tips[i].z);
                float distance=Vector2.Distance(restingTip,target);
                float proximity=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.1f,5.2f,distance));
                float reach=proximity*(.52f+.48f*attention);
                ReachAmount=Mathf.Max(ReachAmount,reach);
                Vector3 tip=Vector3.Lerp(restingTip,target+new Vector3(-.15f,-.26f,0),reach*.90f);
                Vector3 shoulder=shoulders[i]+new Vector3(Mathf.Sin(phase*.73f)*.50f,Mathf.Cos(phase)*.40f,0);
                Vector3 wrist=wrists[i]+curl*.55f+(tip-restingTip)*.58f;
                wrist+=new Vector3(Mathf.Cos(phase)*.55f,Mathf.Sin(phase)*.65f,0);
                FillAmbient(centers[i],radii[i],roots[i],shoulder,wrist,tip,i,t);
                if(encounter!=null&&encounter.BindingWeight>0)
                    for(int ring=0;ring<=Rings;ring++)
                    {
                        float u=(float)ring/Rings;
                        if(encounter.BindingPoint(i,u,transform.TransformPoint(roots[i]),transform.TransformPoint(shoulder),out var contact,out var thickness))
                        {
                            centers[i][ring]=Vector3.Lerp(centers[i][ring],transform.InverseTransformPoint(contact),encounter.BindingWeight);
                            radii[i][ring]=Mathf.Lerp(radii[i][ring],thickness,encounter.BindingWeight);
                        }
                    }
                currentTips[i]=centers[i][Rings];
                Skin(vertices[i],centers[i],radii[i]);
                meshes[i].vertices=vertices[i];meshes[i].RecalculateNormals();meshes[i].RecalculateBounds();
                limbs[i].GetComponent<Renderer>().SetPropertyBlock(lightProperties);
            }
        }

        public static void WriteShape(Vector3[] v,Vector3 p0,Vector3 p1,Vector3 p2,Vector3 p3,int seed,float time)
        {
            var points=new Vector3[Rings+1];var widths=new float[Rings+1];
            FillAmbient(points,widths,p0,p1,p2,p3,seed,time);Skin(v,points,widths);
        }

        static void FillAmbient(Vector3[] points,float[] widths,Vector3 p0,Vector3 p1,Vector3 p2,Vector3 p3,int seed,float time)
        {
            for(int j=0;j<=Rings;j++)
            {
                float u=(float)j/Rings,q=1-u;
                Vector3 center=q*q*q*p0+3*q*q*u*p1+3*q*u*u*p2+u*u*u*p3;
                Vector3 tangent=(3*q*q*(p1-p0)+6*q*u*(p2-p1)+3*u*u*(p3-p2)).normalized;
                Vector3 side=Vector3.Cross(tangent,Vector3.forward).normalized;
                float phase=time*(.73f+seed*.055f)-u*8.8f+seed*2.7f;
                float distal=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.30f,1,u));
                center+=side*(Mathf.Sin(phase)*.21f*u+Mathf.Sin(phase*1.61f+u*5)*.13f*distal);
                center+=Vector3.forward*Mathf.Sin(phase*.83f+seed)*.12f*distal;
                float hook=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.66f,1,u));
                float unfurl=.5f+.5f*Mathf.Sin(time*.49f+seed*3.1f);
                center+=side*Mathf.Sin(hook*Mathf.Lerp(2.4f,4.7f,unfurl))*.42f*hook;
                center-=tangent*hook*hook*Mathf.Lerp(.10f,.52f,unfurl);
                float radius=.17f*Mathf.Pow(q,1.34f)+.0025f;
                radius+=.028f*Mathf.Pow(.5f+.5f*Mathf.Cos(u*78+seed),9)*q;
                points[j]=center;widths[j]=radius;
            }
        }

        static void Skin(Vector3[] vertices,Vector3[] points,float[] widths)
        {
            Vector3 side=Vector3.right,previous=Vector3.up;
            for(int j=0;j<=Rings;j++)
            {
                Vector3 tangent=(points[Mathf.Min(Rings,j+1)]-points[Mathf.Max(0,j-1)]).normalized;
                if(tangent.sqrMagnitude<.01f)tangent=previous;
                side=Vector3.ProjectOnPlane(side,tangent);
                if(side.sqrMagnitude<.001f)side=Vector3.Cross(tangent,Mathf.Abs(tangent.z)<.9f?Vector3.forward:Vector3.up);
                side.Normalize();Vector3 up=Vector3.Cross(side,tangent).normalized;previous=tangent;
                for(int k=0;k<Sides;k++)
                {
                    float a=(float)k/Sides*Mathf.PI*2;
                    vertices[j*Sides+k]=points[j]+side*Mathf.Cos(a)*widths[j]+up*Mathf.Sin(a)*widths[j]*.83f;
                }
            }
        }

        void OnDestroy()
        {
            if(meshes!=null)foreach(var mesh in meshes)if(mesh!=null)Destroy(mesh);
        }
    }
}
