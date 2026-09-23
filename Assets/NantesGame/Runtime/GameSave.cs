using System;
using System.IO;
using UnityEngine;

namespace NantesGame.Gameplay
{
    [Serializable]
    public sealed class GameSaveData
    {
        public int version = 1;
        public string scene = GameFlow.Level;
        public string savedAt;
        public Vector3 position, velocity;
        public Quaternion rotation;
        public float pitch, flashlightCharge, scannerRange;
        public bool flashlightOn;
        public int food;
        public SavedBody[] bodies;
    }

    [Serializable]
    public sealed class SavedBody
    {
        public string id;
        public Vector3 position, velocity, angularVelocity;
        public Quaternion rotation;
        public bool active, held, kinematic, collisions;
    }

    public static class GameSave
    {
        public static string FilePath
        {
            get
            {
                string folder = Application.persistentDataPath;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                var args = Environment.GetCommandLineArgs();
                int index = Array.IndexOf(args, "-saveDirectory");
                if (index >= 0 && index + 1 < args.Length) folder = args[index + 1];
#endif
                return Path.Combine(folder, "save-game.json");
            }
        }

        public static bool TryRead(out GameSaveData data, out string message)
        {
            data = Read(FilePath);
            if (data != null) { message = ""; return true; }
            data = Read(FilePath + ".bak");
            if (data != null) { message = "Using the previous save."; return true; }
            message = File.Exists(FilePath) || File.Exists(FilePath + ".bak")
                ? "Save could not be loaded." : "No saved game";
            return false;
        }

        static GameSaveData Read(string path)
        {
            try
            {
                if (!File.Exists(path) || new FileInfo(path).Length > 4 * 1024 * 1024) return null;
                var data = JsonUtility.FromJson<GameSaveData>(File.ReadAllText(path));
                return Valid(data) ? data : null;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException)
            { return null; }
        }

        public static bool Write(GameSaveData data, out string message)
        {
            if (!Valid(data)) { message = "Could not save. Please try again."; return false; }
            string path = FilePath, temporary = path + ".tmp";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                data.savedAt = DateTime.UtcNow.ToString("O");
                using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(data, true));
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }
                if (File.Exists(path))
                {
                    // Do not replace a good backup with an unreadable file.
                    if (Read(path) != null) File.Replace(temporary, path, path + ".bak");
                    else File.Replace(temporary, path, null);
                }
                else File.Move(temporary, path);
                message = "Game saved";
                return true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                Debug.LogWarning("Save failed: " + e.Message);
                message = "Could not save. Check available disk space and folder access.";
                return false;
            }
        }

        static bool Valid(GameSaveData data)
        {
            if (data == null || data.version != 1 || data.scene != GameFlow.Level || data.bodies == null || data.bodies.Length > 10000) return false;
            if (!Finite(data.position) || !Finite(data.velocity) || !Valid(data.rotation) || !Finite(data.pitch) || Mathf.Abs(data.pitch) > 90) return false;
            if (!Finite(data.flashlightCharge) || data.flashlightCharge < 0 || data.flashlightCharge > 3600 || data.food < 0 || data.food > 999) return false;
            if (!Finite(data.scannerRange) || data.scannerRange < 5 || data.scannerRange > 100) return false;
            var ids = new System.Collections.Generic.HashSet<string>();
            foreach (var body in data.bodies)
                if (body == null || string.IsNullOrEmpty(body.id) || !ids.Add(body.id) || !Finite(body.position) || !Finite(body.velocity) || !Finite(body.angularVelocity) || !Valid(body.rotation)) return false;
            return true;
        }
        static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        static bool Finite(Vector3 value) => Finite(value.x) && Finite(value.y) && Finite(value.z) && value.sqrMagnitude < 1e12f;
        static bool Valid(Quaternion q) => Finite(q.x) && Finite(q.y) && Finite(q.z) && Finite(q.w) && Mathf.Abs(Quaternion.Dot(q, q) - 1) < .01f;
    }
}
