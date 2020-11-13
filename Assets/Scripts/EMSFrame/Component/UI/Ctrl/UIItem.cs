//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace UnityFrame
{
	//作为实体控制器使用
	public class UIItem : UIUpdateGroup,IEntityHnadle,IReleasable,IOnStart
	{
        [SerializeField] protected float m_TimeAutoRelease = 0;
        private int m_CoAutoRelease = 0;

        public int eLayer { get; set; }
        public bool isReleased { get; set; }
        public float timeStamp { get; set; }
        public bool isActive { get { return this.gameObject.activeSelf; } set { this.gameObject.SetActive(value); } }

		public void UF_FitContentSize(int axis){
			this.rectTransform.SetSizeWithCurrentAnchors ((RectTransform.Axis)axis, UnityEngine.UI.LayoutUtility.GetPreferredSize (this.rectTransform, (int)axis));
		}

		public void UF_FitContentSize(){
            UF_FitContentSize(0);
            UF_FitContentSize(1);
		}
		
		public void UF_SetAsFirstSibling(){
			this.transform.SetAsFirstSibling ();
		}

		public void UF_SetAsLastSibling(){
			this.transform.SetAsLastSibling ();
		}

        public void UF_OnStart() {
            if (m_TimeAutoRelease > 0) {
                UF_AutoRelease(m_TimeAutoRelease);
            }
        }

        public override void UF_OnReset()
        {
            base.UF_OnReset();
            if (m_CoAutoRelease != 0)
            {
                FrameHandle.UF_RemoveCouroutine(m_CoAutoRelease);
            }
            m_CoAutoRelease = 0;
        }

        //指定时间内自动释放
        public void UF_AutoRelease(float value){
            if (m_CoAutoRelease != 0) {
                FrameHandle.UF_RemoveCouroutine(m_CoAutoRelease,true);
            }
            m_CoAutoRelease = FrameHandle.UF_AddCoroutine(UF_IAutoRelease(value),true);
        }

		System.Collections.IEnumerator UF_IAutoRelease(float value){
			yield return new WaitForSeconds (value);
            m_CoAutoRelease = 0;
            this.Release ();
		}

        public void Release() {
            isReleased = true;
        }


        public void UF_ExecuteTweenAlpha(string valchunk) {
            if (string.IsNullOrEmpty(valchunk)) {
                return;
            }
            List<string> list = ListCache<string>.Acquire();
            GHelper.UF_SplitStringWithCount(valchunk,4,list,';');
            float vform = GHelper.UF_ParseFloat(list[0]);
            float vto = GHelper.UF_ParseFloat(list[1]);
            float vduration = GHelper.UF_ParseFloat(list[2]);
            bool vtimescale = GHelper.UF_ParseBool(list[3]);
            UF_CrossRenderAlpha(vform, vto, vduration, vtimescale);
            ListCache<string>.Release(list);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (!isReleased && !GlobalSettings.IsApplicationQuit)
            {
                Debug.LogError(string.Format("Entity UI [{0}] has been destory with no release state", this.name));
            }
        }
    }
}

