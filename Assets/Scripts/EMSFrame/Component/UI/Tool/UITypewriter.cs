//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;
using UnityEngine;

namespace UnityFrame
{
	[RequireComponent(typeof(UILabel))]
	public class UITypewriter : UIObject
	{
		public float duration;
		public UILabel target; 

		private string m_CacheText;

		public bool isPlaying{ get{ return m_MotionID != 0;}}

		private int m_MotionID = 0;

        protected override void Awake()
        {
            if (target == null)
            {
                target = this.GetComponent<UILabel>();
            }
        }

        //打印机效果
        public int Play(){
			return Play (duration);
		}

		//播放打字机效果
		public int Play(float duration){
			return PlayWithCallback (duration, null);
		}

		public int PlayWithCallback(float dura,DelegateMethod callback){
			if (target == null || !target.gameObject.activeInHierarchy)
				return 0;

			if (isPlaying) {
				Stop ();
			}

			//不支持富文本
			target.supportRichText = false;
			duration = dura;
			m_CacheText = target.text;
			m_MotionID =  FrameHandle.UF_AddCoroutine (ITypewriterMotion(target,m_CacheText,callback));

			return m_MotionID;
		}

		public void Stop(){
			if (m_MotionID != 0) {
				FrameHandle.UF_RemoveCouroutine (m_MotionID);
				m_MotionID = 0;
				if (target != null) {
					target.text = m_CacheText;
				}
			}
		}

		IEnumerator ITypewriterMotion(UILabel label,string text,DelegateMethod callback){
			for (int k = 1; k < text.Length+1; k++) {
				label.text =  text.Substring (0,k);
				yield return new WaitForSeconds (duration);
			}

			m_MotionID = 0;
			if (callback != null)
				callback (null);
			
		}


	}
}