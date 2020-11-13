//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace UnityFrame {
	public class Debugger : HandleSingleton<Debugger>,IOnAwake,IOnUpdate,IOnGUI,IOnApplicationQuit {

		public const string TAG_LOG = "Log";
		public const string TAG_ERROR = "Error";
		public const string TAG_WARN = "Warn";
		public const string TAG_EXCEPTION = "Exception";

		public const int MAX_LOGGER_RECORD = 15000;

		public const int TRACK_LUA_EVENT = 10;
		public const int TRACK_RES_LOAD = 20;
		public const int TRACK_NET_PROTO = 30;

//#if UNITY_EDITOR
//        public static bool IsActive = true;
//#else
//		public static bool IsActive = false;
//#endif
		public static bool IsActive = true;


		//loger
		public MsgLoger logger{get{return m_Loger;}}
		private MsgLoger m_Loger = new MsgLoger (MAX_LOGGER_RECORD);

		private ConsoleView m_ConsoleView = new ConsoleView();

		//信息跟踪
		public Dictionary<int,MsgTracker> MsgTrackers{get{ return m_MsgTrackers;}}

		private Dictionary<int,MsgTracker> m_MsgTrackers = new Dictionary<int, MsgTracker>();
		//记录信息
		public Dictionary<string,string> MsgRecords{get{return m_DicMsgRecord;}}

		private Dictionary<string,string> m_DicMsgRecord = new Dictionary<string, string>();

		private string NowDataTime{get{ return GTime.DateHMS;}}


		public void UF_OnAwake()
		{
#if !UNITY_EDITOR
			Application.RegisterLogCallback (UF_OnDebugLogCallBack);
#endif
            string taglogstart = string.Format("\n\n\n------------------New Console Log At :[{0}]------------------\n",this.NowDataTime);

            UF_LogMessage(TAG_LOG,taglogstart);

			m_ConsoleView.UF_OnStart ();
		}

		public void OnGUI(){
			if(IsActive)
				m_ConsoleView.UF_OnDraw();

		}

		public void UF_OnUpdate(){

            UF_UpdateUnLockDebugger();

			#if UNITY_EDITOR 
			if(Input.GetKey(KeyCode.Space)){
				if(Input.GetKey(KeyCode.S)){
					GTime.TimeScaleBase = 0.45f;
				}
				else{
					GTime.TimeScaleBase = 3.0f;
				}
			}
			if(Input.GetKeyUp(KeyCode.Space)){
				GTime.TimeScaleBase = 1.0f;
			}

            if (Input.GetKeyDown(KeyCode.F8))
            {
                GameMain.Instance.UF_GameReboot();
            }
            #endif

        }


		private int mPassIdx = 0;

		private int[] UnlockPassSqueue = { 1, 1, 4, 4, 2, 3, 2 ,3,1,2,4,3,1,2,4,3};

		private int UF_ClickSideType(Vector3 pos){
			int posx = (int)pos.x - Screen.width / 2;
			int posy = (int)pos.y - Screen.height / 2;

			if (posx < 0 && posy > 0) {
				return 1;
			} else if (posx > 0 && posy > 0) {
				return 2;
			} else if (posx < 0 && posy < 0) {
				return 3;
			} else if (posx > 0 && posy < 0) {
				return 4;
			}
			return 0;
		}


		private void UF_UpdateUnLockDebugger(){
			if (IsActive)
				return;
			if (DeviceInput.UF_Down(0)) {
				if (mPassIdx >= UnlockPassSqueue.Length) {
					IsActive = true;
					mPassIdx = 0;
					return;
				} else {
					int side = UF_ClickSideType(DeviceInput.UF_DownPosition(0));
					if (side == UnlockPassSqueue[mPassIdx]) {
						mPassIdx++;    
					} else {
						mPassIdx = 0;
					}
				}
			}
		}




		private void UF_OnDebugLogCallBack(string arg1,string arg2,LogType ltype){
			switch (ltype) {
			case LogType.Log:Debugger.UF_Log(arg1);break;
			case LogType.Warning:Debugger.UF_Warn(arg1);break;
			case LogType.Error:Debugger.UF_Error(arg1);break;
			case LogType.Exception:Debugger.UF_Exception(arg1,arg2);break;
			case LogType.Assert:Debugger.UF_Error(arg1);break;
			}
		}


		public string UF_GetTagMessage(object tag){
			return this.m_Loger.UF_GetTagMessage(tag.ToString());
		}


		public void UF_LogMessage(string tag,object msg){
			this.m_Loger.UF_Log(tag, msg.ToString ());
		}

		public static void UF_LogTag(string tag,string msg){
			Debugger.UF_GetInstance().UF_LogMessage(tag, msg);
		}

		public static void UF_Log(string msg, params object[] para)
		{
			msg = string.Format(msg, para);
			UF_Log(msg);
		}

		public static void UF_Log(string msg){
			if (!IsActive) {
				return;
			}
			#if UNITY_EDITOR
			Debug.Log(msg);
			#endif
			Debugger.UF_GetInstance().UF_LogMessage(TAG_LOG, msg);
		}

		public static void UF_Warn(string warn){
			if (!IsActive) {
				return;
			}
			#if UNITY_EDITOR
			Debug.LogWarning(warn);
			#endif
			Debugger.UF_GetInstance().UF_LogMessage(TAG_WARN, warn);
		}

		public static void UF_Error(string error){
			System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
			System.Text.StringBuilder sb = StrBuilderCache.Acquire();
			sb.AppendLine(error);
			sb.AppendLine("Stack:");
			foreach(System.Diagnostics.StackFrame sf in st.GetFrames()){
				sb.AppendLine(string.Format("\t{0}.{1}:{2}",
					sf.GetMethod().DeclaringType.FullName,
					sf.GetMethod().Name,
					sf.GetFileLineNumber()
				));
			}
			string info = StrBuilderCache.GetStringAndRelease (sb);
			#if UNITY_EDITOR
			Debug.LogError(info);
			#endif
			//ingore active ,still write to loger
			Debugger.UF_GetInstance().UF_LogMessage(TAG_ERROR, info);
		}

		public static void UF_Exception(System.Exception e){
            UF_Exception(e.Message, e.StackTrace);
		}

		public static void UF_Exception(string message,string stacktrace){
			#if UNITY_EDITOR
			Debug.LogError(string.Format("{0}\n{1}",message,stacktrace));
			#endif
			Debugger.UF_GetInstance().UF_LogMessage(TAG_ERROR, string.Format("{0}\n{1}",message,stacktrace));
		}

		//消息记录
		public static void UF_RecordMsg(string tag,string msg){
			if (!IsActive) {return;}
			if (!Debugger.UF_GetInstance().m_DicMsgRecord.ContainsKey (tag)) {
				Debugger.UF_GetInstance().m_DicMsgRecord.Add (tag, msg);
			} 
			else {
				Debugger.UF_GetInstance().m_DicMsgRecord [tag] = msg;
			}
		}

		public static void UF_TrackMsg(int type,int stamp,string info){
			if (!IsActive) {return;}
			if (!Debugger.UF_GetInstance ().m_MsgTrackers.ContainsKey (type)) {
				Debugger.UF_GetInstance ().m_MsgTrackers.Add (type, new MsgTracker (80));
			}
			Debugger.UF_GetInstance ().m_MsgTrackers [type].UF_Add(info,stamp);
		}


		public static void UF_TrackRes(string resName,int stamp){
            UF_TrackMsg(TRACK_RES_LOAD, stamp, resName);
		}

		public static void UF_TrackNetProtol(int protoCode,int size,int corCode,int state = 0){
			if (!IsActive) {return;}
			if (state == 0) {
                UF_TrackMsg(TRACK_NET_PROTO,(int)GTime.UF_GetSystemSeconds(),string.Format ("[{0}] <color=yellow>Send-> {1} | size: {2} | corcode:{3}</color>", GTime.UF_GetLongTimeString(), protoCode, size,corCode));
			} else {
                UF_TrackMsg(TRACK_NET_PROTO,(int)GTime.UF_GetSystemSeconds(),string.Format ("[{0}] <color=cyan>Receive-> {1} | size: {2} | corcode:{3}</color>",GTime.UF_GetLongTimeString(),protoCode, size,corCode));
			}
		}

		public void OnApplicationQuit(){
			m_Loger.Dispose ();
		}

	}
}
