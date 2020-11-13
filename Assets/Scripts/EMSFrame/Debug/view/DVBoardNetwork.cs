//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------
using UnityEngine;

namespace UnityFrame
{
	public class DVBoardNetwork : IDebugViewBoard
	{
		const int TAG_INFO = 0;

		const int TAG_PROTOCAL = 1;

        const int TAG_HACK = 2;

        private int m_CurrentTag;


		public void UF_DrawDetail (Rect rect){
			if(GUILayout.Button("Info",GUILayout.Height(40))){
				m_CurrentTag = TAG_INFO;
			}
			if(GUILayout.Button("Protocal",GUILayout.Height(40))){
				m_CurrentTag = TAG_PROTOCAL;
			}
            if (GUILayout.Button("Hack", GUILayout.Height(40)))
            {
                m_CurrentTag = TAG_HACK;
            }
            GUILayout.Space(40);
            GUI.color = Color.red;
            if (GUILayout.Button("BreakAll", GUILayout.Height(40)))
            {
                //测试断开所有链接
                NetworkSystem.UF_GetInstance().UF_CloseAllConnection(true);
            }
            GUI.color = Color.white;
        }

		public void UF_DrawInfo(Rect rect){
			switch(m_CurrentTag){
			case TAG_INFO:
                UF_DrawTagInfo();break;
			case TAG_PROTOCAL:
                    UF_DrawTagProtocal();break;
            case TAG_HACK:
                UF_DrawTagHack(); break;
            }
		}

		//网络信息，链接，网速等
		private void UF_DrawTagInfo(){
			GUILayout.Space (4);
			GUILayout.Box(NetworkSystem.UF_GetInstance().ToString());
			GUILayout.Space (4);
		}

		private string UF_TrackMsgForeach(string info,int stamp){
            //return string.Format ("{0} | {1}", GTime.UF_FormatDateTime(stamp, "hh:mm:ss"), info);
            return info;
        }

		//协议收发状态
		private void UF_DrawTagProtocal(){
			if (Debugger.UF_GetInstance ().MsgTrackers.ContainsKey (Debugger.TRACK_NET_PROTO)) {
				MsgTracker tracker = Debugger.UF_GetInstance ().MsgTrackers [Debugger.TRACK_NET_PROTO];
				GUILayout.Box (tracker.UF_ForeachToString(UF_TrackMsgForeach));
			}

		}

        private void UF_DrawTagHack() {
#if UNITY_EDITOR
            GUILayout.Label("IP:");
            string hack_ip = GUILayout.TextField(UnityEditor.EditorPrefs.GetString("hack_ip",""), GUILayout.Height(40));
            GUILayout.Label("Port:");
            string hack_port = GUILayout.TextField(UnityEditor.EditorPrefs.GetString("hack_port", ""), GUILayout.Height(40));
            GUILayout.Label("UserID:");
            string hack_userid = GUILayout.TextField(UnityEditor.EditorPrefs.GetString("hack_userid", ""), GUILayout.Height(40));
            GUILayout.Label("Token:");
            string hack_token = GUILayout.TextArea(UnityEditor.EditorPrefs.GetString("hack_token", ""), GUILayout.Height(60));
            GUILayout.Label("OpenID:");
            string hack_openid = GUILayout.TextArea(UnityEditor.EditorPrefs.GetString("hack_openid", ""), GUILayout.Height(60));

            if (GUILayout.Button("Hack Connect", GUILayout.Height(60))) {
                MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_LUA,DefineLuaEvent.E_LOGIN_HACK, hack_ip, hack_port, hack_userid, hack_token, hack_openid);
            }

            if (GUI.changed) {
                UnityEditor.EditorPrefs.SetString("hack_ip", hack_ip);
                UnityEditor.EditorPrefs.SetString("hack_port", hack_port);
                UnityEditor.EditorPrefs.SetString("hack_userid", hack_userid);
                UnityEditor.EditorPrefs.SetString("hack_token", hack_token);
                UnityEditor.EditorPrefs.SetString("hack_openid", hack_openid);
            }
#endif

        }

    }
}

