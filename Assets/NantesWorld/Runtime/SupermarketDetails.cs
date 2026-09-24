using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NantesGame.World
{
    public sealed class SupermarketDetails : MonoBehaviour
    {
        readonly List<Renderer> changed = new List<Renderer>();
        readonly List<Material[]> original = new List<Material[]>();
        WorldAtmosphere atmosphere;
        RenderPipelineAsset previousPipeline;
        UniversalRenderPipelineAsset lightingPipeline;

        void Start()
        {
            previousPipeline=QualitySettings.renderPipeline;
            if(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset pipeline)
            {
                lightingPipeline=Instantiate(pipeline);
                lightingPipeline.name="Supermarket lighting";
                lightingPipeline.shadowDistance=Mathf.Max(130,pipeline.shadowDistance);
                QualitySettings.renderPipeline=lightingPipeline;
            }
            var ground=Resources.Load<Material>("WorldSurfaces/Outside");
            var floor=Resources.Load<Material>("WorldSurfaces/Floor");
            var wall=Resources.Load<Material>("WorldSurfaces/Wall");
            var roof=Resources.Load<Material>("WorldSurfaces/Roof");
            var metal=Resources.Load<Material>("WorldSurfaces/Fixture");
            var tube=Resources.Load<Material>("WorldSurfaces/Tube");
            foreach(var renderer in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if(renderer.gameObject.scene != gameObject.scene) continue;
                Material replacement=null;
                if(renderer.name=="Store_Exterior") replacement=ground;
                else if(renderer.name.StartsWith("Floor_Squared")) replacement=floor;
                else if(renderer.name.Contains("Wall_")) replacement=wall;
                if(!replacement) continue;
                changed.Add(renderer); original.Add(renderer.sharedMaterials);
                var materials=new Material[renderer.sharedMaterials.Length];
                for(int i=0;i<materials.Length;i++) materials[i]=replacement;
                renderer.sharedMaterials=materials;
            }
            Roof(roof);
            for(float z=-18;z<78;z+=12)
                Cube("Roof support",new Vector3(0,3.24f,z),new Vector3(58.3f,.15f,.13f),metal,false);
            int index=0;
            for(float z=-12;z<78;z+=14)
            for(float x=-23;x<29;x+=12)
            {
                Cube("Ceiling fixture",new Vector3(x,3.22f,z),new Vector3(.38f,.1f,1.55f),metal,false);
                int slot=index++;
                bool damaged=slot%6==4;
                bool working=slot%6==1 || damaged;
                if(!working) continue;
                var diffuser=Cube("Lamp diffuser",new Vector3(x,3.157f,z),new Vector3(.27f,.035f,1.36f),tube,false);
                var lamp=new GameObject("Ceiling light");lamp.transform.SetParent(transform,false);lamp.transform.position=new Vector3(x,3.10f,z);
                var light=lamp.AddComponent<Light>();
                light.type=LightType.Spot;light.transform.rotation=Quaternion.Euler(90,0,0);
                light.spotAngle=120;light.innerSpotAngle=65;light.range=9.5f;
                light.color=index%7==0?new Color(1,.74f,.43f):new Color(.70f,.83f,.78f);
                light.intensity=damaged?.65f:1.25f;light.shadows=LightShadows.Soft;
                light.shadowBias=.015f;light.shadowNormalBias=.08f;
                light.shadowCustomResolution=512;
                if(damaged) lamp.AddComponent<DamagedFixture>().Setup(light,diffuser.GetComponent<Renderer>(),slot);
            }
            atmosphere=GetComponent<WorldAtmosphere>();
            if(!atmosphere) atmosphere=GetComponentInChildren<WorldAtmosphere>();
            //A little fill keeps the aisles readable between working lamps.
            if(atmosphere) atmosphere.lightScale=.20f;
        }

        void Roof(Material material)
        {
            var roof=new GameObject("Store roof");
            roof.transform.SetParent(transform,false);
            roof.transform.position=new Vector3(0,3.38f,28.8f);
            roof.AddComponent<BoxCollider>().size=new Vector3(58.6f,.4f,97.6f);
            //Small sections keep the roof's shadows intact when the camera turns.
            for(float z=-20;z<77.6f;z+=8)
            for(float x=-29.3f;x<29.3f;x+=8)
            {
                float width=Mathf.Min(8,29.3f-x),length=Mathf.Min(8,77.6f-z);
                var section=Cube("Roof section",new Vector3(x+width/2,3.38f,z+length/2),
                    new Vector3(width+.02f,.4f,length+.02f),material,false);
                section.transform.SetParent(roof.transform,true);
            }
        }

        GameObject Cube(string name,Vector3 position,Vector3 scale,Material material,bool solid)
        {
            var cube=GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name=name;cube.transform.SetParent(transform,false);
            cube.transform.position=position;cube.transform.localScale=scale;
            cube.GetComponent<Renderer>().sharedMaterial=material;
            if(!solid) Destroy(cube.GetComponent<Collider>());
            return cube;
        }

        void OnDestroy()
        {
            for(int i=0;i<changed.Count;i++) if(changed[i]) changed[i].sharedMaterials=original[i];
            if(atmosphere) atmosphere.lightScale=1;
            if(lightingPipeline)
            {
                if(QualitySettings.renderPipeline==lightingPipeline) QualitySettings.renderPipeline=previousPipeline;
                Destroy(lightingPipeline);
            }
        }
    }
}
