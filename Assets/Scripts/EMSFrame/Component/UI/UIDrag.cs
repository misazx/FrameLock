//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UnityFrame{
	[DisallowMultipleComponent]
	public class UIDrag : UIObject,IPointerDownHandler, IPointerUpHandler, IEventSystemHandler,IUIUpdate {
		public bool canDrag = true;
		public bool autoBack = true;
		public bool centerAligned = true;
		public float smoothBack = 0f;
		public string ePressDown = string.Empty;
		public string ePressUp = string.Empty;
		public string eParam = string.Empty;

		private Vector3 m_SourceLPosition;
		private Vector3 m_AlignedDPos;

		private bool m_IsDragging = false;

		private UIDragGroup m_DragGroup;

		public override void UF_SetValue (object value){
			if (value == null) {return;}
			eParam = value.ToString ();
		}

		public void UF_SetDragGroup(UIDragGroup dGroup){
			m_DragGroup = dGroup;
		}


		private Vector3 UF_GetPressPosition(){
			Vector3 pos = UIManager.UICamera.ScreenToWorldPoint (DeviceInput.UF_PressPosition(0));
			pos.z = this.transform.position.z;
			return pos;
		}

		public void OnPointerDown (PointerEventData eventData){
			if (!this.enabled) {
				return;
			}
			if (DeviceInput.UF_Down(0)) {
				m_SourceLPosition = this.transform.localPosition;
				m_IsDragging = canDrag && true;
				if (!centerAligned) {
					m_AlignedDPos = this.transform.position - UF_GetPressPosition();
				}
				if (!string.IsNullOrEmpty (ePressDown)) {
					MessageSystem.UF_GetInstance ().UF_Send(DefineEvent.E_UI_OPERA,ePressDown, eParam,this);
				}
			}
		}

		public void OnPointerUp (PointerEventData eventData){
			if (!this.enabled) {
				return;
			}
			if (DeviceInput.UF_Up(0)) {
				//Vector2 anchoredPosition = this.rectTransform.anchoredPosition;
				if (autoBack) {
					if (smoothBack <= 0) {
						this.transform.localPosition = m_SourceLPosition;
					} else {
						FrameHandle.UF_AddCoroutine (UF_ISmoothBack(this.transform.localPosition, m_SourceLPosition, smoothBack));
					}
				}

				m_IsDragging = canDrag && false;

				//判断Group中的碰撞
				if (m_DragGroup != null) {
					m_DragGroup.UF_UpdateCollision(this,eventData);
				}

				if (!string.IsNullOrEmpty (ePressUp)) {
					MessageSystem.UF_GetInstance ().UF_Send(DefineEvent.E_UI_OPERA,ePressUp, eParam,this);
				}
			}
		}

		IEnumerator UF_ISmoothBack(Vector3 vfrom,Vector3 vto,float duration){
			float progress = 0;
			float tickBuff = 0;
			while (progress < 1) {
				tickBuff += GTime.UnscaleDeltaTime;
				progress = Mathf.Clamp01(tickBuff / duration);
				Vector3 current = progress * vto + (1 - progress) * vfrom;
				this.transform.localPosition = current;
				yield return null;
			}
		}

		void Update(){
			if (m_IsDragging) {
				if (centerAligned) {
					this.transform.position = UF_GetPressPosition();
				} else {
					this.transform.position = UF_GetPressPosition() + m_AlignedDPos;
				}
			}
		}

	}
}