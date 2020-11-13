//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame
{
	public class DVBoardRecord : IDebugViewBoard
	{
		private string m_CurrentKey;

		public void UF_DrawDetail (Rect rect){
			foreach(KeyValuePair<string,string> item in Debugger.UF_GetInstance().MsgRecords){
				if (GUILayout.Button (item.Key,GUILayout.Height(40))) {
					m_CurrentKey = item.Key;
				}
			}
		}

		public void UF_DrawInfo(Rect rect){
			if (!string.IsNullOrEmpty (m_CurrentKey)) {
				GUILayout.Box (Debugger.UF_GetInstance().MsgRecords[m_CurrentKey], GUILayout.Width (rect.width));
			}

            UF_DrawPerformAction(rect);


        }

        private void UF_DrawPerformAction(Rect rect) {
            string val = string.Format("Perform Action: \n Count: {0} \n Tick: {1}",
                PerformActionManager.UF_GetInstance().actionCount,
                PerformActionManager.UF_GetInstance().tickTimes);
            GUILayout.Box(val, GUILayout.Width(rect.width));
        }


	}
}

