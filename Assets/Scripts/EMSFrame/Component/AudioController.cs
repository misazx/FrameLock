//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------
using System.Collections.Generic;
using System.Collections;
using UnityEngine;



namespace UnityFrame
{
	public enum AudioPlayMode{
		ONCE,
		LOOP,
	}

	//[RequireComponent(typeof(AudioSource))]
	public class AudioController : EntityObject,IOnAwake,IOnReset
    {
		public AudioSource audioSource;
		public int audioType = 0;
		public AudioPlayMode playMode;
		//是否静态唯一
		public bool isStatic = false;

		private float m_Volume = 1.0f;
		private float m_SourceVolume = 1.0f;

		public bool isPlaying{
			get{ 
				if (audioSource != null) {
					return audioSource.isPlaying;
				}
				return false;
			}
		}

		public bool isLoop{
			get{ 
				return playMode == AudioPlayMode.LOOP;
			}
		}

		public float volume {
			get{ 
				if (audioSource != null) {
					return m_Volume;
				}
				return 1.0f;
			}
			set{ 
				if (audioSource != null) {
					m_Volume = Mathf.Abs (value);
					audioSource.volume = m_Volume * m_SourceVolume;
				}
			}
		}

		public float lenght{get{
				if (audioSource != null && audioSource.clip != null) {
					return audioSource.clip.length;
				} else {
					return 0;
				}
			}
		}
			

		public void UF_OnAwake()
		{
			if (audioSource == null) {
				m_SourceVolume = audioSource.volume;
			}
			if (audioSource == null) {
				Debugger.UF_Error (string.Format ("AudioController[{0}] counld not have AudioSource,the audio will not be played",this.name));
				return;
			}
			if (audioSource.clip == null) {
				Debugger.UF_Warn (string.Format ("AudioClip is missing in AudioController[{0}],the audio will not be played",this.name));
			} else {
				audioSource.loop = this.playMode == AudioPlayMode.LOOP;	
			}
		}

		public void UF_OnReset()
		{
			if (audioSource != null) {
				audioSource.volume = 1; 
				audioSource.Stop ();
			}
        }


		public void UF_Play()
		{
			if (audioSource.clip == null) {
				Debugger.UF_Error (string.Format ("AudioController[{0}] has no AudioClip,Play Failed",audioSource.name));
				return;
			}
			if (audioSource != null) {
				if (audioSource.clip.loadState == AudioDataLoadState.Failed) {
					Debugger.UF_Warn (string.Format ("Audio Clip[{0}] AudioDataLoadState.Failed", this.name));
				} else if (audioSource.clip.loadState == AudioDataLoadState.Loading) {
					Debugger.UF_Warn (string.Format ("Audio Clip[{0}] AudioDataLoadState.Loading", this.name));
				} else {
					audioSource.Play ();
				}
			}
		}
			
		public void UF_Stop()
		{
			if (audioSource != null) {
				audioSource.Stop ();
			}
		}

		public int UF_SmoothVolume(float targetVolume,float duration){
			if (targetVolume == m_Volume) {
				return 0;
			}
			return FrameHandle.UF_AddCoroutine (UF_ISmoothVolume(targetVolume,duration));
		}

		IEnumerator UF_ISmoothVolume(float targetVolume,float duration){
			if (!this.isPlaying)
				yield break;
			float svolume = m_Volume;
			float durbuf = 0; 
			while (true) {
				durbuf += GTime.DeltaTime;
				float progress = Mathf.Clamp01 (durbuf / duration);
				this.volume = svolume * (1 - progress) + progress * targetVolume;
				if (progress >= 1 || !this.isPlaying) {
					this.volume = targetVolume;
					break;
				}
			}
		}


		public int UF_SmoothPlay(float duration,float startVolume = 0f){
			this.UF_Play();
			if (startVolume < m_Volume && duration > 0) {
				return FrameHandle.UF_AddCoroutine (UF_ISmoothPlay(duration, startVolume));	
			}
			return 0;
		} 
			
		IEnumerator UF_ISmoothPlay(float duration,float startVolume){
			float svolume = m_Volume;
			float durbuf = 0;
			while (true) {
				durbuf += GTime.DeltaTime;
				float progress = Mathf.Clamp01 (durbuf / duration);
				this.volume = svolume * (1 - progress) + progress * startVolume;
				if (progress >= 1 || !this.isPlaying) {
					this.volume = svolume;
					break;
				}
				yield return null;
			}
		}

		public int UF_SmoothStop(float duration,float endVolume = 0f){
			if (duration <= 0 || System.Math.Abs(endVolume - m_Volume) < 0.001f) {
                UF_Stop();
				return 0;
			}
			return FrameHandle.UF_AddCoroutine (UF_ISmoothStop(duration,endVolume));
		}

		IEnumerator UF_ISmoothStop(float duration,float endVolume){
			float svolume = m_Volume;
			float durbuf = 0;
			while (true) {
				durbuf += GTime.DeltaTime;
				float progress = Mathf.Clamp01 (durbuf / duration);
				this.volume =  svolume * (1 - progress) + progress * endVolume;
				if (progress >= 1 || !this.isPlaying) {
					this.volume = m_Volume;
					break;
				}
				yield return null;
			}
			this.UF_Stop();
		}

		public override string ToString ()
		{
			return string.Format ("Name[{0}]   Type[{1}]   Lenght[{3}]",this.name,this.audioType,this.lenght);
		}


	}
}

