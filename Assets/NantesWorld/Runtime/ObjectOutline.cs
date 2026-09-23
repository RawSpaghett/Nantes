using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace NantesGame.World
{
    [DisallowMultipleComponent]
    public sealed class ObjectOutline : MonoBehaviour
    {
        public Material material;
        [Range(0, 8)] public float width = 2.5f;
        public Color color = new Color(.85f, .065f, .035f);
        public bool highlighted;
        readonly List<Renderer> sources = new List<Renderer>();
        readonly List<Renderer> shells = new List<Renderer>();
        readonly List<Mesh> meshes = new List<Mesh>();
        readonly List<bool> expanded = new List<bool>();
        Material maskMaterial, outlineMaterial, clearMaterial;
        MaterialPropertyBlock properties;
        bool initialized;

        void Start() { Initialize(); Refresh(); }
        void OnEnable() { if (initialized) Refresh(); }
        void OnDisable() { foreach (var shell in shells) if (shell) shell.enabled = false; }

        public void SetHighlighted(bool value)
        {
            highlighted = value;
            if (!initialized) Initialize();
            Refresh();
        }

        void Initialize()
        {
            if (initialized || !material) return;
            initialized = true;
            properties = new MaterialPropertyBlock();
            maskMaterial = MakePass("Outline mask", 3099, CullMode.Off, 0, CompareFunction.Always, StencilOp.Replace);
            outlineMaterial = MakePass("Outline edge", 3100, CullMode.Off, 15, CompareFunction.NotEqual, StencilOp.Keep);
            clearMaterial = MakePass("Outline cleanup", 3101, CullMode.Off, 0, CompareFunction.Always, StencilOp.Zero);
            foreach (var source in GetComponentsInChildren<Renderer>(true))
            {
                Mesh mesh = null;
                if (source is SkinnedMeshRenderer skin) mesh = skin.sharedMesh;
                else if (source is MeshRenderer && source.TryGetComponent<MeshFilter>(out var filter)) mesh = filter.sharedMesh;
                if (!mesh) continue;
                var outlineMesh = SmoothCopy(mesh);
                AddShell(source, outlineMesh, maskMaterial, false);
                AddShell(source, outlineMesh, outlineMaterial, true);
                AddShell(source, outlineMesh, clearMaterial, false);
            }
        }

        Material MakePass(string name, int queue, CullMode cull, int colorMask, CompareFunction compare, StencilOp operation)
        {
            var pass = new Material(material) { name = name, renderQueue = queue };
            pass.SetInt("_Cull", (int)cull); pass.SetInt("_ColorMask", colorMask);
            pass.SetInt("_StencilComp", (int)compare); pass.SetInt("_StencilOp", (int)operation);
            return pass;
        }

        void AddShell(Renderer source, Mesh outlineMesh, Material pass, bool expand)
        {
            var child = new GameObject(pass.name) { layer = source.gameObject.layer };
            child.transform.SetParent(source.transform, false);
            Renderer shell;
            if (source is SkinnedMeshRenderer original)
            {
                var copy = child.AddComponent<SkinnedMeshRenderer>();
                copy.sharedMesh = outlineMesh; copy.bones = original.bones; copy.rootBone = original.rootBone;
                copy.localBounds = original.localBounds; copy.updateWhenOffscreen = original.updateWhenOffscreen;
                shell = copy;
            }
            else
            {
                child.AddComponent<MeshFilter>().sharedMesh = outlineMesh;
                shell = child.AddComponent<MeshRenderer>();
            }
            var materials = new Material[outlineMesh.subMeshCount];
            for (int i = 0; i < materials.Length; i++) materials[i] = pass;
            shell.sharedMaterials = materials;
            shell.shadowCastingMode = ShadowCastingMode.Off; shell.receiveShadows = false;
            shell.lightProbeUsage = LightProbeUsage.Off; shell.reflectionProbeUsage = ReflectionProbeUsage.Off;
            shell.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            shell.allowOcclusionWhenDynamic = false;
            sources.Add(source); shells.Add(shell); expanded.Add(expand);
        }

        Mesh SmoothCopy(Mesh source)
        {
            if (!source.isReadable) return source;
            var mesh = Instantiate(source); mesh.name = source.name + " outline"; meshes.Add(mesh);
            var vertices = mesh.vertices; var normals = mesh.normals;
            if (normals.Length != vertices.Length) { mesh.RecalculateNormals(); normals = mesh.normals; }
            var groups = new Dictionary<Vector3, Vector3>();
            for (int i = 0; i < vertices.Length; i++)
            {
                groups.TryGetValue(vertices[i], out var sum);
                groups[vertices[i]] = sum + normals[i];
            }
            for (int i = 0; i < vertices.Length; i++)
                if (groups[vertices[i]].sqrMagnitude > .00001f) normals[i] = groups[vertices[i]].normalized;
            mesh.normals = normals;
            return mesh;
        }

        void LateUpdate() { Refresh(); }
        void Refresh()
        {
            if (!initialized) return;
            properties.SetColor("_OutlineColor", color);
            for (int i = 0; i < shells.Count; i++)
            {
                if (!shells[i]) continue;
                shells[i].enabled = isActiveAndEnabled && highlighted && sources[i] && sources[i].enabled;
                properties.SetFloat("_Width", expanded[i] ? width : 0);
                shells[i].SetPropertyBlock(properties);
                if (!sources[i]) continue;
                if (sources[i] is SkinnedMeshRenderer source && shells[i] is SkinnedMeshRenderer shell)
                    for (int shape = 0; shape < source.sharedMesh.blendShapeCount; shape++)
                        shell.SetBlendShapeWeight(shape, source.GetBlendShapeWeight(shape));
            }
        }

        void OnDestroy()
        {
            foreach (var shell in shells) if (shell) Destroy(shell.gameObject);
            foreach (var mesh in meshes) if (mesh) Destroy(mesh);
            if (maskMaterial) Destroy(maskMaterial);
            if (outlineMaterial) Destroy(outlineMaterial);
            if (clearMaterial) Destroy(clearMaterial);
        }
    }
}
