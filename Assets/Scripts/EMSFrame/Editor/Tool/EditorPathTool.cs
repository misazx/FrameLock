using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Security.AccessControl;

public class EditorPathTool
{
    public static string GetFileAssetsDir(string filePath)
    {
        return Path.GetDirectoryName(filePath);
    }
    public static string GetFileShortDir(string filePath)
    {
        string _path = Path.GetDirectoryName(filePath);
        DirectoryInfo _info = new DirectoryInfo(_path);
        return _info.Name;
    }
    /// <summary>
    /// 检查 并且新建目录
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string CheckAndCreateDir(string path)
    {
        DirectoryInfo _info = new DirectoryInfo(path);
        if(_info.Exists == false)
        {
            _info.Create();
           // Directory.CreateDirectory(path);
        }
        return path;
    }
    /// <summary>
    /// Assets 下的Full路径转化为相对于Assets的路径
    /// </summary>
    /// <param name="path">Assets 下的Full路径</param>
    /// <returns></returns>
    public static string GetRelativePath(string path)
    {
        string _temp = Application.dataPath.Remove(Application.dataPath.Length - 6, 6);
        return path.Replace(_temp, "");
    }
    /// <summary>
    /// Assets的相对路径 转化为 Full路径 
    /// </summary>
    /// <param name="path">Assets的相对路径</param>
    /// <returns></returns>
    public static string GetFullAssetsPath(string path)
    {
        string _temp = Application.dataPath.Remove(Application.dataPath.Length - 6, 6);
        return (_temp + path);
    }
    /// <summary>
    /// 获取选中文件加的文件夹名字
    /// </summary>
    /// <returns></returns>
    public static string GetSelectDirName()
    {
        if (Selection.activeObject == null)
            return null;
        string _path = AssetDatabase.GetAssetPath(Selection.activeObject);
        return GetSelectDirName(_path);
    }
    /// <summary>
    /// 获取选中文件加的文件夹名字
    /// </summary>
    /// <returns></returns>
    public static string GetSelectDirName(string path)
    {
        DirectoryInfo _dirInfo = new DirectoryInfo(path);
        return _dirInfo.Name;
    }
    /// <summary>
    /// 获取选中文件加的文件夹名字
    /// </summary>
    /// <returns></returns>
    public static string[] GetSelectDirNames()
    {
        if (Selection.objects == null || Selection.objects.Length == 0)
            return null;
        List<string> _path = new List<string>();
        foreach (var item in Selection.objects)
        {
            string _tempPath = AssetDatabase.GetAssetPath(item);
            DirectoryInfo _dirInfo = new DirectoryInfo(_tempPath);
            _path.Add(_dirInfo.Name);
        }
        return _path.ToArray();
    }
    /// <summary>
    /// 获取选中文件加的文件夹名字
    /// </summary>
    /// <returns></returns>
    public static string[] GetSelectDirNames(string[] paths)
    {
        List<string> _path = new List<string>();
        foreach (var item in paths)
        {
            DirectoryInfo _dirInfo = new DirectoryInfo(item);
            _path.Add(_dirInfo.Name);
        }
        return _path.ToArray();
    }
    /// <summary>
    /// 获取选中文件的文件名字
    /// </summary>
    /// <returns></returns>
    public static string GetSelectFileName(bool splitExtension = false)
    {
        if (Selection.activeObject == null)
            return null;
        string _path = AssetDatabase.GetAssetPath(Selection.activeObject);
        FileInfo _dirInfo = new FileInfo(_path);
        return splitExtension ? _dirInfo.Name.Replace(_dirInfo.Extension,"") : _dirInfo.Name;
    }
    /// <summary>
    /// 获取选中文件的文件名字
    /// </summary>
    /// <returns></returns>
    public static string GetSelectFileName(string path,bool splitExtension = false)
    {
        FileInfo _dirInfo = new FileInfo(path);
        return splitExtension ? _dirInfo.Name.Replace(_dirInfo.Extension, "") : _dirInfo.Name;
    }

    /// <summary>
    /// 获取选中文件的文件名字
    /// </summary>
    /// <returns></returns>
    public static string[] GetSelectFileNames(bool splitExtension = false,string[] pattern = null)
    {
        if (Selection.objects == null || Selection.objects.Length == 0)
            return null;
        List<string> _path = new List<string>();
        foreach (var item in Selection.objects)
        {
            string _tempPath = AssetDatabase.GetAssetPath(item);
            if (IsEndWith(_tempPath, pattern))
                continue;
            FileInfo _dirInfo = new FileInfo(_tempPath);
            string _item = splitExtension ? _dirInfo.Name.Replace(_dirInfo.Extension, "") : _dirInfo.Name; 
            _path.Add(_item);
        }
        return _path.ToArray();
    }
    /// <summary>
    /// 获取选中文件的相对路径
    /// </summary>
    /// <returns></returns>
    public static string[] GetSelectFilePaths(string[] pattern = null)
    {
        if (Selection.objects == null || Selection.objects.Length == 0)
            return null;
        List<string> _path = new List<string>();
        foreach (var item in Selection.objects)
        {
            string _tempPath = AssetDatabase.GetAssetPath(item);
            if (IsEndWith(_tempPath, pattern))
                continue;
            _path.Add(_tempPath);
        }
        return _path.ToArray();
    }
    /// <summary>
    /// 获取文件夹下的文件
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="pattern"></param>
    /// <param name="opt"></param>
    /// <returns></returns>
    public static string[] GetSubFiles(string dir,string[] pattern = null,SearchOption opt = SearchOption.TopDirectoryOnly)
    {
        string[] _allFiles = Directory.GetFiles(dir, "*.*", opt);
        List<string> _tempFiles = new List<string>();
        foreach (var item in _allFiles)
        {
            if (IsEndWith(item, pattern))
                continue;
            _tempFiles.Add(item);
        }
        return _tempFiles.ToArray();
    }
    /// <summary>
    /// 获取指定的类型文件
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="pattern"></param>
    /// <param name="opt"></param>
    /// <returns></returns>
    public static string[] GetSubPatternFiles(string dir, string[] pattern = null, SearchOption opt = SearchOption.TopDirectoryOnly)
    {
        string[] _allFiles = Directory.GetFiles(dir, "*.*", opt);
        List<string> _tempFiles = new List<string>();
        foreach (var item in _allFiles)
        {
            if (!IsEndWith(item, pattern))
                continue;
            _tempFiles.Add(item);
        }
        return _tempFiles.ToArray();
    }
    /// <summary>
    /// 获取子文件夹
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="opt"></param>
    /// <returns></returns>
    public static string[] GetSubDirs(string dir, SearchOption opt = SearchOption.TopDirectoryOnly)
    {
        string[] _allFiles = Directory.GetDirectories(dir, "*",opt);
        List<string> _tempFiles = new List<string>();
        foreach (var item in _allFiles)
        {
            _tempFiles.Add(item);
        }
        return _tempFiles.ToArray();
    }
    /// <summary>
    /// 文件是否 以xxx后缀名结尾
    /// </summary>
    /// <param name="content"></param>
    /// <param name="patterns"></param>
    /// <returns></returns>
    static bool IsEndWith(string content,string[] patterns)
    {
        if (patterns == null)
            return false;
        foreach (var item in patterns)
        {
            if (content.TrimEnd().EndsWith(item))
                return true;
        }
        return false;
    }
    static bool IsInclude(string item,string[] arr)
    {
        return arr != null && Array.IndexOf<string>(arr, item) != -1;
    }
}
