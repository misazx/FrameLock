//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UnityFrame
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	public class UIUpdateGroup : UIObject, IUIUpdateGroup, IOnReset
	{
		[HideInInspector] [SerializeField] private List<Object> m_ListUpdateUI = new List<Object>();
		//记静态加入的UI
		protected Dictionary<string, IUIUpdate> m_MapStaticUI = null;
        //记录动态加入的UI
        protected Dictionary<string, IUIUpdate> m_MapDynamicUI = null;

		public List<Object> ListUpdateUI { get { return m_ListUpdateUI; } }

        public Dictionary<string,IUIUpdate> MapStaticUI{
			get{ 
				if (m_MapStaticUI == null) {
					m_MapStaticUI = DictionaryPool<string,IUIUpdate>.Get ();
                    UF_MapStaticUIElement();
                }
				return m_MapStaticUI;
			}
			private set{ 
				if (m_MapStaticUI != null && value == null) {
					DictionaryPool<string,IUIUpdate>.Release (m_MapStaticUI);
				}
				m_MapStaticUI = null;
			}
		}

		public Dictionary<string,IUIUpdate> MapDynamicUI{
			get{
				if (m_MapDynamicUI == null) {
					m_MapDynamicUI = DictionaryPool<string,IUIUpdate>.Get ();
				}
				return m_MapDynamicUI;
			}
			private set{ 
				if (m_MapDynamicUI != null && value == null) {
					DictionaryPool<string,IUIUpdate>.Release (m_MapDynamicUI);
				}
				m_MapDynamicUI = null;
			}
		}

		public int dynamicCount{
			get{ if (m_MapDynamicUI != null)
					return m_MapDynamicUI.Count;
				else
					return 0;
			}
		}

		public int staticCount{
			get{ if (m_MapStaticUI != null)
				return m_MapStaticUI.Count;
			else
				return 0;
			}
		}
			
		public bool UF_SetKValue(string key,object value){
			IUIUpdate ui = this.UF_GetUI (key) as IUIUpdate;
			if (ui != null) {
				ui.UF_SetValue (value);
				return true;
			} else {
				return false;
			}
		}

		public bool UF_SetKActive(string key,bool value){
			IUIUpdate ui = this.UF_GetUI (key) as IUIUpdate;
			if (ui != null) {
				ui.UF_SetActive (value);
				return true;
			} else {
				return false;
			}
		}


		public void UF_SetGrey(bool opera){UIColorTools.UF_SetGrey(this.gameObject, opera);}
		public void UF_SetAlpha(float value){UIColorTools.UF_SetAlpha(this.gameObject, value);}
		public void UF_SetColor(UnityEngine.Color value){UIColorTools.UF_SetColor(this.gameObject, value);}
        public void UF_SetRenderAlpha(float value) { UIColorTools.UF_SetRenderAlpha(this.gameObject, value); }
        public void UF_SetRenderColor(UnityEngine.Color value) { UIColorTools.UF_SetRenderColor(this.gameObject, value); }
        public int UF_CrossAlpha(float vfrom, float vto, float duration, bool ingoreTimeScale) { return UIColorTools.UF_CrossAlpha(this.gameObject, vfrom, vto, duration, ingoreTimeScale); }
        public int UF_CrossColor(Color vfrom, Color vto, float duration, bool ingoreTimeScale){return UIColorTools.UF_CrossColor(this.gameObject, vfrom, vto, duration, ingoreTimeScale);}
        public int UF_CrossRenderAlpha(float vfrom, float vto, float duration, bool ingoreTimeScale) { return UIColorTools.UF_CrossRenderAlpha(this.gameObject, vfrom, vto, duration, ingoreTimeScale); }
        public int UF_CrossRenderColor(Color vfrom, Color vto, float duration, bool ingoreTimeScale) { return UIColorTools.UF_CrossRenderColor(this.gameObject, vfrom, vto, duration, ingoreTimeScale); }


        public bool UF_CheckUI(string key){
			return UF_GetUI(key) != null;
		}

		public bool UF_CheckDynamicUI(string key){
			return MapDynamicUI.ContainsKey (key);
		}

		public IUIUpdate UF_GetUI(string key){
			if (MapStaticUI.ContainsKey (key)) {
				return MapStaticUI [key];
			}
			else if (MapDynamicUI.ContainsKey (key)) {
				return MapDynamicUI [key];
			}
//			#if UNITY_EDITOR
//			Debugger.UF_Warn (string.Format("Group[{0}] UF_GetUI[{1}] is Null",this.name,key));
//			#endif
			return null;
		}

		public IUIUpdate UF_GetDynamicUI(string key){
			if (MapDynamicUI.ContainsKey (key)) {
				return MapDynamicUI [key];
			}
			return null;
		}

		public IUIUpdate UF_GetStaticUI(string key){
			if (MapStaticUI.ContainsKey (key)) {
				return MapStaticUI [key];
			}
			return null;
		}

			
		public Vector2 UF_GetUIPosition(string key){
			IUIUpdate ui = this.UF_GetUI (key) as IUIUpdate;
			if (ui != null) {
				return ui.rectTransform.anchoredPosition;
			} else {
				return Vector2.zero;
			}
		}

		public void UF_AddUI(IUIUpdate ui){
            UF_AddUI(ui, false);
		}

		public virtual void UF_AddUI(IUIUpdate ui,bool firstSibling){
			if(ui != null)
			{
				if (!string.IsNullOrEmpty(ui.updateKey) && !MapDynamicUI.ContainsKey (ui.updateKey)) 
				{
					MapDynamicUI.Add (ui.updateKey, ui);
				}
				else
				{
					//如果没有或有重复UpdateKey ，则使用InstanceID 来代替
					int insID = ((Object)ui).GetInstanceID();
					MapDynamicUI.Add (insID.ToString(),ui);
					Debugger.UF_Warn(string.Format("UI Update Key[{0}] Is Same Or Empty,Map With Instance ID:{1}",ui.updateKey ?? "",insID));
				}
				GameObject uiobject = (ui as MonoBehaviour).gameObject;
				uiobject.transform.SetParent (this.transform,false);
				uiobject.transform.localScale = Vector3.one;
				if (firstSibling)
					uiobject.transform.SetAsFirstSibling ();
			}
		}

		//只能移除动态类型
		public virtual void UF_RemoveUI(string key){
			if (MapDynamicUI.ContainsKey (key)) {
				IUIUpdate ui = MapDynamicUI [key];
                if (ui is IReleasable)
                {
                    (ui as IReleasable).Release();
                }
                else if (ui is IOnReset) {
					(ui as IOnReset).UF_OnReset ();
				}
				MapDynamicUI.Remove(key);
			}
		}
			
		//只能更改动态加入的UI UpdateKey
		public bool UF_ChangeUIUpdateKey(string source,string target){
			IUIUpdate ui = this.UF_GetDynamicUI(source);
			if (ui == null) {
				Debugger.UF_Warn(string.Format("Change UI UpdateKey Failed,Can not find UI[{0}] in Dynamic",source ?? ""));
				return false;
			}
			else if(MapDynamicUI.ContainsKey(target)){
				Debugger.UF_Warn(string.Format("Change UI UpdateKey Failed,Same UI UpdateKey{0}] Has Exist",target ?? ""));
				return false;
			}
			ui.updateKey = target;
            //移除原有Key
            MapDynamicUI.Remove(source);
            //添加新key
            MapDynamicUI.Add (target, ui);
			return true;
		}


        private void UF_MapStaticUIElement()
        {
            if (m_ListUpdateUI != null)
            {
                for (int k = 0; k < m_ListUpdateUI.Count; k++)
                {
                    IUIUpdate iuiupdate = m_ListUpdateUI[k] as IUIUpdate;
                    if (iuiupdate != null)
                    {
                        //init static map
                        if (!MapStaticUI.ContainsKey(iuiupdate.updateKey))
                        {
                            MapStaticUI.Add(iuiupdate.updateKey, iuiupdate);
                        }
                        else
                        {
                            if (Debugger.IsActive)
                                Debugger.UF_Warn(string.Format("Same Update Key [{0}] in UI[{1}]", this.name, iuiupdate.updateKey));
                        }
                    }
                }
            }
        }


        protected void UF_ResetMapUI(Dictionary<string,IUIUpdate> mapUI,bool needClear,bool needDisable){
			if(mapUI != null){
				foreach (KeyValuePair<string,IUIUpdate> item in mapUI) {
					var ui = item.Value;
					if (ui != null && !this.Equals(ui))
                    {
                        if (ui is IReleasable)
                        {
                            (ui as IReleasable).Release();
                            if(needDisable){ ui.UF_SetActive(false); }
                        }
                        else if (ui is IOnReset) {
							(ui as IOnReset).UF_OnReset ();
						}
					}
				}
				if(needClear)
					mapUI.Clear ();
			}
		}

		public virtual void UF_OnReset ()
		{
			if (!Application.isPlaying)
				return;
            UF_ResetMapUI(m_MapStaticUI,false,false);
            UF_ResetMapUI(m_MapDynamicUI,true,true);
		}

			

		protected override void OnDestroy ()
		{
			MapStaticUI = null;
			MapDynamicUI = null;
		}





	}
}

