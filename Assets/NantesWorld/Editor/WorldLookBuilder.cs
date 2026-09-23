using System;
using System.IO;
using System.Linq;
using NantesGame.World;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

public static class WorldLookBuilder
{
    const string Root = "Assets/NantesWorld";
    const string ScenePath = Root + "/Preview/WorldLook.unity";
    const string Source = "Assets/Boilerplate/Scenes/Prototype_Main.unity";

    [MenuItem("Tools/Nantes/Build world look preview")]
    public static void Build()
    {
        Prepare();
        var args = Environment.GetCommandLineArgs();
        int index = Array.IndexOf(args, "-worldOutput");
        string output = index >= 0 && index + 1 < args.Length ? args[index + 1] : Path.GetFullPath("Build/WorldLook/Nantes World Look.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        var result = BuildPipeline.BuildPlayer(new[] { ScenePath }, output, BuildTarget.StandaloneWindows64, BuildOptions.None);
        if (result.summary.result != BuildResult.Succeeded) throw new Exception("World preview build failed.");
        foreach (string path in new[] { Root + "/Shaders/ObjectOutline.shader", Root + "/Shaders/OvercastSky.shader" })
            if (ShaderUtil.ShaderHasError(AssetDatabase.LoadAssetAtPath<Shader>(path))) throw new Exception("Shader failed: " + path);
        AssetDatabase.ExportPackage(new[] { Root + "/Shaders/ObjectOutline.shader", Root + "/Runtime/ObjectOutline.cs", Root + "/Materials/Red outline.mat", "Assets/NantesUI/Documentation/Object-outline.md" },
            Path.Combine(Path.GetDirectoryName(output), "Nantes-Outline.unitypackage"), ExportPackageOptions.Default);
        Debug.Log("WORLD_BUILD_PASS " + output);
        EditorApplication.Exit(0);
    }

    [MenuItem("Tools/Nantes/Prepare world look preview")]
    public static void Prepare()
    {
        foreach (var folder in new[] { "Materials", "Prefabs", "Preview" }) Directory.CreateDirectory(Root + "/" + folder);
        AssetDatabase.Refresh();
        var outline = MakeMaterial("Red outline", "Nantes/World/Object Outline");
        outline.SetColor("_OutlineColor", new Color(.85f, .065f, .035f));
        outline.SetFloat("_Width", 2.5f);

        string skyPath = Root + "/Textures/kloppenheim_07_puresky_2k.hdr";
        var importer = (TextureImporter)AssetImporter.GetAtPath(skyPath);
        importer.textureShape = TextureImporterShape.Texture2D;
        importer.sRGBTexture = false;
        importer.wrapModeU = TextureWrapMode.Repeat;
        importer.wrapModeV = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Trilinear;
        importer.maxTextureSize = 2048;
        importer.isReadable = false;
        importer.SaveAndReimport();
        var panorama = AssetDatabase.LoadAssetAtPath<Texture2D>(skyPath);
        if (!panorama) throw new Exception("Cloud panorama did not import.");
        var sky = MakeMaterial("Overcast sky", "Nantes/World/Overcast Sky");
        sky.SetTexture("_Panorama", panorama);
        if (!sky.GetTexture("_Panorama")) throw new Exception("Cloud panorama is not assigned.");
        sky.SetFloat("_Exposure", .7f);
        sky.SetFloat("_Rotation", 85);
        sky.SetFloat("_CloudCover", .18f);
        sky.SetFloat("_Wind", .003f);
        sky.SetColor("_Tint", new Color(.72f, .78f, .76f));
        sky.SetColor("_Horizon", new Color(.14f, .135f, .105f));

        var scene = EditorSceneManager.OpenScene(Source);
        EditorSceneManager.SaveScene(scene, ScenePath);
        var originalLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        var originalVolumes = Object.FindObjectsByType<Volume>(FindObjectsSortMode.None);
        var sourceRenderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int rendererCount = sourceRenderers.Length;
        foreach (var behaviour in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (behaviour && !(behaviour is Volume) && !(behaviour is UniversalAdditionalCameraData) && !(behaviour is UniversalAdditionalLightData)) behaviour.enabled = false;
        foreach (var rb in Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None)) { rb.isKinematic = true; rb.useGravity = false; }
        foreach (var agent in Object.FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None)) agent.enabled = false;
        foreach (var camera in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)) camera.enabled = false;
        foreach (var listener in Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None)) listener.enabled = false;
        foreach (var audio in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None)) audio.enabled = false;

        var atmosphere = new GameObject("World atmosphere");
        atmosphere.SetActive(false);
        atmosphere.AddComponent<WorldAtmosphere>().sky = sky;
        var daylight = AddLight("Overcast light", atmosphere.transform, LightType.Directional, new Color(.76f, .79f, .74f), .65f, 0);
        daylight.transform.rotation = Quaternion.Euler(38, -28, 0);
        daylight.shadows = LightShadows.Soft;
        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(Root + "/Materials/World look.asset");
        if (!profile) { profile = ScriptableObject.CreateInstance<VolumeProfile>(); AssetDatabase.CreateAsset(profile, Root + "/Materials/World look.asset"); }
        foreach (var item in profile.components.ToArray()) { profile.components.Remove(item); Object.DestroyImmediate(item, true); }
        var grade = profile.Add<ColorAdjustments>(true);
        grade.postExposure.value = 0;
        grade.contrast.value = 14;
        grade.saturation.value = -22;
        var tone = profile.Add<Tonemapping>(true); tone.mode.value = TonemappingMode.ACES;
        var vignette = profile.Add<Vignette>(true); vignette.intensity.value = .18f; vignette.smoothness.value = .65f;
        var grain = profile.Add<FilmGrain>(true); grain.type.value = FilmGrainLookup.Thin1; grain.intensity.value = .10f; grain.response.value = .8f;
        foreach (var item in profile.components) AssetDatabase.AddObjectToAsset(item, profile);
        var volume = atmosphere.AddComponent<Volume>(); volume.isGlobal = true; volume.priority = 10; volume.sharedProfile = profile;
        PrefabUtility.SaveAsPrefabAsset(atmosphere, Root + "/Prefabs/World atmosphere.prefab");

        var food = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).FirstOrDefault(t => t.name == "CocoCereals");
        if (!food) throw new Exception("Could not find the existing CocoCereals object for the outline preview.");
        var foodRenderers = food.GetComponentsInChildren<Renderer>();
        if (foodRenderers.Length == 0) throw new Exception("Food preview object has no renderers.");
        var bounds = foodRenderers[0].bounds;
        foreach (var renderer in foodRenderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
        var effect = food.gameObject.AddComponent<ObjectOutline>();
        effect.material = outline;
        Debug.Log("OUTLINE_TARGET " + food.name + " bounds=" + bounds + " forward=" + food.forward);
        var offset = new Vector3(32.44469f, 0, -11.83207f);

        var cameraRoot = new GameObject("Preview camera");
        var cam = cameraRoot.AddComponent<Camera>();
        cam.fieldOfView = 58; cam.nearClipPlane = .03f; cam.farClipPlane = 350; cam.allowHDR = true; cam.tag = "MainCamera";
        var cameraData = cam.GetUniversalAdditionalCameraData();
        cameraData.renderPostProcessing = true;
        cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        cameraData.antialiasingQuality = AntialiasingQuality.High;
        cameraRoot.AddComponent<AudioListener>();
        var preview = cameraRoot.AddComponent<WorldLookPreview>();
        preview.atmosphere = atmosphere; preview.originalLights = originalLights; preview.originalVolumes = originalVolumes;
        preview.view = cam; preview.examples = new[] { effect };
        float distance = Mathf.Max(.8f, bounds.size.magnitude * 1.5f);
        preview.views = new[]
        {
            View("Exterior", new Vector3(-47, 2.7f, -39) + offset, new Vector3(-30, 4f, -7) + offset),
            View("Maze", new Vector3(-31, 1.7f, -6) + offset, new Vector3(-27, 1.6f, 16) + offset),
            View("Outline", bounds.center + new Vector3(.35f, .35f, 1).normalized * distance, bounds.center),
            View("Through wall", OccludedPosition(food, bounds), bounds.center)
        };
        preview.flashlight = AddLight("Preview flashlight", cameraRoot.transform, LightType.Spot, new Color(.90f, .93f, .87f), 12, 25);
        preview.flashlight.transform.localPosition = new Vector3(.16f, -.12f, .1f);
        preview.flashlight.spotAngle = 52; preview.flashlight.innerSpotAngle = 31; preview.flashlight.shadows = LightShadows.Soft; preview.flashlight.enabled = false;
        cam.transform.SetPositionAndRotation(preview.views[0].position, preview.views[0].rotation);
        if (Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length != rendererCount)
            throw new Exception("Preview unexpectedly changed scene geometry.");
        Debug.Log("SCENE_GEOMETRY_PASS " + rendererCount + " original renderers; no geometry added.");
        RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared;
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(scene);
        Debug.Log("WORLD_PREPARE_PASS source=" + Source);
    }

    static Transform View(string name, Vector3 position, Vector3 target)
    {
        var go = new GameObject(name + " view");
        go.transform.position = position; go.transform.LookAt(target);
        return go.transform;
    }

    static Vector3 OccludedPosition(Transform target, Bounds bounds)
    {
        Physics.SyncTransforms();
        foreach (float radius in new[] { 2f, 4f, 8f, 12f })
        for (int step = 0; step < 16; step++)
        {
            float angle = step * Mathf.PI / 8;
            var position = bounds.center + new Vector3(Mathf.Cos(angle) * radius, 1.35f, Mathf.Sin(angle) * radius);
            if (Physics.CheckSphere(position, .1f, ~0, QueryTriggerInteraction.Ignore)) continue;
            bool blocked = true;
            for (int corner = 0; corner < 9 && blocked; corner++)
            {
                var point = corner == 8 ? bounds.center : bounds.center + Vector3.Scale(bounds.extents * .8f,
                    new Vector3((corner & 1) == 0 ? -1 : 1, (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1));
                if (!Physics.Linecast(position, point, out var hit, ~0, QueryTriggerInteraction.Ignore)
                    || hit.transform.IsChildOf(target) || hit.distance > Vector3.Distance(position, point) - .15f) blocked = false;
            }
            if (blocked) { Debug.Log("OUTLINE_OCCLUSION_PASS all target corners blocked from " + position); return position; }
        }
        throw new Exception("Could not find a fully blocked outline test view in the existing scene.");
    }

    static Material MakeMaterial(string name, string shader)
    {
        string path = Root + "/Materials/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!material)
        {
            var found = Shader.Find(shader);
            if (!found) throw new Exception("Missing shader: " + shader);
            material = new Material(found); AssetDatabase.CreateAsset(material, path);
        }
        return material;
    }

    static Light AddLight(string name, Transform parent, LightType type, Color color, float intensity, float range)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var light = go.AddComponent<Light>(); light.type = type; light.color = color; light.intensity = intensity; light.range = range;
        return light;
    }
}
