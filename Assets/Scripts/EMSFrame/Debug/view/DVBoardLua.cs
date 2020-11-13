//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using LuaInterface;

namespace UnityFrame
{
	public class DVBoardLua : IDebugViewBoard
	{
		const int TAG_INFO = 0;
		const int TAG_EVNET = 1;

		private string m_LogInfo = string.Empty;

		private int m_CurrentTag;

		public void UF_DrawDetail (Rect rect){
			GUILayout.Space (4);
			if (GUILayout.Button ("Trace Global",GUILayout.Height(40))) {
				m_LogInfo = UF_GetLuaGInfo();
				m_CurrentTag = TAG_INFO;
			}
			GUILayout.Space (4);
			if (GUILayout.Button ("Ref CS Object",GUILayout.Height(40))) {
				m_LogInfo = UF_GetLuaRefCSObject();
				m_CurrentTag = TAG_INFO;
			}
			GUILayout.Space (4);
			if (GUILayout.Button ("Ref Unity Object",GUILayout.Height(40))) {
				m_LogInfo = UF_GetLuaRefUnityObject();
				m_CurrentTag = TAG_INFO;
			}

			GUILayout.Space (4);
			if (GUILayout.Button ("Lua Event Track",GUILayout.Height(40))) {
				m_CurrentTag = TAG_EVNET;
                UF_DebugEvent();
			}

			GUILayout.Space (4);
		}


		private string UF_TrackMsgForeach(string info,int stamp){
			if (stamp > 100) {
				info = string.Format ("<color=red>{0}   -> cost: {1}</color>", info, stamp);
			} else if (stamp > 33) {
				info = string.Format ("<color=yellow>{0}   -> cost: {1}</color>", info, stamp);
			} else if (stamp < 0) {
				info = "-------------------------------------------------------------------------------------------------------------";
			}
			else {
				info = string.Format ("{0} -> cost: {1}", info,stamp);
			} 
			return info;
		}


		public void UF_DrawInfo(Rect rect){
			if (m_CurrentTag == TAG_INFO) {
				GUILayout.Label (m_LogInfo, GUILayout.Width (rect.width));
			}
			else if (m_CurrentTag == TAG_EVNET) {
				if (Debugger.UF_GetInstance ().MsgTrackers.ContainsKey (Debugger.TRACK_LUA_EVENT)) {
					MsgTracker tracker = Debugger.UF_GetInstance ().MsgTrackers [Debugger.TRACK_LUA_EVENT];
					GUILayout.Label (tracker.UF_ForeachToString(UF_TrackMsgForeach));
				}
			}
		}



		private string UF_GetLuaRefCSObject(){
			ObjectTranslator objectTranslator = GLuaState.GetTranslator(System.IntPtr.Zero);
			Dictionary<System.Type,int> dicRefState = new Dictionary<System.Type, int> ();
			List<KeyValuePair<System.Type,int>> lstSort = new List<KeyValuePair<System.Type, int>> ();
			foreach (KeyValuePair<object,int> item in objectTranslator.objectsBackMap) {
				System.Type t = item.Key.GetType();
				if (dicRefState.ContainsKey (t)) {
					dicRefState [t] += 1;
				} else {
					dicRefState.Add (t, 1);
				}
			}
			foreach (var item in dicRefState) {
				lstSort.Add (item);
			}

			lstSort.Sort ((x, y) => -x.Value.CompareTo(y.Value));

			string msg = "";
			foreach (var each in lstSort)
			{
				msg += each.Key.ToString() + " : " + each.Value.ToString() + "\n";
			}
			return msg;
		}

		private string UF_GetLuaRefUnityObject(){
			string msg = "";
			ObjectTranslator objectTranslator = GLuaState.GetTranslator(System.IntPtr.Zero);

			Dictionary<string,int> mDicUnityObject = new Dictionary<string, int> ();

			foreach (KeyValuePair<object,int> item in objectTranslator.objectsBackMap) {
				Object uobj = item.Key as Object;
				if(uobj != null){
					string key = uobj.name + "(" + uobj.GetType ().ToString () + ")";
					if (!mDicUnityObject.ContainsKey (key)) {
						mDicUnityObject.Add (key, 1);
					} else {
						mDicUnityObject [key] += 1;
					}
				}
			}
			foreach (KeyValuePair<string,int> item in mDicUnityObject) {
				msg = msg + item.Key + "   |   " + item.Value + "\n";
			}
			return msg;
		}


		private void UF_DebugEvent(){
			GLuaState lua = GLuaState.Get (System.IntPtr.Zero);
			int oldtop = lua.LuaGetTop ();
			try{
				lua.LuaGetGlobal ("Lua");
				if (lua.LuaIsNil (-1) || !lua.lua_istable(-1)) {
					return;
				}
				lua.LuaGetField(-1,"global");
				if (lua.LuaIsNil (-1) || !lua.lua_istable(-1)) {
					return;
				}
				lua.LuaGetField(-1,"event");
				if (lua.LuaIsNil (-1) || !lua.lua_istable(-1)) {
					return;
				}
				lua.LuaGetField(-1,"debug_event");
				if (lua.LuaIsNil (-1) || !lua.lua_isfunction(-1)) {
					return;
				}
				lua.Push (true);
				lua.LuaCall (1, 0);
			}catch(System.Exception ex){
				Debugger.UF_Exception (ex);
			}
			lua.LuaSetTop (oldtop);
		}

		private string UF_GetLuaGInfo(){
			GLuaState lua = GLuaState.Get (System.IntPtr.Zero);
			int oldtop = lua.LuaGetTop ();
			lua.LuaGetGlobal ("TraceGlobal");
			string info = string.Empty;
			if (!lua.LuaIsNil (-1) && lua.lua_isfunction(-1)) {
				lua.LuaCall (0, 1);
				info = lua.LuaToString (-1);
			}
			lua.LuaSetTop (oldtop);
			return info;
		}

	}
}

