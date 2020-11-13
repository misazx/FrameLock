//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System;
using System.IO;
using LuaInterface;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;

namespace UnityFrame
{
	public class LuaFramework:HandleSingleton<LuaFramework>,IOnAwake,IOnUpdate,IOnSyncUpdate,IOnStart,IOnApplicationPause,IOnApplicationQuit,IOnReset{

		private GLuaState m_Luastate;

		private bool m_IsInited = false;

		private const string MODULE_NAME = "Modules";

		private static string LuaRoot{
			get{ 
				if (GlobalSettings.IsAppCheck && GlobalSettings.IsRawAsset) {
					return GlobalPath.RawScriptPath;
				} else {
					return GlobalPath.ScriptPath;
				}
			}
		}
		//逻辑目录路径
		private string LuaRuntimePath{get{return LuaRoot + "Lua";}}
			
		Dictionary<string,DirectoryInfo> m_ModulePackages = new Dictionary<string, DirectoryInfo> ();

		private LuaFunction lfHandleNetMsg = null;

		private LuaFunction lfHandleEventMsg = null;

		public bool isInited{get{ return m_IsInited;}}

		public LuaFunction LFHandleEventMsg{get{ return lfHandleEventMsg;}}

		public LuaFunction LFHandleNetMsg{get{ return lfHandleNetMsg;}}

		public void UF_CallLuaFunction(string method){
			if (m_Luastate != null) {
				LuaFunction func = m_Luastate.GetFunction(method);
				if (func != null) {
					func.Call ();
					func.Dispose ();
					func = null;
				}
			}
		}


        private bool UF_DoFile(IntPtr L,string filePath, string fileName)
        {
#if UNITY_EDITOR
            return LuaDLL.tolua_dofilebuffer(L,filePath, fileName) == 0;
#else
            //审核模式读取Raw中资源,并且需要字节偏移
            int reskey = GlobalSettings.IsAppCheck && GlobalSettings.IsRawAsset ? GlobalSettings.ResBKey : 0;
            return LuaDLL.glua_dofilebuffer(L, filePath, fileName, GlobalSettings.EncBKey, reskey) == 0;
#endif
        }


		private void UF_LuaBaseStart(GLuaState luastate){
			if (luastate != null) {
				string toluaroot = LuaRoot;

				string[] luafiles= {
                    "Main.lua",

                    "Mathf.lua",        //ToLua/UnityEngine/Mathf.lua
                    "Vector3.lua",      //ToLua/UnityEngine/Vector3.lua
                    "Quaternion.lua",   //ToLua/UnityEngine/Quaternion.lua
                    "Vector2.lua",      //ToLua/UnityEngine/Vector2.lua
                    "Vector4.lua",      //ToLua/UnityEngine/Vector4.lua
                    "Color.lua",        //ToLua/UnityEngine/Color.lua
                    "Ray.lua",          //ToLua/UnityEngine/Ray.lua
                    "Bounds.lua",       //ToLua/UnityEngine/Bounds.lua
                    "RaycastHit.lua",   //ToLua/UnityEngine/RaycastHit.lua
                    "Touch.lua",        //ToLua/UnityEngine/Touch.lua
                    "LayerMask.lua",    //ToLua/UnityEngine/LayerMask.lua
                    "Plane.lua",        //ToLua/UnityEngine/Plane.lua
                    "TypeOf.lua",       //ToLua/System/TypeOf.lua
                    "ValueType.lua",    //ToLua/System/ValueType.lua
                    "BindingFlags.lua", //ToLua/System/BindingFlags.lua
				};

                int top = m_Luastate.LuaGetTop();
                for (int k = 0; k < luafiles.Length; k++) {
                    var assetinfo = AssetDataBases.UF_GetAssetInfo(luafiles[k]);
                    if (assetinfo == default(AssetDataBases.AssetFileInfo)) {
                        throw new LuaException(string.Format("Can not get lua file[{0}] in AssetDataBase,Load file failed!", luafiles[k]), LuaException.GetLastError());
                    }
                    if (!UF_DoFile(m_Luastate.LuaGetL(), assetinfo.path, Path.GetFileNameWithoutExtension(assetinfo.name)))
                    {
                        string err = m_Luastate.LuaToString(-1);
                        m_Luastate.LuaSetTop(top);
                        throw new LuaException(err, LuaException.GetLastError());
                    }
                }
                m_Luastate.LuaSetTop(top);

                //打开基础库
                LuaUnityLibs.OpenLuaLibs(luastate.LuaGetL());

				luastate.Start();

			}
		}

		//加载全部Lua模块
		internal IEnumerator UF_InitFramework(){
			if (!m_IsInited) {
				Debugger.UF_Log ("LuaFramework  ->  Start");
				m_Luastate = new GLuaState ();

                //第三方库
                //m_Luastate.OpenLibs(LuaDLL.luaopen_struct);
                m_Luastate.OpenLibs(LuaDLL.luaopen_lpeg);
                //m_Luastate.OpenLibs(LuaDLL.luaopen_bit);
                m_Luastate.LuaSetTop(0);

                //加载基础库
                UF_LuaBaseStart(m_Luastate);

				//静态绑定
				LuaBinder.Bind(m_Luastate);

				//自定义绑定
				LuaCustomBinder.UF_Bind(m_Luastate);

                yield return null;
				//开启一个加载线程加载全部Runtime.Lua模块
				Thread thread = new Thread(new ThreadStart(UF_InitLuaModules));
				thread.Start ();
				//主线程等待全部Lua加载
				while (!m_IsInited) {
					yield return null;
				}
				yield return null;
				m_Luastate.Collect ();
				System.GC.Collect ();
				yield return null;

				//获取固有方法
				lfHandleNetMsg = m_Luastate.GetFunction("OnHandleProtocol");
				lfHandleEventMsg = m_Luastate.GetFunction("OnHandleEvent");

                //模块加载完成，主函数启动
                UF_CallLuaFunction("MainAwake");
				yield return null;
			} else {
				Debugger.UF_Warn ("LuaFramework is already Init");
			}

			yield break;
		}


		private void UF_InitLuaModules(){
			int tickstart = System.Environment.TickCount;
            //获取所有Lua代码资源文件
            var listRuntimes = AssetDataBases.UF_GetAllRuntimeInfos();

            //创建模块包文件
            UF_LoadLuaModulesPackages(listRuntimes);

            //加载所有模块全部lua文件
            UF_LoadLuaModulesFiles(listRuntimes);

			int tickend = System.Environment.TickCount;

			Debugger.UF_Log (string.Format ("InitLuaModules Use Time: {0}", Math.Abs (tickend - tickstart)));

			m_IsInited = true;
		}
			
		private void UF_HandleLuaEvent(string eventId,object param = null){
			if (string.IsNullOrEmpty (eventId)) {
				Debugger.UF_Error ("HandleLuaEvent ,eventId is null,call lua function failed");
				return;
			}
			if (lfHandleEventMsg == null) {
				Debugger.UF_Error ("HandleLuaEvent,lua call function is null");
				return;
			}
			try {
				lfHandleEventMsg.BeginPCall ();
				lfHandleEventMsg.Push (eventId);		
				if(param != null)
					lfHandleEventMsg.Push (param);
				lfHandleEventMsg.PCall ();
				lfHandleEventMsg.EndPCall ();
			} catch (System.Exception e) {
				Debugger.UF_Exception (e);
				lfHandleEventMsg.EndPCall ();
			}
		}

		private void UF_HandleLuaEvent(object[] args){
			if (lfHandleEventMsg == null) {
				Debugger.UF_Error ("HandleLuaEvent,lua call function is null");
				return;
			}
			if (args == null || args.Length == 0) {
				Debugger.UF_Error ("HandleLuaEvent,call args is null or empty");
				return;
			}
			try {
				lfHandleEventMsg.BeginPCall ();
				if (args != null && args.Length > 0) {
					for (int k = 0; k < args.Length; k++) {
						lfHandleEventMsg.Push (args [k]);			
					}
				}
				lfHandleEventMsg.PCall ();
				lfHandleEventMsg.EndPCall ();
			} catch (System.Exception e) {
				Debugger.UF_Exception (e);
				lfHandleEventMsg.EndPCall ();
			}
		}

		private void UF_HandleLuaEvent(string eventId,object[] args){
			if (string.IsNullOrEmpty (eventId)) {
				Debugger.UF_Error ("HandleLuaEvent ,eventId is null,call lua function failed");
				return;
			}
			if (lfHandleEventMsg == null) {
				Debugger.UF_Error ("HandleLuaEvent,lua call function is null");
				return;
			}
				
			if (args == null || args.Length == 0) {
				Debugger.UF_Error ("HandleLuaEvent,call args is null or empty");
				return;
			}
			try {
				lfHandleEventMsg.BeginPCall ();
				lfHandleEventMsg.Push (eventId);
				if (args != null && args.Length > 0) {
					for (int k = 0; k < args.Length; k++) {
						lfHandleEventMsg.Push (args [k]);			
					}
				}
				lfHandleEventMsg.PCall ();
				lfHandleEventMsg.EndPCall ();
			} catch (System.Exception e) {
				Debugger.UF_Exception (e);
				lfHandleEventMsg.EndPCall ();
			}
		}

        //所有模块的包先被加载
        //所有Lua中的文件夹定义为一个包
        private void UF_LoadLuaModulesPackages(List<AssetDataBases.AssetFileInfo> list)
        {
            if (m_Luastate == null || list == null){return;}

            HashSet<string> hashmap = new HashSet<string>();
            //遍历全部的资源信息文件，获取其相对路径
            foreach (var v in list) {
                string dirName = GHelper.UF_GetDirectoryName(v.absName);
                if (!hashmap.Contains(dirName)) {
                    hashmap.Add(dirName);
                }
            }
            int top = m_Luastate.LuaGetTop();
            try
            {
                //设置module 记录包路径模块
                m_Luastate.LuaCreateTable();
                m_Luastate.LuaPushValue(-1);
                m_Luastate.LuaSetGlobal(MODULE_NAME);

                //按层级创建talbe pacakge
                foreach (var v_path in hashmap)
                {
                    int s_top = m_Luastate.LuaGetTop();
                    foreach (var p_name in GHelper.UF_SplitString(v_path, '/')) {
                        if (string.IsNullOrEmpty(p_name)) continue;
                        if (p_name == "Runtimes") {
                            //Runtimes作为全局根结点
                            m_Luastate.LuaGetGlobal("_G");
                            continue;
                        }
                        m_Luastate.LuaGetField(-1, p_name);
                        if (m_Luastate.LuaIsNil(-1)) {
                            m_Luastate.LuaRemove(-1);
                            m_Luastate.LuaCreateTable();
                            m_Luastate.LuaPushValue(-1);
                            m_Luastate.LuaSetField(-3, p_name);
                        }
                    }
                    //设置到Module中记录
                    m_Luastate.LuaGetGlobal(MODULE_NAME);
                    m_Luastate.LuaPushValue(-2);
                    m_Luastate.LuaSetField(-2, v_path);
                    m_Luastate.LuaSetTop(s_top);
                }
            }
            catch (Exception e)
            {
                Debugger.UF_Exception(e);
            }

            m_Luastate.LuaSetTop(top);
        }

        static string[] s_PackageDefine = {
                    "Lua/data",
                    "Lua/define",
                    "Lua/tool",
                    "Lua/protols",
                    "Lua/global",
                    "Lua/struct",
                    "Lua/model",
                    "Lua/work",
                };


        static int UF_GetPackageFileWeight(string n) {
            for (int k = 0; k < s_PackageDefine.Length; k++) {
                if (n.IndexOf(s_PackageDefine[k], StringComparison.Ordinal) > -1) {
                    return k;
                }
            }
            return 100;
        }


        //加载所有模块全部lua文件
        private void UF_LoadLuaModulesFiles(List<AssetDataBases.AssetFileInfo> list)
        {
            if (m_Luastate == null || list == null) { return; }

            int top = m_Luastate.LuaGetTop();

            try
            {
                //packages 作为排序权重
                list.Sort((a, b) => { return UF_GetPackageFileWeight(a.absName) < UF_GetPackageFileWeight(b.absName) ? -1 : 1; });

                m_Luastate.LuaGetGlobal(MODULE_NAME);
                //开始加载文件
                foreach (var v in list) {
                    if (string.IsNullOrEmpty(v.path)) { continue; }
                    //只加载Lua包中文件
                    if (!(v.absName.IndexOf("Runtimes/Lua/", StringComparison.Ordinal) > -1)) { continue;}
                    int s_top = m_Luastate.LuaGetTop();
                    string pkgName = GHelper.UF_GetDirectoryName(v.absName);
                    m_Luastate.LuaGetField(-1, pkgName);
                    if (m_Luastate.LuaIsNil(-1))
                    {
                        Debugger.UF_Error(string.Format("Can not find Module[{0}],Load file[{1}] invalid!", pkgName, v.name));
                        m_Luastate.LuaSetTop(s_top);
                        continue;
                        //throw new LuaException(string.Format("Can not find Module[{0}],Load file[{1}] invalid!", pkgName, v.name), LuaException.GetLastError());
                    }
                    else {
                        int curTop = m_Luastate.LuaGetTop();
                        string r_name = Path.GetFileNameWithoutExtension(v.name);
                        if (!UF_DoFile(m_Luastate.LuaGetL(), v.path, r_name))
                        {

                            Debugger.UF_Error(string.Format("Load Lua Error:{0}", m_Luastate.LuaToString(-1)));
                            m_Luastate.LuaSetTop(s_top);
                            continue;
                            //throw new LuaException(m_Luastate.LuaToString(-1), LuaException.GetLastError());
                        }
                        if (m_Luastate.LuaGetTop() <= curTop)
                        {
                            Debugger.UF_Error(string.Format("Lua File[ {0} ] Not Return Any Value,Load Invaild", v.name));
                            m_Luastate.LuaSetTop(s_top);
                            continue;
                            //throw new LuaException(string.Format("Lua File[ {0} ] Not Return Any Value,Load Invaild", v.name), LuaException.GetLastError());
                        }
                        //添加到对应到包模块中
                        m_Luastate.LuaSetField(-2, r_name);
                    }
                    m_Luastate.LuaSetTop(s_top);
                }

            }
            catch (Exception e)
            {
                //模块无法成功加载
                Debugger.UF_Exception(e);
            }
            m_Luastate.LuaSetTop(top);
        }


		public void UF_LuaGC(){
			if (m_Luastate != null) {
				m_Luastate.LuaGC (LuaGCOptions.LUA_GCCOLLECT);
			}
		}

		private void UF_UpdateMouseInput(){
			if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Escape)) {
                UF_HandleLuaEvent(DefineLuaEvent.E_INPUT_ESCAPE);
			}
		}

		public void UF_OnUpdate(){
			if (m_IsInited) {
				//协议更新
				LuaNetwork.UF_Update(lfHandleNetMsg);
				if (m_Luastate.LuaUpdate (GTime.DeltaTime, GTime.UnscaleDeltaTime) != 0) {
					string error = m_Luastate.LuaToString (-1);
					m_Luastate.LuaPop (2);                
					throw new LuaException (error, LuaException.GetLastError ());
				}
				m_Luastate.LuaPop (1);
				m_Luastate.Collect ();

                UF_UpdateMouseInput();
			}
		}

        public void UF_OnSyncUpdate() {
            if (m_IsInited) {
                if (m_Luastate.LuaFixedUpdate(GTime.RunDeltaTime) != 0) {
                    string error = m_Luastate.LuaToString(-1);
                    m_Luastate.LuaPop(2);
                    throw new LuaException(error, LuaException.GetLastError());
                }
                m_Luastate.LuaPop(1);
            }
        }


        private void UF_OnEventCorotineState(object[] args)
        {
            if (m_IsInited)
                UF_HandleLuaEvent(DefineLuaEvent.E_CS_COROUTINE_STATE, args);
        }

        private void UF_OnEventUIShow(object[] args){ UF_HandleLuaEvent(DefineLuaEvent.E_VIEW_ON_SHOW,args);}
		private void UF_OnEventUIClose(object[] args){ UF_HandleLuaEvent(DefineLuaEvent.E_VIEW_ON_CLOSE,args);}
		private void UF_OnEventConnectionState(object[] args){ UF_HandleLuaEvent(DefineLuaEvent.E_CONNECTION_STATE_CHANGE,args);}

		private void UF_OnEventAnimatorClip(object[] args){ UF_HandleLuaEvent(DefineLuaEvent.E_ANIMATION_CLIP,args);}

        private void UF_OnEventPerformActionClip(object[] args) { UF_HandleLuaEvent(DefineLuaEvent.E_PERFORM_ACTION_CLIP, args); }

        private void UF_OnEventAvatarTrigger(object[] args){ UF_HandleLuaEvent(DefineLuaEvent.E_AVATAR_TRIGGER, args);}
        private void UF_OnEventAvatarCollision(object[] args) { UF_HandleLuaEvent(DefineLuaEvent.E_AVATAR_COLLISION, args); }
        private void UF_OnEventAvatarBlock(object[] args) { UF_HandleLuaEvent(DefineLuaEvent.E_AVATAR_BLOCK, args); }
        private void UF_OnEventDipCollision(object[] args) { UF_HandleLuaEvent(DefineLuaEvent.E_DIP_COLLISION, args); }
        
        public void UF_OnAwake() {
            //注册消息监听，转发到消息到 Lua 中
            var msgSys = MessageSystem.UF_GetInstance();
            msgSys.UF_AddListener(DefineEvent.E_LUA, UF_HandleLuaEvent);
            msgSys.UF_AddListener(DefineEvent.E_COROUTINE_STATE, UF_OnEventCorotineState);
            msgSys.UF_AddListener(DefineEvent.E_NET_CONNECT_STATE, UF_OnEventConnectionState);

            msgSys.UF_AddListener(DefineEvent.E_UI_OPERA, UF_HandleLuaEvent);
            msgSys.UF_AddListener(DefineEvent.E_UI_SHOW, UF_OnEventUIShow);
            msgSys.UF_AddListener(DefineEvent.E_UI_CLOSE, UF_OnEventUIClose);
            
            msgSys.UF_AddListener(DefineEvent.E_ANIMATION_CLIP, UF_OnEventAnimatorClip);
            msgSys.UF_AddListener(DefineEvent.E_PERFORM_ACTION_CLIP, UF_OnEventPerformActionClip);
            

            msgSys.UF_AddListener(DefineEvent.E_AVATAR_TRIGGER, UF_OnEventAvatarTrigger);
            msgSys.UF_AddListener(DefineEvent.E_AVATAR_COLLISION, UF_OnEventAvatarCollision);
            msgSys.UF_AddListener(DefineEvent.E_AVATAR_BLOCK, UF_OnEventAvatarBlock);
            msgSys.UF_AddListener(DefineEvent.E_DIP_COLLISION, UF_OnEventDipCollision);

            msgSys.UF_AddListener(DefineEvent.E_TRIGGER_CONTROLLER, UF_HandleLuaEvent);
        }

		public void UF_OnStart(){
            //调用Start 函数启动
            UF_CallLuaFunction("MainStart");
		}

		public void OnApplicationPause(bool state){
			if (state) {
                UF_LuaGC();
			}
			if (isInited) {
                UF_HandleLuaEvent(DefineLuaEvent.E_APP_BACKGROUND_STATE, state);
			}
		}

		public void OnApplicationQuit(){
            UF_OnReset();
		}
        
		public void UF_OnReset(){
			if(m_Luastate != null)
				m_Luastate.Dispose ();
			m_IsInited = false;
            lfHandleEventMsg = null;
            lfHandleNetMsg = null;
            m_Luastate = null;
			System.GC.Collect ();
		}

	}
}

