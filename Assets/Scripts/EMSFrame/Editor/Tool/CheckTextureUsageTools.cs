using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityFrame;
using UnityEditor;
using UnityEngine;

public class CheckTextureUsageTools : Editor
{

    private static List<string> findPath = new List<string>();
    [MenuItem("Assets/GameTools/UI/查找Texture被引用的UI预设集合")]
    public static void CheckTextureUsage()
    {
        if (Selection.objects == null || Selection.objects.Length < 1)
        {
            Debug.LogError(" You didn't select any objects");
            return;
        }
        List<Texture> texs = new List<Texture>();
        for (int i = 0; i < Selection.objects.Length; i++)
        {
            Object select = Selection.objects[i];
            if (select is Texture)
            {
                texs.Add(select as Texture);
            }
        }
        if (texs.Count < 1)
        {
            Debug.LogError(" You didn't select the texture.");
            return;
        }
        findPath.Clear();
        findPath.Add(Application.dataPath + "/AssetBases/PrefabAssets/");
        CheckUsage(findPath, texs);

    }
    //[MenuItem("Assets/GameTools/UI/查找Texture被引用的特效和AVA集合")]
    //public static void CheckEfxTextureUsage()
    //{
    //    if (Selection.objects == null || Selection.objects.Length < 1)
    //    {
    //        Debug.LogError(" You didn't select any objects");
    //        return;
    //    }
    //    Object select = Selection.objects[0];
    //    if (!(select is Texture))
    //    {
    //        Debug.LogError(" You didn't select the texture.");
    //        return;
    //    }
    //    Texture tex = select as Texture;
    //    if (!tex) return;
    //    Debug.Log("start check :" + tex.name);
    //    findPath.Clear();
    //    findPath.Add(Application.dataPath + "/AssetBases/PrefabAssets/base/efx/");
    //    //findPath.Add(Application.dataPath + "/AssetBases/PrefabAssets/base/ava/");
    //    CheckUsage_old(findPath, tex);

    //}

    
    private static void CheckUsage(List<string> path, List<Texture> texs)
    {
        Dictionary<string, List<string>> preTexDic = GetPresUsageTexDic(path);

        Dictionary<string, List<string>> usageDic = new Dictionary<string, List<string>>();

        for (int i = 0; i < texs.Count; i++)
        {
            string p = AssetDatabase.GetAssetPath(texs[i]);
            p = p.Replace("Assets/A", "A");
            p = Application.dataPath + @"/" + p;
            p = p + ".meta";
            p = p.Replace(@"\", @"/");
            string text = System.IO.File.ReadAllText(p);
            Regex reg = new Regex(@"guid:\s(.*)\n");
            Match match = reg.Match(text);
            string value = match.Groups[1].Value;
            if (!string.IsNullOrEmpty(value))
            {
                
                usageDic.Add(texs[i].name,new List<string>());

                foreach (KeyValuePair<string, List<string>> var in preTexDic)
                {
                    if (var.Value.Contains(value))
                    {
                        usageDic[texs[i].name].Add(var.Key);
                    }
                }
            }
        }
        foreach (KeyValuePair<string, List<string>> var in usageDic)
        {
            if (var.Value.Count == 0)
            {
                Debug.LogError(var.Key + " not been found any usage");
            }
            else
            {
                string temp =  var.Key + " : {\n";
                for (int i = 0; i < var.Value.Count; i++)
                {
                    temp += var.Value[i] + "\n";
                }
                temp += "}";
                Debug.Log(temp);
            }
        }

        


        
    }

    //private static void CheckUsage_old(List<string> path, Texture tex)
    //{
    //    bool isUsage = false;
    //    for (int i = 0; i < path.Count; i++)
    //    {
    //        if (!Directory.Exists(path[i]))
    //        {
    //            Debug.LogError( path[i] + " is not exit");
    //            continue;
    //        }
    //        DirectoryInfo directoryInfo = new DirectoryInfo(path[i]);
    //        List<FileInfo> fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories).ToList();
    //        if (fileInfos.Count < 1) continue;
    //        fileInfos.RemoveAll(s => s.Name.EndsWith(".meta"));
    //        for (int j = 0; j < fileInfos.Count; j++)
    //        {
    //            string p = fileInfos[j].DirectoryName.Replace(@"\", @"/");
    //            p = p.Replace(Application.dataPath, "Assets");
    //            p = p + @"/" + fileInfos[j].Name;
    //            GameObject o = AssetDatabase.LoadAssetAtPath<GameObject>(p);
    //            if (!o) continue;
    //            List<Renderer> renderers = o.transform.GetComponentsInChildren<Renderer>(true).ToList();
    //            for (int k = 0; k < renderers.Count; k++)
    //            {
    //                if (renderers[k].sharedMaterial && renderers[k].sharedMaterial.mainTexture == tex)
    //                {
    //                    isUsage = true;
    //                    Debug.Log(p);
    //                    break;
    //                }
    //            }
    //        }

    //    }
    //    if (!isUsage) Debug.Log( tex.name + " not been found any usage");
    //}


    /// <summary>
    /// 获取路径下所有预设引用Texture集合的集合
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static Dictionary<string, List<string>> GetPresUsageTexDic(List<string> path)
    {
        Dictionary<string, List<string>> preTexDic = new Dictionary<string, List<string>>();
        for (int i = 0; i < path.Count; i++)
        {
            if (!Directory.Exists(path[i]))
            {
                Debug.LogError(path[i] + " is not exit");
                continue;
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(path[i]);
            List<FileInfo> fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories).ToList();
            if (fileInfos.Count < 1) continue;
            fileInfos.RemoveAll(s => s.Name.EndsWith(".meta"));
            fileInfos.RemoveAll(s => s.Name.StartsWith("ui_atlas"));
            for (int j = 0; j < fileInfos.Count; j++)
            {
                if (!fileInfos[j].Name.StartsWith("ui") || fileInfos[j].Name == "ui")
                    continue;
                string p = fileInfos[j].DirectoryName.Replace(@"\", @"/");
                p = p + @"/" + fileInfos[j].Name;
                preTexDic.Add(fileInfos[j].Name, new List<string>());
                preTexDic[fileInfos[j].Name] = GetPreTexUsageList(p);
            }
        }
        return preTexDic;
    }
    /// <summary>
    /// 获取路径下预设引用Texture集合
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static List<string> GetPreTexUsageList(string path)
    {
        List<string> result = new List<string>();
        string text = System.IO.File.ReadAllText(path);
        Regex reg = new Regex(@"m_Texture:\s{.*guid:\s(.*),");
        MatchCollection match = reg.Matches(text);
        if (match.Count == 0) return result;
        //preTexDic.Add(fileInfos[j].Name,new List<string>());
        for (int k = 0; k < match.Count; k++)
        {
            string value = match[k].Groups[1].Value;
            if (!string.IsNullOrEmpty(value))
            {
                result.Add(value);
            }
        }
        return result;
    }


  

}
