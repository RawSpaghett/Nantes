using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace NantesGame.Tablet.Editor
{
    public static class TabletFont
    {
        public static TMP_FontAsset Prepare()
        {
            const string path="Assets/NantesTablet/Resources/Stray SDF.asset";
            var asset=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if(asset)return asset;
            Directory.CreateDirectory("Assets/NantesTablet/Resources");AssetDatabase.Refresh();
            var source=AssetDatabase.LoadAssetAtPath<Font>("Assets/NantesTablet/Fonts/Stray.ttf");
            if(!source)throw new System.Exception("Stray.ttf is missing.");
            asset=TMP_FontAsset.CreateFontAsset(source,96,9,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,false);
            asset.name="Stray SDF";
            if(!asset.TryAddCharacters(" ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz",out string missing))
                throw new System.Exception("Stray is missing letters: "+missing);
            asset.atlasPopulationMode=AtlasPopulationMode.Static;
            AssetDatabase.CreateAsset(asset,path);
            foreach(var texture in asset.atlasTextures){texture.name="Stray atlas";AssetDatabase.AddObjectToAsset(texture,asset);}
            asset.material.name="Stray material";AssetDatabase.AddObjectToAsset(asset.material,asset);
            EditorUtility.SetDirty(asset);AssetDatabase.SaveAssets();return asset;
        }
    }
}
