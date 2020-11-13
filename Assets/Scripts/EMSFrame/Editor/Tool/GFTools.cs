using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityFrame;
using System;
using Object = UnityEngine.Object;


public class GFTools
{
    //[MenuItem("GameTools/Change TO ", priority = 11)]
    //public static void AAAAAA()
    //{
    //    var selects = Selection.gameObjects;

    //    foreach(var v in selects) {
    //        UnityFrame.SecenElementLayout s_layout = v.GetComponent<UnityFrame.SecenElementLayout>();

    //        UnityFrame.Assets.AssetSceneLayout layout = new UnityFrame.Assets.AssetSceneLayout();
    //        layout.mapCol = s_layout.mapCol;
    //        layout.mapRow = s_layout.mapRow;
    //        foreach (var d in s_layout.Data) {
    //            UnityFrame.Assets.SecenLayoutData data = new UnityFrame.Assets.SecenLayoutData();
    //            data.x = d.x;
    //            data.y = d.y;
    //            data.block = d.block;
    //            data.param = d.param;
    //            data.rotation = d.rotation;
    //            layout.Data.Add(data);
    //        }

    //        string path = AssetDatabase.GetAssetPath(s_layout);
    //        path = path.Replace(".prefab", ".asset");
    //        AssetDatabase.CreateAsset(layout, path);
    //    }
    //}



    //  [MenuItem("GameTools/Prefab/CreateMapElementLayout", priority = 11)]
    //  public static void CreateMapElementLayout()
    //  {
    //      GameObject go = new GameObject("sce_lay_temp");
    //go.AddComponent<SecenElementLayout>();
    //  }


    [MenuItem("Assets/GameTools/Audio/CreateAudio")]
    public static void CreateAudio()
    {
        Object tarSelect = Selection.activeObject;
        if (tarSelect == null) return;
        if (!(tarSelect is AudioClip)) return;

        GameObject go = new GameObject(tarSelect.name);
        var controller = go.AddComponent<AudioController>();
        var audioSource =  go.AddComponent<AudioSource>();
        audioSource.clip = tarSelect as AudioClip;
        controller.audioSource = audioSource;
    }



    [MenuItem("GameTools/Prefab/CreateSceneMap", priority = 10)]
    public static void CreateMapElement()
    {
        GameObject go = new GameObject("sce_obj_temp");
		go.AddComponent<SceneElement>();
    }
	
    [MenuItem("GameTools/Prefab/Clean Missing Script", priority = 9)]
    public static void RemoveMissingScript()
    {
        GameObject go = Selection.activeGameObject;
        var list = go.GetComponentsInChildren<Transform>(true);        
        Debug.Log(list.Length);
        foreach (var item in list)
        {
            RemoveMissingScript(item.gameObject);
        }
    }

    static private void RemoveMissingScript(GameObject item)
    {
        SerializedObject so = new SerializedObject(item);
        var soProperties = so.FindProperty("m_Component");
        var components = item.GetComponents<Component>();
        int propertyIndex = 0;
        foreach (var c in components)
        {
            if (c == null)
            {
                Debug.Log(item.gameObject.name);
                soProperties.DeleteArrayElementAtIndex(propertyIndex);
            }
            ++propertyIndex;
        }
        so.ApplyModifiedProperties();
    }

    [MenuItem("GameObject/编辑预设", priority = -2)]
    [MenuItem("GameTools/Prefab/编辑预设", priority = 2)]
    public static void UnpackPrefab()
    {
        GameObject go = Selection.activeGameObject;
        if(go != null)
            PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
    }

    [MenuItem("GameObject/保存预设", priority = -1)]
    [MenuItem("GameTools/Prefab/保存预设", priority = 1)]
    public static void SavePrefab()
    {
        GameObject go = Selection.activeGameObject;
        if (go == null)
            return;
                
        string folder = null;
        int index = go.name.IndexOf("_", StringComparison.Ordinal);
        if(index > -1)
        {
            folder = go.name.ToLower().Substring(0, index);
        }        
        if (!string.IsNullOrEmpty(folder))
        {
            string folderPath = "Assets/AssetBases/PrefabAssets/" + folder;
            if(!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError("不存在该类型预设===>" + folder);
                return;
            }
            string fileName = folderPath + "/" + go.name.ToLower() + ".prefab";
            if (File.Exists(fileName))
            {
                if(EditorUtility.DisplayDialog("警告","已存在"+ go.name + "预设,是否覆盖？", "覆盖", "取消"))
                {
                    PrefabUtility.SaveAsPrefabAssetAndConnect(go, fileName, InteractionMode.AutomatedAction);
                    Debug.Log("覆盖预设成功:" + fileName);
                }                
            }
            else
            {
                if(PrefabUtility.IsPrefabAssetMissing(go))
                {
                    UnpackPrefab();
                }
                PrefabUtility.SaveAsPrefabAssetAndConnect(go,fileName , InteractionMode.AutomatedAction);
                Debug.Log("保存预设成功:" + fileName);
            }            
        }            
        else
            Debug.LogError("不存在该类型预设===>" + folder);
    }

    /*
        [MenuItem("Assets/GameTools/Tools/还原声音预设", priority = 10002)]
        static void CreateSoundPrefab()
        {
            //TODO 读取配置文件
            StreamReader _fs = File.OpenText(Application.streamingAssetsPath + "/wavConfig.txt");
            if(_fs != null)
            {
                string _line = _fs.ReadToEnd();
                char[] _split = new char[] { '\r','\n' };
                string[] _contents = _line.Split(_split);
                foreach (var item in _contents)
                {
                    if (string.IsNullOrEmpty(item))
                        continue;

                    string[] _items = item.Split('|');
                    GameObject _prefab = new GameObject();
                    AudioController _controller = _prefab.AddComponent<AudioController>();
                    _controller.Audio = _controller.transform.GetComponent<AudioSource>();
                    string _pName = _items[0];
                    string _clip = _items[1];
                    string _musicType = _items[2];
                    string _playMode = _items[3];

                    if(string.IsNullOrEmpty(_clip) == false)
                    {
                        _controller.GetComponent<AudioSource>().clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/AssetBases/SourceAssets/Sound/" + _clip);
                    }
                    _controller.PlayMode = (AudioController.AudioPlayMode)(Enum.Parse(typeof(AudioController.AudioPlayMode),_playMode));
                    _prefab.name = _pName;
                    PrefabUtility.CreatePrefab(string.Format("Assets/{0}.prefab",_pName), _prefab);
                    Resources.UnloadAsset(_prefab.transform.GetComponent<AudioSource>().clip);
                    DestroyImmediate(_prefab);
                }
            }
            Debug.Log("还原声音预设 成功");
            AssetDatabase.Refresh();
        }
        */
    [MenuItem("Assets/GameTools/Tools/提取大规格UI", priority = 10003)]
    static void RemoveBigUI()
    {
        List<string> _allFiles = new List<string>();
        foreach (var item in Selection.objects)
        {
            string _tempPath = AssetDatabase.GetAssetPath(item);
            string[] _tempFileName = EditorPathTool.GetSubPatternFiles(_tempPath, new string[] { ".jpg", ".png" }, SearchOption.AllDirectories);
            _allFiles.AddRange(_tempFileName);
        }
        
        string _temp1 = "Assets/AssetBases/SourceAssets/UI";
        string _temp2 = "Assets/AssetBases/SourceAssets/UI_Big";
        EditorPathTool.CheckAndCreateDir(_temp2);
        Dictionary<string, string> _moveDic = new Dictionary<string, string>();
        foreach (var item in _allFiles)
        {
            Texture _tex = AssetDatabase.LoadAssetAtPath<Texture>(item);
            int _W = _tex.width;
            int _H = _tex.height;
            Resources.UnloadAsset(_tex);
            if ((_W * _H) > (128 * 128))
            {
                string _newPath = item.Replace(_temp1, _temp2);                
//                string _testNewPath = EditorPathTool.CheckAndCreateDir(EditorPathTool.GetFileAssetsDir(_newPath));
                _moveDic.Add(item,_newPath);
                
            }
        }
        AssetDatabase.Refresh();
        foreach (var item in _moveDic)
        {
            AssetDatabase.MoveAsset(item.Key, item.Value);
        }
        //Debug.LogFormat("name={0},width={1},height={2}", _tex.name, _tex.width, _tex.height);
        Debug.Log("RemoveBigUI ok=========");
        AssetDatabase.Refresh();
    }
    /*
    [MenuItem("Assets/GameTools/Tools/RemoveMatAndPrefab", priority = 10004)]
    static void RemoveMats()
    {               
        string _dir = AssetDatabase.GetAssetPath(Selection.activeObject);
        string[] _files =  EditorPathTool.GetSubPatternFiles(_dir, new string[] { ".mat", ".prefab" }, SearchOption.AllDirectories);
        foreach (var item in _files)
        {
            string _temp = EditorPathTool.GetRelativePath(item);
            AssetDatabase.DeleteAsset(_temp);
        }
        Debug.Log("remove ok=========");
        AssetDatabase.Refresh();
    }
    */

	[MenuItem("GameTools/Tools/打开持久路径")]
	static void OpenPersistentDataPath()
	{
		Application.OpenURL(Application.persistentDataPath);
	}

    [MenuItem("Assets/GameTools/Tools/更新UI图片名字格式", priority = 10005)]
    static void RenameUI()
    {
        Dictionary<string,List<string>> _allFiles = new Dictionary<string, List<string>>();
        foreach (var item in Selection.objects)
        {
            string _tempPath = AssetDatabase.GetAssetPath(item);
            List<string> _temp = new List<string>();
            string[] _tempFileName = EditorPathTool.GetSubPatternFiles(_tempPath, new string[] { ".jpg", ".png",".tga" }, SearchOption.AllDirectories);
            _temp.AddRange(_tempFileName);
            string _dir = EditorPathTool.GetSelectDirName(_tempPath);
            _allFiles.Add(_dir, _temp);
            
        }
        foreach (var item in _allFiles)
        {            
            foreach (var it in item.Value)
            {
                string[] _tempNames = EditorPathTool.GetSelectFileName(it,true).Split('@');
                string _tempName = _tempNames[0];
                if (_tempNames.Length > 1)
                    _tempName = _tempNames[_tempNames.Length - 1];
                
                string _temp = AssetDatabase.RenameAsset(it, item.Key + "@" + _tempName);
                if(!string.IsNullOrEmpty(_temp))
                {
                    Debug.LogErrorFormat("重命名UI出错:{0}", _temp);
                }
            }
        }        
        Debug.Log("RenameUI ok=========");
        AssetDatabase.Refresh();
    }


    [MenuItem("Assets/GameTools/Tools/将该目录下的所有fbx改为generic", priority = 10006)]
    static void Change2Generic()
    {
        Object _obj = Selection.activeObject;
        string[] _temp = Directory.GetFiles(AssetDatabase.GetAssetPath(_obj), "*.FBX", SearchOption.AllDirectories);
        HashSet<string> _paths = new HashSet<string>();
        foreach (var item in _temp)
        {
            _paths.Add(item);
        }
        foreach (var item in _paths)
        {
            ModelImporter _ai = ModelImporter.GetAtPath(item) as ModelImporter;
            _ai.animationType = ModelImporterAnimationType.Generic;            
            _ai.SaveAndReimport();
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("将该目录下的所有fbx改为generic 完成");
    }


    static Object[] GetSelectedTextures() 
    { 
        return Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets); 
    }

    [MenuItem("Assets/GameTools/Tools/Texture/批量去除MipMap")]
    static void SelectedChangeMimMap() { 
        bool enabled = false;
        Object[] textures = GetSelectedTextures(); 
        Selection.objects = new Object[0];
        foreach (Texture2D texture in textures)  {
            string path = AssetDatabase.GetAssetPath(texture); 
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter; 
            textureImporter.mipmapEnabled = enabled;
            textureImporter.wrapMode = TextureWrapMode.Clamp;
            AssetDatabase.ImportAsset(path); 
        }
        AssetDatabase.SaveAssets();
    }

    static void ChangeTextureMaxSize(int size){
        Object[] textures = GetSelectedTextures(); 
        Selection.objects = new Object[0];
        foreach (Texture2D texture in textures)  {
            string path = AssetDatabase.GetAssetPath(texture); 
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter; 
            textureImporter.maxTextureSize = size;
            AssetDatabase.ImportAsset(path); 
        }
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Assets/GameTools/Tools/Texture/批量设置贴图256")]
    static void SelectedChangeTextureMaxSize256() { 
        ChangeTextureMaxSize(256);
    }
    [MenuItem("Assets/GameTools/Tools/Texture/批量设置贴图512")]
    static void SelectedChangeTextureMaxSize512() { 
        ChangeTextureMaxSize(512);
    }

    /*  批量修改预设名字
    [MenuItem("Assets/GameTools/Tools/Test", priority = 10002)]
    static void  Change ()
    {
        Debug.Log("start================");
        Object _obj = Selection.activeObject;
        string _path = AssetDatabase.GetAssetPath(_obj);
        string[] _dirs =  Directory.GetDirectories(_path);
        foreach (var item in _dirs)
        {
            DirectoryInfo _dirInfo = new DirectoryInfo(item);
            string[] _subFiles = Directory.GetFiles(item, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var it in _subFiles)
            {
                if (it.EndsWith(".meta"))
                    continue;
                string _newPath;
                try
                {
                    string _newName = it.Replace("@" + _dirInfo.Name + "_", "@");
                    FileInfo _info = new FileInfo(_newName);
                    _newPath = AssetDatabase.RenameAsset(GetRelative(it), _info.Name.Replace(_info.Extension, ""));
                }
                catch (System.Exception)
                {
                    continue;
                }
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/GameTools/Tools/Test2", priority = 10003)]
    static void Change2()
    {
        Debug.Log("start================");
        Object _obj = Selection.activeObject;
        string _path = AssetDatabase.GetAssetPath(_obj);
        string[] _dirs = Directory.GetDirectories(_path);
        foreach (var item in _dirs)
        {
            DirectoryInfo _dirInfo = new DirectoryInfo(item);
            string[] _subFiles = Directory.GetFiles(item, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var it in _subFiles)
            {
                if (it.EndsWith(".meta"))
                    continue;
                string _newPath;
                try
                {

                    string _str = _dirInfo.Name + "@" + _dirInfo.Name;
                    if (it.Contains(_str) == false)
                        continue;
                    string _newName = it.Replace(_str, _dirInfo.Name);
                    FileInfo _info = new FileInfo(_newName);
                    _newPath = AssetDatabase.RenameAsset(GetRelative(it), _info.Name.Replace(_info.Extension, ""));
                }
                catch (System.Exception)
                {
                    continue;
                }
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    */

    /// <summary>
    /// 获取相对于Project目录下的路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    static string GetRelative(string path)
    {
        return EditorPathTool.GetRelativePath(path);
    }
    /// <summary>
    /// 全路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    static string GetFull(string path)
    {
        return EditorPathTool.GetFullAssetsPath(path);
    }
}
