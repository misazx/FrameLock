//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace UnityFrame{
	
	public class UIToggleGroup : UnityEngine.UI.ToggleGroup,IUIUpdate
	{

		[SerializeField]private string m_UpdateKey;

		public string updateKey{
			get{ return m_UpdateKey;}
			set{ m_UpdateKey = value;}
		}
			
		public RectTransform rectTransform{get{ return this.transform as RectTransform;}}

		public void Add(UIToggle toggle){
			if (toggle != null) {
				toggle.UF_SetGroup(this);
			}
		}

		public void UF_SetActive (bool active){
			this.gameObject.SetActive (active);
		}

		public void UF_SetValue (object value){
			if (value == null) {return;}
			UIToggle togger = value as UIToggle;
			if (togger != null) {
				togger.UF_SetGroup(this);
			}
		}



	}


}