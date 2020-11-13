//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityFrame{
	public class UIDragGroup : UIObject,IOnReset{

		//拖拽放下事件派发
		public string eDragDown = "";

		//允许触碰多个
		public bool allowMultiCollision = false;

		[SerializeField]private List<UIDrag> m_Drags = new List<UIDrag>();

//		private Vector2[] mPoints = new Vector2[4];

		//记录原列表索引，用于Reset时移除对UIDrag的引用，让GC内存更碎片化
		private int m_PtrSource = 0;

		public List<UIDrag> drags{get{return m_Drags;}}

		public override void UF_SetValue (object value){
			if (value == null) {return;}
			UIDrag drag = value as UIDrag;
            UF_Add(drag);
		}

        protected override void Awake()
        {
            base.Awake();
            if (m_Drags != null)
            {
                for (int k = 0; k < m_Drags.Count; k++)
                {
                    if (m_Drags[k] != null)
                    {
                        m_Drags[k].UF_SetDragGroup(this);
                    }
                }
                m_PtrSource = m_Drags.Count;
            }
        }


        public void UF_Add(UIDrag drag){
			if (drag != null && !m_Drags.Contains(drag)) {
				m_Drags.Add (drag);
				drag.UF_SetDragGroup(this);
			}
		}

		public void UF_Remove(UIDrag drag){
			if (drag != null) {
				if (m_Drags.Remove (drag)) {
					drag.UF_SetDragGroup(null);
				}
			}
		}

		private Rect UF_getRect(Rect rect,Vector3 pos){
			rect.x = pos.x + rect.x;
			rect.y = pos.y + rect.y;
			return rect;
		}
//
//		private bool checkIsDragCollsion(Rect a,Rect b){
//			//			Rect rect = Rect.zero;
//			//			Rect pice = Rect.zero;
//			//			//比较面积
//			//			if (a.width * a.height > b.width * b.height) {
//			//				rect = a;	
//			//				pice = b;
//			//			} else {
//			//				rect = b;
//			//				pice = a;
//			//			}
//			//
//			//			mPoints [0] = new Vector2 (pice.x, pice.y);
//			//			mPoints [1] = new Vector2 (pice.x + pice.width, pice.y);
//			//			mPoints [2] = new Vector2 (pice.x + pice.width, pice.y + pice.height);
//			//			mPoints [3] = new Vector2 (pice.x, pice.y + pice.height);
//			//
//			//			for(int k = 0;k < mPoints.Length;k++){
//			//				if (rect.Contains (mPoints [k])) {
//			//					return true;
//			//				}
//			//			}
//
//			Vector2 point = new Vector2 (a.x + a.width / 2.0f, a.y + a.height / 2);
//			return b.Contains (point);
//
//		}
//
//
//		private bool CheckCollsion(Rect box,Vector2 pos){
//			return box.Contains (pos);
//		}
//
//
//
//		public void UpdateCollision(UIDrag target,Vector2 pos){
//			if (m_Drags != null && m_Drags.Count > 0) {
//				UIDrag drag = null;
//				for (int k = 0; k < m_Drags.Count; k++) {
//					drag = m_Drags [k];
//					if (drag != null && drag!= target && drag.gameObject.activeInHierarchy && drag.enabled) {
////						if (checkIsDragCollsion (getRect (target.rectTransform.rect, pos),getRect (drag.rectTransform.rect, drag.rectTransform.anchoredPosition))) {
//						if(CheckCollsion(getRect (drag.rectTransform.rect, drag.rectTransform.anchoredPosition),pos)){
//							SendEventMessage (target, m_Drags [k]);
//							break;
//						}
//					}
//				}
//			}
//		}
//

		//EventSystem 射线方式检测，适配各种对其
		internal void UF_UpdateCollision(UIDrag target,PointerEventData eventData){

			if (m_Drags != null && m_Drags.Count > 0) {

				List<UnityEngine.EventSystems.RaycastResult> listRaycastResult = ListCache<UnityEngine.EventSystems.RaycastResult>.Acquire ();
				UnityEngine.EventSystems.EventSystem.current.RaycastAll (eventData, listRaycastResult);
				if (listRaycastResult.Count > 0) {
					for (int i = 0; i < listRaycastResult.Count; i++) {
						var go = listRaycastResult [i].gameObject;
						for (int k = 0; k < m_Drags.Count; k++) {
							if (m_Drags [k] != null&& target.gameObject != go && m_Drags [k].gameObject == go) {
                                UF_SendEventMessage(target, m_Drags [k]);
								if (!allowMultiCollision) {
									ListCache<UnityEngine.EventSystems.RaycastResult>.Release(listRaycastResult);
									return;
								}
							}
						}
					}
				}
				ListCache<UnityEngine.EventSystems.RaycastResult>.Release(listRaycastResult);
			}

		}

		private void UF_SendEventMessage(UIDrag source,UIDrag target){
			if (!string.IsNullOrEmpty (eDragDown)) {
				MessageSystem.UF_GetInstance ().UF_Send(DefineEvent.E_UI_OPERA,eDragDown,source,target,this);

			}
		}


		public void UF_OnReset (){
			UIDrag drag = null;
			for (int k = m_PtrSource; k < m_Drags.Count; k++) {
				drag = m_Drags [k];
				if (drag != null) {
					drag.UF_SetDragGroup(null);
				}
				m_Drags.RemoveAt (k);
				k--;
			}
		}


	}
}