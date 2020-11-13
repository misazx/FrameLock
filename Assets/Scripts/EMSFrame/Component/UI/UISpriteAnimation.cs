//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame{

	public class UISpriteAnimation : UISprite
	{
		public enum AnimatedModeType{
			Once,
			Loop,
			PingPong
		}
			
		[SerializeField]private List<Sprite> m_SpriteSet = new List<Sprite>();
		public AnimatedModeType animatedModeType;
		public bool ignoreTimeScale = false;
		public bool isPlayOnActive = false;
		public float interval = 0.25f;
		public float delay = 0;
		public bool active = true;

		private string m_PrefixSpriteName = "";
		private float m_IntervalBuffer = 0;
		private float m_DelayBuffer = 0;
		private int m_CurrentIdx = 0;
		private Color m_SourceColor;

		public List<Sprite> spriteSet{get{ return m_SpriteSet;}}

		public override void UF_SetSprite(string spriteName)
		{
			if (string.IsNullOrEmpty (spriteName)) {
				return;
			}

			base.UF_SetSprite(spriteName);

			if (!string.IsNullOrEmpty (m_PrefixSpriteName) && !spriteName.Contains(m_PrefixSpriteName)) {
				m_SpriteSet.Clear ();
                UF_InitSpriteSet();	
			}
		}


		private void UF_InitSpriteSet(){
			if (sprite == null) {
				return;
			}
			if (m_SpriteSet == null || m_SpriteSet.Count == 0) {
				string prefix = sprite.name.Substring (0, sprite.name.Length - 1);
				if (prefix != m_PrefixSpriteName) {
					m_PrefixSpriteName = prefix;
					m_SpriteSet = UIManager.UF_GetInstance ().UF_GetAnimateSqueueSprites(m_PrefixSpriteName);
				}
			}
		}

		public void UF_Play(){
			
			active = true;
			this.enabled = true;

			m_CurrentIdx = 0;
			m_IntervalBuffer = 0;
			m_DelayBuffer = 0;

			m_SourceColor = this.color;
			if (delay > 0) {
				Color hideColor = this.color;
				hideColor.a = 0;
				this.color = hideColor;
			}

            UF_InitSpriteSet();
			if (m_SpriteSet != null && m_SpriteSet.Count > 0) {
				this.sprite = m_SpriteSet[0];
			}
		}

		public void UF_Stop(){
			active = false;
		}


		protected override void OnEnable ()
		{
			base.OnEnable ();
			if(isPlayOnActive){
				active = true;
				this.enabled = true;
				m_CurrentIdx = 0;
				m_IntervalBuffer = 0;
				m_DelayBuffer = 0;
			}
		}

		protected override void Start()
		{
			base.Start();
			#if UNITY_EDITOR
			if (!Application.isPlaying) {
				return;
			}
#endif
            UF_Play();

		}


		void Update(){
			#if UNITY_EDITOR
			if (!Application.isPlaying) {
				return;
			}
			#endif

			if (active && m_SpriteSet != null &&  m_SpriteSet.Count > 0) {
				float deltaTime = 0;
				if (ignoreTimeScale) {
					deltaTime = Time.unscaledDeltaTime;
				} else {
					deltaTime = Time.deltaTime;
				}

				if (m_DelayBuffer >= delay) {
					m_IntervalBuffer += deltaTime;
					if (m_IntervalBuffer >= interval) {
						m_IntervalBuffer	= 0;
						switch (animatedModeType) {
						case AnimatedModeType.Loop:
                                UF_AnimateLoop();
							break;
						case AnimatedModeType.Once:
                                UF_AnimateOnce();
							break;
						case AnimatedModeType.PingPong:
                                UF_AnimatePingPong();
							break;
						}
					}
				} else {
					m_DelayBuffer += deltaTime;
					if (m_DelayBuffer >= delay) {
						this.color = m_SourceColor;
					}
				}
			}
		}


		private void UF_AnimateOnce(){
			if (m_CurrentIdx >= m_SpriteSet.Count) {
				active = false;
				if (!isPlayOnActive) {
					this.enabled = false;
				}
			} else {
				this.sprite = m_SpriteSet [m_CurrentIdx];
			}
			m_CurrentIdx++;
		}


		private void UF_AnimateLoop(){
			if (m_CurrentIdx >= m_SpriteSet.Count) {
                UF_Play();
			} else {
				this.sprite = m_SpriteSet [m_CurrentIdx];
			}
			m_CurrentIdx++;
		}

		private void UF_AnimatePingPong(){

			int count = m_SpriteSet.Count;
			int tcount = count * 2;
			if (m_CurrentIdx >= tcount) {
				m_CurrentIdx = 0;
			} 
			if (m_CurrentIdx >= count) {
				this.sprite = m_SpriteSet [tcount - m_CurrentIdx - 1];
			} else {
				this.sprite = m_SpriteSet [m_CurrentIdx];
			}
			m_CurrentIdx++;
		}


		public void Dispose(){
			if(m_SpriteSet != null)
				m_SpriteSet.Clear ();
		}

	}

}