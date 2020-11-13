//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFrame{
	public class UpgradeView {
		
		private UIView m_ViewUpgrade;

        private UIView m_ViewPreStart;

        private UILabel m_Info;
		private UILabel m_Version;
		private UISlider m_Progress;


        private UIView UF_CreateView(string viewname) {
            UIView view = CEntitySystem.UF_GetInstance().UF_CreateFromPool(viewname) as UIView;
            if (view == null)
            {
                view = AssetSystem.UF_LoadComponentFromResources<UIView>("Prefabs/" + viewname);
                CEntitySystem.UF_GetInstance().UF_AddToActivity(view);
            }
            if (view != null)
            {
                UIManager.UF_GetInstance().UF_AddToCanvas("UI System", view);
            }
            return view;
        }

        public void UF_ShowPreStart()
        {
            if (m_ViewPreStart == null)
            {
                //底图
                m_ViewPreStart = UF_CreateView("ui_view_pre_start");
                if (m_ViewPreStart != null)
                {
					m_ViewPreStart.UF_SetActive(true);
				}
			}
        }


		public void UF_Show(){
            if(GlobalSettings.IsAppCheck) return;
            if (m_ViewUpgrade == null) {
                m_ViewUpgrade = UF_CreateView("ui_view_upgrade");
                if (m_ViewUpgrade != null) {
					m_Info = m_ViewUpgrade.UF_GetUI("lb_info") as UILabel;
					m_Progress = m_ViewUpgrade.UF_GetUI("sl_progress") as UISlider;
					m_Version = m_ViewUpgrade.UF_GetUI("lb_version") as UILabel;
                    m_ViewUpgrade.UF_SetActive(true);
                }
			}
        }

        public void UF_Close(){
			if (m_ViewUpgrade != null) {
				m_ViewUpgrade.Release ();
			}
            if (m_ViewPreStart != null) {
                m_ViewPreStart.Release();
            }
            m_ViewPreStart = null;
            m_ViewUpgrade = null;
			m_Info = null;
			m_Version = null;
			m_Progress = null;
		}

		public void UF_Update(){
			if (m_Progress != null) {
				float progress = UpgradeSystem.UF_GetInstance ().UpgradeProgress;
				if (System.Math.Abs(m_Progress.rawValue - progress) > 0.0001f) {
					m_Progress.rawValue = progress;
				}
			}

            UF_SetInfo(UpgradeSystem.UF_GetInstance ().UpgradeInfo);
		}


		public void UF_DisplayProgress(bool value){
			if (m_ViewUpgrade != null) {
				m_ViewUpgrade.UF_SetKActive("sl_progress", value);
			}
		}

		public void UF_SetProgress(float value){
			if (m_Progress != null) {
				m_Progress.rawValue = value;
			}
		}

		public void UF_SetInfo(string info){
			if (m_Info != null) {
				if (!string.IsNullOrEmpty (info)) {
					m_Info.text = info;
				}
			}
		}

		public void UF_SetVersion(string version){
			if (m_Version != null) {
				m_Version.text = version;
			}
		}

	}
}
