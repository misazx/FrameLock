//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;
using System.Threading;
using System.Collections.Generic;

namespace UnityFrame
{
	public class MessageSystem : HandleSingleton<MessageSystem>,IOnUpdate,IOnReset
	{
		protected struct Message {
			public int eventID;
			public object[] args;
		}
			
		protected List<Message> m_ListMessages = new List<Message>();

		protected Dictionary<int,DelegateMessage> m_DicListeners = new Dictionary<int, DelegateMessage> ();

		[System.ThreadStatic] static List<object> m_ListSendStack = new List<object>();


        /// <summary>
        /// 直接发送消息，同步处理
        /// 忽略线程安全
        /// </summary>
        public void UF_Send(int eventID){
			this.UF_Send(eventID,null);
		}

		/// <summary>
		/// 直接发送消息，同步处理
		/// 忽略线程安全
		/// </summary>
		public void UF_Send(int eventID,params object[] args){
			if (m_DicListeners.ContainsKey (eventID)) {
				m_DicListeners [eventID].Invoke (args);
			} else {
				Debugger.UF_Warn (string.Format("No Listener[{0}] To Dispatch",eventID));
			}
		}


        public void UF_BeginSend(){
			m_ListSendStack.Clear ();
		}

		public void UF_PushParam(object value){
			m_ListSendStack.Add (value);
		}

		public void UF_EndSend(int eventID){
			if (m_ListSendStack.Count > 0) {
                UF_Send(eventID, m_ListSendStack.ToArray ());
			}
		}
			
		/// <summary>
		/// 发送消息到队列，异步处理
		/// 线程安全
		/// </summary>
		public void UF_Post(int eventID){
            UF_Post(eventID, null);
		}

		/// <summary>
		/// 发送消息到队列，异步处理
		/// 线程安全
		/// </summary>
		public void UF_Post(int eventID,params object[] args){
			lock (m_ListMessages) {
				Message msg = new Message ();
				msg.eventID = eventID;
				msg.args = args;
				m_ListMessages.Add (msg);
			}
		}

		public void UF_RemoveListener(int eventID){
			if (m_DicListeners.ContainsKey (eventID)) {
				m_DicListeners.Remove (eventID);
			}
		}

		public void UF_RemoveListener(int eventID,DelegateMessage method){
			if (m_DicListeners.ContainsKey (eventID)) {
				if (m_DicListeners [eventID] != null) {
					m_DicListeners [eventID] -= method;
				}
			}
		}
			
		public void UF_AddListener(int eventID,DelegateMessage method){
			if (m_DicListeners.ContainsKey (eventID)) {
				if (m_DicListeners [eventID] != null) {
					m_DicListeners [eventID] += method;
				} else {
					m_DicListeners[eventID] = method;
				}
			} else {
				m_DicListeners.Add (eventID, method);
			}
		}


		public void UF_OnUpdate(){
			if (m_ListMessages.Count > 0) {
				Message[] messages = null;
				lock (m_ListMessages) {
					messages = m_ListMessages.ToArray();	
					m_ListMessages.Clear ();
				}
				if (messages != null) {
					for (int k = 0; k < messages.Length; k++) {
						if (m_DicListeners.ContainsKey (messages [k].eventID)) {
							m_DicListeners [messages [k].eventID].Invoke (messages [k].args);
						}
					}
				}
			}
		}

		//接收外部消息
		internal void UF_HandleNativeMessage(string msg){
			if(!string.IsNullOrEmpty(msg)){
                Debugger.UF_LogTag("Native Msg", msg);
                int idxEventId = msg.IndexOf(';');
				if (idxEventId > -1) {
					string e = msg.Substring(0, idxEventId);
					string d = msg.Substring(idxEventId + 1);
                    if (e.StartsWith("E_"))
                    {
                        MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_LUA, e, d);
                    }
                    else if (e == "NATIVE_INFO")
                    {
                        MsgDataStruct msgData = new MsgDataStruct();
                        msgData.UF_SetTable(d);
                        GlobalSettings.UF_SetNativeInfo(msgData);
                    }
                    else if (e == "SDK_INFO")
                    {
                        MsgDataStruct msgData = new MsgDataStruct();
                        msgData.UF_SetTable(d);
                        GlobalSettings.UF_SetSDKInfo(msgData);
                    }
                    else
                    {
                        Debugger.UF_Warn("Unknow External Message:" + e);
                    }
				}
			}

		}



        public void UF_OnReset() {
            m_ListMessages.Clear();
            m_ListSendStack.Clear();
        }

    }
}

