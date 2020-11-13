//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace UnityFrame{
	public class UIDropdown : Dropdown,IUIUpdate {

		public string eValueChange = "";

		public string eParam= "";

		private int m_SelectIdx = 0;

		[SerializeField]private string m_UpdateKey;

		public string updateKey{
			get{ return m_UpdateKey;}
			set{ m_UpdateKey = value;}
		}

		public void UF_SetActive(bool active){
			this.gameObject.SetActive (active);	
		}

		public void UF_SetValue (object value){
			if (value == null) {return;}
		}

		public RectTransform rectTransform{get{ return this.transform as RectTransform;}}

		public override void OnSelect (UnityEngine.EventSystems.BaseEventData eventData)
		{
			base.OnSelect (eventData);
			if (m_SelectIdx != this.value) {
				m_SelectIdx = this.value;
				//发送UI事件
				if (!string.IsNullOrEmpty (eValueChange)) {
					MessageSystem.UF_GetInstance ().UF_Send(DefineEvent.E_UI_OPERA,eValueChange, eParam,this);
				}
			}
		}

		public void UF_AddOption(string info)
		{
			this.options.Add (new OptionData(info));
		}

		public void UF_RemoveOption(int idx){
			this.options.RemoveAt (idx);
		}

		//执行Refresh后，AddOption 或 UF_RemoveOption 才会生效
		public void UF_Refresh(){
			this.RefreshShownValue ();
		}

		public void UF_ClearOption(){
			this.options.Clear ();
			this.RefreshShownValue ();
		}

		public void UF_RemoveOption(string info){
			int k = 0;
			for (k = 0; k < this.options.Count; k++) {
				if (this.options [k].text == info) {
					break;
				}
			}
			if (k != 0) {
				UF_RemoveOption (k);
			}
		}


		public string SelectOptionText{
			get{ 
				if (this.options.Count > 0) {
					return this.options [Mathf.Clamp(m_SelectIdx,0,this.options.Count - 1)].text;
				} else {
					return "";
				}
			}
		}




	}

}
