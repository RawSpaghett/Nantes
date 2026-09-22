using UnityEditor;
using UnityEditor.SceneManagement;

namespace NantesGame.UI.Editor
{
    public static class NantesMenuBuilder
    {
        const string Root = "Assets/NantesUI/";
        public const string ScenePath = Root + "Scenes/NantesMenuPreview.unity";
        public const string PrefabPath = Root + "Prefabs/NantesMenu.prefab";

        [MenuItem("Tools/Nantes UI/Open menu preview")]
        public static void OpenPreview()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(ScenePath);
        }
    }
}
