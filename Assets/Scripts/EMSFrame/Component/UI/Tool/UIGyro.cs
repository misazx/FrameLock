//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System;
using UnityEngine;

namespace UnityFrame
{
	public class UIGyro : MonoBehaviour
	{
		
		public UITexture texture;
		[Range(0, 1)] public float showWidthPercent = 0.6f;
		[Range(0, 1)] public float showHeightPercent = 0.6f;
		[Range(0, 1)] public float border = 0.15f;

		private Gyroscope m_Gyro;
		private float m_GyroX = 0;
		private float m_GyroY = 0;
		private float m_GyroZ = 0;
		private float m_GyroW = 0;
		private float m_DefaultX;
		private float m_DefaultY;

		private void Start()
		{
#if UNITY_EDITOR || UNITY_STANDALONE
			return;
#elif UNITY_ANDROID || UNITY_IPHONE
						if (!texture) return;
			m_DefaultX = (1 - showWidthPercent)/2;
			m_DefaultY = (1 - showHeightPercent)/2;
			texture.uvRect = new Rect(m_DefaultX, m_DefaultY, showWidthPercent, showHeightPercent);
			m_Gyro = Input.gyro;
			m_Gyro.enabled = true;
			UF_ResetGyro();
#endif
        }


        /// <summary>
        /// 重置陀螺仪记录参数
        /// </summary>
        private void UF_ResetGyro()
		{
			if (m_Gyro == null) return;
			m_GyroX = m_Gyro.attitude.x;
			m_GyroY = m_Gyro.attitude.y;
			m_GyroZ = m_Gyro.attitude.z;
			m_GyroW = m_Gyro.attitude.w;
		}

		private void Update()
		{
            UF_CheckGyro();
		}

		/// <summary>
		/// 检测陀螺仪
		/// </summary>
		private void UF_CheckGyro()
		{
			if (m_Gyro == null || !texture ) return;
			float abs_x = Math.Abs(m_Gyro.attitude.x - m_GyroX);
			float abs_y = Math.Abs(m_Gyro.attitude.y - m_GyroY);
			if (abs_x < 0.005f && abs_y < 0.005f) return;
			m_GyroX = m_Gyro.attitude.x - m_GyroX;
			m_GyroY = m_Gyro.attitude.y - m_GyroY;
			m_GyroZ = m_Gyro.attitude.z;
			m_GyroW = m_Gyro.attitude.w;
			float x = 0;
			float y = 0;
			if (abs_x > abs_y)
			{
				float p_x = m_GyroX/border;
				x = m_DefaultX*p_x*(m_GyroZ > 0 ? -1 : 1);
			}
			else
			{
				float p_y = m_GyroY/border;
				y = m_DefaultY*p_y*(m_GyroZ > 0 ? -1 : 1);
			}
            UF_ResetGyro();
			if (m_GyroW < 0 && m_GyroW < -0.5f)
			{
				float temp = x;
				x = y;
				y = -temp;
			}
            UF_SetUVRect(x, y);
		}

		/// <summary>
		/// 设置UV
		/// </summary>
		/// <param name="_x"></param>
		/// <param name="_y"></param>
		private void UF_SetUVRect(float _x, float _y)
		{
			if (!texture) return;
			float x = texture.uvRect.x + _x;
			float y = texture.uvRect.y + _y;
			x = x > (1 - showWidthPercent) ? (1 - showWidthPercent) : x;
			x = x < 0 ? 0 : x;
			y = y > (1 - showHeightPercent) ? (1 - showHeightPercent) : y;
			y = y < 0 ? 0 : y;
			texture.uvRect = new Rect(x, y, showWidthPercent, showHeightPercent);
		}
	}
}
