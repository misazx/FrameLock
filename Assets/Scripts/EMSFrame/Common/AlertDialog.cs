//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame{
	public class AlertDialog : HandleSingleton<AlertDialog>
	{
		private static UIView m_AlertBox;
        protected static UIView AlertBox {
            get {
                if (m_AlertBox == null)
                {
                    m_AlertBox = CEntitySystem.UF_GetInstance().UF_CreateFromPool("ui_view_alertbox") as UIView;
                    if (m_AlertBox == null) {
                        m_AlertBox = AssetSystem.UF_LoadComponentFromResources<UIView>("Prefabs/ui_view_alertbox");
                        CEntitySystem.UF_GetInstance().UF_AddToActivity(m_AlertBox);
                    }
                    if (m_AlertBox != null) {
                        UIManager.UF_GetInstance().UF_AddToCanvas("UI System", m_AlertBox);
                        m_AlertBox.UF_SetActive(false);
                    }
                }
                return m_AlertBox;
            }
        }


		public static void UF_Show(string title,string btinfo,string content,DelegateMethod callback){
			if (AlertBox == null) {
				Debugger.UF_Error ("AlertDialog is not init");
				return;
			}
            AlertBox.UF_SetActive (true);
			UILabel uititle = AlertBox.UF_GetUI ("lb_title") as UILabel;
			if (uititle != null) {uititle.text = title;}
			UILabel uibtinfo = AlertBox.UF_GetUI ("lb_ok") as UILabel;
			if (uibtinfo != null) {uibtinfo.text = btinfo;}
			UILabel uicontent = AlertBox.UF_GetUI ("lb_content") as UILabel;
			if (uicontent != null) {uicontent.text = content;}
            AlertBox.UF_SetKActive ("bt_cancel", false);
			UIButton uibutton = AlertBox.UF_GetUI ("bt_ok") as UIButton;
			if (uibutton != null) {
				UnityEngine.Events.UnityAction wrap_callback = delegate(){
                    UF_Close();
					if(callback != null){callback(null);}
				};
				uibutton.onClick.RemoveAllListeners ();
				uibutton.onClick.AddListener(wrap_callback);
			}
		}

		public static void UF_ShowOkCancel(string title,string leftInfo,string rightInfo, string content,DelegateMethod callbackOK, DelegateMethod callbackCancle)
        {
			if (AlertBox == null) {
				Debugger.UF_Error ("AlertDialog is not init");
				return;
			}
            UF_Show(title, rightInfo, content, callbackOK);

            UILabel uibtinfo = AlertBox.UF_GetUI("lb_cancel") as UILabel;
            if (uibtinfo != null) { uibtinfo.text = leftInfo; }

            UIButton uibutton = AlertBox.UF_GetUI("bt_cancel") as UIButton;
            if (uibutton != null)
            {
                uibutton.UF_SetActive(true);
                UnityEngine.Events.UnityAction wrap_callback = delegate () {
                    UF_Close();
                    if (callbackCancle != null) { callbackCancle(null); }
                };
                uibutton.onClick.RemoveAllListeners();
                uibutton.onClick.AddListener(wrap_callback);
            }
		}



		public static void UF_Close(){
            if (m_AlertBox != null) {
                m_AlertBox.UF_SetActive(false);
                UIButton uibutton = m_AlertBox.UF_GetUI("bt_ok") as UIButton;
                m_AlertBox.Release();
                m_AlertBox = null;
                if (uibutton != null) { uibutton.onClick.RemoveAllListeners(); }
            }
		}
			
	}









}

