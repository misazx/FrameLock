using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEditor;



public static class ProductionTools {

    struct KeyValue
    {
        public string Key;
        public string Value;
    }

    static void BUILD_LOG(string info){
        Debug.Log("BUILDING PRODUCTION:  " + info);
    }

	static bool BuildProduction(string outPath,BuildTarget buildTarget,string buildtag = ""){
		string DevelopmentTag = "development";
        if(buildTarget == BuildTarget.iOS || buildTarget == BuildTarget.Android){
			if (buildtag == DevelopmentTag) {
				BuildPipeline.BuildPlayer (EditorBuildSettings.scenes, outPath, buildTarget, BuildOptions.AcceptExternalModificationsToPlayer | BuildOptions.Development|BuildOptions.ConnectWithProfiler);
			} else {
				BuildPipeline.BuildPlayer (EditorBuildSettings.scenes, outPath, buildTarget, BuildOptions.AcceptExternalModificationsToPlayer);
			}
        }
        else{
			if (buildtag == DevelopmentTag) {
				BuildPipeline.BuildPlayer (EditorBuildSettings.scenes, outPath, buildTarget, BuildOptions.Development|BuildOptions.ConnectWithProfiler);
			} else {
				BuildPipeline.BuildPlayer (EditorBuildSettings.scenes, outPath, buildTarget, BuildOptions.None);
			}
        }
        return true;
    }



    static BuildTarget WrapBuildTarget(string targetName){
        if (targetName == "iOS" || targetName == "IOS" || targetName == "ios") {
            return BuildTarget.iOS;
        } else if (targetName == "Android" || targetName == "android" || targetName == "ANDROID") {
            return BuildTarget.Android;
        } else if (targetName == "Win64" || targetName == "win64" || targetName == "WIN64") {
            return BuildTarget.StandaloneWindows64;
        } else {
            return BuildTarget.StandaloneWindows;
        }
    }



    static List<KeyValue> GetCommandLineVariables()
    {
        List<KeyValue> keyvals = new List<KeyValue>();

        foreach(string arg in System.Environment.GetCommandLineArgs()) 
        {
            if ( !arg.StartsWith("-D") )
                continue;
            int index = arg.IndexOf('=');
            if ( index < 0 )
                continue;
            KeyValue keyvalue = new KeyValue();
            keyvalue.Key = arg.Substring(2,index-2).Trim();
            keyvalue.Value = arg.Substring(index+1).Trim();
            keyvals.Add(keyvalue);
        }
        return keyvals;
    }



    static string GetKeyToValue(List<KeyValue> collection,string key,string deafult = ""){
        foreach (KeyValue value in collection) {
            if (value.Key == key) {
                return value.Value;
            }
        }
        return deafult;
    }


    static void SwitchPlatform(BuildTarget buildTarget){
        EditorUserBuildSettings.SwitchActiveBuildTarget(buildTarget);
    }


    static string GetTableValue(Hashtable table, string key) {
        var obj = table[key];
        if (obj != null)
        {
            return (obj as string).Trim();
        }
        else {
            return "";
        }
    }

    /// <summary>
    /// 设置构建参数
    /// </summary>
    static void SetGlobalBuildParam(){
        BUILD_LOG("Set Global Build Params");


        //使用Gradle集成
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.androidBuildType = AndroidBuildType.Release;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

        EditorUserBuildSettings.iOSBuildConfigType = iOSBuildType.Release;


        var setting = MiniJSON.MiniJSON.jsonDecodeFromFile(string.Format("{0}/Scripts/Editor/Production/BuildSettings.json", Application.dataPath)) as Hashtable;
        if (setting != null) {
            //SDK位置与发布机一致
            BUILD_LOG("Use BuildSettings.json to Setting Global Build Params");
            BUILD_LOG("AndroidSdkRoot:" + GetTableValue(setting, "AndroidSdkRoot"));
            BUILD_LOG("AndroidNdkRoot:" + GetTableValue(setting, "AndroidNdkRoot"));
            //BUILD_LOG("JdkPath:" + GetTableValue(setting, "AndroidJdkRoot"));

            //安卓签名
            PlayerSettings.Android.keystoreName = GetTableValue(setting, "AndroidKeystoreName");
            PlayerSettings.Android.keystorePass = GetTableValue(setting, "AndroidKeystorePass");
            PlayerSettings.Android.keyaliasName = GetTableValue(setting, "AndroidKeyaliasName");
            PlayerSettings.Android.keyaliasPass = GetTableValue(setting, "AndroidKeyaliasPass");

            EditorPrefs.SetString("AndroidSdkRoot", GetTableValue(setting, "AndroidSdkRoot"));
            EditorPrefs.SetString("AndroidNdkRoot", GetTableValue(setting, "AndroidNdkRoot"));
            //key: AndroidNdkRoot + NdkVersion
            EditorPrefs.SetString("AndroidNdkRoot"+ GetTableValue(setting, "AndroidNdkVersion"), GetTableValue(setting, "AndroidNdkRoot"));
            //EditorPrefs.SetString("JdkPath", GetTableValue(setting, "AndroidJdkRoot"));
        }
        else{
            BUILD_LOG("Can not Load BuildSettings.json to Setting Global Build Params");
        }
    }


    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// 命令行相关
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    //切换平台
    static void CommandLineSwitchPlatform(){
        string target = GetKeyToValue(GetCommandLineVariables(),"target");
        SwitchPlatform(WrapBuildTarget(target));
    }
        

    //命令行构建
    static void CommandLineBuildProduction(){
        
        List<KeyValue> commandLineArgs = GetCommandLineVariables();
        BuildTarget buildTarget = WrapBuildTarget(GetKeyToValue(commandLineArgs, "target"));
        string outPath = GetKeyToValue(commandLineArgs, "out_dir");
		string buildtag = GetKeyToValue(commandLineArgs, "build_tag");

        SetGlobalBuildParam();

        SwitchPlatform(buildTarget);
        BUILD_LOG("start to building production:" + outPath + "|" + buildtag);
        BuildProduction(outPath,buildTarget,buildtag);
        BUILD_LOG("production build over");
    }


    //命令行构建资源
    static void CommandLineBuildAsset(){
        
        List<KeyValue> commandLineArgs = GetCommandLineVariables();
        BuildTarget buildTarget = WrapBuildTarget(GetKeyToValue(commandLineArgs, "target"));
        string outPath = GetKeyToValue(commandLineArgs, "out_dir");

        SwitchPlatform(buildTarget);

        BUILD_LOG("start to building asset:" + outPath);
        AssetsBuilderTools.BuildAllBundleToPath(outPath,buildTarget);
        BUILD_LOG("asset build over");
    }


    //命令行构建替换资源
    static void CommandLineBuildRebunldeAsset()
    {

        List<KeyValue> commandLineArgs = GetCommandLineVariables();
        BuildTarget buildTarget = WrapBuildTarget(GetKeyToValue(commandLineArgs, "target"));
        string outPath = GetKeyToValue(commandLineArgs, "out_dir");
        BUILD_LOG("start to building rebundle:" + outPath);
        AssetsBuilderTools.BuildReviewRebundleAssetTo(outPath, buildTarget);
        BUILD_LOG("rebundle build over");

    }


    //命令行构建冗余代码
    static void CommandLineBuildUnuseScript()
    {
        List<KeyValue> commandLineArgs = GetCommandLineVariables();
        int count = 0;
        int.TryParse(GetKeyToValue(commandLineArgs, "count", "0"), out count);
        if (count <= 0) return;
        BUILD_LOG("start to building unused script:" + count);
        //GenScriptUtility.GenScript(count);
        BUILD_LOG("unused script build over");
    }


    //命令行方法混淆
    static void CommandLineBuildFunctionConfusion() {
        List<KeyValue> commandLineArgs = GetCommandLineVariables();
        string pathWordDB = GetKeyToValue(commandLineArgs, "worddb", "");
        BUILD_LOG("Input WordDB: " + pathWordDB);
        BUILD_LOG("start to building function confusion:");
        //GenScriptUtility.ReadWordDB(pathWordDB);
        //Refactor.RefactorMethod();
        BUILD_LOG("function confusion build over");
    }
    


}
