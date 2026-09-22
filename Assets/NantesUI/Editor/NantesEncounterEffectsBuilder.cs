using UnityEditor;
using UnityEngine;

namespace NantesGame.UI.Editor
{
    public static class NantesEncounterEffectsBuilder
    {
        public static void Create(NantesSpaceMotion motion,NantesEncounterDirector encounter)
        {
            var audio=motion.GetComponent<NantesEncounterAudio>();
            if(audio==null)audio=motion.gameObject.AddComponent<NantesEncounterAudio>();
            audio.motion=motion;audio.encounter=encounter;
            audio.earthFlick=Clip("Earth-Flick");audio.bind=Clip("Ship-Bind");
            audio.strain=Clip("Ship-Strain-Loop");audio.breakaway=Clip("Ship-Breakaway");
            var shutter=motion.GetComponent<NantesPlanetShutter>();
            if(shutter==null)shutter=motion.gameObject.AddComponent<NantesPlanetShutter>();
            shutter.motion=motion;shutter.encounter=encounter;
        }
        static AudioClip Clip(string name)
        {
            string path="Assets/NantesUI/Audio/"+name+".wav";
            var importer=(AudioImporter)AssetImporter.GetAtPath(path);
            var settings=importer.defaultSampleSettings;
            settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;
            settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;
            importer.defaultSampleSettings=settings;importer.loadInBackground=false;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
    }
}
