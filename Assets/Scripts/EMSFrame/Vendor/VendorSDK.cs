//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame{
	public static class VendorSDK
	{		
		#if UNITY_EDITOR || UNITY_STANDALONE
		private static WinSDKActivity m_WinSDKActivity = new WinSDKActivity();
		#endif

		public static void UF_Init(){
			#if UNITY_EDITOR || UNITY_STANDALONE
				m_WinSDKActivity.Init();
			#endif

            

		}

		public static void UF_SafeCall(string method,string arg){
			FrameHandle.UF_CallMethod(
				() => {
					VendorSDK.UF_Call(method,arg);
				}
			);
		}

		//only call on main thread
		public static void UF_Call(string method,string arg){
			if (!string.IsNullOrEmpty (method)) {
				if (arg == null)
					arg = string.Empty;
				try{
					Debugger.UF_Log(string.Format("Call SDK Method:{0}\n Param:{1} " ,method,arg));
					#if UNITY_EDITOR || UNITY_STANDALONE
					m_WinSDKActivity.CallSDKMethod(method,arg);
					#elif UNITY_ANDROID
					AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
					AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
					jo.Call ("onUnityCall",method,arg);
					jo.Dispose();
					jc.Dispose();
					#elif UNITY_IPHONE
					DLLImport.__UnityCall(method,arg);
					#endif
				}
				catch(System.Exception e){
					Debugger.UF_Exception(e);
				}
			}
		}

	}



}