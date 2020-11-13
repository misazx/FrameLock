using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityFrame;

public class ReAssetsBuilderTools
{

#if UNITY_IOS
	public static BuildTarget TargetPlat = BuildTarget.iOS;
#elif UNITY_ANDROID
	public static BuildTarget TargetPlat = BuildTarget.Android;
#elif UNITY_STANDALONE_OSX
	public static BuildTarget TargetPlat = BuildTarget.StandaloneOSXIntel;
#elif UNITY_STANDALONE
    public static BuildTarget TargetPlat = BuildTarget.StandaloneWindows;
#else
	public static BuildTarget TargetPlat = BuildTarget.StandaloneOSXIntel;
#endif


    static bool FindAndCopyAssetTo(string name, string serachPath, string toPath, ref string fullName)
    {
        DirectoryInfo di = new DirectoryInfo(serachPath);
        FileInfo[] arrayfiles = di.GetFiles("*.*", SearchOption.AllDirectories);
        if (arrayfiles == null)
            return false;

        foreach (FileInfo fi in arrayfiles)
        {
            if (Path.GetFileNameWithoutExtension(fi.Name) == name)
            {
                fullName = string.Format("{0}rew_{1}", toPath, fi.Name);
                fi.CopyTo(fullName, true);
                AssetDatabase.Refresh();
                return true;
            }
        }
        return false;
    }

    [MenuItem("GameTools/Rebundle/生成审核UI替换资源", false)]
    static void GenReviewUIReplacementAsset()
    {
        GameObject target = Selection.activeGameObject;
        if (target == null)
            return;
        GameObject instance = GameObject.Instantiate<GameObject>(target);

        instance.name = target.name;
        instance.transform.parent = target.transform.parent;
        RectTransform rectInstance = instance.GetComponent<RectTransform>();
        RectTransform rectTarget = target.GetComponent<RectTransform>();
        rectInstance.pivot = rectTarget.pivot;
        rectInstance.anchorMin = rectTarget.anchorMin;
        rectInstance.anchorMax = rectTarget.anchorMax;
        rectInstance.anchoredPosition = rectTarget.anchoredPosition;
        rectInstance.localScale = rectTarget.localScale;

        target.SetActive(false);

        string sourcePath = UnityFrame.GlobalPath.AssetBasesPath + "SourceAssets/UI/";
        string genSourcePath = UnityFrame.GlobalPath.AssetBasesPath + "ReviewAssets/Source/";
        string genSpritePath = UnityFrame.GlobalPath.AssetBasesPath + "ReviewAssets/Source/sprite/";
        string genTexturePath = UnityFrame.GlobalPath.AssetBasesPath + "ReviewAssets/Source/texture/";
        string prefabPath = UnityFrame.GlobalPath.AssetBasesPath + "ReviewAssets/Prefab/";

        if (!Directory.Exists(genSourcePath))
        {
            Directory.CreateDirectory(genSourcePath);
        }
        if (!Directory.Exists(prefabPath))
        {
            Directory.CreateDirectory(prefabPath);
        }
        if (!Directory.Exists(genSpritePath))
        {
            Directory.CreateDirectory(genSpritePath);
        }

        if (!Directory.Exists(genTexturePath))
        {
            Directory.CreateDirectory(genTexturePath);
        }

        ////抽离Sprite
        UISprite[] sprites = instance.GetComponentsInChildren<UISprite>(true);
        if (sprites != null)
        {
            foreach (UISprite item in sprites)
            {
                string fileFullName = "";
                if (item == null || item.sprite == null)
                    continue;
                if (FindAndCopyAssetTo(item.sprite.name, sourcePath, genSpritePath, ref fileFullName))
                {
                    string absPath = "Assets" + fileFullName.Replace(Application.dataPath, "");

                    TextureImporter ti = TextureImporter.GetAtPath(absPath) as TextureImporter;
                    if (ti != null)
                    {
                        ti.textureType = TextureImporterType.Sprite;
                        ti.spriteImportMode = SpriteImportMode.Single;
                        ti.alphaIsTransparency = true;
                        ti.SaveAndReimport();
                        item.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(absPath);
                    }
                }
            }
        }

        //		//替换文本
        UILabel[] labels = instance.GetComponentsInChildren<UILabel>(true);
        GameObject temp = new GameObject("Temp_UILabel");
        Font defaultFont = temp.AddComponent<Text>().font;
        if (labels != null)
        {
            foreach (UILabel label in labels)
            {
                label.font = defaultFont;
            }
        }
        Object.DestroyImmediate(temp);

        //保存
        string fileName = prefabPath + instance.name + ".prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(instance, fileName, InteractionMode.AutomatedAction);
    }

    [MenuItem("GameTools/Rebundle/保存审核预设", false)]
    static void SaveReviewUIReplacementAsset() {
        GameObject target = Selection.activeGameObject;
        if (target == null)
            return;
        string fileName = "Assets/ReviewAssets/Prefab/" + target.name + ".prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(target, fileName, InteractionMode.AutomatedAction);
    }

    [MenuItem("GameTools/Rebundle/自动移除引用图集", false)]
    static void RemoveUnRefAtlaInPrefabAssets()
    {
        GameObject target = Selection.activeGameObject;
        if (target == null)
            return;

        foreach (UISprite sp in target.GetComponentsInChildren<UISprite>(true)) {
            if (sp.sprite != null) {
                string path = AssetDatabase.GetAssetPath(sp.sprite);
                if (path.IndexOf("PrefabAssets") > -1) {
                    sp.sprite = null;
                    Debug.Log("Remove Prefab Atla Ref:" + path);
                }
            }
        }
    }

    [MenuItem("GameTools/Rebundle/自动移除未激活Sprite&Texture", false)]
    static void RemoveUnActiveSprite()
    {
        GameObject target = Selection.activeGameObject;
        if (target == null)
            return;

        foreach (UISprite sp in target.GetComponentsInChildren<UISprite>(true))
        {
            if (sp.sprite != null)
            {
                if (!sp.gameObject.active)
                    sp.sprite = null;
            }
        }

        foreach (UITexture uitex in target.GetComponentsInChildren<UITexture>(true))
        {
            if (!uitex.gameObject.active)
                uitex.texture = null;
        }
    }

    [MenuItem("GameTools/Rebundle/自动移除审核无引用Sprite")]
    static void RemoveUnRefAssetInReviewAssets()
    {
        string prefabPath = UnityFrame.GlobalPath.AssetBasesPath + "ReviewAssets/Prefab";
        string sourcePath = UnityFrame.GlobalPath.AssetBasesPath + "ReviewAssets/Source";
        string spritePath = UnityFrame.GlobalPath.AssetBasesPath + "ReviewAssets/Source/sprite";

        if (!Directory.Exists(prefabPath))
        {
            return;
        }
        DirectoryInfo di = new DirectoryInfo(sourcePath);

        Dictionary<string, int> mapAssetRefer = new Dictionary<string, int>();

        FileInfo[] arrayfiles = di.GetFiles("*.*", SearchOption.AllDirectories);
        if (arrayfiles == null || arrayfiles.Length == 0)
            return;

        foreach (FileInfo fi in arrayfiles)
        {
            if (Path.GetExtension(fi.Name) == ".png")
            {
                if (!mapAssetRefer.ContainsKey(Path.GetFileNameWithoutExtension(fi.Name)))
                {
                    mapAssetRefer.Add(Path.GetFileNameWithoutExtension(fi.Name), 0);
                }
            }
        }


        FileInfo[] arrayPrefabs = (new DirectoryInfo(prefabPath)).GetFiles("*.*", SearchOption.AllDirectories);
        if (arrayPrefabs == null || arrayPrefabs.Length == 0)
            return;

        foreach (FileInfo fi in arrayPrefabs)
        {
            if (Path.GetExtension(fi.Name) == ".meta") continue;

            string absPath = fi.FullName.Substring(fi.FullName.IndexOf("Assets"));

            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(absPath);

            if (go == null) continue;

            UISprite[] uisprites = go.GetComponentsInChildren<UISprite>(true);
            if (uisprites != null)
                foreach (UISprite uisprite in uisprites)
                {

                    if (uisprite.sprite != null && mapAssetRefer.ContainsKey(uisprite.sprite.name))
                    {
                        mapAssetRefer[uisprite.sprite.name]++;
                    }
                }
        }

        foreach (KeyValuePair<string, int> item in mapAssetRefer)
        {
            if (item.Value == 0)
            {
                string fullpath = string.Format("{0}/{1}.png", spritePath, item.Key);
                if (File.Exists(fullpath))
                {
                    Debug.Log("Remove Asset:" + fullpath);
                    File.Delete(fullpath);
                }
            }
        }

        Debug.Log("UnRef Sprite Remove Finish");

        AssetDatabase.Refresh();
    }

    [MenuItem("GameTools/Rebundle/构建全部审核AB资源")]
    public static void BuildReviewRebundleAsset()
    {
        string outDir = UnityFrame.GlobalPath.AssetBasesPath + "ReviewAssets/Bundle";
        AssetsBuilderTools.BuildReviewRebundleAssetTo(outDir, TargetPlat);
    }


}
