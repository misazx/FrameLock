//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace UnityFrame{

	//消息跟踪
	public class MsgTracker {
 
		private string[] m_TrackInfo;
		private int[] m_TrackStamp;

		private int m_PtrIdx = 0;
		private int m_PtrCurIdx = 0;

		public MsgTracker(int queueCount){
			m_TrackInfo = new string[queueCount];
			m_TrackStamp = new int[queueCount];
		}

		public int UF_Begain(){
			return System.Environment.TickCount;
		}
			
		public void UF_End(int last,string info){
			int usedTick = Mathf.Abs (System.Environment.TickCount - last);
            UF_Add(info,usedTick);
		}

		public void UF_Add(string info,int stamp){
			m_TrackInfo [m_PtrIdx] = info;
			m_TrackStamp [m_PtrIdx] = stamp;
			m_PtrCurIdx = m_PtrIdx;
			m_PtrIdx++;
			if (m_PtrIdx == m_TrackInfo.Length) {
				m_PtrIdx = 0;
			}
		}

		private string UF_MsgToString(string info,int stamp){
			return string.Format ("{0} | {1}", info, stamp);
		}

		public string UF_ForeachToString(DelegateMsgToStrForeach method){
			try{
				System.Text.StringBuilder sb = StrBuilderCache.Acquire();
				sb.Remove(0,sb.Length);
				if (m_PtrCurIdx == m_TrackInfo.Length - 1) {
					for (int k = 0; k < m_TrackInfo.Length; k++) {
						sb.AppendLine (method(m_TrackInfo[k],m_TrackStamp[k]));
					}
				}
				else if(m_TrackInfo[m_PtrCurIdx + 1] != null) {
					for (int k = m_PtrCurIdx + 1; k < m_TrackInfo.Length; k++) {
						sb.AppendLine (method(m_TrackInfo[k],m_TrackStamp[k]));
					}
					for (int k = 0; k <= m_PtrCurIdx; k++) {
						sb.AppendLine (method(m_TrackInfo[k],m_TrackStamp[k]));
					}
				}
				else{
					for (int k = 0; k <= m_PtrCurIdx; k++) {
						sb.AppendLine (method(m_TrackInfo[k],m_TrackStamp[k]));
					}
				}
				return StrBuilderCache.GetStringAndRelease(sb);
			}
			catch(System.Exception e){
				Debugger.UF_Exception (e);
				return string.Empty;
			}
		}

		public override string ToString ()
		{
			return UF_ForeachToString(UF_MsgToString);
		}

	}
}