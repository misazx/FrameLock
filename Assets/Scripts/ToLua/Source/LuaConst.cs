using UnityEngine;

public static class LuaConst
{


	#if UNITY_EDITOR
		public static string luaDir = Application.dataPath + "/Runtimes/Lua";                //lua逻辑代码目录
		public static string toluaDir = Application.dataPath + "/Runtimes/To"+"Lua";        //tolua lua文件目录
		public static string luaRuntime = Application.dataPath + "/Runtimes";				
	#elif UNITY_STANDALONE
		public static string luaDir = Application.dataPath + "/Resources/Runtimes/Lua";                //lua逻辑代码目录
		public static string toluaDir = Application.dataPath + "/Resources/Runtimes/To"+"Lua/";        //tolua lua文件目录
		public static string luaRuntime = Application.dataPath + "/Resources/Runtimes";			
	#else
		public static string luaDir = Application.persistentDataPath + "/Runtimes/Lua";                //lua逻辑代码目录
		public static string toluaDir = Application.persistentDataPath + "/Runtimes/To"+"Lua/";        //tolua lua文件目录
		public static string luaRuntime = Application.persistentDataPath + "/Runtimes";		
	#endif

	public static string toluaCoreDir = Application.dataPath + "/Scripts/To"+"Lua";	//tolua core代码目录

#if UNITY_STANDALONE
    public static string osDir = "Win";
#elif UNITY_ANDROID
    public static string osDir = "Android";            
#elif UNITY_IPHONE
    public static string osDir = "iOS";        
#else
    public static string osDir = "";        
#endif 

    public static string luaResDir = string.Format("{0}/{1}/Lua", Application.persistentDataPath, osDir);      //手机运行时lua文件下载目录    

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN    
    public static string zbsDir = "D:/ZeroBraneStudio/lualibs/mobdebug";        //ZeroBraneStudio目录       
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
	public static string zbsDir = "/Applications/ZeroBraneStudio.app/Contents/ZeroBraneStudio/lualibs/mobdebug";
#else
    public static string zbsDir = luaResDir + "/mobdebug/";
#endif    

	public static bool openLuaSocket = false;            //是否打开Lua Socket库
    public static bool openZbsDebugger = false;         //是否连接ZeroBraneStudio调试
}