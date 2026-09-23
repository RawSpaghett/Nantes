using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TMPro;

namespace NantesGame.UI.Editor
{
    public static class NantesHorrorRefresh
    {
        const string Root="Assets/NantesUI/";
        [MenuItem("Tools/Nantes UI/Apply horror art direction")]
        public static void Apply()
        {
            Texture("Logo/NantesLogo.png",4096);
            Texture("ThirdParty/3DHaupt/ShipAlbedo.jpg",4096);
            Texture("ThirdParty/3DHaupt/ShipEmission.jpg",4096);
            var theme=AssetDatabase.LoadAssetAtPath<NantesTheme>(Root+"NantesTheme.asset");
            if(theme==null){theme=ScriptableObject.CreateInstance<NantesTheme>();AssetDatabase.CreateAsset(theme,Root+"NantesTheme.asset");}
            theme.bone=new Color(.78f,.82f,.80f);theme.muted=new Color(.51f,.58f,.58f);
            theme.disabled=new Color(.25f,.29f,.29f);theme.blood=new Color(.48f,.16f,.14f);
            theme.deepRed=new Color(.10f,.13f,.13f);EditorUtility.SetDirty(theme);
            var prefab=PrefabUtility.LoadPrefabContents(NantesMenuBuilder.PrefabPath);
            try
            {
                var menu=prefab.GetComponent<NantesMenu>();menu.theme=theme;
                var frame=prefab.transform.Find("Safe frame");
                var selector=frame.GetComponentInChildren<NantesTendrilSelector>(true);
                if(selector==null)
                {
                    var selectorObject=new GameObject("Searching selector tendril",typeof(RectTransform),typeof(NantesTendrilSelector));
                    selectorObject.transform.SetParent(frame,false);
                    selector=selectorObject.GetComponent<NantesTendrilSelector>();
                }
                var selectorRect=selector.rectTransform;
                selectorRect.anchorMin=Vector2.zero;selectorRect.anchorMax=Vector2.one;
                selectorRect.offsetMin=selectorRect.offsetMax=Vector2.zero;
                selectorRect.SetAsFirstSibling();selector.raycastTarget=false;selector.menu=menu;menu.selector=selector;
                selector.whipSound=AssetDatabase.LoadAssetAtPath<AudioClip>(Root+"Audio/Selector-Whip.wav");
                foreach(var name in new[]{"NANTES SDF wordmark","NANTES engraved wordmark"})
                {var old=frame.Find(name);if(old!=null)Object.DestroyImmediate(old.gameObject);}
                var logoObject=new GameObject("NANTES engraved wordmark",typeof(RectTransform),typeof(RawImage));
                logoObject.transform.SetParent(frame,false);
                var rect=logoObject.GetComponent<RectTransform>();
                rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);
                rect.anchoredPosition=new Vector2(140,-179);rect.sizeDelta=new Vector2(1060,165);
                var logo=logoObject.GetComponent<RawImage>();logo.raycastTarget=false;
                logo.texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"Logo/NantesLogo.png");
                var logoMaterial=new Material(Shader.Find("Nantes/Engraved Logo"));
                logo.material=Save(logoMaterial,Root+"Logo/NantesLogo.mat");
                logoObject.AddComponent<NantesLogoAnimator>().menu=menu;
                foreach(var text in prefab.GetComponentsInChildren<TMP_Text>(true))
                {
                    text.color=text.fontSize<=15?theme.muted:theme.bone;
                    if(text.name=="Asset credits")
                    {
                        text.text=NantesCredits.Text;
                        text.fontSize=12;text.rectTransform.sizeDelta=new Vector2(610,230);
                        Place(text.rectTransform,0,222);
                    }
                }
                foreach(var item in prefab.GetComponentsInChildren<NantesMenuItem>(true))
                {
                    item.outline.color=theme.bone;
                    item.label.fontSize=20;item.label.characterSpacing=3;
                    var marker=item.marker.GetComponent<Image>();if(marker!=null){marker.enabled=false;marker.raycastTarget=false;}
                }
                foreach(var image in prefab.GetComponentsInChildren<Image>(true))
                {
                    if(image.name=="Fill"||image.name=="On")image.color=theme.bone;
                    if(image.name=="Track"||image.name=="Box")image.color=theme.deepRed;
                    if(image.name=="Box")image.color=new Color(.23f,.28f,.29f);
                    if(image.name=="Handle")image.color=theme.bone;
                }
                Place((RectTransform)menu.mainPage.transform,1300,420);
                var buttons=new[]{menu.newGameButton,menu.continueButton,menu.settingsButton,menu.extrasButton,menu.quitButton};
                float[] rows={0,76,170,246,322};
                for(int i=0;i<buttons.Length;i++)Place((RectTransform)buttons[i].transform,0,rows[i]);
                Place(menu.continueHint.rectTransform,58,130);
                foreach(var page in new[]{menu.settingsPage,menu.extrasPage,menu.quitPage})Place((RectTransform)page.transform,1210,365);
                foreach(var text in prefab.GetComponentsInChildren<TMP_Text>(true))
                    if(text.text.StartsWith("ARROWS")){Place(text.rectTransform,990,994);text.alignment=TextAlignmentOptions.MidlineRight;}
                PrefabUtility.SaveAsPrefabAsset(prefab,NantesMenuBuilder.PrefabPath);
            }
            finally{PrefabUtility.UnloadPrefabContents(prefab);}
            var scene=EditorSceneManager.OpenScene(NantesMenuBuilder.ScenePath);
            var motion=Object.FindAnyObjectByType<NantesSpaceMotion>();
            var preview=Object.FindAnyObjectByType<NantesMenuPreview>();
            if(preview!=null&&preview.previewNote!=null){Place(preview.previewNote.rectTransform,100,994);preview.previewNote.rectTransform.sizeDelta=new Vector2(760,30);preview.previewNote.alignment=TextAlignmentOptions.MidlineLeft;}
            motion.planet.localPosition=new Vector3(-9.8f,-9.5f,17);
            motion.flightLegSeconds=13;
            var stars=NantesSpaceBuilder.Stars();
            var starPath=Root+"World/Starfield.asset";
            var previousStars=AssetDatabase.LoadAssetAtPath<Mesh>(starPath);
            if(previousStars!=null){EditorUtility.CopySerialized(stars,previousStars);Object.DestroyImmediate(stars);stars=previousStars;EditorUtility.SetDirty(stars);}
            else AssetDatabase.CreateAsset(stars,starPath);
            motion.stars.GetComponent<MeshFilter>().sharedMesh=stars;
            var earth=motion.planet.GetComponent<Renderer>().sharedMaterial;
            earth.SetColor("_Color",new Color(.34f,.47f,.54f));earth.SetFloat("_Exposure",.82f);earth.SetFloat("_Saturation",.28f);
            earth.SetFloat("_Ambient",.0015f);earth.SetVector("_LightDirection",new Vector4(-.45f,.68f,.1f,0));
            earth.SetColor("_RimColor",new Color(.15f,.42f,.56f));earth.SetFloat("_Rim",.28f);earth.SetFloat("_RimPower",6);EditorUtility.SetDirty(earth);
            Atmosphere(motion);
            var haze=motion.dust.sharedMaterial;
            haze.SetColor("_ColdTint",new Color(.16f,.24f,.28f));haze.SetColor("_RustTint",new Color(.25f,.075f,.11f));
            haze.SetFloat("_Opacity",.80f);haze.SetVector("_Flow",new Vector4(.0015f,-.0006f,8,5));
            haze.SetVector("_Band",new Vector4(.66f,-.22f,3.1f,6.3f));EditorUtility.SetDirty(haze);
            var smoke=motion.foregroundSmoke[0].sharedMaterial;
            smoke.SetColor("_ColdTint",new Color(.18f,.25f,.27f));smoke.SetColor("_RustTint",new Color(.24f,.08f,.12f));
            smoke.SetFloat("_Opacity",.95f);EditorUtility.SetDirty(smoke);
            UseShip(motion);
            var depth=motion.GetComponent<NantesTitleDepth>();if(depth==null)depth=motion.gameObject.AddComponent<NantesTitleDepth>();
            depth.motion=motion;depth.title=Object.FindAnyObjectByType<NantesLogoAnimator>();
            var encounter=motion.GetComponent<NantesEncounterDirector>();if(encounter==null)encounter=motion.gameObject.AddComponent<NantesEncounterDirector>();
            encounter.motion=motion;encounter.chancePerEncounter=.10f;
            UseMusic();
            NantesEncounterEffectsBuilder.Create(motion,encounter);
            NantesPresenceBuilder.Create(motion);
            var cinema=motion.GetComponent<NantesCinematicFrame>();if(cinema==null)cinema=motion.gameObject.AddComponent<NantesCinematicFrame>();
            cinema.motion=motion;cinema.presentation=Save(new Material(Shader.Find("Nantes/Cinematic World")),Root+"World/CinematicWorld.mat");
            cinema.bloomProcessing=Save(new Material(Shader.Find("Nantes/Bloom Filter")),Root+"World/BloomFilter.mat");
            EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("NANTES_HORROR_ART_PASS");
        }

        static void Atmosphere(NantesSpaceMotion motion)
        {
            var old=motion.planet.Find("Atmospheric scattering");if(old!=null)Object.DestroyImmediate(old.gameObject);
            var shell=new GameObject("Atmospheric scattering",typeof(MeshFilter),typeof(MeshRenderer));
            shell.transform.SetParent(motion.planet,false);shell.transform.localScale=Vector3.one*1.009f;
            shell.GetComponent<MeshFilter>().sharedMesh=motion.planet.GetComponent<MeshFilter>().sharedMesh;
            var material=new Material(Shader.Find("Nantes/Planet Atmosphere"));
            material.SetVector("_LightDirection",new Vector4(-.45f,.68f,.1f,0));
            material.SetColor("_Color",new Color(.15f,.39f,.50f));material.SetFloat("_Intensity",.85f);
            var renderer=shell.GetComponent<MeshRenderer>();renderer.sharedMaterial=Save(material,Root+"World/PlanetAtmosphere.mat");
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
        }

        static void UseMusic()
        {
            foreach(var audio in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
            {audio.Stop();audio.playOnAwake=false;audio.clip=null;audio.loop=false;}
            const string path=Root+"ThirdParty/JuliusH/Universe-Space-Sounds.ogg";
            var importer=(AudioImporter)AssetImporter.GetAtPath(path);
            var settings=importer.defaultSampleSettings;
            settings.loadType=AudioClipLoadType.Streaming;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.85f;
            importer.defaultSampleSettings=settings;importer.loadInBackground=true;importer.SaveAndReimport();
            var go=GameObject.Find("Menu music - JuliusH");
            if(go==null)go=new GameObject("Menu music - JuliusH");
            var source=go.GetComponent<AudioSource>();if(source==null)source=go.AddComponent<AudioSource>();
            source.clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            source.loop=true;source.playOnAwake=false;source.spatialBlend=0;source.volume=0;
            if(go.GetComponent<NantesMenuMusic>()==null)go.AddComponent<NantesMenuMusic>();
        }

        static void UseShip(NantesSpaceMotion motion)
        {
            var importer=(ModelImporter)AssetImporter.GetAtPath(Root+"ThirdParty/3DHaupt/IntergalacticShip.obj");
            importer.materialImportMode=ModelImporterMaterialImportMode.None;
            importer.importNormals=ModelImporterNormals.Import;importer.importTangents=ModelImporterTangents.CalculateMikk;importer.SaveAndReimport();
            if(motion.saucer!=null)Object.DestroyImmediate(motion.saucer.gameObject);
            motion.wake=null;
            var flight=new GameObject("Intergalactic ship flight");flight.transform.SetParent(motion.transform,false);motion.saucer=flight.transform;
            var body=new GameObject("Ship bank and attitude");body.transform.SetParent(flight.transform,false);motion.saucerBody=body.transform;
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"ThirdParty/3DHaupt/IntergalacticShip.obj");
            var model=(GameObject)PrefabUtility.InstantiatePrefab(asset);model.name="Intergalactic spaceship - Dennis Haupt";model.transform.SetParent(body.transform,false);
            var mesh=model.GetComponentInChildren<MeshFilter>().sharedMesh;
            model.transform.localScale=Vector3.one*(3.6f/mesh.bounds.size.z);
            var mat=new Material(Shader.Find("Nantes/Space Surface"));
            mat.SetTexture("_MainTex",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"ThirdParty/3DHaupt/ShipAlbedo.jpg"));
            mat.SetTexture("_EmissionMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"ThirdParty/3DHaupt/ShipEmission.jpg"));
            mat.SetColor("_Color",new Color(.40f,.48f,.51f));mat.SetFloat("_Saturation",.08f);mat.SetFloat("_AlbedoFloor",.12f);
            mat.SetFloat("_Ambient",.018f);mat.SetFloat("_Exposure",.85f);mat.SetFloat("_Specular",.22f);mat.SetFloat("_Gloss",48);mat.SetFloat("_Rim",.32f);mat.SetFloat("_RimPower",4.5f);
            mat.SetColor("_RimColor",new Color(.22f,.36f,.42f));
            mat.SetFloat("_MappedEmission",.22f);mat.SetColor("_EmissionTint",new Color(.08f,.72f,1f));
            mat.SetVector("_LightDirection",new Vector4(-.45f,.68f,.1f,0));
            mat=Save(mat,Root+"World/IntergalacticShip.mat");
            var renderers=model.GetComponentsInChildren<Renderer>();
            foreach(var renderer in renderers){renderer.sharedMaterial=mat;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;}
            var lights=flight.AddComponent<NantesVesselLights>();lights.motion=motion;lights.apertures=renderers;
            var jetMesh=NantesExhaustBuilder.Create();var jetMeshPath=Root+"World/ThrusterCone.asset";
            var oldMesh=AssetDatabase.LoadAssetAtPath<Mesh>(jetMeshPath);
            if(oldMesh!=null){EditorUtility.CopySerialized(jetMesh,oldMesh);Object.DestroyImmediate(jetMesh);jetMesh=oldMesh;}else AssetDatabase.CreateAsset(jetMesh,jetMeshPath);
            var jetMaterial=Save(new Material(Shader.Find("Nantes/Engine Exhaust")),Root+"World/ThrusterExhaust.mat");
            float modelScale=3.6f/mesh.bounds.size.z;
            var jets=flight.AddComponent<NantesThrusters>();jets.motion=motion;
            // Engine ports use centered model coordinates.
            jets.mainJets=new[]{Jet("Port main engine",body.transform,new Vector3(-1.025f,-.014f,-4.0f)*modelScale,Quaternion.identity,jetMesh,jetMaterial),Jet("Starboard main engine",body.transform,new Vector3(1.034f,-.018f,-4.0f)*modelScale,Quaternion.identity,jetMesh,jetMaterial)};
            jets.brakingJets=new[]{Jet("Port retrojet",body.transform,new Vector3(-.56f,-.08f,1.5f),Quaternion.Euler(0,180,0),jetMesh,jetMaterial),Jet("Starboard retrojet",body.transform,new Vector3(.56f,-.08f,1.5f),Quaternion.Euler(0,180,0),jetMesh,jetMaterial)};
            jets.sideJets=new[]{Jet("Port attitude jet",body.transform,new Vector3(-1.03f,-.06f,.1f),Quaternion.Euler(0,90,0),jetMesh,jetMaterial),Jet("Starboard attitude jet",body.transform,new Vector3(1.03f,-.06f,.1f),Quaternion.Euler(0,-90,0),jetMesh,jetMaterial)};
            motion.Evaluate(0);
        }

        static void Place(RectTransform rect,float x,float y){rect.anchoredPosition=new Vector2(x,-y);}
        static Transform Jet(string name,Transform parent,Vector3 position,Quaternion rotation,Mesh mesh,Material material)
        {
            var jet=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));jet.transform.SetParent(parent,false);jet.transform.localPosition=position;jet.transform.localRotation=rotation;
            jet.GetComponent<MeshFilter>().sharedMesh=mesh;var r=jet.GetComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;return jet.transform;
        }
        static void Texture(string path,int size)
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(Root+path);importer.maxTextureSize=size;importer.sRGBTexture=true;
            importer.alphaIsTransparency=path.Contains("Logo/");importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Trilinear;
            importer.textureCompression=path.Contains("Logo/")?TextureImporterCompression.Uncompressed:TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();
        }
        static Material Save(Material material,string path)
        {
            var existing=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(existing!=null){EditorUtility.CopySerialized(material,existing);Object.DestroyImmediate(material);EditorUtility.SetDirty(existing);return existing;}
            AssetDatabase.CreateAsset(material,path);return material;
        }
    }
}
