//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace UnityFrame
{
	//特效效果实体
	public class FXController : EntityObject,IOnAwake,IOnSyncUpdate,IOnReset
	{
		public float life = 3.0f;
		public bool playOnActive = true;
		//播放结束后,自动回收
		public bool autoRelease = true;

        //public bool useFixedEuler = false;

        [HideInInspector][SerializeField]private List<EffectBase> m_Effects = new List<EffectBase> ();

		//状态，0 停止，1 播放，2 暂停
		private int m_State = 0;

		private float m_Size = 1.0f;
		private Transform m_FollowTarget;
		private Vector3 m_FollowOffset = Vector3.zero;
		private bool m_FollowEuler = false;

		private float m_LifeTickBuf = 0;

		public float leftLife{get{ return life - m_LifeTickBuf;}}

		public Vector3 fixedEuler{ get; set;}

		public List<EffectBase> effects{ get{ return m_Effects;}}

        public int sortingOrder { get; set; }
        
        public bool isPlaying{get{ return m_State != 0;}
			private set{ m_State = value ? 1 : 0;}
		}

		public bool isPause{get{ return m_State == 2;}
			private set{ m_State = value ? 2 : m_State;}
		}

        public float size{
			get{ 
				return m_Size;
			}
			set{ 
				if(value < 0 || value >= m_Size){
					return;
				}
				this.transform.localScale = (this.transform.localScale / m_Size) * value;
				m_Size = value; 
			}
		}

		public EffectBase UF_FindEffect(string effectName){
			return EffectControl.UF_FindEffect(m_Effects,effectName);
		}

        //设置粒子状态
        protected void UF_SetParticlesState(int state)
        {
            List<ParticleSystem> list = ListCache<ParticleSystem>.Acquire();
            this.GetComponentsInChildren<ParticleSystem>(false, list);
            foreach (var item in list)
            {
                switch (state) {
                    case 1:
                        if (item.isPlaying)
                        {
                            item.Stop(false);
                        }
                        item.Play(false);
                        if (sortingOrder != 0) {
                            ParticleSystemRenderer renderer = item.GetComponent<ParticleSystemRenderer>();
                            if (renderer != null)
                            {
                                renderer.sortingOrder = sortingOrder;
                            }
                        }
                        ; break;
                    case 2: item.Stop(false);break;
                    case 3: item.Pause(false); break;
                    case 4: item.Play(false); break;
                }
            }
            ListCache<ParticleSystem>.Release(list);
        }


		public void UF_Play(Vector3 pos,Vector3 euler){
			this.transform.position = pos;
			this.transform.eulerAngles = euler;
            UF_Play();
		}


		public void UF_Play(Vector3 pos,Vector3 euler,float size){
			this.transform.position = pos;
			this.transform.eulerAngles = euler;
			this.size = size;
            UF_Play();
		}

		public void UF_Play()
		{
			isPlaying = true;
			m_LifeTickBuf = 0;
			this.gameObject.SetActive (true);
            UF_SetParticlesState(1);
            EffectControl.UF_Play(m_Effects);
            EffectControl.UF_ResetTailRender(this.gameObject);
        }
		public void UF_Stop()
		{
			isPlaying = false;
            UF_SetParticlesState(2);
            EffectControl.UF_Stop(m_Effects);
		}

		public void UF_Pause(){
			isPause = true;
            UF_SetParticlesState(3);
            EffectControl.UF_Pause(m_Effects);
		}

		public void UF_Continue(){
			isPause = false;
            UF_SetParticlesState(4);
            EffectControl.UF_Continue(m_Effects);
		}


		//设置特效跟随目标
		public void UF_FollowTo(GameObject gameObject){
            UF_FollowTo(gameObject, Vector3.zero,false);
		}

		public void UF_FollowTo(GameObject gameObject,Vector3 offset){
            UF_FollowTo(gameObject, offset, false);
		}
		public void UF_FollowTo(GameObject gameObject,Vector3 offset,bool followEuler){
			m_FollowOffset = offset;
			m_FollowEuler = followEuler;
			if (gameObject != null) {
				m_FollowTarget = gameObject.transform;
				this.transform.position = m_FollowTarget.position + m_FollowOffset;
			} else {
				m_FollowTarget = null;
			}
		}

		private void UF_UpdateFollow(){
			if (m_FollowTarget != null) {
				this.transform.position = m_FollowTarget.position + m_FollowOffset;
				if(m_FollowEuler)
					this.transform.eulerAngles = m_FollowTarget.eulerAngles;
				if (!m_FollowTarget.gameObject.activeSelf || !m_FollowTarget.gameObject.activeInHierarchy) {
                    UF_SetParticlesState(2);
                    //this.Stop();
                }
			}
		}

		public void UF_OnAwake()
		{
            EffectControl.UF_Init(m_Effects);
        }


        private void OnEnable()
        {
            if (playOnActive)
            {
                UF_Play();
            }
        }


        public void UF_OnReset()
		{
			m_FollowTarget = null;
			isPlaying = false;
			this.size = 1.0f;
            sortingOrder = 0;
            EffectControl.UF_Reset(m_Effects);
        }

        //设置剩余时间
        public void UF_SetLeftTime(float value) {
            m_LifeTickBuf = Mathf.Max(0, life - value);
        }
	
		public void UF_OnSyncUpdate(){
			if (!isPlaying || isPause)
				return;

            //if (useFixedEuler)
            //	this.transform.eulerAngles = Vector3.zero;

            UF_UpdateFollow();

			EffectControl.UF_Run(m_Effects,GTime.DeltaTime, GTime.UnscaleDeltaTime);

            if (life <= 0) return;

            if (m_LifeTickBuf < life) {
				m_LifeTickBuf += GTime.DeltaTime;
			} else {
				m_LifeTickBuf = life;
				isPlaying = false;
				if(autoRelease)
					this.Release ();
			}
		}


    }
}

