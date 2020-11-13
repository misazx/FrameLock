//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityFrame
{
	public class UIButton : UnityEngine.UI.Button, IUIUpdate,IOnReset
	{
		//忽略蒙版，穿透点击
		public bool ingoreMask = false;

		[SerializeField]private string m_UpdateKey;
		[SerializeField]private UIEventClicker m_EventClicker = new UIEventClicker();

		private bool m_IsGrey = false;
		private Vector2 m_ClickPosition = Vector2.zero;
        private Vector2 m_PressPosition = Vector2.zero;

        public Vector2 clickPosition { get { return m_ClickPosition; } }
        public Vector2 pressPosition { get { return m_PressPosition; } }

        public string updateKey{get { return m_UpdateKey; }set { m_UpdateKey = value; }}

		public UIEventClicker eventClicker{ get{ return m_EventClicker;}}

		public RectTransform rectTransform{get{ return this.transform as RectTransform;}}

		public void UF_SetActive(bool active)
		{
			this.gameObject.SetActive(active);
		}

		public void UF_SetValue (object value){
			if (value == null) {return;}
			this.eParamA = value.ToString();

		}
			
		public string ePressClick{
			set{eventClicker.ePressClick = value;}
			get{return eventClicker.ePressClick;}
		}
			
		public string eParamA{
			set{eventClicker.UF_SetEParam(value);}
			get{return eventClicker.UF_GetParam(0);}
		}

		public string eParamB{
			set{eventClicker.UF_SetEParam(1,value);}
			get{return eventClicker.UF_GetParam(1);}
		}

		public string eParamC{
			set{eventClicker.UF_SetEParam(2,value);}
			get{return eventClicker.UF_GetParam(2);}
		}
			
		public bool grey{get{ return m_IsGrey;}set{this.UF_SetGrey (value);}}

		public bool enable{get{return this.enabled;}set{ this.UF_SetEnable(value);}}

		public void SetEParam(int index, string param)
		{
			eventClicker.UF_SetEParam(index, param);
		}

		public void UF_SetGrey(bool opera){
			if (m_IsGrey != opera) {
				m_IsGrey = opera;
				UIColorTools.UF_SetGrey(this.gameObject,opera);
			}
		}

		public void UF_SetEnable(bool opera)
		{
			this.enabled = opera;
			UF_SetGrey(!opera);
		}

		private void PointerClick(IPointerClickHandler clicker,PointerEventData eventData){
			if (clicker != null) {
				clicker.OnPointerClick (eventData);
			}
		}

		private IPointerClickHandler UF_GetPointerClickHandler(GameObject gobj){
			IPointerClickHandler ret = gobj.GetComponent<IPointerClickHandler>();
			return ret;
		}


		private void UF_OnIngoreMask(PointerEventData eventData){
			List<UnityEngine.EventSystems.RaycastResult> listRaycastResult = ListCache<UnityEngine.EventSystems.RaycastResult>.Acquire ();
			UnityEngine.EventSystems.EventSystem.current.RaycastAll (eventData, listRaycastResult);
			if (listRaycastResult.Count > 0) {
				int idx = 0;
				for (idx = 0; idx < listRaycastResult.Count; idx++) {
					if (listRaycastResult [idx].gameObject == this.gameObject) {
						//往下没有button
						if (++idx < listRaycastResult.Count) {
							//穿透下一个,触发下一个事件
							PointerClick(UF_GetPointerClickHandler(listRaycastResult [idx].gameObject),eventData);
						}
						return;
					}
				}
				//没有目标对象，则第一个索引为下一个clicker
				PointerClick(UF_GetPointerClickHandler(listRaycastResult [0].gameObject),eventData);
			}
			ListCache<UnityEngine.EventSystems.RaycastResult>.Release(listRaycastResult);
		}
			
		public override void OnPointerClick(PointerEventData eventData)
		{
			if (!this.enabled)
				return;

			m_ClickPosition = eventData.position;

			base.OnPointerClick(eventData);

			if (!eventClicker.UF_OnDoubleClick(this)) {
				eventClicker.UF_OnClick(this);
			}

			if (ingoreMask) {
				eventData.Reset ();
                UF_OnIngoreMask(eventData);
			}
		}


		public override void OnPointerDown(PointerEventData eventData)
		{
            m_PressPosition = eventData.position;
            base.OnPointerDown(eventData);
			this.eventClicker.UF_OnDown(this);
        }

		public override void OnPointerUp(PointerEventData eventData)
		{
            m_PressPosition = eventData.position;
            base.OnPointerUp(eventData);
			this.eventClicker.UF_OnUp(this);
		}


		public void Click(){
			if (this.onClick != null) {
				this.onClick.Invoke ();
			}
			this.eventClicker.UF_OnClick(this);
		}


        public void UF_OnReset() {
            UF_SetGrey(false);
        }


	}

}