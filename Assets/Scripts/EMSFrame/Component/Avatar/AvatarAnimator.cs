//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame{
	[System.Serializable]
	public class AvatarAnimator {
		struct PlayQueueElemelt{
			public AnimatorClip clip;
			public DelegateMethod eventOnFinish;
			public bool triggerEvent;
			public float speed;
            public string playParam;
		}

		private bool m_IsActive = true;

        [SerializeField]private List<AnimatorClip> m_AnimatorClips = new List<AnimatorClip>();

		private AvatarController m_Avatar;
		private Animator m_Animator;
		private AnimatorClip m_CurrentPlayClip;
		private DelegateMethod m_PlayingCallback;
		private Dictionary<string,AnimatorClip> m_DicAnimatorClips = new Dictionary<string, AnimatorClip>();
		//播放队列
		private List<PlayQueueElemelt> m_ListPlayQueue = new List<PlayQueueElemelt>();

        //触发索引
        private int m_EventTriggerIndex = 0;
		private float m_PlayingTime = 0;
		private float m_PlayingLength = 0;
		private bool m_IsOver = true;
		private bool m_IsPause = false;
        private bool m_isPlayingElemelt = false;

        [SerializeField]private float m_Speed = 1;
		//控速度
		private float m_CtrlSpeed = 1;

        public AnimatorClip currentClip{get{return m_CurrentPlayClip;}}

		public List<AnimatorClip> listClips{get{return m_AnimatorClips;}}

        public bool isActive{ get{ return m_IsActive;} set{ m_IsActive = value;}}

		public bool isPlaying{get{return !m_IsOver;}}

		public bool isOver{get{ return m_IsOver;}}

		public bool isPause{get{return m_IsPause;}}

		public float playingTime{get{ return m_PlayingTime;}}

		public float playingLength{get{ return m_PlayingLength;}}

        public bool lockPlay { get; set; }

		public float speed{
			get{ 
				return m_Speed;
			}
			set{ 
				m_Speed = value;
				if (m_Animator != null) {
					m_Animator.speed = m_CtrlSpeed * m_Speed;
				}
			}
		}

		private float ctrlSpeed{
			set{ 
				m_Speed = value;
				if (m_Animator != null) {
					m_Animator.speed = m_CtrlSpeed * m_Speed;
				}
			}
		}

		public void UF_OnAwake(AvatarController avatar){
			m_Avatar = avatar;
			m_Animator = avatar.GetComponentInChildren<Animator> ();
			this.UF_MapAnimatorClip();
		}


		public void UF_SetAnimatorCullType(string cullType){
			if (m_Animator != null) {
                try { m_Animator.cullingMode = (AnimatorCullingMode)System.Enum.Parse(typeof(AnimatorCullingMode), cullType); }
                catch (System.Exception e) { Debugger.UF_Exception(e); }
			}
		}

        /// <summary>
        /// 映射Action到字典中，key为对于的action命
        /// </summary>
        private void UF_MapAnimatorClip()
        {
            m_DicAnimatorClips.Clear();
            for (int i = 0; i < listClips.Count; i++)
            {
                if (m_DicAnimatorClips.ContainsKey(listClips[i].name) || listClips[i].name == "") { continue; }
                //add to dictionary
                m_DicAnimatorClips.Add(listClips[i].name, listClips[i]);
            }
        }


		public float UF_GetClipLength(string strName){
			AnimatorClip actionClip = UF_GetAnimatorClip(strName);
            if (actionClip != null) {
                AnimationClip animaClip = UF_GetAnimationClip(actionClip.clipName);
                if (animaClip != null)
                {
                    return animaClip.length;
                }
            }
			return 1;
		}

		public float currentClipLength{
			get{ 
				if (m_CurrentPlayClip != null) {
					return m_CurrentPlayClip.length;
				} else {
					return 0;
				}
			}
		}

		public float currentClipSpeed{
			get{ 
				if (m_CurrentPlayClip != null) {
					return m_CurrentPlayClip.speed;
				} else {
					return 0;
				}
			}
		}

		public string currentClipParam{
			get{ 
				if (m_CurrentPlayClip != null) {
					return m_CurrentPlayClip.param;
				} else {
					return string.Empty;
				}
			}
		}

		public string currentClipName{
			get{
				if (m_CurrentPlayClip != null) {
					return m_CurrentPlayClip.name;
				} else {
					return string.Empty;
				}
			}
		}


		private UnityEngine.AnimationClip UF_GetAnimationClip(string name){
			if (m_Animator != null && m_Animator.runtimeAnimatorController != null) {
				AnimationClip[] animationClips = m_Animator.runtimeAnimatorController.animationClips;
				for (int k = 0; k < animationClips.Length; k++) {
					if (animationClips [k].name == name)
						return animationClips [k];
				}
			}
			return null;
		}

        public bool UF_HasAnimatorClip(string strName) {
            return UF_GetAnimatorClip(strName) != null;
        }


        public AnimatorClip UF_GetAnimatorClip(string strName){
			if(m_DicAnimatorClips.ContainsKey(strName)){
				return m_DicAnimatorClips[strName];
			}
			for(int k = 0;k < listClips.Count;k++){
				if(listClips[k].name == strName){
					return listClips[k];
				}
			}
			return null;
		}


		public string UF_GetClipPlayingParam(string strName){
			var clip = UF_GetAnimatorClip(strName);
			if (clip != null) {
				return 	clip.playingParam;
			}
			else{
				return string.Empty;
			}
		}

		public bool UF_SetClipPlayingParam(string strName,string value){
			var clip = UF_GetAnimatorClip(strName);
			if (clip != null) {
				clip.playingParam = value;
				return true;
			} else {
				return false;
			}
		}

		/// <summary>
		/// 播放Action,并清空之前的播放列表，可用';'间隔多个Action的播放
		/// </summary>
		public void UF_Play(string strName){
            UF_Play(strName, false, 1,null);
		}

		/// <summary>
		/// 播放Action,并清空之前的播放列表，可用';'间隔多个Action的播放，activeClipEvent是否激活Clip中的事件
		/// </summary>
		public void UF_Play(string strName,bool triggerEvent){
            UF_Play(strName, triggerEvent, 1,null);
		}

        /// <summary>
        /// 播放Action,并清空之前的播放列表，可用';'间隔多个Action的播放，所有Action播放完毕会触发完成事件
        /// </summary>
        public void UF_Play(string strName,bool triggerEvent,float fSpeed, string playParam = "", DelegateMethod eventFinish = null){
			if (string.IsNullOrEmpty (strName))
				return;
            if (lockPlay) {
                Debugger.UF_Warn(string.Format("Animator Has been lock,play {0} failed", strName));
                return;
            }
            //避免重复播放
            if (m_isPlayingElemelt && m_CurrentPlayClip != null && m_CurrentPlayClip.name == strName) {
                return;
            }
            UF_ClearPlayQueue();
			m_CurrentPlayClip = null;
			m_IsOver = false;
			m_IsPause = false;
			this.ctrlSpeed = 1;
			if (strName.IndexOf (';') > -1) {
                List<string> inlist = ListCache<string>.Acquire();
				GHelper.UF_SplitString(strName, inlist);
                for (int k = 0; k < inlist.Count; k++) {
					if (!string.IsNullOrEmpty (inlist[k])) {
                        UF_AddToPlayQueue(inlist[k], triggerEvent, fSpeed, string.Empty, eventFinish);
					}
				}
                ListCache<string>.Release(inlist);
            } else {
                UF_AddToPlayQueue(strName, triggerEvent,fSpeed,playParam, eventFinish);
			}

			//立即执行更新一次，避免播放速度过快导致显示异常
			this.UF_OnUpdate ();
		}


        /// <summary>
        /// 添加到播放队列
        /// </summary>
        public void UF_AddToPlayQueue(string strName,bool triggerEvent,float fSpeed = 1.0f,string playParam = "",DelegateMethod eventFinish = null){
            if (lockPlay) {
                Debugger.UF_Warn(string.Format("Animator Has been lock,play {0} failed", strName));
                return;
            }
            AnimatorClip action = UF_GetAnimatorClip(strName);
			if (action == null) {
				Debugger.UF_Warn(string.Format("Count not found the AnimatorClip[{0}] to Play ",strName));
				if (eventFinish != null) {
					eventFinish (null);
				}
				return;
			}
			PlayQueueElemelt element;
			element.triggerEvent = triggerEvent;
			element.clip = action;
			element.eventOnFinish = eventFinish;
			element.speed = fSpeed;
            element.playParam = playParam;
            m_ListPlayQueue.Add (element);
		}


		public void UF_ClearPlayQueue(){
			m_ListPlayQueue.Clear ();
		}
			
		/// <summary>
		/// 继续当前Action
		/// </summary>
		public void UF_Continue(){
			if(m_CurrentPlayClip != null){
				m_IsPause = false;	
				this.ctrlSpeed = 1;
			}
		}
		/// <summary>
		/// 停止当前Action
		/// </summary>
		public void UF_Pause(){
			if(m_CurrentPlayClip != null && m_Avatar != null){
				m_IsPause = true;	
				this.ctrlSpeed = 0;
			}
		}

		public void UF_Stop(){
			if(m_CurrentPlayClip != null){
				m_IsOver = true;
                UF_ClearPlayQueue();
				m_CurrentPlayClip = null;
				m_PlayingCallback = null;
				this.ctrlSpeed = 0;
			}
		}

		public void UF_SetAnimateFrameAt(string strName,float normalTime){
			this.UF_Stop();
			AnimatorClip action = UF_GetAnimatorClip(strName);
			if (action != null && m_Animator != null) {
				m_Animator.Play (action.clipName,0,normalTime);
				m_Animator.Update(normalTime);
				this.ctrlSpeed = 0;
			}
		}

		public float UF_GetAnimationClipRealLength(string strName){
			var aClip = UF_GetAnimatorClip(strName);
            if (aClip == null) {
                Debugger.UF_Warn(string.Format("Can not get animator clip:{0}", strName));
                return 0;
            }
			UnityEngine.AnimationClip clip = UF_GetAnimationClip(aClip.clipName);
			float lenght = 0;
			if (clip != null) {
				//真实播放长度 
				lenght = ((1.0f / aClip.speed) * clip.length);
			} else {
                //设置的长度
				lenght = aClip.length;
			}
			return lenght;
		}

		private void UF_PlayAnimatorClip(AnimatorClip aClip){
            //获取影片播放长度
            m_PlayingLength = UF_GetAnimationClipRealLength(m_CurrentPlayClip.name) / m_CurrentPlayClip.playingSpeed;
            m_PlayingLength = GHelper.ShortFloat(m_PlayingLength);
            //设置当前影片播放速度
            this.speed = m_CurrentPlayClip.speed * m_CurrentPlayClip.playingSpeed;
            //重置索引
            m_EventTriggerIndex = 0;
            //重置当前影片播放时间
            m_PlayingTime = 0;
            m_IsOver = false;
            m_isPlayingElemelt = true;

            //播放影片动画
            if (m_Animator == null){
				Debugger.UF_Warn ("Animator Component is null,Animation will not play!");
				return;
			}
			switch (aClip.crossMode) {
				case AnimatorClip.CrossMode.CrossFabe:
                    m_Animator.CrossFadeInFixedTime(aClip.clipName, aClip.fadeFactor);
                    break;
				case AnimatorClip.CrossMode.Direct:
					if (aClip.wrapMode == WrapMode.Loop) {
                        m_Animator.Play(aClip.clipName);
                    } else {
						m_Animator.Play(aClip.clipName,0,0);
					}
					break;
				default:
					m_Animator.Play (aClip.clipName);
					break;
			}
		}


		private void UF_UpdatePlayClipEvent(){
			if (m_CurrentPlayClip != null) {
                m_PlayingTime += GTime.RunDeltaTime;
				float progress = m_PlayingLength <= 0 ? 1:Mathf.Clamp01 (m_PlayingTime / m_PlayingLength);
                progress = GHelper.ShortFloat(progress);
                if (m_CurrentPlayClip.playingAcitveEvent) {
					var count = m_CurrentPlayClip.clipEvents.Count;
					for (int k = m_EventTriggerIndex; k < count; k++) {
						if (progress >= m_CurrentPlayClip.clipEvents [k].trigger) {
							m_EventTriggerIndex++;
                            UF_ExcuteAnimatorClipEvent(m_CurrentPlayClip.name, m_CurrentPlayClip.playingParam, m_CurrentPlayClip.clipEvents [k].name, m_CurrentPlayClip.clipEvents [k].param);
						}
					}
				}
				if (progress >= 1) {
                    m_PlayingTime = m_PlayingLength;
                    m_isPlayingElemelt = false;
					GHelper.UF_SafeCallDelegate (m_CurrentPlayClip.playingCallback, null);
				}
			}
		}

        public void UF_OnUpdate(){
			if (m_IsActive && !m_IsOver && !m_IsPause) {
				if (m_CurrentPlayClip != null && m_isPlayingElemelt) {
                    UF_UpdatePlayClipEvent();
				}
				else{
					if (m_ListPlayQueue.Count > 0) {
						PlayQueueElemelt element = m_ListPlayQueue [0];
                        m_ListPlayQueue.RemoveAt(0);

                        m_CurrentPlayClip = element.clip;
                        m_CurrentPlayClip.playingAcitveEvent = element.triggerEvent;
                        m_CurrentPlayClip.playingCallback = element.eventOnFinish;
                        m_CurrentPlayClip.playingSpeed = element.speed;
                        m_CurrentPlayClip.playingParam = element.playParam;
                        UF_PlayAnimatorClip(m_CurrentPlayClip);

                        UF_UpdatePlayClipEvent();
					} else {
                        if (m_CurrentPlayClip != null && !m_isPlayingElemelt && m_CurrentPlayClip.isLoop)
                        {
                            UF_PlayAnimatorClip(m_CurrentPlayClip);
                            UF_UpdatePlayClipEvent();
                        }
                        else {
                            if (m_CurrentPlayClip != null) {
                                m_CurrentPlayClip.ResetPlayingState();
                                m_CurrentPlayClip = null;
                            }
                            m_IsOver = true;
                        }
					}
				}
			}
		}


		//执行动画事件
		private void UF_ExcuteAnimatorClipEvent(string clipName,string clipParam,string eventName,string eventParam){
			if (m_Avatar != null) {
				MessageSystem.UF_GetInstance ().UF_Send(DefineEvent.E_ANIMATION_CLIP,m_Avatar,clipName, clipParam, eventName, eventParam);


			}
		}


		public void UF_OnReset(){
            if (m_DicAnimatorClips == null)
				return;
			m_ListPlayQueue.Clear();
			m_CurrentPlayClip = null;
			m_PlayingCallback = null;
			m_EventTriggerIndex = 0;
            lockPlay = false;
        }
			
	}

}