using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityFrame;
using UnityFrame.Assets;


[CustomEditor(typeof(AssetUIAtlas), true)]
public class AssetUIAtlasEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AssetUIAtlas uiAtla = target as AssetUIAtlas;


        GUILayout.Space(6);
        EditorGUIUtility.labelWidth = 110;


        GUILayout.Label(string.Format("图集: "));

        Texture2D atla_texture = EditorGUILayout.ObjectField(uiAtla.texture, typeof(Texture2D), true) as Texture2D;

        string path_texture = AssetDatabase.GetAssetPath(atla_texture);

        if (atla_texture != null)
        {
            GUILayout.Label(string.Format("{0} x {1}\n{2}\n{3}",
                atla_texture.width,
                atla_texture.height,
                atla_texture.format,
                atla_texture.filterMode));
        }

        if (EditorTools.DrawHeader("显示图集", true, false))
        {
            Rect rect = GUILayoutUtility.GetRect(320, 320, GUI.skin.box);

            GUI.Box(rect, uiAtla.texture);
        }

        if (uiAtla.sprites != null)
        {

            GUILayout.Label(string.Format("元件数[{0}]", uiAtla.sprites.Count));
        }

        DrawTargetArray(uiAtla.sprites, true);

        if (GUILayout.Button("刷新元件"))
        {
            FillSpriteArray(uiAtla.sprites, path_texture);
        }

        if (uiAtla.texture != atla_texture)
        {

            if (uiAtla.texture != null)
            {
                AssetImporter ai = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(uiAtla.texture));
                ai.assetBundleName = string.Empty;
            }

            if (atla_texture != null)
            {
                string assetbundleName = AssetDatabase.GetAssetPath(target)
                    .Replace(".prefab", "")
                    .Replace(".asset", "")
                    .Replace("Assets/AssetBases/PrefabAssets/", "");
                AssetImporter ai = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(atla_texture));
                ai.assetBundleName = assetbundleName;
            }

            uiAtla.texture = atla_texture;

            FillSpriteArray(uiAtla.sprites, path_texture);



            EditorTools.SetDirty(uiAtla);
        }
    }
    /// <summary>
    /// 刷新 图集信息
    /// </summary>
    /// <param name="uiAtla"></param>
    /// <param name="path_texture"></param>
    public static void RefreshAtlaSprites(AssetUIAtlas uiAtla, string path_texture)
    {
        if (uiAtla == null || string.IsNullOrEmpty(path_texture))
            return;

        FillSpriteArray(uiAtla.sprites, path_texture);
    }
    public static void FillSpriteArray(List<Sprite> array, string path)
    {
        array.Clear();
        Object[] sprite_objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
        for (int k = 0; k < sprite_objs.Length; k++)
        {
            array.Add(sprite_objs[k] as Sprite);
        }
    }

    private void DrawTargetArray(List<Sprite> array, bool showtype = false)
    {
        if (array == null)
            return;

        if (EditorTools.DrawHeader("显示元件", false, false))
        {
            for (int k = 0; k < array.Count; k++)
            {
                if (array[k].Equals(default(Sprite)))
                {
                    continue;
                }
                UISpriteEditor.DrawSpriteElement(array[k]);
                GUILayout.Space(5);
            }
        }
    }

}





//[CustomEditor(typeof(UIAtlas), true)]
//public class UIAtlaEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        UIAtlas uiAtla = target as UIAtlas;


//        GUILayout.Space(6);
//        EditorGUIUtility.labelWidth = 110;


//        GUILayout.Label(string.Format("图集: "));

//        Texture2D atla_texture = EditorGUILayout.ObjectField(uiAtla.texture, typeof(Texture2D), true) as Texture2D;

//        string path_texture = AssetDatabase.GetAssetPath(atla_texture);

//        if (atla_texture != null)
//        {
//            GUILayout.Label(string.Format("{0} x {1}\n{2}\n{3}",
//                atla_texture.width,
//                atla_texture.height,
//                atla_texture.format,
//                atla_texture.filterMode));
//        }

//        if (EditorTools.DrawHeader("显示图集", true, false))
//        {
//            Rect rect = GUILayoutUtility.GetRect(320, 320, GUI.skin.box);

//            GUI.Box(rect, uiAtla.texture);
//        }

//        if (uiAtla.sprites != null)
//        {

//            GUILayout.Label(string.Format("元件数[{0}]", uiAtla.sprites.Count));
//        }

//        DrawTargetArray(uiAtla.sprites, true);

//        if (GUILayout.Button("刷新元件"))
//        {
//            FillSpriteArray(uiAtla.sprites, path_texture);
//        }

//        if (uiAtla.texture != atla_texture)
//        {

//            if (uiAtla.texture != null)
//            {
//                AssetImporter ai = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(uiAtla.texture));
//                ai.assetBundleName = string.Empty;
//            }

//            if (atla_texture != null)
//            {
//                string assetbundleName = AssetDatabase.GetAssetPath(target).Replace(".prefab", "").Replace("Assets/AssetBases/PrefabAssets/", "");
//                AssetImporter ai = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(atla_texture));
//                ai.assetBundleName = assetbundleName;
//            }

//            uiAtla.texture = atla_texture;

//            FillSpriteArray(uiAtla.sprites, path_texture);



//            EditorTools.SetDirty(uiAtla);
//        }
//    }
//    /// <summary>
//    /// 刷新 图集信息
//    /// </summary>
//    /// <param name="uiAtla"></param>
//    /// <param name="path_texture"></param>
//    public static void RefreshAtlaSprites(UIAtlas uiAtla, string path_texture)
//    {
//        if (uiAtla == null || string.IsNullOrEmpty(path_texture))
//            return;

//        FillSpriteArray(uiAtla.sprites, path_texture);
//    }
//    public static void FillSpriteArray(List<Sprite> array, string path)
//    {
//        array.Clear();
//        Object[] sprite_objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
//        for (int k = 0; k < sprite_objs.Length; k++)
//        {
//            array.Add(sprite_objs[k] as Sprite);
//        }
//    }

//    private void DrawTargetArray(List<Sprite> array, bool showtype = false)
//    {
//        if (array == null)
//            return;

//        if (EditorTools.DrawHeader("显示元件", false, false))
//        {
//            for (int k = 0; k < array.Count; k++)
//            {
//                if (array[k].Equals(default(Sprite)))
//                {
//                    continue;
//                }
//                UISpriteEditor.DrawSpriteElement(array[k]);
//                GUILayout.Space(5);
//            }
//        }
//    }

//}


