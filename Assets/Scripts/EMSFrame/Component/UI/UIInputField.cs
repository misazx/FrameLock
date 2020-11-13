//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace UnityFrame{

	public class UIInputField : UnityEngine.UI.InputField,IUIUpdate
	{
		[SerializeField]private string m_UpdateKey;

		public string updateKey{
			get{ return m_UpdateKey;}
			set{ m_UpdateKey = value;}
		}

		public RectTransform rectTransform{get{ return this.transform as RectTransform;}}

		//检查字库中是否有无法显示的文字
		public bool UF_CheckFontCharInvalid(){
			if (this.textComponent == null || this.textComponent.font == null)
				return true;
			CharacterInfo o;
			for(int k = 0;k < this.textComponent.text.Length;k++){
				if(!this.textComponent.font.GetCharacterInfo (this.textComponent.text[k],out o)){
					return true;
				}
			}
			return false;
		}

		public void UF_SetActive(bool active){
			this.gameObject.SetActive (active);
		}

		public void UF_SetValue (object value){
			if (value == null) {return;}
			this.text = value.ToString ();
		}
			
	}

}