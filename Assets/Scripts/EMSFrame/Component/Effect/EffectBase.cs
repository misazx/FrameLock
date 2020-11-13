//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;


namespace UnityFrame{
	public enum EffectModeType{
		Once,
		Loop,
		PingPong
	}

	public class EffectBase : MonoBehaviour
	{
        [SerializeField] protected bool m_IsActive = true;
        public string effectName = string.Empty;
        public EffectModeType modeType;
		public float duration = 2.0f;
		public float delay = 0;
		public bool ingoreTimeScale;

		protected bool m_IsOver = true;
		protected bool m_IsPause;
        protected bool m_IsOnRun = false;
        protected float m_CurrentDelay = 0;
		protected float m_CurrentDuration = 0;
		protected float m_RiseSide = 1;
		protected float m_Porgress = 0;

		public float porgress{get{ return m_Porgress;}}

		public bool isOver{get{ return m_IsOver;}}
		public bool isPlaying{get{ return !m_IsOver;}}
		public bool isPause{get{ return m_IsPause;}}
		public bool isReverse{get{return m_RiseSide < 0;}}
		public bool isActive{get{ return m_IsActive;}set{ m_IsActive = value;}}

        private bool m_invokeStart;

		public void Init(){
            UF_OnInit();
		}

		public void Play(){
			m_IsOver = false;
			m_CurrentDuration = 0;
			m_CurrentDelay = 0;
			m_RiseSide = 1;
            m_invokeStart = false;
            UF_OnPlay();
        }

		public void Reverse(){
			m_IsOver = false;
			m_CurrentDuration = duration;
			m_CurrentDelay = 0;
			m_RiseSide = -1;
            m_invokeStart = false;
            UF_OnPlay();
        }

		public void Stop(){
			m_IsOver = true;
            UF_OnStop();
		}

		public void Pause(){
			m_IsPause = true;
		}

		public void Continue(){
			m_IsPause = false;
		}

		public void SetToSource(){
			m_CurrentDuration = 0;
            UF_OnRun(0);
		}

		public void SetToTarget(){
			m_CurrentDuration = duration;
            UF_OnRun(1);
		}

        public void SetTo(float progress)
        {
            this.UF_OnRun(progress);
        }

		public void Run(float deltaTime,float unscaleDeltaTime){
			if (!m_IsActive || m_IsOver || m_IsPause) {
				return;
			}

            float delta = ingoreTimeScale ? unscaleDeltaTime : deltaTime;

			if (delta <= 0) {
				return;
			}
			
			if (m_CurrentDelay >= delay) {
                m_IsOnRun = true;
                if (!m_invokeStart)
                {
                    m_invokeStart = true;
                    UF_OnStart();
                }

                m_CurrentDuration += delta * m_RiseSide;

				if (System.Math.Abs(duration) < 0.0001f)
					m_Porgress = 1;
				else
					m_Porgress = Mathf.Clamp01 (m_CurrentDuration / duration);
                UF_OnRun(m_Porgress);

				if (m_CurrentDuration >= duration || m_CurrentDuration < 0) {
					if (modeType == EffectModeType.Once) {
						m_IsOver = true;
					} else if (modeType == EffectModeType.Loop) {
						if (isReverse) {
							Reverse ();
						} else {
							Play ();
						}
					} else if (modeType == EffectModeType.PingPong) {
						m_RiseSide = -m_RiseSide;
						m_CurrentDuration = Mathf.Clamp (m_CurrentDuration, 0, duration);
					}
				}

			} else {
                m_IsOnRun = false;
                m_CurrentDelay += delta;
				m_CurrentDelay = Mathf.Clamp (m_CurrentDelay, 0, delay);
			}
		}

		public void Reset(){ UF_UF_OnReset();}
		protected virtual void UF_OnInit(){}
		protected virtual void UF_OnPlay(){}
		protected virtual void UF_OnStop(){}
        protected virtual void UF_OnStart(){}
		protected virtual void UF_OnRun(float progress) {}
		protected virtual void UF_UF_OnReset(){}

	}

}