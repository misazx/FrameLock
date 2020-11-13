//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------
using System.Collections;
using System.Collections.Generic;

namespace UnityFrame
{
	public static class FrameHandle
	{
		static List<IFrameHandle> s_ListHandle = new List<IFrameHandle>();

		static Dictionary<IFrameHandle,int> s_MapHandle = new Dictionary<IFrameHandle, int> ();

		static List<DelegateVoid> s_CallMethod = new List<DelegateVoid> ();

		static Dictionary<int,UnityEngine.Coroutine> s_CoroutineMap = new Dictionary<int, UnityEngine.Coroutine>();

		static int CoroutineUID {get{ return m_UID++;}}

		static int m_UID = 0;

        public static bool ActiveSyncUpdate = true;

		public static void UF_AddHandle(IFrameHandle handle){
			if (handle != null && !s_MapHandle.ContainsKey (handle)) {
				if (handle is IOnAwake) {
					(handle as IOnAwake).UF_OnAwake ();
				}
				s_ListHandle.Add (handle);
				s_MapHandle.Add (handle,s_ListHandle.Count - 1);
			}
		}

		public static void UF_RemoveHandle(IFrameHandle handle){
			if (handle != null) {
				s_ListHandle.Remove (handle);
				s_MapHandle.Remove (handle);
			}
		}

		public static int UF_AddCoroutine(IEnumerator method,bool blockEvent = false){
			int id = CoroutineUID;
			GameMain.Instance.StartCoroutine (UF_InvokeCorotine(id,method,blockEvent));
			return id;
		}

		static IEnumerator UF_InvokeCorotine(int id,IEnumerator method, bool blockEvent)
        {
			UnityEngine.Coroutine coroutine = GameMain.Instance.StartCoroutine(method);
			//绑定到Map中
			s_CoroutineMap.Add (id,coroutine);
			yield return coroutine;
			//解除绑定 
			s_CoroutineMap.Remove (id);
			//发送携程完成消息
            if(!blockEvent)
			    MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_COROUTINE_STATE,1,id);
		}

		public static void UF_RemoveCouroutine (int id, bool blockEvent = false)
        {
			if (s_CoroutineMap.ContainsKey (id)) {
				GameMain.Instance.StopCoroutine (s_CoroutineMap[id]);
				s_CoroutineMap.Remove (id);
                //携程调用中断
                if (!blockEvent)
                    MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_COROUTINE_STATE,2,id);
			}
		}

		public static void UF_CallMethod(DelegateVoid method){
			if (method != null) {
				lock (s_CallMethod) {
					s_CallMethod.Add (method);
				}
			}
		}


		static void UF_PollingCallMethods(){
			if (s_CallMethod.Count > 0) {
				DelegateVoid[] delegates = null;
				lock (s_CallMethod) {
					delegates = s_CallMethod.ToArray ();
					s_CallMethod.Clear ();
				}

				if (delegates != null) {
					for (int k = 0; k < delegates.Length; k++) {
						if (delegates [k] != null) {
							try {
								delegates [k].Invoke ();
							} catch (System.Exception e) {
								//继续执行下一个
								Debugger.UF_Exception(e);
							}
						}
					}
				}

			}
		}
			

		public static void UF_OnStart(){
			for(int k = 0;k < s_ListHandle.Count;k++){
				if (s_ListHandle [k] != null && s_ListHandle[k] is IOnStart) {
					(s_ListHandle [k] as IOnStart).UF_OnStart ();
				}
			}
		}


        public static void UF_OnUpdate(){
            UF_PollingCallMethods();

			for(int k = 0;k < s_ListHandle.Count;k++){
				if (s_ListHandle [k] != null && s_ListHandle[k] is IOnUpdate) {
					(s_ListHandle [k] as IOnUpdate).UF_OnUpdate ();
				}
			}
		}


        public static void UF_OnFixedUpdate()
        {
            for (int k = 0; k < s_ListHandle.Count; k++)
            {
                if (s_ListHandle[k] != null && s_ListHandle[k] is IOnFixedUpdate)
                {
                    (s_ListHandle[k] as IOnFixedUpdate).UF_OnFixedUpdate();
                }
            }
        }


        //同步更新，用于帧同步处理更新
        public static void UF_OnSyncUpdate()
        {
            if (!ActiveSyncUpdate) return;

            for (int k = 0; k < s_ListHandle.Count; k++)
            {
                if (s_ListHandle[k] != null && s_ListHandle[k] is IOnSyncUpdate)
                {
                    (s_ListHandle[k] as IOnSyncUpdate).UF_OnSyncUpdate();
                }
            }
        }

        public static void UF_OnLateUpdate(){
			for(int k = 0;k < s_ListHandle.Count;k++){
				if (s_ListHandle [k] != null && s_ListHandle[k] is IOnLateUpdate) {
					(s_ListHandle [k] as IOnLateUpdate).UF_OnLateUpdate ();
				}
			}
		}

		//每秒执行一次
		public static void UF_OnSecondUpdate(){
			if (GTime.FrameCount % GTime.FrameRate == 0) {
				for(int k = 0;k < s_ListHandle.Count;k++){
					if (s_ListHandle [k] != null && s_ListHandle[k] is IOnSecnodUpdate) {
						(s_ListHandle [k] as IOnSecnodUpdate).UF_OnSecnodUpdate ();
					}
				}
			}
		}
			

		public static void OnGUI(){
			for(int k = 0;k < s_ListHandle.Count;k++){
				if (s_ListHandle [k] != null && s_ListHandle[k] is IOnGUI) {
					(s_ListHandle [k] as IOnGUI).OnGUI ();
				}
			}
		}
			
		public static void OnApplicationPause(bool state){
			for(int k = 0;k < s_ListHandle.Count;k++){
				if (s_ListHandle [k] != null && s_ListHandle[k] is IOnApplicationPause) {
					(s_ListHandle [k] as IOnApplicationPause).OnApplicationPause(state);
				}
			}
		}

        public static void UF_OnReset()
        {
            for (int k = 0; k < s_ListHandle.Count; k++)
            {
                if (s_ListHandle[k] != null && s_ListHandle[k] is IOnReset)
                {
                    (s_ListHandle[k] as IOnReset).UF_OnReset();
                }
            }
        }

        public static void OnApplicationQuit(){
			for(int k = 0;k < s_ListHandle.Count;k++){
				if (s_ListHandle [k] != null && s_ListHandle[k] is IOnApplicationQuit) {
					(s_ListHandle [k] as IOnApplicationQuit).OnApplicationQuit();
				}
			}
		}

	}
}


