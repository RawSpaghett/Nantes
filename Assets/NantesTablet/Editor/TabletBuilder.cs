using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace NantesGame.Tablet.Editor
{
    public static class TabletBuilder
    {
        const string Root="Assets/NantesTablet";
        public const string ScenePath=Root+"/Scenes/TabletPreview.unity";
        public const string PrefabPath=Root+"/Prefabs/Tablet.prefab";
        static Material shell,trim,rubber,edge,black,steel,lit,amber,glass,floor,shelf,box,legend;
        static TMP_FontAsset font;

        [MenuItem("Tools/Nantes Tablet/Create preview")]
        public static void CreatePreview()
        {
            foreach(string folder in new[]{"Materials","Meshes","Prefabs","Scenes"})Directory.CreateDirectory(Root+"/"+folder);
            AssetDatabase.Refresh();
            font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/NantesUI/Fonts/NantesDisplay SDF.asset");
            shell=Surface("Graphite",new Color(.12f,.137f,.135f),.72f,.43f);
            trim=Surface("Inner frame",new Color(.061f,.077f,.078f),.77f,.57f);
            rubber=Surface("Rubber",new Color(.025f,.029f,.028f),.04f,.18f);
            edge=Surface("Worn edges",new Color(.25f,.27f,.25f),.8f,.47f);
            black=Surface("Recesses",new Color(.005f,.008f,.008f),.15f,.22f);
            steel=Surface("Fasteners",new Color(.19f,.21f,.20f),.86f,.56f);
            lit=Surface("Display light",new Color(.19f,.53f,.46f),.4f,.55f,new Color(.18f,.65f,.55f)*1.6f);
            legend=Surface("Key legends",new Color(.26f,.33f,.29f),.05f,.1f,new Color(.018f,.034f,.029f));
            amber=Surface("Warning mark",new Color(.55f,.36f,.12f),.25f,.28f);
            floor=Surface("Floor",new Color(.052f,.063f,.063f),.15f,.35f);
            shelf=Surface("Shelves",new Color(.068f,.072f,.064f),.64f,.36f);
            box=Surface("Containers",new Color(.11f,.115f,.086f),.05f,.17f);
            SurfaceGrain();
            glass=new Material(Shader.Find("Nantes/Tablet Glass"));glass=Save(glass,"Materials/Display.mat");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            RenderSettings.skybox=null;RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.075f,.09f,.10f);RenderSettings.ambientIntensity=.7f;
            RenderSettings.defaultReflectionMode=DefaultReflectionMode.Custom;RenderSettings.customReflection=Reflection();RenderSettings.reflectionIntensity=.7f;
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogColor=new Color(.012f,.026f,.029f);RenderSettings.fogDensity=.034f;
            QualitySettings.antiAliasing=4;QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.High;
            var cameraGO=new GameObject("Preview camera",typeof(Camera),typeof(AudioListener));cameraGO.tag="MainCamera";
            var camera=cameraGO.GetComponent<Camera>();camera.transform.position=new Vector3(0,.15f,-9.4f);camera.fieldOfView=39;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.005f,.012f,.015f);camera.nearClipPlane=.05f;camera.farClipPlane=70;camera.allowHDR=true;camera.cullingMask=~(1<<30);
            cameraGO.AddComponent<TabletPreviewOptics>().shader=Shader.Find("Nantes/Tablet Preview Optics");
            var tablet=new GameObject("Field tablet");tablet.transform.position=new Vector3(0,.04f,0);tablet.transform.rotation=Quaternion.Euler(5,-7,-2);
            var controller=tablet.AddComponent<TabletController>();controller.font=font;controller.alienFont=TabletFont.Prepare();controller.viewCamera=camera;
            BuildDevice(tablet.transform,controller);
            PrefabUtility.SaveAsPrefabAsset(tablet,PrefabPath);
            BuildEnvironment();
            LightAt("Device key",new Vector3(-4,4.5f,-5),Quaternion.LookRotation(new Vector3(4,-4.5f,5)).eulerAngles,LightType.Spot,new Color(.76f,.87f,.87f),3.2f,16,85);
            LightAt("Device rim",new Vector3(4,2,1),Vector3.zero,LightType.Point,new Color(.28f,.56f,.65f),5.2f,9);
            LightAt("Screen bounce",new Vector3(-1,-1,-3.5f),Vector3.zero,LightType.Point,new Color(.25f,.56f,.51f),1.35f,6);
            var preview=new GameObject("Tablet preview").AddComponent<TabletPreview>();preview.tablet=controller;preview.font=font;
            EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();
            Debug.Log("TABLET_SCENE_READY");
        }

        static void BuildDevice(Transform parent,TabletController controller)
        {
            Body("Rear shell",parent,Vector3.zero,7.76f,4.74f,.34f,.43f,rubber);
            Body("Metal housing",parent,new Vector3(0,0,-.15f),7.6f,4.59f,.24f,.36f,shell);
            Body("Frame lip",parent,new Vector3(-.12f,.05f,-.276f),6.73f,4.075f,.042f,.1f,steel);
            Body("Glass gasket",parent,new Vector3(-.12f,.05f,-.309f),6.66f,4.0f,.044f,.085f,black);
            Body("Display frame",parent,new Vector3(-.12f,.05f,-.333f),6.53f,3.91f,.024f,.068f,trim);
            var display=new GameObject("Glass display",typeof(MeshFilter),typeof(MeshRenderer));display.transform.SetParent(parent,false);
            display.transform.localPosition=new Vector3(-.12f,.05f,-.351f);display.transform.localScale=new Vector3(6.3f,3.675f,1);
            var quad=new Mesh{name="Display quad"};quad.vertices=new[]{new Vector3(-.5f,-.5f,0),new Vector3(-.5f,.5f,0),new Vector3(.5f,.5f,0),new Vector3(.5f,-.5f,0)};
            quad.uv=new[]{new Vector2(0,0),new Vector2(0,1),new Vector2(1,1),new Vector2(1,0)};quad.triangles=new[]{0,1,2,0,2,3};quad.RecalculateNormals();quad=Save(quad,"Meshes/Display.asset");
            display.GetComponent<MeshFilter>().sharedMesh=quad;display.GetComponent<MeshRenderer>().sharedMaterial=glass;
            controller.displaySurface=display.transform;controller.screenRenderer=display.GetComponent<Renderer>();
            foreach(int sign in new[]{-1,1}){
                Body("Corner bumper",parent,new Vector3(sign*3.43f,2.04f,-.12f),.75f,.59f,.37f,.13f,rubber);
                Body("Corner bumper",parent,new Vector3(sign*3.43f,-2.04f,-.12f),.75f,.59f,.37f,.13f,rubber);
                for(int i=0;i<3;i++){
                    Cube("Bumper rib",parent,new Vector3(sign*(3.27f+i*.13f),2.12f,-.328f),new Vector3(.045f,.34f,.018f),trim);
                    Cube("Bumper rib",parent,new Vector3(sign*(3.27f+i*.13f),-2.12f,-.328f),new Vector3(.045f,.34f,.018f),trim);
                }
            }
            for(int i=0;i<6;i++){
                float x=i<3?-3.48f:3.48f,y=(i%3-1)*1.78f;
                Cylinder("Fastener socket",parent,new Vector3(x,y,-.30f),.095f,.018f,black);
                Cylinder("Captive screw",parent,new Vector3(x,y,-.326f),.067f,.014f,steel);
                var slot=Cube("Screw slot",parent,new Vector3(x,y,-.343f),new Vector3(.078f,.012f,.006f),black);slot.transform.localRotation=Quaternion.Euler(0,0,i*47+13);
            }
            for(int i=0;i<11;i++) {
                Body("Speaker recess",parent,new Vector3(-3.5f,(i-5)*.103f,-.29f),.17f,.041f,.02f,.013f,black);
                Cube("Speaker edge",parent,new Vector3(-3.5f,(i-5)*.103f-.019f,-.304f),new Vector3(.145f,.006f,.004f),steel);
            }
            for(int i=0;i<3;i++){
                float y=.95f-i*.94f;
                Body("Key well",parent,new Vector3(3.47f,y,-.30f),.47f,.58f,.05f,.065f,black);
                var key=Body(i==0?"Home key":i==1?"Scanner key":"Power key",parent,new Vector3(3.47f,y,-.344f),.36f,.47f,.04f,.045f,rubber);
                var collider=key.AddComponent<BoxCollider>();collider.size=new Vector3(.45f,.57f,.1f);
                var hardware=key.AddComponent<TabletHardwareKey>();hardware.tablet=controller;hardware.action=i==0?0:i==1?1:2;
                HardwareIcon(parent,new Vector3(3.47f,y,-.371f),i);
            }
            var diode=Cube("Status diode",parent,new Vector3(2.72f,2.121f,-.306f),new Vector3(.13f,.026f,.018f),lit);controller.statusLight=diode.GetComponent<Renderer>();
            for(int i=0;i<9;i++)Cube("Bottom grip",parent,new Vector3(-.7f+i*.19f,-2.09f,-.303f),new Vector3(.10f,.061f,.015f),rubber);
            Cube("Accent inlay",parent,new Vector3(-2.58f,-2.11f,-.306f),new Vector3(.58f,.024f,.012f),amber);
            Label3D("N A N T E S",parent,new Vector3(-2.86f,2.115f,-.311f),.087f,new Color(.48f,.53f,.49f),2.2f,.15f);
            Label3D("04 / 09",parent,new Vector3(1.9f,2.115f,-.311f),.071f,new Color(.33f,.4f,.37f),.7f,.12f);
            Label3D("II  /  072",parent,new Vector3(1.90f,-2.11f,-.31f),.066f,new Color(.31f,.37f,.33f),.8f,.12f);
            for(int i=0;i<16;i++)Cube("Serial ticks",parent,new Vector3(-2.85f+i*.037f,-2.085f,-.306f),new Vector3(i%3==0?.019f:.008f,i%3==0?.052f:.035f,.003f),steel);
            Cube("Housing seam",parent,new Vector3(0,2.233f,-.287f),new Vector3(5.98f,.011f,.008f),black);
            Cube("Lower housing seam",parent,new Vector3(0,-2.233f,-.287f),new Vector3(5.98f,.011f,.008f),black);
            for(int i=0;i<8;i++){
                float x=-2.85f+i*.78f;
                Cube("Edge wear",parent,new Vector3(x,2.24f,-.283f),new Vector3(.09f+(i%3)*.013f,.006f,.005f),edge);
            }
        }

        static void HardwareIcon(Transform p,Vector3 pos,int kind)
        {
            if(kind==0){Stroke(p,pos,new Vector2(-.10f,0),new Vector2(0,.09f));Stroke(p,pos,new Vector2(0,.09f),new Vector2(.10f,0));Stroke(p,pos,new Vector2(-.07f,0),new Vector2(-.07f,-.08f));Stroke(p,pos,new Vector2(.07f,0),new Vector2(.07f,-.08f));Stroke(p,pos,new Vector2(-.07f,-.08f),new Vector2(.07f,-.08f));}
            else {
                for(int i=0;i<22;i++){
                    float a=(kind==1?i*360f/22:i*280f/22-230)*Mathf.Deg2Rad,b=(kind==1?(i+1)*360f/22:(i+1)*280f/22-230)*Mathf.Deg2Rad;
                    Stroke(p,pos,new Vector2(Mathf.Cos(a),Mathf.Sin(a))*.084f,new Vector2(Mathf.Cos(b),Mathf.Sin(b))*.084f);
                }
                Stroke(p,pos,Vector2.zero,kind==1?new Vector2(.065f,.085f):new Vector2(0,.105f));
            }
        }
        static void Stroke(Transform parent,Vector3 origin,Vector2 a,Vector2 b)
        {
            var line=Cube("Etched icon",parent,origin+(Vector3)((a+b)*.5f),new Vector3((b-a).magnitude,.008f,.003f),legend);
            line.transform.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg);
        }
        static void Label3D(string text,Transform parent,Vector3 pos,float size,Color color,float width,float height)
        {
            var obj=new GameObject("Housing marking");obj.transform.SetParent(parent,false);obj.transform.localPosition=pos;
            var label=obj.AddComponent<TextMeshPro>();label.font=font;label.text=text;label.fontSize=size*10;label.color=color;label.alignment=TextAlignmentOptions.MidlineLeft;label.rectTransform.sizeDelta=new Vector2(width,height);label.rectTransform.pivot=new Vector2(0,.5f);label.textWrappingMode=TextWrappingModes.NoWrap;
        }

        static void BuildEnvironment()
        {
            var env=new GameObject("Abandoned aisle").transform;
            Cube("Floor",env,new Vector3(0,-3.4f,16),new Vector3(24,.3f,65),floor);
            Cube("Ceiling",env,new Vector3(0,7,19),new Vector3(24,.3f,55),shelf);
            Cube("Back wall",env,new Vector3(0,1.8f,43),new Vector3(24,11,.35f),shelf);
            var random=new System.Random(74);
            foreach(int side in new[]{-1,1})for(int bay=0;bay<7;bay++) {
                float x=side*6.2f,z=4+bay*5;
                for(int h=0;h<4;h++) {
                    float y=-3.08f+h*1.85f;
                    Cube("Shelf deck",env,new Vector3(x,y,z),new Vector3(2.3f,.10f,4.72f),shelf);
                    Cube("Shelf lip",env,new Vector3(x-side*1.14f,y+.08f,z),new Vector3(.045f,.18f,4.72f),trim);
                    for(int k=0;k<4;k++){
                        if(random.NextDouble()<.38)continue;
                        float height=.2f+(float)random.NextDouble()*.55f;
                        var pack=Cube("Abandoned carton",env,new Vector3(x+(float)random.NextDouble()*.9f-.45f,y+height*.5f+.055f,z-1.7f+k*1.02f),new Vector3(.35f+(float)random.NextDouble()*.6f,height,.5f+(float)random.NextDouble()*.3f),box);
                        pack.transform.localRotation=Quaternion.Euler(0,(float)random.NextDouble()*12-6,0);
                    }
                }
                foreach(int end in new[]{-1,1}){
                    Cube("Upright",env,new Vector3(x-side*1.1f,.02f,z+end*2.35f),new Vector3(.10f,6.4f,.09f),steel);
                    Cube("Back upright",env,new Vector3(x+side*1.1f,.02f,z+end*2.35f),new Vector3(.10f,6.4f,.09f),steel);
                }
            }
            for(int j=0;j<5;j++){
                float z=5+j*7;
                Cube("Ceiling housing",env,new Vector3(0,6.7f,z),new Vector3(3.7f,.17f,.6f),trim);
                var strip=Cube("Ceiling strip",env,new Vector3(0,6.58f,z),new Vector3(3.25f,.035f,.15f),lit);
                LightAt("Aisle light",new Vector3(0,5.9f,z),new Vector3(90,0,0),LightType.Spot,new Color(.27f,.48f,.48f),2.4f,15,100);
            }
            Cube("Distant doorway",env,new Vector3(0,-.5f,42.6f),new Vector3(3.6f,5.8f,.08f),black);
            Cube("Door indicator",env,new Vector3(0,2.7f,42.4f),new Vector3(.72f,.045f,.06f),amber);
            LightAt("Distant warning",new Vector3(0,2,40),Vector3.zero,LightType.Point,new Color(.65f,.11f,.025f),2.2f,5);
            for(int i=0;i<7;i++){
                var debris=Cube("Floor debris",env,new Vector3(-3+i*.85f,-3.2f,10+i*2.5f),new Vector3(.7f,.09f,.5f),box);debris.transform.localRotation=Quaternion.Euler(0,i*23,0);
            }
        }

        static GameObject Body(string name,Transform parent,Vector3 pos,float w,float h,float depth,float corner,Material material)
        {
            string key=$"{w:F3}_{h:F3}_{depth:F3}_{corner:F3}".Replace(".","_");
            string path=Root+"/Meshes/Body_"+key+".asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            var generated=BevelBox(w,h,depth,corner);
            if(!mesh){mesh=generated;AssetDatabase.CreateAsset(mesh,path);}else{EditorUtility.CopySerialized(generated,mesh);Object.DestroyImmediate(generated);EditorUtility.SetDirty(mesh);}
            var obj=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));obj.transform.SetParent(parent,false);obj.transform.localPosition=pos;obj.GetComponent<MeshFilter>().sharedMesh=mesh;obj.GetComponent<MeshRenderer>().sharedMaterial=material;return obj;
        }
        static Mesh BevelBox(float w,float h,float depth,float corner)
        {
            var vertices=new List<Vector3>();var triangles=new List<int>();var uv=new List<Vector2>();
            float bevel=Mathf.Min(depth*.25f,.035f);
            Vector2[] Ring(float inset){float x=w*.5f-inset,y=h*.5f-inset,c=Mathf.Max(.005f,corner-inset*.3f);return new[]{new Vector2(-x+c,y),new Vector2(x-c,y),new Vector2(x,y-c),new Vector2(x,-y+c),new Vector2(x-c,-y),new Vector2(-x+c,-y),new Vector2(-x,-y+c),new Vector2(-x,y-c)};}
            void Tri(Vector3 a,Vector3 b,Vector3 c){int n=vertices.Count;Vector3 faceNormal=Vector3.Cross(b-a,c-a).normalized;Vector2 UV(Vector3 p){if(Mathf.Abs(faceNormal.z)>.6f)return new Vector2(p.x,p.y);if(Mathf.Abs(faceNormal.y)>.6f)return new Vector2(p.x,p.z);return new Vector2(p.z,p.y);}vertices.Add(a);vertices.Add(b);vertices.Add(c);uv.Add(UV(a));uv.Add(UV(b));uv.Add(UV(c));triangles.Add(n);triangles.Add(n+1);triangles.Add(n+2);}
            var face=Ring(bevel);var outside=Ring(0);
            for(int i=0;i<8;i++){
                int j=(i+1)%8;Vector3 a=new Vector3(face[i].x,face[i].y,-depth/2),b=new Vector3(face[j].x,face[j].y,-depth/2),c=new Vector3(outside[i].x,outside[i].y,-depth/2+bevel),d=new Vector3(outside[j].x,outside[j].y,-depth/2+bevel),e=new Vector3(outside[i].x,outside[i].y,depth/2),f=new Vector3(outside[j].x,outside[j].y,depth/2);
                Tri(new Vector3(0,0,-depth/2),a,b);Tri(a,c,b);Tri(b,c,d);Tri(c,e,d);Tri(d,e,f);Tri(new Vector3(0,0,depth/2),f,e);
            }
            var mesh=new Mesh{name="Beveled housing"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.SetUVs(0,uv);mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();return mesh;
        }
        static GameObject Cube(string name,Transform parent,Vector3 pos,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=pos;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;Object.DestroyImmediate(go.GetComponent<Collider>());return go;
        }
        static void Cylinder(string name,Transform parent,Vector3 pos,float radius,float depth,Material mat)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=pos;go.transform.localRotation=Quaternion.Euler(90,0,0);go.transform.localScale=new Vector3(radius*2,depth*.5f,radius*2);go.GetComponent<Renderer>().sharedMaterial=mat;Object.DestroyImmediate(go.GetComponent<Collider>());
        }
        static void LightAt(string name,Vector3 pos,Vector3 rot,LightType type,Color color,float intensity,float range,float angle=60)
        {
            var go=new GameObject(name,typeof(Light));go.transform.position=pos;go.transform.rotation=Quaternion.Euler(rot);var light=go.GetComponent<Light>();light.type=type;light.color=color;light.intensity=intensity;light.range=range;light.spotAngle=angle;light.shadows=name=="Device key"?LightShadows.Soft:LightShadows.None;
        }
        static Material Surface(string name,Color color,float metallic,float smoothness,Color emission=default)
        {
            bool urp=GraphicsSettings.currentRenderPipeline!=null;
            var mat=new Material(Shader.Find(urp?"Universal Render Pipeline/Lit":"Standard")){name=name};
            mat.color=color;mat.SetColor("_BaseColor",color);mat.SetFloat("_Metallic",metallic);mat.SetFloat("_Glossiness",smoothness);mat.SetFloat("_Smoothness",smoothness);
            if(emission!=default){mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",emission);}
            return Save(mat,"Materials/"+name+".mat");
        }
        static void SurfaceGrain()
        {
            const int size=256;
            var normal=new Texture2D(size,size,TextureFormat.RGBA32,true,true){name="Housing grain",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear};
            var paint=new Texture2D(size,size,TextureFormat.RGBA32,true,false){name="Housing finish",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear};
            var random=new System.Random(492);
            var normals=new Color[size*size];var colors=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++) {
                float nx=(float)(random.NextDouble()-.5)*.28f,ny=(float)(random.NextDouble()-.5)*.28f;
                normals[x+y*size]=new Color(.5f+nx,.5f+ny,1,1);
                float grain=.78f+(float)random.NextDouble()*.18f;
                float scuff=Mathf.Pow(Mathf.PerlinNoise(x*.045f,y*.045f),5)*.17f;
                colors[x+y*size]=new Color(grain+scuff,grain+scuff,grain+scuff,.56f);
            }
            normal.SetPixels(normals);normal.Apply();paint.SetPixels(colors);paint.Apply();normal=Save(normal,"Materials/Grain.asset");paint=Save(paint,"Materials/Finish.asset");
            foreach(var mat in new[]{shell,trim,steel,rubber,edge}) {
                mat.SetTexture("_BumpMap",normal);mat.SetFloat("_BumpScale",mat==rubber?.5f:.24f);mat.EnableKeyword("_NORMALMAP");mat.SetTextureScale("_BumpMap",new Vector2(4,4));
                mat.mainTexture=paint;mat.mainTextureScale=new Vector2(3,3);EditorUtility.SetDirty(mat);
            }
        }
        static Cubemap Reflection()
        {
            const int size=64;var cube=new Cubemap(size,TextureFormat.RGBAHalf,true){name="Aisle reflections"};
            for(int face=0;face<6;face++) {
                var colors=new Color[size*size];
                for(int y=0;y<size;y++)for(int x=0;x<size;x++) {
                    float u=(x+.5f)/size*2-1,v=(y+.5f)/size*2-1;
                    Vector3 d=face==0?new Vector3(1,-v,-u):face==1?new Vector3(-1,-v,u):face==2?new Vector3(u,1,v):face==3?new Vector3(u,-1,-v):face==4?new Vector3(u,-v,1):new Vector3(-u,-v,-1);d.Normalize();
                    float strip=Mathf.Pow(Mathf.Clamp01(1-Mathf.Abs(d.y-.62f)*10),4)*Mathf.Clamp01(-d.z*3);
                    float rim=Mathf.Pow(Mathf.Max(0,Vector3.Dot(d,new Vector3(-.7f,.3f,-.5f).normalized)),12);
                    colors[x+y*size]=new Color(.017f,.024f,.028f)+new Color(.58f,.68f,.64f)*strip+new Color(.16f,.25f,.29f)*rim;
                }
                cube.SetPixels(colors,(CubemapFace)face);
            }
            cube.Apply(true);return Save(cube,"Materials/AisleReflection.asset");
        }
        static T Save<T>(T obj,string relative) where T:Object {string path=Root+"/"+relative;var existing=AssetDatabase.LoadAssetAtPath<T>(path);if(existing){EditorUtility.CopySerialized(obj,existing);Object.DestroyImmediate(obj);return existing;}AssetDatabase.CreateAsset(obj,path);return obj;}

        [MenuItem("Tools/Nantes Tablet/Open preview")]
        public static void Open(){EditorSceneManager.OpenScene(ScenePath);}

        public static void Build()
        {
            CreatePreview();
            PlayerSettings.companyName="Nantes";PlayerSettings.productName="Nantes Tablet";PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;PlayerSettings.defaultIsNativeResolution=true;PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=true;
            string output=Path.GetFullPath("../../outputs/Nantes_Tablet_Preview/Nantes_Tablet_Preview.exe");Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Tablet preview build failed");
            AssetDatabase.ExportPackage(new[]{Root,"Assets/NantesUI/Fonts","Assets/NantesUI/Documentation/Licenses","Assets/NantesUI/Documentation/Tablet.md"},"../../outputs/Nantes_Tablet.unitypackage",ExportPackageOptions.Recurse|ExportPackageOptions.IncludeDependencies);
            Debug.Log("TABLET_BUILD_PASS");
        }
    }
}
