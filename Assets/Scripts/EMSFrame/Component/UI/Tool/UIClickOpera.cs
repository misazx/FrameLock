//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;


namespace UnityFrame
{
	public class UIClickOpera :  UIObject,IPointerDownHandler, IPointerUpHandler,IPointerClickHandler, IEventSystemHandler {

		const float IntervalDoubleClick = 0.25f;

		private float m_ClickLastTime = 0;

		[SerializeField]private UnityEvent m_UEClick;
		[SerializeField]private UnityEvent m_UEPressDown;
		[SerializeField]private UnityEvent m_UEPressUp;
		[SerializeField]private UnityEvent m_UEDoubleClick;

		public void UF_InvokeClick(){
			if (m_UEClick != null) {
				m_UEClick.Invoke ();
			}
		}

		public void UF_InvokeDoubleClick(){
			if (m_UEDoubleClick != null) {
				m_UEDoubleClick.Invoke ();
			}
		}

		public void UF_InvokePressDown(){
			if (m_UEPressDown != null) {
				m_UEPressDown.Invoke ();
			}
		}
		public void UF_InvokePressUp(){
			if (m_UEPressUp != null) {
				m_UEPressUp.Invoke ();
			}
		}

		public void OnPointerClick (PointerEventData eventData){
			this.UF_InvokeClick();

			float clickDetla = (Time.unscaledTime - m_ClickLastTime);
			if (clickDetla <= IntervalDoubleClick) {
				m_ClickLastTime = 0;
				this.UF_InvokeDoubleClick();
			}
		}

		public void OnPointerDown (PointerEventData eventData)
		{
			this.UF_InvokePressDown();
		}
			
		public void OnPointerUp (PointerEventData eventData)
		{
			this.UF_InvokePressUp();
		}


	}

}
