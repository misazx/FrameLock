//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityFrame{
	public interface IDebugViewBoard  {
		void UF_DrawInfo(Rect rect);
		void UF_DrawDetail (Rect rect);
	}

	public class ConsoleView {
		enum ViewBoardTag{
			NONE,
			OUTPUT,
			ASSET,
			RECORD,
            LUA,
            NETWORK,
            SYSTEM,
		}

		private ViewBoardTag m_ViewBoardTag = ViewBoardTag.NONE;

		private IDebugViewBoard m_CurDebugView = null;

		private	Vector2 m_ScrollAreaDetail = Vector2.zero;
		private	Vector2 m_ScrollAreaInfo = Vector2.zero;

		private float m_WidthTag = 0.15f;
		private float m_WidhDetail = 0.15f;
		private float m_WidthInfo = 0.7f;
		private float m_WidthSpace = 10;

		private Rect m_Win;

		private Dictionary<ViewBoardTag,IDebugViewBoard> m_DicDebugView = new Dictionary<ViewBoardTag, IDebugViewBoard> ();

		public void UF_OnStart(){
			m_DicDebugView.Add (ViewBoardTag.NONE, null);
			m_DicDebugView.Add (ViewBoardTag.OUTPUT, new DVBoardOutput ());
			m_DicDebugView.Add (ViewBoardTag.ASSET, new DVBoardAsset ());
			m_DicDebugView.Add (ViewBoardTag.NETWORK, new DVBoardNetwork ());
			m_DicDebugView.Add (ViewBoardTag.LUA, new DVBoardLua ());
			m_DicDebugView.Add (ViewBoardTag.RECORD, new DVBoardRecord ());
			m_DicDebugView.Add (ViewBoardTag.SYSTEM, new DVBoardSystem ());
		}


		public void UF_OnUpdate(){
			
        }


		public void UF_OnDraw(){
			if (m_ViewBoardTag == ViewBoardTag.NONE) {
				m_Win = GUILayout.Window(1,m_Win, UF_DrawWinEnter, "FPS: " + GTime.FPS);
			} 
			else {
                UF_DrawAreaTag();
                UF_DrawAreaDetial();
                UF_DrawAreaInfo();	
			}
		}

		public static bool UF_LayoutButton(string info,float h = 40){
			return GUILayout.Button (info,GUILayout.Height(h));
		}

		//绘制区域标签
		private void UF_DrawAreaTag(){
			Rect rect = new Rect (0, 0, m_WidthTag * Screen.width - m_WidthSpace, Screen.height);

			//模拟黑屏
			GUI.Box(new Rect(0,0,Screen.width,Screen.height),"");
			GUI.Box(new Rect(0,0,Screen.width,Screen.height),"");

			GUILayout.BeginArea (rect);

			if (UF_LayoutButton("Close",80)) {
				m_ViewBoardTag = ViewBoardTag.NONE;
			}
			if (UF_LayoutButton("Output")) {
				m_ViewBoardTag = ViewBoardTag.OUTPUT;
			}
			if (UF_LayoutButton("GM")) {
				m_ViewBoardTag = ViewBoardTag.NONE;
                MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_LUA, DefineLuaEvent.E_DEBUG_GM);
			}
			if (UF_LayoutButton("Asset")) {
				m_ViewBoardTag = ViewBoardTag.ASSET;
			}
			if (UF_LayoutButton("Record")) {
				m_ViewBoardTag = ViewBoardTag.RECORD;
			}
			if (UF_LayoutButton("Lua")) {
				m_ViewBoardTag = ViewBoardTag.LUA;
			}
			if (UF_LayoutButton("Network")) {
				m_ViewBoardTag = ViewBoardTag.NETWORK;
			}
			if (UF_LayoutButton("System")) {
				m_ViewBoardTag = ViewBoardTag.SYSTEM;
			}

            m_CurDebugView = m_DicDebugView[m_ViewBoardTag];
				
			GUILayout.EndArea ();
		}

		//绘制区域类型
		private void UF_DrawAreaDetial(){
			if (m_CurDebugView != null) {
				Rect rect = new Rect (m_WidthTag * Screen.width, 0, m_WidhDetail * Screen.width- m_WidthSpace, Screen.height);
				GUILayout.BeginArea (rect);
				m_ScrollAreaDetail = GUILayout.BeginScrollView (m_ScrollAreaDetail);
				m_CurDebugView.UF_DrawDetail(rect);	
				GUILayout.EndScrollView ();
				GUILayout.EndArea ();
			}
		}

		//绘制区域细节
		private void UF_DrawAreaInfo(){
			if (m_CurDebugView != null) {
                //Rect rect = GUILayoutUtility.GetRect(Screen.width, Screen.height, GUI.skin.box);

                Rect rect = new Rect((m_WidthTag + m_WidhDetail) * Screen.width, 0, m_WidthInfo * Screen.width - m_WidthSpace, Screen.height);
                GUILayout.BeginArea (rect);
                m_ScrollAreaInfo = GUILayout.BeginScrollView (m_ScrollAreaInfo);

                int textBoxSize = GUI.skin.box.fontSize;
                int textAreaSize = GUI.skin.textArea.fontSize;
                int textLabelSize = GUI.skin.label.fontSize;
                GUI.skin.box.fontSize = 16;
                GUI.skin.textArea.fontSize = 14;
                GUI.skin.label.fontSize = 15;

                m_CurDebugView.UF_DrawInfo(rect);
                GUI.skin.label.fontSize = textLabelSize;
                GUI.skin.textArea.fontSize = textAreaSize;
                GUI.skin.box.fontSize = textBoxSize;

                GUILayout.EndScrollView ();
				GUILayout.EndArea ();
			}
		}


		void UF_DrawWinEnter(int id){
			MsgLoger logger = Debugger.UF_GetInstance ().logger;
			GUILayout.Label(
				string.Format("<color=white>L:{0}</color>  <color=yellow>W:{1}</color>  <color=red>E:{2}</color>",
					logger.UF_GetTagCount(Debugger.TAG_LOG),
					logger.UF_GetTagCount(Debugger.TAG_WARN),
					logger.UF_GetTagCount(Debugger.TAG_ERROR)));
			if(GUILayout.Button("Console",GUILayout.Width(100),GUILayout.Height(80))){
				m_ViewBoardTag = ViewBoardTag.OUTPUT;
			}
			GUI.DragWindow();
		}



	}
}