//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

namespace UnityFrame
{
	public class UIToggle : UnityEngine.UI.Toggle, IUIUpdate, IOnReset
	{
		[SerializeField]private string m_UpdateKey;
		[SerializeField]private UIEventClicker m_EventClicker = new UIEventClicker();

		[SerializeField]private UnityEvent m_UEValueTrue;

		[SerializeField]private UnityEvent m_UEValueFalse;

		private ToggleGroup m_SourceGroup;

		private bool m_IsGrey = false;

		public UIEventClicker eventClicker{ get{ return m_EventClicker;}}

		public string updateKey
		{
			get { return m_UpdateKey; }
			set { m_UpdateKey = value;}
		}
		public RectTransform rectTransform{get{ return this.transform as RectTransform;}}
		public bool grey{get{ return m_IsGrey;}set{this.UF_SetGrey (value);}}
		public bool enable{get{return this.enabled;}set{ this.UF_SetEnable(value);}}

		public void UF_SetActive(bool active)
		{
			this.gameObject.SetActive(active);
		}

		public void UF_SetValue (object value){
			if (value == null) {return;}
		}

		public string ePressClick{
			set{m_EventClicker.ePressClick = value;}
			get{return m_EventClicker.ePressClick;}
		}

		public string eParamA{
			set{m_EventClicker.UF_SetEParam(value);}
			get{return m_EventClicker.UF_GetParam(0);}
		}

		public string eParamB{
			set{m_EventClicker.UF_SetEParam(1,value);}
			get{return m_EventClicker.UF_GetParam(1);}
		}

		public string eParamC{
			set{m_EventClicker.UF_SetEParam(2,value);}
			get{return m_EventClicker.UF_GetParam(2);}
		}
			
		private void UF_onValueHandle(bool isSelect)
		{
			if (!isSelect)
			{
				if (m_UEValueFalse != null) {m_UEValueFalse.Invoke ();}
			}
			else
			{
				if (m_UEValueTrue != null){m_UEValueTrue.Invoke();}
			}
		}
			
		public void UF_SetEParam(int index, string param)
		{
			m_EventClicker.UF_SetEParam(index, param);
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


		protected override void Awake()
		{
			onValueChanged.AddListener(UF_onValueHandle);
			base.Awake();
			if (m_SourceGroup == null) {
				m_SourceGroup = this.group;
			}
		}

		public override void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
		{
			base.OnPointerClick(eventData);
			m_EventClicker.UF_OnClick(this);
		}

		public override void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
		{
			base.OnPointerDown(eventData);
			m_EventClicker.UF_OnDown(this);
		}

		public override void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData)
		{
			base.OnPointerUp(eventData);
			m_EventClicker.UF_OnUp(this);
        }


		public void UF_SetGroup(UnityEngine.UI.ToggleGroup togglegroup)
		{
			if (m_SourceGroup == null) {
				m_SourceGroup = this.group;
			}
            if (this.group != null && togglegroup == this.group)
                return;
            if (this.group != null)
			{
				group.UnregisterToggle(this);
			}
			this.group = null;
			if (togglegroup != null)
			{
				this.group = togglegroup;
			}
		}

		public void Click(){
			if (this.onValueChanged != null) {
				isOn = !isOn;
			}
			this.m_EventClicker.UF_OnClick(this);
		}

		public void UF_OnReset()
		{	
			if (this.group != null && m_SourceGroup != this.group)
			{
				group.UnregisterToggle(this);
				this.group = m_SourceGroup;
			}

			this.isOn = false;
		}



	}

}