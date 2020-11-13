using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using LuaInterface;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using FileMode = System.IO.FileMode;

public class GenerateInvalidCodeTools : EditorWindow
{
    private Vector2 offset = new Vector2(3f, 6f);
    private int generateCodeCount = 10;
  
    private string generateCodeClassNamePrefix = "Invalid";
    private string functionNameSuffix = "Func";
    private string generateCodeOutPath ;
    private string codeMgrPath;
    private TextAsset CodeTemplateObject ;
    private string CodeTemplate = "";
    /// <summary>
    /// 类名标签
    /// </summary>
    private string ClassNameTag = "ClassName";
    /// <summary>
    /// 静态方法标签
    /// </summary>
    private string FunctionNameTag = "FunctionName";
    /// <summary>
    /// 参数标签
    /// </summary>
    private string ParamNameTag = "Param";
    /// <summary>
    /// 执行标签
    /// </summary>
    private string executeTag = "///TODO:???#";
    /// <summary>
    /// 执行开始标签
    /// </summary>
    private char executeStartTag = '#';
    /// <summary>
    /// 执行结束标签
    /// </summary>
    private char executeEndTag = '%';

    /// <summary>
    /// 执行记录
    /// </summary>
    private Dictionary<string,string> executeDic = new Dictionary<string, string>(); 

    private string[] Param = new string[]
    {
        "",
        "int a = 112",
        "int a = 2687, bool b = true",
        "int a = 30, bool b = false, short c = 1465",
        "int a = 456, bool b = true, short c = 451 , long d = 123",
    };

    private short randomSeed = 1;
    private System.Random random;

    

    [MenuItem("GameTools/Tools/GenInvalidCode")]
    public static void GenInvalidCode()
    {
        EditorWindow ew = GetWindow<GenerateInvalidCodeTools>(false, "生成无效代码");
        ew.minSize = new Vector2(500, 300);
    }
    protected void OnEnable()
    {
        generateCodeOutPath = Application.dataPath + @"/Scripts/UnitTest/InvalidCode/";
        codeMgrPath = @"Assets/Scripts/UnitTest/ICodeMgr.cs";
        if (!CodeTemplateObject)
            CodeTemplateObject = AssetDatabase.LoadAssetAtPath(@"Assets/Editor/EditorTools/InvalidCodeTemplate.txt", typeof(TextAsset)) as TextAsset;
    }

    protected void OnGUI()
    {
        Rect rect = new Rect(offset.x, offset.y, position.width - offset.x * 2, position.height - 2 * offset.y);
        DrawContent(rect);
    }

    private void DrawContent(Rect rect)
    {
        GUIStyle bb = new GUIStyle();
        bb.normal.background = null;    
        bb.normal.textColor = new Color(1, 0, 0);   
        Rect contentRect = rect;
        GUILayout.BeginArea(contentRect);
        EditorGUILayout.BeginVertical();


        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("生成代码数量", GUILayout.Width(90));
        string inputCount = GUILayout.TextField(generateCodeCount.ToString()) ;
        try
        {
            generateCodeCount = int.Parse(inputCount);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
        GUILayout.Label("生成代码类名前缀", GUILayout.Width(120));
        generateCodeClassNamePrefix = GUILayout.TextField(generateCodeClassNamePrefix);
        GUILayout.Label("生成方法名后缀", GUILayout.Width(105));
        functionNameSuffix = GUILayout.TextField(functionNameSuffix);

        EditorGUILayout.EndHorizontal();


        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("生成代码路径", GUILayout.Width(90));
        GUILayout.Label(generateCodeOutPath);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("生成代码模板", GUILayout.Width(90));
        TextAsset temp = EditorGUILayout.ObjectField(CodeTemplateObject, typeof(TextAsset)) as TextAsset;
        if (temp != CodeTemplateObject)
        {
            CodeTemplateObject = temp;
            CodeTemplate = temp.ToString();
        }
        if (string.IsNullOrEmpty(CodeTemplate) && CodeTemplateObject != null)
        {
            CodeTemplate = CodeTemplateObject.ToString();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Label("注意：先generate 再Add", bb);


        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("GenerateAll", EditorStyles.miniButtonLeft))
        {
            executeDic.Clear();
            DelectDir(generateCodeOutPath);
            GenerateAllCode();
        }
        if (GUILayout.Button("AddExecute", EditorStyles.miniButtonMid))
        {
            if (executeDic.Count.Equals(0) && EditorUtility.DisplayDialog("Warning!",
                            "是否先进行 GenerateAll,再执行当前操作?", "Yes",
                            "No"))
            {
                executeDic.Clear();
                DelectDir(generateCodeOutPath);
                GenerateAllCode();
            }
            if (CleanExecute())
                AddExecute();
        }
        
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        GUILayout.EndArea();
    }
    /// <summary>
    /// 生成所有代码
    /// </summary>
    private void GenerateAllCode()
    {
        string name = "";
        for (int i = 0; i < generateCodeCount; i++)
        {
            name = generateCodeClassNamePrefix + GetRandomName();
            while (executeDic.ContainsKey(name))
            {
                name = generateCodeClassNamePrefix + GetRandomName();
            }

            GenerateCode(name);
            if (!executeDic.ContainsKey(name))
                executeDic.Add(name, name + functionNameSuffix);
        }
        AssetDatabase.Refresh();
        
        
    }
    /// <summary>
    /// 生成代码
    /// </summary>
    /// <param name="name">类名</param>
    private void GenerateCode(string name)
    {
        
        if (File.Exists(generateCodeOutPath + name + ".cs")) return;
        using (FileStream fs = new FileStream(generateCodeOutPath + name + ".cs", FileMode.Append, FileAccess.Write))
        {
            fs.Lock(0, fs.Length);
            StreamWriter sw = new StreamWriter(fs);
            string temp = CodeTemplate;
            temp = temp.Replace(ClassNameTag, name);
            temp = temp.Replace(FunctionNameTag, name + functionNameSuffix);
            int p = GetRandomNumber(0, 4);
            temp = temp.Replace(ParamNameTag,Param[p]);
            randomSeed += 1;
            sw.Write(temp);
            fs.Unlock(0, fs.Length);
            sw.Flush();
        }
    }
    /// <summary>
    /// 加入到执行脚本中
    /// </summary>
    private void AddExecute()
    {
        TextAsset mgr = AssetDatabase.LoadAssetAtPath(codeMgrPath, typeof(TextAsset)) as TextAsset;
        string temp = mgr.ToString();
        string co = "";
        foreach (KeyValuePair<string, string> var in executeDic)
        {
            co += "        " + var.Key + "." + var.Value + "();\n";
        }
        temp = temp.Replace(executeTag, executeTag + "\n" + co);
        using (FileStream fs = new FileStream(AssetDatabase.GetAssetPath(mgr), FileMode.Create, FileAccess.Write))
        {
            fs.Lock(0, fs.Length);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(temp);
            fs.Unlock(0, fs.Length);
            sw.Flush();
        }
        AssetDatabase.Refresh();
    }
    /// <summary>
    /// 清除执行脚本
    /// </summary>
    private bool CleanExecute()
    {
        TextAsset mgr = AssetDatabase.LoadAssetAtPath(codeMgrPath, typeof(TextAsset)) as TextAsset;
        if (!mgr)
        {
            EditorUtility.DisplayDialog("Warning!",
                "找不到文件" + codeMgrPath, "确认");
            return false;
        }
        string temp = mgr.ToString();
        int startIndex = temp.ToCharArray().ToList().FindIndex(s=> s == executeStartTag);
        int endIndex = temp.ToCharArray().ToList().FindIndex(s => s == executeEndTag);
        temp = temp.Remove(startIndex + 1, endIndex - 5 - startIndex);
        using (FileStream fs = new FileStream(AssetDatabase.GetAssetPath(mgr), FileMode.Create, FileAccess.Write))
        {
            fs.Lock(0, fs.Length);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(temp);
            fs.Unlock(0, fs.Length);
            sw.Flush();
        }
        AssetDatabase.Refresh();
        return true;
    }
    /// <summary>
    /// 获取随机姓名
    /// </summary>
    /// <returns></returns>
    private string GetRandomName()
    {

        int p = GetRandomNumber(8, 12);
        string res = "";
        byte[] btNumber = new byte[] {};
        for (int i = 0; i < p; i++)
        {
            btNumber = new byte[] { (byte)GetRandomNumber(65, 90) }; ;
            res += System.Text.ASCIIEncoding.UTF8.GetString(btNumber);
        }
        return res;
    }
    /// <summary>
    /// 获取随机数字
    /// </summary>
    /// <param name="mix">最小值</param>
    /// <param name="max">最大值</param>
    /// <returns></returns>
    private int GetRandomNumber(int mix, int max)
    {
        max += 1;
        System.Random random = new System.Random(randomSeed);
        int p = random.Next(mix, max);
        randomSeed += 1;
        return p;
    }
    /// <summary>
    /// 删除目录下文件
    /// </summary>
    /// <param name="srcPath"></param>
    private void DelectDir(string srcPath)
    {
        try
        {
            DirectoryInfo dir = new DirectoryInfo(srcPath);
            FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  
            foreach (FileSystemInfo i in fileinfo)
            {
                if (i is DirectoryInfo)            
                {
                    DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                    subdir.Delete(true);          
                }
                else
                {
                    File.Delete(i.FullName);      
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }



}
