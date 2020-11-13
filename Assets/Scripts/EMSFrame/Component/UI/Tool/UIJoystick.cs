//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

namespace UnityFrame
{
	public class UIJoystick : UIObject,IOnReset, IPointerDownHandler,IPointerUpHandler
    {
        public float limtPointDistance = 150.0f;
		public float deadZoom = 20;
		public float gravity = 0;
        public float resumeSpeed = 0.1f;

        public GameObject joyRoot;
        public GameObject joyRange;
        public GameObject joyPoint;

        public bool rotateRange;
        public bool rootRevert;


        private Vector3 m_SourceDragPos = Vector3.zero;
		private Vector3 m_TargetDragPos = Vector3.zero;
		private Vector3 m_MoveForward = Vector3.zero;

        private Vector3 m_SourceRootPos;

        private bool m_IsPress = false;
        private bool m_IsActive = true;
        private bool m_IsResumePoint = false;

        private DelegateMethod m_EventJoyPressDown;
        private DelegateMethod m_EventJoyPressMove;
        private DelegateMethod m_EventJoyPressUp;

        [SerializeField]private UnityEvent m_UEventPressDown;
        [SerializeField]private UnityEvent m_UEventPressUp;

        public bool active { get { return m_IsActive; } set { m_IsActive = value; gameObject.SetActive(m_IsActive); if (!m_IsActive) m_IsPress = false; } }
        public bool isPress { get { return m_IsPress; } }
        public Vector3 moveForward { get { return m_MoveForward; } }
        protected Camera uiCamera { get { return UIManager.UICamera; } }
        protected bool maskGraphicTrigger { get; set; }

        


        

        protected override void Awake()
        {
            base.Awake();
            if (joyRoot == null) {
                joyRoot = this.gameObject;
            }
            m_SourceRootPos = joyRoot.transform.localPosition;
            if (joyRange == null) {
                joyRange = new GameObject("range");
                joyRange.transform.parent = joyRoot.transform;
            }   
            if (joyPoint == null)
            {
                joyPoint = new GameObject("point");
                joyPoint.transform.parent = joyRange.transform;
            }

        }

        public void UF_SetEventJoyPressDown(DelegateMethod method) {
            m_EventJoyPressDown = method;
        }


        public void UF_SetEventJoyPressMove(DelegateMethod method)
        {
            m_EventJoyPressMove = method;
        }

        public void UF_SetEventJoyPressUp(DelegateMethod method)
        {
            m_EventJoyPressUp = method;
        }


        private void UF_CallDelegateEvent(DelegateMethod method){
			if (method != null) {
                method.Invoke(this);
			}
		}

        private void UF_CallUnityEvent(UnityEvent uevent)
        {
            if (uevent != null)
            {
                uevent.Invoke();
            }
        }

        public void UF_ResetGraphicTrigger() {
            maskGraphicTrigger = false;
        }


        private void UF_SetjoyRange(GameObject range){
			joyRange = range;
		}

		public void UF_SetJoyPoint(GameObject point){
			joyPoint = point;
		}



        protected void UF_ResumePoint() {
            m_IsResumePoint = true;
            mResumeTick = 0;
            mResumeCurPos = joyPoint.transform.localPosition;
        }



        public void OnPointerDown(PointerEventData eventData) {
            maskGraphicTrigger = true;
        }

        public void OnPointerUp(PointerEventData eventData) {
            maskGraphicTrigger = false;
        }


        // Update is called once per frame
        void Update () {
			if(!active)
				return;
            UpdateInput();
            UF_UpdateResumePoint();
        }


        private Vector3 mResumeCurPos;
        private float mResumeTick;

        private void UF_UpdateResumePoint() {
            if (!m_IsResumePoint)
                return;
            if (resumeSpeed <= 0)
            {
                joyPoint.transform.localPosition = Vector3.zero;
                m_IsResumePoint = false;
                return;
            }

            mResumeTick += GTime.DeltaTime;
            float progress = Mathf.Clamp01(mResumeTick / resumeSpeed);
            joyPoint.transform.localPosition = (1 - progress) * mResumeCurPos;
            if (progress >= 1) {
                joyPoint.transform.localPosition = Vector3.zero;
                
                m_IsResumePoint = false;
            }
        }


        private void UF_UpdateRevertRoot() {
            if (!rootRevert)
                return;
            joyRoot.transform.localPosition = m_SourceRootPos;
        }


        private void UF_UpdateRotate(Vector3 forward) {
            if(rotateRange){
                var vAngle =  MathX.UF_EulerAngle(forward);
                if (joyRange != null) {
                    var e = joyRange.transform.localEulerAngles;
                    e.z = -vAngle.y;
                    joyRange.transform.localEulerAngles = e;

                }
            }
        }

        private void UpdateInput(){
			if(maskGraphicTrigger && DeviceInput.UF_Press(0)){
                if (uiCamera == null)
                    return;
                //停止复位
                m_IsResumePoint = false;

                if (!m_IsPress){
					m_SourceDragPos = DeviceInput.UF_DownPosition(0);
					joyRoot.gameObject.transform.position = uiCamera.ScreenToWorldPoint(m_SourceDragPos);
					m_IsPress = true;
                    UF_CallDelegateEvent(m_EventJoyPressDown);
                    UF_CallUnityEvent(m_UEventPressDown);
                }
				if(m_IsPress){
					m_TargetDragPos = DeviceInput.UF_PressPosition(0);
					float dist = Vector3.Distance(m_SourceDragPos,m_TargetDragPos);
                    float limitV = limtPointDistance * Screen.width / UIManager.FixedWidth;

                    if (dist > limitV)
                    {
						m_TargetDragPos = m_SourceDragPos +  (m_TargetDragPos - m_SourceDragPos).normalized * limitV;
					}
                    if(joyPoint != null)
					    joyPoint.gameObject.transform.position = uiCamera.ScreenToWorldPoint(m_TargetDragPos);
//					float angle = 0;
					Vector3 _moveVector = MathX.UF_Foward(m_SourceDragPos, m_TargetDragPos);
                    if (dist > deadZoom)
                    {
                        m_MoveForward = new Vector3(_moveVector.x, gravity, _moveVector.y).normalized;
                        UF_CallDelegateEvent(m_EventJoyPressMove);
                        UF_UpdateRotate(m_MoveForward);
                    }
				}
			}
			else{
				if(m_IsPress){
                    m_IsPress = false;
                    UF_ResumePoint(); UF_UpdateRevertRoot();
                    UF_CallDelegateEvent(m_EventJoyPressUp);
                    UF_CallUnityEvent(m_UEventPressUp);
                }
			}
		}


        public void UF_OnReset() {
            m_IsPress = false;
            maskGraphicTrigger = false;

        }

	}
}

