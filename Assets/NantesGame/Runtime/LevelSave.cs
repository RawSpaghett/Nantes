using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace NantesGame.Gameplay
{
    public sealed class LevelSave : MonoBehaviour
    {
        public GamePause pause;
        readonly Dictionary<string, Rigidbody> bodies = new Dictionary<string, Rigidbody>();
        bool ready;
        static readonly FieldInfo Pitch = Field(typeof(PlayerMovement), "lookRotation");
        static readonly FieldInfo View = Field(typeof(PlayerMovement), "camHolder");
        static readonly FieldInfo Charge = Field(typeof(FlashlightScript), "timer");
        static readonly FieldInfo Light = Field(typeof(FlashlightScript), "flashlight");
        static readonly FieldInfo Hold = Field(typeof(ThrowableObjects), "playerHoldPosition");
        FlashlightScript flashlight;

        static FieldInfo Field(Type type, string name) => type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);

        void Awake()
        {
            // Capture scene paths before picking up an object changes its parent.
            foreach (var body in FindObjectsByType<Rigidbody>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (body.GetComponent<ThrowableObjects>() || body.GetComponent<Enemy>()) bodies.Add(Path(body.transform), body);
        }

        void Start()
        {
            flashlight = pause.movement.GetComponentInChildren<FlashlightScript>();
            ready = Pitch != null && View != null && Charge != null && Light != null && Hold != null;
            if (!ready) Debug.LogError("Save bindings need updating for the player scripts.");
        }

        static string Path(Transform item)
        {
            string path = item.name + "[" + item.GetSiblingIndex() + "]";
            while (item.parent) { item = item.parent; path = item.name + "[" + item.GetSiblingIndex() + "]/" + path; }
            return path;
        }

        public bool Save(out string message)
        {
            if (!ready) { message = "The level is not ready to save."; return false; }
            var player = pause.movement;
            var light = flashlight ? (GameObject)Light.GetValue(flashlight) : null;
            var data = new GameSaveData
            {
                scene = player.gameObject.scene.path,
                position = player.transform.position, rotation = player.transform.rotation,
                velocity = player.GetComponent<Rigidbody>().linearVelocity,
                pitch = (float)Pitch.GetValue(player),
                flashlightCharge = flashlight ? (float)Charge.GetValue(flashlight) : 0,
                flashlightOn = light && light.activeSelf,
                food = pause.chest.tablet.FoodCount, scannerRange = pause.chest.tablet.Range,
                bodies = bodies.Select(pair => Capture(pair.Key, pair.Value)).ToArray()
            };
            return GameSave.Write(data, out message);
        }

        static SavedBody Capture(string id, Rigidbody body)
        {
            if (!body) return new SavedBody { id = id, rotation = Quaternion.identity, active = false };
            var item = body.GetComponent<ThrowableObjects>();
            return new SavedBody
            {
                id = id, position = body.position, rotation = body.rotation, velocity = body.linearVelocity,
                angularVelocity = body.angularVelocity, active = body.gameObject.activeSelf,
                held = item && item.isHeld, kinematic = body.isKinematic, collisions = body.detectCollisions
            };
        }

        public void Restore(GameSaveData data)
        {
            if (!ready) throw new InvalidOperationException("The level is not ready to restore.");
            var player = pause.movement;
            var rb = player.GetComponent<Rigidbody>();
            player.transform.SetPositionAndRotation(data.position, data.rotation);
            rb.position = data.position; rb.rotation = data.rotation; rb.linearVelocity = data.velocity; rb.angularVelocity = Vector3.zero;
            Pitch.SetValue(player, data.pitch);
            ((GameObject)View.GetValue(player)).transform.rotation = Quaternion.Euler(data.pitch, data.rotation.eulerAngles.y, 0);
            if (flashlight)
            {
                Charge.SetValue(flashlight, data.flashlightCharge);
                var light = (GameObject)Light.GetValue(flashlight);
                if (light) light.SetActive(data.flashlightOn);
            }
            pause.chest.tablet.SetFoodCount(data.food); pause.chest.tablet.SetRange(data.scannerRange);
            foreach (var saved in data.bodies)
            {
                if (!bodies.TryGetValue(saved.id, out var body) || !body) continue;
                var item = body.GetComponent<ThrowableObjects>();
                if (item)
                {
                    item.isHeld = saved.held;
                    if (saved.held) body.transform.SetParent((Transform)Hold.GetValue(item), true);
                }
                body.gameObject.SetActive(saved.active);
                body.transform.SetPositionAndRotation(saved.position, saved.rotation);
                body.position = saved.position; body.rotation = saved.rotation;
                body.isKinematic = saved.kinematic; body.detectCollisions = saved.collisions;
                if (!body.isKinematic) { body.linearVelocity = saved.velocity; body.angularVelocity = saved.angularVelocity; }
            }
            Physics.SyncTransforms();
        }

        void OnApplicationQuit()
        {
            if (ready && !ScreenTransition.Busy) Save(out _);
        }
    }
}
