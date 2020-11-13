//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------
using System;
using LuaInterface;
using UnityFrame;



namespace UnityFrame
{	
	//自定义绑定，注册类
	public static class LuaCustomBinder
	{
		public static void UF_Bind(GLuaState luastate)
		{
            //注册全局方法
            UF_RegisterGFunction(luastate);
            //注册UI索引方法
            UF_RegisterUI(luastate);
			//自定义的注册类
			LuaNetwork.UF_Register(luastate);
		}


		//绑定全局方法
		static void UF_RegisterGFunction(GLuaState luastate){
			if (luastate == null)
				return ;

			IntPtr L = luastate.LuaGetL ();

			LuaDLL.tolua_pushcsfunction(L, UF_PrintTag);
			LuaDLL.lua_setglobal(L, "printt");

			LuaDLL.tolua_pushcsfunction(L, UF_PrintWarn);
			LuaDLL.lua_setglobal(L, "warn");

			LuaDLL.tolua_pushcsfunction(L, UF_PrintError);
			LuaDLL.lua_setglobal(L, "error");

            LuaDLL.tolua_pushcsfunction(L, UF_Unit64ToString);
            LuaDLL.lua_setglobal(L, "unit64_tostring");

            LuaDLL.tolua_pushcsfunction(L, UF_MonoGC);
			LuaDLL.lua_setglobal(L, "GCMono");

			LuaDLL.tolua_pushcsfunction(L, UF_TrackMsg);
			LuaDLL.lua_setglobal(L, "TrackMsg");

			LuaDLL.lua_pushboolean (L, GlobalSettings.DebugMode);
			LuaDLL.lua_setglobal(L,"DEBUG_MODE");

		}


		//绑定UI
		//绑定索引方法，在Lua中直接索引更新键与直接赋值更新键
		static void UF_RegisterUI(GLuaState L){
            L.LuaGetGlobal("_G");
            L.BeginModule("UnityFrame");
			//UIUpdateGroup
			L.BeginClass(typeof(UnityFrame.UIUpdateGroup), typeof(UnityFrame.UIObject));
			L.RegFunction (".gui", UF_get_ui);
			L.RegFunction (".sui", UF_set_ui);
            L.EndClass();
			//UIContent
			L.BeginClass(typeof(UnityFrame.UIContent), typeof(UnityFrame.UIObject));
			L.RegFunction (".gui", UF_get_ui);
			L.RegFunction (".sui", UF_set_ui);
			L.EndClass();

			L.EndModule();
		}

		///----------------------------------------------------------------------------------------------------------------------
		/// 自定义G方法
		///----------------------------------------------------------------------------------------------------------------------
		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_TrackMsg(IntPtr L){
			if (!Debugger.IsActive)
				return 0;
			int type = 0;
			int stamp = 0;
			string info = string.Empty;

			if (LuaDLL.lua_isnumber (L, 1) == 1) {
				type = (int)LuaDLL.lua_tonumber(L,1);
			}
			if (LuaDLL.lua_isstring(L, 2) == 1)
			{
				info = LuaDLL.lua_tostring(L, 2);
			}
			if (LuaDLL.lua_isnumber(L, 3) == 1)
			{
				stamp = (int)(LuaDLL.lua_tonumber(L, 3) * 1000);
			}
			Debugger.UF_TrackMsg(type,stamp,info);
			return 0;
		}

		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_MonoGC(IntPtr L){
			System.GC.Collect ();
			return 0;
		}


		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_PrintTag(IntPtr L){
			if (!Debugger.IsActive)
				return 0;

			string tag = string.Empty;

			if (LuaDLL.lua_isstring(L, 1) == 1)
			{
				tag = LuaDLL.lua_tostring(L, 1);
                tag = string.Format("<color=green>{0}</color>", tag);
            }
			else{
				tag = Debugger.TAG_LOG;
			}
			return UF_PrintInfo(L,tag,2,false);
		}
		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_PrintWarn(IntPtr L){
            if (!Debugger.IsActive)
                return 0;
            return UF_PrintInfo(L, Debugger.TAG_WARN, 1,false);
		}
		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_PrintError(IntPtr L){
			return UF_PrintInfo(L, Debugger.TAG_ERROR,1,true);
		}


		static int UF_PrintInfo(IntPtr L,string tag,int start,bool tracestack)
		{
			int n = LuaDLL.lua_gettop(L);
			System.Text.StringBuilder sb = StringBuilderCache.Acquire();
			try
			{
				#if UNITY_EDITOR
				int line = LuaDLL.tolua_where(L, 1);
				string filename = LuaDLL.lua_tostring(L, -1);
				LuaDLL.lua_settop(L, n);

				if (!filename.Contains("."))
				{
					sb.AppendFormat("[{0}.lua:{1}]:", filename, line);
				}
				else
				{
					sb.AppendFormat("[{0}:{1}]:", filename, line);
				}
				#endif

				for (int i = start; i <= n; i++)
				{
					if (i > 1) sb.Append("    ");

					if (LuaDLL.lua_isstring(L, i) == 1)
					{
						sb.Append(LuaDLL.lua_tostring(L, i));
					}
					else if (LuaDLL.lua_isnil(L, i))
					{
						sb.Append("nil");
					}
					else if (LuaDLL.lua_isboolean(L, i))
					{
						sb.Append(LuaDLL.lua_toboolean(L, i) ? "true" : "false");
					}
					else
					{
						IntPtr p = LuaDLL.lua_topointer(L, i);

						if (p == IntPtr.Zero)
						{
							sb.Append("nil");
						}
						else
						{
							sb.AppendFormat("{0}:0x{1}", LuaDLL.luaL_typename(L, i), p.ToString("X"));
						}
					}
				}
				if(tracestack){
                    int top = LuaDLL.lua_gettop(L);
                    LuaDLL.lua_getglobal(L,"debug");
					LuaDLL.lua_getfield(L,-1,"traceback");
					LuaDLL.lua_pcall(L,0,1,0);
                    string stackmsg = LuaDLL.lua_tostring(L, -1);
                    LuaDLL.lua_settop(L, top);
                    sb.AppendLine();
					sb.Append(stackmsg);
				}
				return 0;
			}
			catch (Exception e)
			{
				return LuaDLL.toluaL_exception(L, e);
			}
			finally{
				LuaDLL.lua_settop(L,n);		

				#if UNITY_EDITOR
				switch(tag){
				case Debugger.TAG_LOG:UnityEngine.Debug.Log(StringBuilderCache.GetStringAndRelease(sb));break;
				case Debugger.TAG_ERROR:UnityEngine.Debug.LogError(StringBuilderCache.GetStringAndRelease(sb));break;
				case Debugger.TAG_WARN:UnityEngine.Debug.LogWarning(StringBuilderCache.GetStringAndRelease(sb));break;
				default:UnityEngine.Debug.Log(string.Format("[{0}]{1}",tag,StringBuilderCache.GetStringAndRelease(sb)));break;
				}
				#endif
				Debugger.UF_LogTag(tag,StringBuilderCache.GetStringAndRelease(sb));
			}


		}

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int UF_Unit64ToString(IntPtr L)
        {
             if (LuaDLL.lua_isnumber(L, 1) == 1) {
                ulong val = (ulong)LuaDLL.lua_tonumber(L, 1);
                string strVal = val.ToString();
                LuaDLL.lua_pushstring(L, strVal);
            }
            else if(LuaDLL.lua_isstring(L, 1) == 1)
            {
                string val = LuaDLL.lua_tostring(L, 1);
                LuaDLL.lua_pushstring(L, val);
            }
            else
            {
                LuaDLL.lua_pushnil(L);
            }

            return 1;
        }


        ///----------------------------------------------------------------------------------------------------------------------
        /// UI 索引方法
        ///----------------------------------------------------------------------------------------------------------------------

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_get_ui(IntPtr L)
		{
			try
			{
				ToLua.CheckArgsCount(L, 2);
				IUIUpdateGroup target = (IUIUpdateGroup)ToLua.CheckObject(L, 1, typeof(IUIUpdateGroup));
				if(target != null){
					string key = ToLua.CheckString(L, 2);
					//更新键不能与原对象成员变量重名，否则会被覆盖
					ToLua.Push(L, target.UF_GetUI(key));
				}
				else{
					LuaDLL.lua_pushnil(L);
				}
				return 1;
			}
			catch(Exception e)
			{
				return LuaDLL.toluaL_exception(L, e);
			}
		}

		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_set_ui(IntPtr L)
		{
			try
			{
				ToLua.CheckArgsCount(L, 3);
				bool ret = false;
				IUIUpdateGroup target = (IUIUpdateGroup)ToLua.CheckObject(L, 1, typeof(IUIUpdateGroup));
				if(target != null){
					string key = LuaDLL.lua_tostring(L, 2);
					IUIUpdate ui = target.UF_GetUI(key);
					if(ui != null){
						ret = true;
						//判断是否真实是bool 值，去除nil
						if(LuaDLL.lua_isrboolean(L,3)){
							bool active = LuaDLL.lua_toboolean(L, 3);
							ui.UF_SetActive(active);
						}
						else{
							object value = ToLua.ToVarObject(L, 3);
							ui.UF_SetValue(value);
						}
					}
				}
				LuaDLL.lua_pushboolean(L,ret);
				return 1;
			}
			catch(Exception e)
			{
				return LuaDLL.toluaL_exception(L, e);
			}
		}

	}
}

