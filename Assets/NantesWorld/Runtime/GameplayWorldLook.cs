using System.Collections.Generic;
using NantesGame.Gameplay;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace NantesGame.World
{
    public sealed class GameplayWorldLook : MonoBehaviour
    {
        public GameObject atmospherePrefab;
        public Material outlineMaterial;
        public string[] foodPropNames = { "CocoCereals", "ChocolateBar", "Tomato" };
        readonly List<Light> daylight = new List<Light>();
        readonly List<UniversalAdditionalCameraData> cameras = new List<UniversalAdditionalCameraData>();
        GameObject atmosphere;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register() { SceneManager.sceneLoaded -= Loaded; SceneManager.sceneLoaded += Loaded; }

        static void Loaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.path != GameFlow.Level) return;
            var prefab = Resources.Load<GameObject>("GameplayWorldLook");
            if (prefab) SceneManager.MoveGameObjectToScene(Instantiate(prefab), scene);
        }

        void Start()
        {
            foreach (var light in FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (light.gameObject.scene == gameObject.scene && light.type == LightType.Directional && light.enabled)
                { daylight.Add(light); light.enabled = false; }
            foreach (var camera in FindObjectsByType<Camera>(FindObjectsSortMode.None))
                if (camera.gameObject.scene == gameObject.scene && camera.CompareTag("MainCamera"))
                {
                    var data = camera.GetUniversalAdditionalCameraData();
                    if (!data.renderPostProcessing) { cameras.Add(data); data.renderPostProcessing = true; }
                }
            atmosphere = Instantiate(atmospherePrefab, transform);
            atmosphere.SetActive(true);
            gameObject.AddComponent<SupermarketDetails>();
            // Register the food props without changing their prefabs.
            foreach (var item in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                foreach (string name in foodPropNames)
                    if (item.name == name || item.name.StartsWith(name + " (") || item.name == name + "(Clone)") { RegisterFood(item.gameObject); break; }
        }

        void RegisterFood(GameObject target)
        {
            if (target.scene != gameObject.scene || target.GetComponentInParent<FoodScanTarget>()) return;
            var outline = target.GetComponent<ObjectOutline>();
            if (!outline) outline = target.AddComponent<ObjectOutline>();
            outline.material = outlineMaterial;
            target.AddComponent<FoodScanTarget>();
            if (!target.GetComponent<FoodPickup>()) target.AddComponent<FoodPickup>();
        }

        void OnDestroy()
        {
            foreach (var light in daylight) if (light) light.enabled = true;
            foreach (var camera in cameras) if (camera) camera.renderPostProcessing = false;
        }
    }
}
