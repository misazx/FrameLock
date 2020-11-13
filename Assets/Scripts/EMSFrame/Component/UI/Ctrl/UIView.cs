//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityFrame {
	//作为实体控制器使用
	public class UIView : UIItem, ISortingRoot
    {
		//界面类型
		public enum ShowType
		{
			FULL = 1,	//全屏，被全屏类型覆盖的View将会自动隐藏，直到栈前面没有全屏类型，自动隐藏的View将重新显示
			WINDOWS,	//窗口类型，窗口类型将会一直叠加，直到被全屏类型View覆盖，窗口View将自动隐藏
			ALWAYS,		//总是可见类型，不受全屏影响，不会自动隐藏，直到在显示栈中移除
			CONTENT,	//作为内容展示，作用在界面中作为内容界面展示，显示模式类似ALWAYS,依赖于上层界面
		}

		public ShowType viewType = ShowType.FULL;

        public float releaseDelay = 0;

        [HideInInspector][SerializeField]private UnityEvent m_EventOnShow;
		[HideInInspector][SerializeField]private UnityEvent m_EventOnClose;

        [HideInInspector] [SerializeField]private int m_viewOrder;

        private int m_StackOrder;

        private int m_Order;

        private int m_HandleClosing = 0;

        private Canvas m_InternalCanvas;

        public UnityEvent eventOnShow{get{ return m_EventOnShow;}}

		public UnityEvent eventOnClose{get{ return m_EventOnClose;}}

        // 栈层显示顺序
        public int stackOrder {
            get {
                return m_StackOrder;
            }
            set {
                m_StackOrder = value;
                UF_ApplyCanvasSortingOrder();
            }
        }

        // 视图层显示顺序
        public int viewOrder {
            get
            {
                return m_viewOrder;
            }
            set
            {
                m_viewOrder = value;
                UF_ApplyCanvasSortingOrder();
            }
        }

        public int sortingOrder {
            get {
                return m_Order;
            }
            set {
                Debugger.UF_Error("Can not Modify sortingOrder in UIView,Use viewOrder to instead");
            }
        }

        private void UF_ApplyCanvasSortingOrder() {
            //canvas sortingOrder 受 stackOrder 与 view sortingOrder 共同影响
            //viewOrder拥有绝对排序优势
            m_Order = m_viewOrder * 1000 + stackOrder * 100;
            if (m_InternalCanvas != null)
            {
                if(!m_InternalCanvas.overrideSorting)
                    m_InternalCanvas.overrideSorting = true;
                m_InternalCanvas.sortingOrder = sortingOrder;
            }
        }

        public bool isSortingValidate {
            get {
                //content 不需要排序
                return viewType != ShowType.CONTENT;
            }
        }

        private void UF_InitInternalCanvas() {
            if (!isSortingValidate)
                return;
            if (m_InternalCanvas == null)
            {
                m_InternalCanvas = this.gameObject.GetComponent<Canvas>();
                if (m_InternalCanvas == null)
                {
                    m_InternalCanvas = this.gameObject.AddComponent<Canvas>();
                    this.gameObject.AddComponent<GraphicRaycaster>();
                }
            }
        }

		public void UF_SetViewToTop(){
			this.transform.SetAsFirstSibling ();
		}

		public void UF_SetViewToBottom(){
			this.transform.SetAsLastSibling();
		}

		public int UF_GetViewType(){
			return (int)viewType;
		}

		public void UF_SetViewType(int type){
			viewType = (ShowType)type;
		}


		public void UF_InvokeShowEvent(){
			if (m_EventOnShow != null && m_EventOnShow.GetPersistentEventCount() > 0) {
				
				m_EventOnShow.Invoke ();
			}
		}

		public void UF_InvokeCloseEvent(){
			if (m_EventOnClose != null && m_EventOnShow.GetPersistentEventCount() > 0) {
				m_EventOnClose.Invoke ();
			}
		}


		public void UF_Hide(bool value){
			this.UF_SetActive (value);
		}


        protected override void Awake()
        {
            base.Awake();
            UF_InitInternalCanvas();
        }

        public void PlaySound(string wavName) {
            AudioManager.UF_GetInstance().UF_Play(wavName);
        }

        public void UF_OnShow(){
			if (m_HandleClosing > 0) {
				FrameHandle.UF_RemoveCouroutine (m_HandleClosing);
				m_HandleClosing = 0;
			}
			this.UF_SetActive (true);

            UF_ApplyCanvasSortingOrder();

            UF_InvokeShowEvent();
		}


		public void UF_OnClose(){
            //this.UF_SetActive (false);
            UF_InvokeCloseEvent();

            if (releaseDelay <= 0) {
                this.UF_OnReset();
                this.Release();
            }
			else {
				FrameHandle.UF_AddCoroutine (UF_IDelayRelease(releaseDelay));
			}
		}

        public override void UF_OnReset()
        {
            base.UF_OnReset();
            this.m_StackOrder = 0;
        }

        IEnumerator UF_IDelayRelease(float delay){
			yield return new WaitForSeconds (delay);
            this.UF_OnReset();
            this.Release ();
		}



    }
}