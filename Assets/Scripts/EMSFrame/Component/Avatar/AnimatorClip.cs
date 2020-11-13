//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame{
	[System.Serializable]
	public class AnimatorClip {
		public enum CrossMode{
			Direct,
			CrossFabe
		}

		public string name{get{ return m_Name;}set{ m_Name = value;}}
		[SerializeField] private string m_Name;

		public string clipName = string.Empty;

		public WrapMode wrapMode = WrapMode.Default;
		public CrossMode crossMode = CrossMode.Direct;
		public float speed = 1;
		public float fadeFactor = 0.3f;
		public string param = "";
		public float length = 0;

		[SerializeField]private List<ClipEvent> m_ClipEvents = new List<ClipEvent> ();
		public List<ClipEvent> clipEvents {get{ return m_ClipEvents;}}

		//播放时记录的参数值
		public string playingParam{ get;set;}
        internal float playingSpeed { get; set; }
        internal bool playingAcitveEvent { get; set; }
        internal DelegateMethod playingCallback { get; set; }
        
        public void ResetPlayingState() {
            playingParam = string.Empty;
            playingAcitveEvent = false;
            playingCallback = null;

        }


        public bool isLoop { get { return wrapMode == WrapMode.Loop; } }

		public AnimatorClip(){
			this.name = "New Clip";
		}

		public AnimatorClip(string strName){
			this.name = strName;
		}

		public void UF_SetCrossMode(string mode){
			crossMode = (AnimatorClip.CrossMode)System.Enum.Parse(typeof(AnimatorClip.CrossMode), mode);
		}

		public void UF_SetWrapMode(string mode){
			wrapMode =  (WrapMode)System.Enum.Parse(typeof(WrapMode), mode);
		}

		//排序队列,按照先触发的时间排在前面
		public void UF_SortClipEvents(){
			if (m_ClipEvents.Count > 1) {
				ClipEvent target;
				//简单冒泡
				for(int i = 0;i < m_ClipEvents.Count;i++){
					for(int k = 0;k < m_ClipEvents.Count- 1 - i;k++){
						if (m_ClipEvents [k].trigger > m_ClipEvents [k + 1].trigger) {
							target = m_ClipEvents [k + 1];
							m_ClipEvents [k + 1] = m_ClipEvents [k];
							m_ClipEvents [k] = target;
						}
					}
				}
			}
		}

	}
}