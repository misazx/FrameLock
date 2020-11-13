//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityFrame{

	/// <summary>
	/// 声音管理类
	/// 通过加载AudioController 组建来播放声音效果
	/// 每种类型的声音只能单一播放
	/// </summary>
	public class AudioManager : HandleSingleton<AudioManager>,IOnSecnodUpdate,IOnReset
	{
		private GameObject m_WavRoot;
		//总音量
		private float m_Volume = 1.0f;

		//记录类型音量
		private Dictionary<int,float> m_DicAudioTypeVolume = new Dictionary<int, float>();

		//记录加载的音频存活的引用
		private List<AudioController> m_ListAudios = new List<AudioController> ();

		/// <summary>
		/// 根据类型设置声音大小
		/// </summary>
		public void UF_SetTypeVolume(int type,float volume){
			if (m_DicAudioTypeVolume.ContainsKey (type)) {
				m_DicAudioTypeVolume [type] = volume;
			} else {
				m_DicAudioTypeVolume.Add(type,volume);
			}
			for (int k = 0; k < m_ListAudios.Count; k++) {
				if (m_ListAudios [k] != null && !m_ListAudios[k].isReleased) {
					if (m_ListAudios [k].audioType == type) {
						m_ListAudios [k].volume = volume * m_Volume;
					}
				} else {
					m_ListAudios.RemoveAt (k);
					k--;
				}
			}
		}

		public void UF_SetTargetVolume(string audioName,float volume){
			AudioController controller = UF_Find(audioName);
			if (controller != null) {
				controller.volume = volume * m_DicAudioTypeVolume [controller.audioType] * m_Volume;
			}
		}

		public int UF_SmoothTargetVolume(string audioName,float volume,float duration){
            if (duration <= 0)
            {
                UF_SetTargetVolume(audioName, volume);
                return 0;
            }
            else {
                AudioController controller = UF_Find(audioName);
                if (controller != null)
                {
                    return controller.UF_SmoothVolume(volume * m_DicAudioTypeVolume[controller.audioType] * m_Volume, duration);
                }
                else
                {
                    return 0;
                }
            }

		}


		public void UF_SetGlobalVolume(float volume){
			if (m_Volume != volume) {
				m_Volume = Mathf.Clamp01 (volume);
				for (int k = 0; k < m_ListAudios.Count; k++) {
					m_ListAudios [k].volume = m_DicAudioTypeVolume [m_ListAudios [k].audioType] * m_Volume;
				}
			}
		}

		public int UF_SmoothGlobalVolume(float volume,float duration){
			if (duration <= 0) {
                UF_SetGlobalVolume(volume);
				return 0;
			} else {
				return FrameHandle.UF_AddCoroutine (UF_ISmoothGlobalVolume(volume, duration));
			}
		}

		IEnumerator UF_ISmoothGlobalVolume(float volume,float smooth){
			float source = this.m_Volume;
			float target = volume;
			float progress = 0;
			float tickBuff = 0;
			while (progress != 1) {
				float delta = GTime.DeltaTime;
				tickBuff += delta;
				progress = Mathf.Clamp01 (tickBuff / smooth);
                UF_SetGlobalVolume(source * (1.0f - progress) + target * progress);
				yield return null;
			}
		}


		private void UF_AddToWavRoot(GameObject wavObj){
			if (wavObj != null) {
				if (m_WavRoot == null) {
					m_WavRoot = new GameObject ("Wav_Root");
				}	
				wavObj.transform.SetParent (m_WavRoot.transform);
			}
		}

		private void UF_EventAudioLoadFinish(Object param){
			if (param != null) {
				AudioController controller = param as AudioController;
				if (controller != null) {
                    UF_AddToWavRoot(controller.gameObject);
					if (!m_DicAudioTypeVolume.ContainsKey(controller.audioType)) {
						m_DicAudioTypeVolume.Add(controller.audioType, 1.0f);
					}
					controller.volume = m_DicAudioTypeVolume[controller.audioType] * m_Volume;
					m_ListAudios.Add (controller);

					controller.UF_Play();
				}
			}
		}

		public void UF_Play(string audioName){
			if (string.IsNullOrEmpty (audioName))
				return;
			AudioController controller = UF_Find(audioName);
			if (controller != null) {
				controller.UF_Play();
				return;
			}	
			//异步创建音频实体
			CEntitySystem.UF_GetInstance ().UF_AsyncCreate(audioName, UF_EventAudioLoadFinish);
		}


		public void UF_Stop(string audioName){
			if (string.IsNullOrEmpty (audioName))
				return;
			AudioController controller = UF_Find(audioName,true);
			if (controller != null) {
				controller.UF_Stop();
				return;
			}
		}


		public void UF_BatchPlay(string batchName){
			string[] names = GHelper.UF_SplitString (batchName);
			if (names != null) {
				for (int k = 0; k < names.Length; k++) {
                    UF_Play(names [k]);
				}
			}
		}

		public AudioController UF_Find(string audioName,bool includePlaying = false)
        {
			for (int k = 0; k < m_ListAudios.Count; k++) {
				if (m_ListAudios [k] != null && m_ListAudios [k].name == audioName) {
					if (m_ListAudios [k].isStatic || !m_ListAudios [k].isPlaying || includePlaying) {
						return m_ListAudios [k];
					}
				}
			}
			return null;
		}


		public void UF_OnSecnodUpdate(){
            UF_ClearUesless();
		}

		public void UF_ClearUesless(){
			if (m_ListAudios.Count > 0) {
				for (int k = 0; k < m_ListAudios.Count; k++) {
					if (m_ListAudios [k] == null) {
						m_ListAudios.RemoveAt (k);
						k--;
					} else if (!m_ListAudios [k].isPlaying) {
						m_ListAudios [k].Release ();
						m_ListAudios.RemoveAt (k);
						k--;
					}
				}
			}
		}

        public void UF_OnReset() {
            foreach (var item in m_ListAudios) {
                if (item != null) {
                    item.UF_Stop();
                    item.Release();
                }
            }
            m_ListAudios.Clear();
        }



        public override string ToString ()
		{
			System.Text.StringBuilder sb = StrBuilderCache.Acquire ();
			sb.Append(string.Format ("Audio  Count:{0} \n",m_ListAudios.Count));
			for (int k = 0; k < m_ListAudios.Count; k++) {
				sb.Append(string.Format ("\t {0}  | Type:{1}  |  State:{2}\n", m_ListAudios [k].name,m_ListAudios [k].audioType,m_ListAudios [k].isPlaying.ToString()));
			}
			return StrBuilderCache.GetStringAndRelease(sb);
		}


	}

}