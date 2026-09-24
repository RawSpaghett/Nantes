using System.IO;
using UnityEditor;
using UnityEngine;

public static class SupermarketPolishBuilder
{
    [MenuItem("Tools/Nantes/Prepare supermarket details")]
    public static void Prepare()
    {
        const string textures="Assets/NantesWorld/Textures/Surfaces/";
        const string materials="Assets/NantesWorld/Resources/WorldSurfaces/";
        Directory.CreateDirectory(materials);
        AssetDatabase.Refresh();
        foreach(string path in Directory.GetFiles(textures,"*.jpg"))
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.maxTextureSize=1024;importer.wrapMode=TextureWrapMode.Repeat;importer.anisoLevel=4;
            importer.textureType=path.Contains("NormalGL")?TextureImporterType.NormalMap:TextureImporterType.Default;
            importer.sRGBTexture=path.Contains("Color");
            importer.SaveAndReimport();
        }
        Surface("Outside","Asphalt012",3,new Color(.69f,.68f,.62f),0);
        Surface("Floor","Tiles074",3.6f,new Color(.88f,.88f,.85f),0);
        Surface("Wall","Concrete034",1.8f,new Color(.63f,.65f,.57f),1);
        Surface("Roof","Concrete034",3,new Color(.27f,.3f,.29f),0);
        Solid("Fixture",new Color(.055f,.065f,.06f),false);
        Solid("Tube",new Color(.57f,.69f,.60f),true);
        GameplayBuilder.PrepareControls();
        AssetDatabase.SaveAssets();
    }
    static void Surface(string name,string texture,float metres,Color tint,float wallBand)
    {
        var material=Material(name,"Nantes/World Surface");
        const string root="Assets/NantesWorld/Textures/Surfaces/";
        material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(root+texture+"_Color.jpg"));
        material.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(root+texture+"_NormalGL.jpg"));
        material.SetTexture("_Roughness",AssetDatabase.LoadAssetAtPath<Texture2D>(root+texture+"_Roughness.jpg"));
        material.SetColor("_BaseColor",tint);material.SetFloat("_Meters",metres);
        material.SetFloat("_WallBand",wallBand);material.SetFloat("_Smoothness",.35f);
        material.SetFloat("_Saturation",name=="Floor"?.12f:1);
        EditorUtility.SetDirty(material);
    }
    static void Solid(string name,Color color,bool emission)
    {
        var material=Material(name,"Universal Render Pipeline/Lit");material.color=color;
        material.SetFloat("_Smoothness",.22f);
        if(emission){material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",color*.65f);}
        EditorUtility.SetDirty(material);
    }
    static Material Material(string name,string shader)
    {
        string path="Assets/NantesWorld/Resources/WorldSurfaces/"+name+".mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!material){material=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(material,path);}
        return material;
    }
}
