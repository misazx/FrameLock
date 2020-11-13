using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;


public class AssetsBuilderTools {

    public static readonly string ABOutPath = Application.dataPath + "/AssetBases/BundleAssets/";

    //public static readonly string ABOutPath = Application.dataPath + "/StreamingAssets/BundleAssets/";


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

    static void Log(params object[] args){
		if (args != null) {
			string info = "AssetBundle Build Message:[";
			for (int k = 0; k < args.Length; k++) {
				info += args[k].ToString() + " | ";
			}
			info += "]";
			Debug.Log (info);
		}
		
	}

	private static string getBundleName(string name){
		int pidx = name.IndexOf ('@');
		if (pidx > -1) {
			return name.Substring (0, pidx);
		} else {
			return name;
		}
	}		

    private static string GetBundleAssetName(string name){
	    int idx = name.LastIndexOf ('/');
	    if (idx >= 0) {
	    	return name.Substring (idx+1);
	    } else {
	    	return name;
	    }
    }


    private static Dictionary<string,string> GetAllABFileBaseManifest(AssetBundleManifest  mManifest){
    Dictionary<string,string> MapABManifestSet = new Dictionary<string, string>();
        if (mManifest == null) {
            
            Debug.LogError ("AssetBundleManifest is null");
            return MapABManifestSet;
        } else {
            if (mManifest != null) {


            MapABManifestSet.Add("BundleAssets","BundleAssets");
            string[] bundels = mManifest.GetAllAssetBundles ();
            if(bundels != null){
                for (int k = 0; k < bundels.Length; k++) {
                    MapABManifestSet.Add(bundels[k],GetBundleAssetName(bundels[k]));
                }
             }
            }
        }
        return MapABManifestSet;
    }


    private static void DeleteAsset(string path){
        if (File.Exists(path)) {
            File.Delete(path);
        }
    }

    private static void DeleteOldFile(List<string> ListToDelete,string outpath){
        foreach (string value in ListToDelete) {
            Debug.Log("Delete AB :" + value);
//            AssetDatabase.DeleteAsset("Assets/AssetBases/BundleAssets/"+value);
//            AssetDatabase.DeleteAsset("Assets/AssetBases/BundleAssets/"+value + ".manifest");
                DeleteAsset(outpath + value);
                DeleteAsset(outpath + value + ".meta");
                DeleteAsset(outpath +value + ".manifest");
                DeleteAsset(outpath +value + ".manifest.meta");
        }
    }

	//修改生成的log 文件
    private static void FixGenFile(AssetBundleManifest abManifestPath,string outpath){
        string outLogFilePath = outpath + "ABOutFile.log";
        string outLogNewFilePath = outLogFilePath + ".out";
        Dictionary<string,string> MapABManifestSet = GetAllABFileBaseManifest(abManifestPath);
        if (File.Exists(outLogFilePath)) {
        StreamReader sr = new StreamReader(outLogFilePath);
        Dictionary<string,int> MapOutABSet = new Dictionary<string, int>();

        List<string> ListToDelete = new List<string>();

        string line = string.Empty;


        while ((line = sr.ReadLine()) != null) {
                if (!string.IsNullOrEmpty(line)) {
                    MapOutABSet.Add(line, 0);
                }
        }
        sr.Close();

        foreach (string value in MapOutABSet.Keys) {
            if (!MapABManifestSet.ContainsKey(value)) {
                    ListToDelete.Add(value);
            }
        }


        StreamWriter sw = new StreamWriter(File.Create(outLogNewFilePath));
        foreach (string value in MapABManifestSet.Keys) {
            sw.WriteLine(value);        
        }
        sw.Close();
        
        DeleteOldFile(ListToDelete,outpath);

        File.Delete(outLogFilePath);
        File.Move(outLogNewFilePath,outLogFilePath);

    }     

    else{
        
        StreamWriter sw = new StreamWriter(File.Create(outLogFilePath));
        foreach (string value in MapABManifestSet.Keys) {
            sw.WriteLine(value);        
        }
        sw.Close();
    }


}



    public static void BuildAllBundleToPath(string outpath,BuildTarget buildTarget){
        try{
            GenAllAssetBundleTag();
            Log("AB Tag Gen Finish");
            AssetBundleManifest abm =BuildPipeline.BuildAssetBundles (outpath,BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.DeterministicAssetBundle,buildTarget);
            FixGenFile(abm,outpath);

            Log("Build All Bundles","Success");

        }
        catch(System.Exception e){
          Log ("Build All Bundles","Failed", e.Message);
          throw new System.Exception("Bundle Build Failed ",e);
        }
        AssetDatabase.Refresh ();
    }

    public static void CleanAllBundlesAtPath(string path){
        try{
        System.IO.DirectoryInfo folder = new System.IO.DirectoryInfo (path);
        ClearAllFileInFloader(folder);
        AssetDatabase.Refresh ();
        Log("Clean All Bundles","Finished");
        }
        catch(System.Exception e){
        Log("Clean All Bundles","Failed",e.Message);
        }
    }


	[MenuItem("GameTools/AssetBundle/构建全部AB资源")]
	public static void BuildAllBundles(){
        BuildAllBundleToPath(ABOutPath,TargetPlat);
	}


	[MenuItem("GameTools/AssetBundle/清除全部AB资源")]
	public static void CleanAllBundles(){
        CleanAllBundlesAtPath(ABOutPath);
	}   

    [MenuItem("GameTools/AssetBundle/构建全部AB资源到指定路径")]
    public static void BuildAllBundlesTo(){
        string path = EditorUtility.SaveFolderPanel("Build Assets", Application.dataPath, Application.productName);
        if (!string.IsNullOrEmpty(path)) {
            BuildAllBundleToPath(path + "/", TargetPlat);
        }
    }

	[MenuItem("GameTools/AssetBundle/批量生成AB标签")]
	public static void GenAllAssetBundleTag(){
		string path = Application.dataPath + "/AssetBases/PrefabAssets/";
		DirectoryInfo directory = new DirectoryInfo (path);
		FileInfo[] files = directory.GetFiles ("*.*",SearchOption.AllDirectories);
		for (int k = 0; k < files.Length; k++) {
			string fileName = files [k].Name;
			string extension = Path.GetExtension (fileName);
			if (
				extension == ".prefab" ||
				extension == ".controller" ||
				extension == ".png" ||
				extension == ".PNG" ||
				extension == ".jpg" ||
				extension == ".JPG" ||
				extension == ".ttf" ||
				extension == ".TTF" ||
				extension == ".otf"	||
				extension == ".fontsettings"||
                extension == ".asset"  ||
                extension == ".shader"

			) {
				string fileFullName = files [k].FullName.Replace('\\','/');
				string absPath = "Assets" + fileFullName.Replace (Application.dataPath, "");
				AssetImporter ai = AssetImporter.GetAtPath (absPath);
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension (files [k].Name);
				string tag = fileFullName.Replace (path,"").Replace(files [k].Name,"");
				string bundlename = string.Format ("{0}{1}", tag,fileNameWithoutExtension);

                //shader做特殊处理,shader 包
                if (fileName.IndexOf(".shader", System.StringComparison.Ordinal) > -1)
                {
                    tag = tag.Substring(0, tag.IndexOf("shader/", System.StringComparison.Ordinal) + 7);
                    bundlename = string.Format("{0}{1}", tag, "shader_package");
                }else {
                    //处理集合包，@ 相关的包
                    bundlename = string.Format("{0}{1}", tag, Path.GetFileNameWithoutExtension(getBundleName(fileName)));
                }
                ai.assetBundleName = bundlename;
			}
		}
		Log("Gen All AssetBundle Tag","Finish");
		AssetDatabase.Refresh ();
	}


	[MenuItem("GameTools/AssetBundle/构建指定AB到指定目录")]
	public static void BuildSelectTargets(){
		try{
			string path = EditorUtility.SaveFolderPanel("Build Assets", Application.dataPath, Application.productName);
			if (!string.IsNullOrEmpty(path)) {
				buildSelectTargets (BuildAssetBundleOptions.UncompressedAssetBundle,path);
			}
		}
		catch(System.Exception e){
			Log ("Build Select Targets", "Failed", e.Message);
		
		}
	}

//	[MenuItem("GameTools/AssetBundle/Rebuild Select Target")]
	public static void RebuildSelectTargets(){
		try{
			buildSelectTargets (BuildAssetBundleOptions.UncompressedAssetBundle,ABOutPath);
		}
		catch(System.Exception e){
			Log ("Reuild Select Targets", "Failed", e.Message);

		}
	}


	static void buildSelectTargets(BuildAssetBundleOptions optcode,string outPath){
		Object[] selects = Selection.objects;

		System.Collections.Generic.Dictionary<string,AssetBundleBuild> tBuilds = new System.Collections.Generic.Dictionary<string, AssetBundleBuild> ();
		for (int k = 0; k < selects.Length; k++) {
			string path = AssetDatabase.GetAssetPath (selects [k]);
			AssetImporter importer = AssetImporter.GetAtPath (path);
			if (importer != null) {
				if (importer.assetBundleName == null) {
					importer.assetBundleName = selects [k].name;
				}
				if(!tBuilds.ContainsKey(importer.assetBundleName)){
					AssetBundleBuild build = new AssetBundleBuild ();
					build.assetBundleName = importer.assetBundleName;
					tBuilds.Add (importer.assetBundleName,build);
				}
				AssetBundleBuild newbuild = tBuilds [importer.assetBundleName];
				List<string> tml = null;

				if (newbuild.assetNames != null)
					tml = new List<string> (newbuild.assetNames);
				else
					tml = new List<string> ();

				tml.Add (path);
				newbuild.assetNames = tml.ToArray (); 
				tBuilds [importer.assetBundleName] = newbuild;
			}
		}
		List<AssetBundleBuild> list = new List<AssetBundleBuild> (tBuilds.Values);
		
		BuildPipeline.BuildAssetBundles (outPath,list.ToArray() , optcode, TargetPlat);
	}


	static void ClearAllFileInFloader(System.IO.DirectoryInfo folder){
		System.IO.DirectoryInfo[] dires = folder.GetDirectories ();
		
		if(dires != null){
			foreach (System.IO.DirectoryInfo directory in dires) {
				ClearAllFileInFloader (directory);
			}
		}

		FileInfo[] finfos = folder.GetFiles ();
		foreach (System.IO.FileInfo file in finfos) {
			file.Delete ();
		}

	}





	[MenuItem("GameTools/AssetBundle/加载AB资源(同步)")]
	static void Load_AssetsBundle(){
		Resources.UnloadUnusedAssets();

        string openPath = EditorPrefs.GetString("LoadAB", Application.dataPath);
		string path = EditorUtility.OpenFilePanel("Open Avatar", openPath, "");
        EditorPrefs.SetString("LoadAB", Path.GetDirectoryName(path));

        string file_name = System.IO.Path.GetFileNameWithoutExtension (path);
		
		if(path != "" || path != null){
			AssetBundle ab = AssetBundle.LoadFromFile(path);
			if(ab != null){
                foreach (var v in ab.LoadAllAssets()) {
                    if (v is GameObject) {
                        GameObject.Instantiate(v).name = v.name;
                    }
                }
			}
			ab = null;
		}
	}

    [MenuItem("GameTools/AssetBundle/加载AB资源(异步)")]
    static void Load_AssetsBundle_lAnsy()
    {
        Resources.UnloadUnusedAssets();

        string openPath = EditorPrefs.GetString("LoadAB", Application.dataPath);
        string path = EditorUtility.OpenFilePanel("Open Avatar", openPath, "");
        EditorPrefs.SetString("LoadAB", Path.GetDirectoryName(path));


        WWW www = new WWW(path);
        if (www.error != null)
        {
            Debug.LogError(www.error);

        }
        else
        {
            if (www.assetBundle != null)
            {
                foreach (var v in www.assetBundle.LoadAllAssets())
                {
                    if (v is GameObject)
                    {
                        GameObject.Instantiate(v).name = v.name;
                    }
                }
            }
            www.Dispose();
        }
        www = null;
    }

    [MenuItem("GameTools/AssetBundle/加载AB场景")]
    static void Load_AssetBundle_Scene() {
        Resources.UnloadUnusedAssets();

        string openPath = EditorPrefs.GetString("LoadAB", Application.dataPath);
        string path = EditorUtility.OpenFilePanel("Open Avatar", openPath, "");
        EditorPrefs.SetString("LoadAB", Path.GetDirectoryName(path));

        string file_name = System.IO.Path.GetFileNameWithoutExtension(path);

        if (path != "" || path != null)
        {
            AssetBundle ab = AssetBundle.LoadFromFile(path);
            if (ab != null)
            {
                Application.LoadLevel("SceneMain");
            }
            ab = null;
        }
    }




    public static void BuildReviewRebundleAssetTo(string outDir, BuildTarget buildTarget)
    {
        string prefabPath = UnityFrame.GlobalPath.AssetBasesPath + "ReviewAssets/Prefab/";
        DirectoryInfo di = new DirectoryInfo(prefabPath);
        if (!di.Exists)
        {
            Debug.Log("U3D_BUILD: NO Prefab Folder to build");
            return;
        }
        if (!Directory.Exists(outDir))
        {
            Directory.CreateDirectory(outDir);
        }
        FileInfo[] fis = di.GetFiles();
        if (fis == null || fis.Length == 0)
        {
            Debug.Log("U3D_BUILD: NO Prefab Files to build");
            return;
        }
        List<AssetBundleBuild> lstBuild = new List<AssetBundleBuild>();

        for (int k = 0; k < fis.Length; k++)
        {
            string absPath = "Assets" + fis[k].FullName.Replace(Application.dataPath, "");
#if UNITY_STANDALONE || UNITY_EDITOR
            absPath = absPath.Replace("\\", "/");
#endif
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(absPath);
            if (obj == null)
                continue;
            string path = AssetDatabase.GetAssetPath(obj);
            string name = Path.GetFileNameWithoutExtension(path);
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetNames = new string[] { path };
            build.assetBundleName = getBundleName(name);
            lstBuild.Add(build);
        }

        Debug.Log("U3D_BUILD: Try To build listBuild:" + lstBuild.Count);
        if (lstBuild.Count > 0)
        {

            BuildPipeline.BuildAssetBundles(outDir, lstBuild.ToArray(), BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.DeterministicAssetBundle, buildTarget);
        }

    }




}
