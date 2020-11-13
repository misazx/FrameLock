//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace UnityFrame{
	public class UISlider : UnityEngine.UI.Slider,IUIUpdate
	{
		public enum TextInfoStyle{
			Percentage,
			Decimal,
			Real,
		}
			
		public UILabel textInfo;

		public TextInfoStyle textInfoStyle = TextInfoStyle.Percentage;

		public float scaleInfo = 1.0f;

		public float limitMinValue = 0;
		public float limitMaxValue = 1;

		public string eValueChange = "";
		public string eParam= "";

		private int m_HandlerSmooth = 0;

		[SerializeField]private bool m_UseLimitValue = false;

		[SerializeField] private string m_UpdateKey;

		public bool useLimitValue{
			get{ return m_UseLimitValue;}
			set{ 
				m_UseLimitValue = value;
				this.value = this.value;
			}
		}

		public string updateKey{
			get{ return m_UpdateKey;}
			set{ m_UpdateKey = value;}
		}

		public RectTransform rectTransform{get{ return this.transform as RectTransform;}}


		private float UF_ClampValue(float input)
		{
			float num = Mathf.Clamp (input, this.minValue, this.maxValue);
			if (this.wholeNumbers) {
				num = Mathf.Round (num);
			}
			return num;
		}

		public override float value {
			get {
				float result;
				if (this.wholeNumbers) {
					result = Mathf.Round (this.m_Value);
				}
				else {
					result = this.m_Value;
				}
				return result;
			}
			set {
				if (m_UseLimitValue) {
					this.Set (Mathf.Clamp(value,limitMinValue,limitMaxValue),true);
				} else {
					this.Set (value,true);
				}
			}
		}


		//不派发事件，但会触发绑定事件
		public float rawValue {
			get {
				float result;
				if (this.wholeNumbers) {
					result = Mathf.Round (this.m_Value);
				}
				else {
					result = this.m_Value;
				}
				return result;
			}
			set {
				base.Set (value,true);
			}
		}


		public void UF_SetActive(bool active){
			this.gameObject.SetActive (active);
		}

		public void UF_SetValue (object value){
			if (value == null) {return;}
			rawValue = (float)((double)value);
		}



		protected override void Set (float input, bool sendCallback)
		{
			base.Set (input, sendCallback);

			if (sendCallback && !string.IsNullOrEmpty(eValueChange)) {
				MessageSystem.UF_GetInstance ().UF_Send(DefineEvent.E_UI_OPERA,eValueChange,eParam,m_Value,this);
			}
			if(!Application.isPlaying)
                UF_updateTextInfo(this.m_Value);
		}

		protected void UF_updateTextInfo(float value){
			if (textInfo != null) {
				if (textInfoStyle == TextInfoStyle.Percentage) {
					textInfo.text = string.Format("{0}%", (int)(value * 100.0f * scaleInfo));
				} else if (textInfoStyle == TextInfoStyle.Decimal) {
					textInfo.text = string.Format("{0}", value * scaleInfo);
				} else if (textInfoStyle == TextInfoStyle.Real) {
					textInfo.text = string.Format("{0}", (int)(value * 100.0f * scaleInfo));
				}
			}
		}

		public int UF_TweenToTop(float duration){
			this.rawValue = 0;
			return UF_SmoothTo(maxValue,duration,false);
		}

		public int UF_TweenToBottom(float duration){
			this.rawValue = maxValue;
			return UF_SmoothTo(0,duration,false);
		}

		public int UF_SmoothTo(float targetValue,float duration,bool ingoreTS)
		{
			return UF_SmoothTo(targetValue, duration, ingoreTS, null);
		}

		public int UF_SmoothTo(float targetValue,float duration,bool ingoreTimeScale,DelegateMethod callback){
			if (!this.gameObject.activeInHierarchy) {
				return 0;
			}
			if (duration <= 0) {
				this.value = Mathf.Clamp01 (targetValue);
				if (callback != null) {
					callback (null);
				}
				return 0;
			} else {
				if (m_HandlerSmooth != 0) {
					FrameHandle.UF_RemoveCouroutine (m_HandlerSmooth);
				}
				m_HandlerSmooth = FrameHandle.UF_AddCoroutine(UF_ISmoothTo(targetValue, duration, ingoreTimeScale, callback));
			}
			return m_HandlerSmooth;
		}

		IEnumerator UF_ISmoothTo(float targetValue,float duration,bool ingoreTimeScale,DelegateMethod callback){
			float sourceValue = m_Value;
			float tickbuff = 0;
			float progress = 0;
			float timeDelta = 0;
			while (progress != 1) {
				if (ingoreTimeScale) {
					timeDelta = Time.unscaledDeltaTime;
				} else {
					timeDelta = Time.deltaTime;
				}
				tickbuff += timeDelta;
				progress = Mathf.Clamp01 (tickbuff / duration);
				this.value = sourceValue * (1 - progress) + targetValue * progress;	
				yield return null;
			}
			m_HandlerSmooth = 0;
			if (callback != null) {
				callback (null);
			}
			yield break;
		}


		public int UF_SmoothOverFlow(float insValue,float duration,bool ingoreTimeScale){
			return UF_SmoothOverFlow(insValue,duration,ingoreTimeScale,null);
		}

		public int UF_SmoothOverFlow(float insValue,float duration,bool ingoreTimeScale,DelegateMethod callback){
			if (!this.gameObject.activeInHierarchy) {
				return 0;
			}
			if (duration <= 0) {
				this.value = (this.value + insValue) % 1.0f;
				if (callback != null) {
					callback (null);
				}
				return 0;
			} else {
				if (m_HandlerSmooth != 0) {
					FrameHandle.UF_RemoveCouroutine (m_HandlerSmooth);
				}
				m_HandlerSmooth = FrameHandle.UF_AddCoroutine (UF_ISmoothOverFlow(insValue, duration, ingoreTimeScale, callback));
			}
			return m_HandlerSmooth;
		}


		IEnumerator UF_ISmoothOverFlow(float insValue,float duration,bool ingoreTimeScale,DelegateMethod callback){
			float sourceValue = m_Value;
			float tickbuff = 0;
			float progress = 0;
			float timeDelta = 0;

			while (progress != 1) {
				if (ingoreTimeScale) {
					timeDelta = Time.unscaledDeltaTime;
				} else {
					timeDelta = Time.deltaTime;
				}

				tickbuff += timeDelta;

				progress = Mathf.Clamp01 (tickbuff / duration);

				this.value = (sourceValue + progress * insValue) % 1.0f;

				yield return null;
			}
			m_HandlerSmooth = 0;
			if (callback != null) {
				callback (null);
			}
			yield break;
		}


        public void UF_StopSmooth() {
            if (m_HandlerSmooth != 0)
            {
                FrameHandle.UF_RemoveCouroutine(m_HandlerSmooth);
                m_HandlerSmooth = 0;
            }
        }


		void Update(){
			if(Application.isPlaying)
                UF_updateTextInfo(this.m_Value);
		}
			

	}

}