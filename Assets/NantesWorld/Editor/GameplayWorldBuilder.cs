using System.IO;
using System.Linq;
using NantesGame.Gameplay;
using NantesGame.World;
using UnityEditor;
using UnityEngine;

public static class GameplayWorldBuilder
{
    [MenuItem("Tools/Nantes/Update gameplay world look")]
    public static void Prepare()
    {
        const string root = "Assets/NantesWorld/";
        Directory.CreateDirectory(root + "Resources");
        AssetDatabase.Refresh();
        var host = new GameObject("Gameplay world look");
        var look = host.AddComponent<GameplayWorldLook>();
        look.atmospherePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(root + "Prefabs/World atmosphere.prefab");
        look.outlineMaterial = AssetDatabase.LoadAssetAtPath<Material>(root + "Materials/Red outline.mat");
        PrefabUtility.SaveAsPrefabAsset(host, root + "Resources/GameplayWorldLook.prefab");
        Object.DestroyImmediate(host);
        var scenes = new[] { GameFlow.Menu, GameFlow.Level }
            .Select(path => new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.Concat(EditorBuildSettings.scenes.Where(scene =>
            scene.path != GameFlow.Menu && scene.path != GameFlow.Level &&
            scene.path != "Assets/Scenes/CreatureDebug/CreatureDebug.unity")).ToArray();
        AssetDatabase.SaveAssets();
    }
}
