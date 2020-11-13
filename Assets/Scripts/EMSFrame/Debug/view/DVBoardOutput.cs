//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame
{
	public class DVBoardOutput : IDebugViewBoard
	{
		private string m_CurrentTag = Debugger.TAG_LOG;


		private void UF_SetColor(string tag){
			if (tag == Debugger.TAG_LOG) {
				GUI.color = Color.white;
			}
			else if (tag == Debugger.TAG_ERROR) {
				GUI.color = Color.red;
			}
			else if (tag == Debugger.TAG_WARN) {
				GUI.color = Color.yellow;
			}
			else if (tag == Debugger.TAG_EXCEPTION) {
				GUI.color = Color.red;
			}
			else {
				GUI.color = Color.white;
			}
		}

		private void UF_DrawTagMsgButton(string tag,MsgLoger logger){
			UF_SetColor (tag);
			if (GUILayout.Button(string.Format("{0} ({1})",tag,logger.UF_GetTagCount(tag)),GUILayout.Height(40))) {
				m_CurrentTag = tag;
			}
			GUILayout.Space (10);
		}

		public void UF_DrawDetail (Rect rect){
			MsgLoger logger = Debugger.UF_GetInstance ().logger;

			UF_DrawTagMsgButton (Debugger.TAG_LOG,logger);
			UF_DrawTagMsgButton (Debugger.TAG_WARN,logger);
			UF_DrawTagMsgButton (Debugger.TAG_ERROR,logger);
			UF_DrawTagMsgButton (Debugger.TAG_EXCEPTION,logger);

			foreach (string tag in logger.Tags) {
				if (tag == Debugger.TAG_LOG || tag == Debugger.TAG_WARN || tag == Debugger.TAG_ERROR || tag == Debugger.TAG_EXCEPTION) {
					continue;	
				}
				UF_DrawTagMsgButton (tag,logger);
			}
			GUI.color = Color.white;

            if (GUILayout.Button("Clear", GUILayout.Height(40)))
            {
                logger.UF_ClearMessage();
            }
        }

		public void UF_DrawInfo(Rect rect){
			MsgLoger logger = Debugger.UF_GetInstance ().logger;
			UF_SetColor (m_CurrentTag);
			GUILayout.TextArea (logger.UF_GetTagMessage(m_CurrentTag), GUILayout.Width (rect.width - 3));
			GUI.color = Color.white;
		}
	}
}

