//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

namespace UnityFrame{
	public class UIScrollView :ScrollRect,IUIUpdate
	{

		[SerializeField]private string m_UpdateKey;

		public string updateKey{
			get{ return m_UpdateKey;}
			set{ m_UpdateKey = value;}
		}
		public RectTransform rectTransform{get{ return this.transform as RectTransform;}}

		public void UF_SetActive(bool active){
			this.gameObject.SetActive (active);
		}

		public void UF_SetValue (object value){
			if (value == null) {return;}
		}


		protected Vector2 UF_FixVailPos(Vector2 pos){
			if (this.content != null) {
				if (vertical) {
					float sizey = this.content.sizeDelta.y;
					float viewy = this.rectTransform.sizeDelta.y;
					pos.y = Mathf.Clamp (pos.y, pos.y + viewy, sizey - viewy);
				} else {
					pos.y = this.content.anchoredPosition.y;
				}

				if (horizontal) {
					float sizex = this.content.sizeDelta.x;
					float viewx = this.rectTransform.sizeDelta.x;
					pos.x = Mathf.Clamp (pos.x, pos.x - viewx, -sizex + viewx);
				} else {
					pos.x = this.content.anchoredPosition.x;
				}
			}
			return pos;
		}

		public void UF_SetToPos(Vector2 pos){
			if (this.content == null)
				return;
			this.content.anchoredPosition = UF_FixVailPos(pos);
		}


		public int UF_SmoothToPos(Vector2 pos,float duration){
			if (this.content == null)
				return 0;
			if (duration == 0) {
                UF_SetToPos(pos);
				return 0;
			}
			Vector2 tpos = UF_FixVailPos(pos);
			Vector2 spos = this.content.anchoredPosition;
			return FrameHandle.UF_AddCoroutine (UF_ISmoothToPos(spos,tpos,duration));
		}


		IEnumerator UF_ISmoothToPos(Vector2 spos,Vector2 tpos,float duration){
			float progress = 0;
			float tickbuf = 0;
			while(true){
				if (this.content == null || progress >= 1)
					yield break;
				tickbuf += GTime.DeltaTime;
				progress = Mathf.Clamp01(tickbuf / duration);
				this.content.anchoredPosition = Vector2.Lerp (spos,tpos,progress);
				yield return null;

			}

		}




	}

}