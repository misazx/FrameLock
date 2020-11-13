//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace UnityFrame{
	
	public class ScreenShake {
		
		private float m_Duration = 0.5f;
		private float m_ShakeRange = 0.2f;
		private float m_ShakeRate = 0.02f;
		private float m_Attenuation = 0.02f;
		private float m_Padd = -1.0f;
		private float m_ShakeRangeBuf = 0;
		private float m_ShakeDurationBuf = 0;
		private float m_RateBuf = 0;
		private bool m_IsShake = false;
		private Vector3 m_ShakeVector = Vector3.zero;

		public bool isShake{get{ return m_IsShake;}}

		public void UF_ShakeMotion(){
            UF_ShakeMotion(0.2f, 0.02f, 0.02f);
		}
		public void UF_ShakeMotion(float fRange,float fRate,float fAttenuation){
			m_Duration =fRate * fRange / fAttenuation;
			m_ShakeRange = fRange;
			m_ShakeRate = fRate;
			m_Attenuation = fAttenuation;
			m_ShakeDurationBuf = 0;
			m_ShakeRangeBuf = m_ShakeRange;
			m_RateBuf = 0;
			m_IsShake = true;
			m_ShakeVector = Vector3.zero;
		}

		public void Update(ref Vector3 Offset){
			if(m_IsShake){
				m_ShakeDurationBuf += Time.deltaTime;
				if(m_ShakeDurationBuf > m_Duration){
					m_IsShake = false;
					Offset = Vector3.zero;
				}
				else{
					m_RateBuf += Time.deltaTime;
					if(m_RateBuf > m_ShakeRate){
						m_ShakeRangeBuf -= m_Attenuation;
						m_ShakeRangeBuf = Mathf.Clamp(m_ShakeRangeBuf,0,m_ShakeRange);
						m_Padd = -m_Padd;
						m_ShakeVector.y = m_Padd * m_ShakeRangeBuf;
						Offset = m_ShakeVector;
						m_RateBuf = 0;
					}
				}
			}
		}


	}

}