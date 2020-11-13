//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame{

	//界面内容扩展
	//用于嵌入子界面，继承IUIUpdateGroup ，作为UIView的代理组建
	[RequireComponent(typeof(RectTransform))]
	public class UIContent : UIObject,IUIUpdateGroup,IOnReset {
		//自动布局
		public bool autoAdaptLayout;
		private bool m_IsClosed = true;

		private UIView m_Content;

		private bool m_IsLoadProcess = false;

		public new string name {
			get{ 
				if (m_Content != null) {
					return m_Content.name;
				} else {
					return base.name;
				}
			}
		}
			
		public UIView target{get{return m_Content;}}
			
		public IUIUpdate UF_GetUI(string updateKey)
		{
			if (m_Content != null) {
				return target.UF_GetUI (updateKey);
			} else {
				Debugger.UF_Error(string.Format("UIContent[{0}] is null,UF_GetUI[{1}] Failed", this.name,updateKey));
				return null;
			}
		}

		public bool UF_SetKValue(string key,object value){
			if (m_Content != null) {
				return target.UF_SetKValue (key,value);
			} else {
				Debugger.UF_Error(string.Format("UIContent[{0}] is null,UF_SetKValue[{1}] Failed", this.name,key));
				return false;
			}
		}

		public bool UF_SetKActive(string key,bool value){
			if (m_Content != null) {
				return target.UF_SetKActive (key,value);
			} else {
				Debugger.UF_Error(string.Format("UIContent[{0}] is null,UF_SetKActive[{1}] Failed", this.name,key));
				return false;
			}
		}

		private void UF_AddToContent(UIView view){
            view.UF_SetActive(true);
            view.transform.SetParent(this.transform);
            view.transform.localPosition = Vector3.zero;
            view.transform.localScale = Vector3.one;
			if(autoAdaptLayout){
                view.sizeDelta = Vector2.zero;
                view.anchoredPosition = Vector2.zero;
			}

			m_Content = view;
		}

		public override void UF_SetValue (object value)
		{
			if (value != null) {
                if (value is bool) {
                    bool b = (bool)value;
                    UF_Display(b);
                }
				this.UF_Show(value.ToString ());
			} else {
				this.UF_Close();
			}
		}

		//异步加载一个content，并通知回调
		public void UF_Show(string contentName){
			m_IsClosed = false;
            UF_Show(contentName,null);
		}

		public void UF_Show(string contentName,DelegateObject callback){
			if (string.IsNullOrEmpty (contentName)) {
				Debugger.UF_Warn (string.Format ("UIContent AsyncShow Failed! contentName is Null"));
				return;
			}
				
			if (m_Content != null && m_Content.name == contentName) {
				m_IsClosed = false;
                m_Content.UF_SetActive(true);
				GHelper.UF_SafeCallDelegate (callback, m_Content);
				MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_UI_SHOW,m_Content);
				return;
			}

			if (m_Content != null && m_Content.name != contentName) {
				this.UF_Close();
			}

            UF_AsyncShow(contentName,callback);
		}

		//关闭界面并回收 
		public void UF_Close(){
			m_IsClosed = true;
            //this.UF_SetActive(false);
            this.UF_OnReset();
		}


		//显示内容界面，如果界面未加载,则加载界面，如果已存在，则只显示或隐藏
		public void UF_Display(bool value){
			if (m_Content != null) {
                m_Content.UF_SetActive (value);
			}
		}


		private void UF_AsyncShow(string contentName,DelegateObject callback){
			if (m_IsLoadProcess) {
				Debugger.UF_Error (string.Format ("UIContent AsyncShow Failed! Content[{0}] is in loading!",contentName));
				return;
			}
			m_IsLoadProcess = true;

            try
            {
                CEntitySystem.UF_GetInstance().UF_AsyncCreate(contentName,
                (entity) =>
                {
                    m_IsLoadProcess = false;
                    if (entity != null)
                    {
                        if (m_IsClosed)
                        {
                            (entity as IEntityHnadle).isReleased = true;
                            return;
                        }
                        else
                        {
                            UF_AddToContent(entity as UIView);
                            GHelper.UF_SafeCallDelegate(callback, this);
                            MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_UI_SHOW, this);
                        }
                    }
                }
                );
            }
            catch (System.Exception e) {
                Debugger.UF_Error("UIContent AstncShow Exception:");
                Debugger.UF_Exception(e);
            }
        }


		public UIView UF_SyncShow(string contentName){
			if (string.IsNullOrEmpty (contentName)) {
				Debugger.UF_Error (string.Format ("UIContent SyncShow Failed! contentName is Null"));
				return null;
			}

			if (m_Content != null && m_Content.name == contentName) {
				m_IsClosed = false;
                m_Content.UF_SetActive(true);
				MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_UI_SHOW,m_Content);
				return m_Content;
			}

			if (m_Content != null && m_Content.name != contentName) {
				this.UF_Close();
			}

			m_Content = CEntitySystem.UF_GetInstance ().UF_Create(contentName) as UIView;

			if (m_Content != null) {
				m_IsClosed = false;
                m_Content.UF_SetActive (true);
                UF_AddToContent(m_Content);
				MessageSystem.UF_GetInstance ().UF_Send(DefineEvent.E_UI_SHOW, m_Content);
				return m_Content;
			} else {
				Debugger.UF_Warn (string.Format ("UIContent SyncShow Failed! Can not load View[{0}]",contentName));
				return null;
			}
		}


		//重置时，会把内容界面关闭并回收
		public void UF_OnReset (){
			if (m_Content != null) {
				string contentName = m_Content.name;
				m_Content.Release ();
				m_Content = null;
				MessageSystem.UF_GetInstance ().UF_Send(DefineEvent.E_UI_CLOSE, contentName);
			}
		}

	}



}
