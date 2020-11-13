using UnityEngine;
using System.Collections;
using UnityEditor.Callbacks;
using UnityEditor;
using System.IO;
using System;

public class ProductionDebug {
	[PostProcessBuildAttribute()]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
	{
		if (target == BuildTarget.Android)
			PostProcessAndroidBuild(pathToBuiltProject);
	}

	public static void PostProcessAndroidBuild(string pathToBuiltProject)
	{
		UnityEditor.ScriptingImplementation backend = (UnityEditor.ScriptingImplementation)UnityEditor.PlayerSettings.GetScriptingBackend(UnityEditor.BuildTargetGroup.Android);

		if (backend == UnityEditor.ScriptingImplementation.IL2CPP)
		{
			CopyAndroidIL2CPPSymbols(pathToBuiltProject, PlayerSettings.Android.targetArchitectures);
		}
	}

	public static void CopyAndroidIL2CPPSymbols(string pathToBuiltProject, AndroidArchitecture targetDevice)
	{
//		string buildName = Path.GetFileNameWithoutExtension(pathToBuiltProject);
//		FileInfo fileInfo = new FileInfo(pathToBuiltProject);
//		string symbolsDir = fileInfo.Directory.Name;
//		symbolsDir = symbolsDir + "/"+buildName+"_IL2CPPSymbols";
		string symbolsDir = pathToBuiltProject + "/../../IL2CPPSymbols";

		CreateDir(symbolsDir);

		switch (PlayerSettings.Android.targetArchitectures)
		{
            case AndroidArchitecture.All:
                {
                    CopyARM64Symbols(symbolsDir);
                    CopyARMv7Symbols(symbolsDir);
                    CopyX86Symbols(symbolsDir);
                    break;
                }
            case AndroidArchitecture.ARM64:
                {
                    CopyARM64Symbols(symbolsDir);
                    break;
                }
            case AndroidArchitecture.ARMv7:
                {
                    CopyARMv7Symbols(symbolsDir);
                    break;
                }

            default:
                break;
        }
	}

    

    const string libpath = "/../Temp/StagingArea/libs/";
	const string libFilename = "libil2cpp.so.debug";

    private static void CopyARM64Symbols(string symbolsDir)
    {
        try
        {
            string sourcefileARM = Application.dataPath + libpath + "arm64-v8a/" + libFilename;
            CreateDir(symbolsDir + "/arm64-v8a/", true);
            File.Copy(sourcefileARM, symbolsDir + "/arm64-v8a/libil2cpp.so.debug");
        }
        catch (SystemException e) {
            Debug.LogException(e);
        }
    }

    private static void CopyARMv7Symbols(string symbolsDir)
	{
        try
        {
            string sourcefileARM = Application.dataPath + libpath + "armeabi-v7a/" + libFilename;
		    CreateDir(symbolsDir + "/armeabi-v7a/",true);
		    File.Copy(sourcefileARM, symbolsDir + "/armeabi-v7a/libil2cpp.so.debug");
        }
        catch (SystemException e)
        {
            Debug.LogException(e);
        }
    }

	private static void CopyX86Symbols(string symbolsDir)
	{
        try
        {
            string sourcefileX86 = Application.dataPath + libpath + "x86/" + libFilename;
            CreateDir(symbolsDir + "/x86/", true);
            File.Copy(sourcefileX86, symbolsDir + "/x86/libil2cpp.so.debug");
        }
        catch (SystemException e)
        {
            Debug.LogException(e);
        }
    }

	public static void CreateDir(string path,bool newCreate = false)
	{
		if (Directory.Exists (path)) {
			if (newCreate) {
				Directory.Delete (path, true);
			} else {
				return;
			}
		}
		Directory.CreateDirectory(path);
	}

}
