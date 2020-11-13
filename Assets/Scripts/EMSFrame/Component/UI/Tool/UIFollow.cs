//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace UnityFrame{
	
	public class UIFollow : UIObject,IOnReset
	{
		//是否使用距离缩放
		public bool useDistScale = false; 
		//使用世界空间跟踪
		public bool worldSpace = false;

		//基础缩放参数
		public float scaleFactor = 8;
		//偏移值
		public Vector3 offset = Vector3.zero;

		private Transform m_Target = null;

		private Vector3 m_UISpacePos = Vector3.zero;

		private Vector3 m_LastTargetPos = Vector3.zero;
//		private Vector3 mLastCameraPos = Vector3.zero;
		//		private bool m_bFirstSet = false;
		private Camera m_UICamera = null;
		//		private Transform mTransMainCam;
		private Transform m_TransUICam;
		private Transform m_TransSelf;

		public override void UF_SetActive(bool active){
			if (active) {
				this.gameObject.SetActive (true);
                UF_updateFollow();
			}
			else{
				this.gameObject.SetActive (false);	
			}

		}

		public override void UF_SetValue (object value){
			if (value == null) {return;}
			GameObject go = value as GameObject;
            UF_SetTarget(go);
		}

        protected override void Start()
        {
            m_UICamera = UIManager.UICamera;
            //			mTransMainCam = Camera.main.transform;
            m_TransUICam = m_UICamera.transform;
            m_TransSelf = gameObject.transform;
        }


        private void UF_updateFollow(){
			if (m_Target != null)
			{
				Vector3 targetpos = m_Target.position;
				//				Vector3 camerapos = mTransMainCam.position;
				//				if (!m_bFirstSet)
				//				{
				//					updaePlace();
				//					m_bFirstSet = true;
				//					return;
				//				}
				//				判断，如果对象的位置没有改变才调用此方法
				//				if (!MathX.VectorEquals(m_LastTargetPos,targetpos) || !MathX.VectorEquals(mLastCameraPos,camerapos))
				if (!MathX.UF_VectorEquals(m_LastTargetPos,targetpos))
				{
                    UF_updaePlace();
					//					mLastCameraPos = camerapos;
					m_LastTargetPos = targetpos;
				}
				//				updaePlace();
			}
		}



		private void UF_updaePlace()
		{
			if ( m_Target!= null)
			{
				if(m_TransSelf == null)
				{
					m_TransSelf = gameObject.transform;
				}

				Vector3 pos = m_Target.position + offset;


				if (worldSpace) {
					m_TransSelf.position = pos;
				} else {

					if (m_UICamera != null) {
						Vector3 vec = pos - m_TransUICam.position;
//						if (Vector3.Dot(m_TransUICam.forward, vec) < 0) {
//							m_UISpacePos = new Vector3(0, 0, 4000);
//						} else {
							Vector3 tmpW2Screem = Camera.main.WorldToScreenPoint(pos);
							m_UISpacePos = m_UICamera.ScreenToWorldPoint(tmpW2Screem);
							m_UISpacePos.z = 0;
//						}
						m_TransSelf.position = m_UISpacePos;
						if(useDistScale){
							float _size = Mathf.Clamp(scaleFactor/Vector3.Magnitude(vec),0,1);
							m_TransSelf.localScale = new Vector3(_size,_size,_size);
						}
					}
				}
			}
		}

		public void UF_SetPosition(Vector3 position){
			if (worldSpace) {
				this.gameObject.transform.position = position + offset;
			} else {
				if (m_UICamera != null) {
					Vector3 tmpW2Screem = Camera.main.WorldToScreenPoint (position);
					Vector3 uipos = m_UICamera.ScreenToWorldPoint (tmpW2Screem);
					uipos.z = 0;
					this.gameObject.transform.position = uipos;
				}
			}
		}

        public void UF_SetTarget(IUIUpdate ui)
        {
            UF_SetTarget(((MonoBehaviour)ui).gameObject);
        }

        public void UF_SetTarget(GameObject go){
            UF_SetTarget(go.transform);
        }

        public void UF_SetTarget(Transform tar)
        {
            //			m_bFirstSet = false;
            m_Target = tar;
            m_LastTargetPos = Vector3.zero;
            //			mLastCameraPos = Vector3.zero;
            UF_updaePlace();
        }

        public void UF_OnReset(){
			m_Target = null;
			//m_UICamera = null;
		}
			


		void LateUpdate()
		{
            UF_updateFollow();
		}


	}
}