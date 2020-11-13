//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFrame
{
	/// <summary>
	/// UI 计时器，该计时器用于在UI中，可以自定义format,单位是秒
	/// </summary>	
	public class UIClock : UIObject,IOnReset {
        //时钟运行时类型
        public enum ClockRunType {
            RealTime,
            RunTime,
            FixedTime,
            SyncTime,
        }


        public UILabel target;

		public long clockTimestamp = 0;

		public string format = string.Empty;

        [SerializeField] private ClockRunType m_RunType;
        /// <summary>
        /// 格式为<a [+,-,*,/] b>
        /// 比如<yyyy-1>-mm-dd会先变为<2018-1>-06-29，再运行表达式为2017-06-29
        /// 不支持表达式 使用括号与符号优先级
        /// </summary>
        public bool useRichText = false;

		[SerializeField]private UnityEvent m_UEventTick;

		private DelegateVoid m_EventClockOver;

		private int m_TickBuffer = 0;

        private float m_RunBuffer = 0;

        private bool m_IsOver = true;

		private bool m_IsPause = false;

		private bool m_IsNormal = false;

		private System.DateTime m_DateTime;

		private long m_ISide = 1;

		private int m_TickOverflow = 0;

		public override void UF_SetValue (object value){
			if (value == null) {return;}
			clockTimestamp = (long)value;
		}

		//设置一个计时器,时钟倒计时
		public void UF_SetAsTimer(long timestamp,string _format,DelegateVoid _callback){
			m_IsOver = false;
			m_IsPause = false;
			m_ISide = -1;
			clockTimestamp = timestamp;
			m_IsNormal = true;
			format = _format;
			m_TickBuffer = System.Environment.TickCount;
            m_RunBuffer = 0;
            m_EventClockOver = _callback;
			m_DateTime = GTime.UF_TimestampToDateTime(clockTimestamp, m_IsNormal);
			m_TickOverflow = 0;
            UF_UpdateText();
		}

		//设置为时钟，指定时间戳
		public void UF_SetAsClock(long timestamp,string _format){
            UF_SetAsTimer(timestamp, _format, null);
			m_ISide = 1;
			m_TickOverflow = 0;
			m_IsOver = false;
			m_IsPause = false;
			m_IsNormal = false;
		}

		//设置为时钟，以当前系统时间戳开始
		public void UF_SetAsClock(string _format){
            UF_SetAsTimer((long)GTime.UF_GetSystemSeconds(), _format, null);
			m_ISide = 1;
			m_IsOver = false;
			m_IsNormal = false;
		}

		public void UF_SetAsNormalClock(long timestamp, string _format)
		{
            UF_SetAsClock(timestamp, _format);
			m_IsNormal = true;
			m_DateTime = GTime.UF_TimestampToDateTime(clockTimestamp,m_IsNormal);
            UF_UpdateText();
		}



		public void UF_Pause(){
			m_IsPause = true;
		}

		public void UF_Continue(){
			m_IsPause = false;
		}

		private void UF_UpdateText(){
            if (target != null) {
                if (string.IsNullOrEmpty(format))
                {
                    target.text = clockTimestamp.ToString();
                }
                else
                {
                    if (useRichText)
                    {
                        target.text = GHelper.UF_ParseTextArithmetic(m_DateTime.ToString(format));
                    }
                    else
                    {
                        target.text = m_DateTime.ToString(format);
                    }
                }
            }
			if (m_UEventTick != null) {
				m_UEventTick.Invoke ();
			}
		}


        void UF_UpdateOnRealTime() {
            if (m_IsOver || m_IsPause) return;

            int tick = System.Environment.TickCount + m_TickOverflow;
            int durationTick = Mathf.Abs(tick - m_TickBuffer);
            int stamp = durationTick / 1000;
            if (stamp >= 1)
            {
                m_TickOverflow = durationTick - stamp * 1000;
                m_TickBuffer = tick;
                clockTimestamp += stamp * m_ISide;
                if (clockTimestamp < 0)
                    clockTimestamp = 0;
                if (target != null)
                {
                    //m_DateTime.AddSeconds (stamp);
                    m_DateTime = GTime.UF_TimestampToDateTime(clockTimestamp, m_IsNormal);
                    UF_UpdateText();
                }
            }
            if (clockTimestamp == 0 && m_ISide < 0)
            {
                m_IsOver = true;
                GHelper.UF_SafeCallDelegate(m_EventClockOver);
            }
        }

        void UF_UpdateRunTime(float dtime) {
            if (m_IsOver || m_IsPause) return;
            m_RunBuffer += GTime.DeltaTime;
            if (m_RunBuffer > 1)
            {
                m_RunBuffer = m_RunBuffer - 1.0f;
                clockTimestamp += m_ISide;
                if (clockTimestamp < 0)
                    clockTimestamp = 0;
                if (target != null)
                {
                    m_DateTime = GTime.UF_TimestampToDateTime(clockTimestamp, m_IsNormal);
                    UF_UpdateText();
                }
            }
            if (clockTimestamp == 0 && m_ISide < 0)
            {
                m_IsOver = true;
                GHelper.UF_SafeCallDelegate(m_EventClockOver);
            }
        }



		void Update(){
            if (m_RunType == ClockRunType.RealTime)
            {
                UF_UpdateOnRealTime();
            }
            else if (m_RunType == ClockRunType.RunTime) {
                UF_UpdateRunTime(GTime.DeltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (m_RunType == ClockRunType.FixedTime)
            {
                UF_UpdateRunTime(GTime.FixedTimeRate);
            }
            else if (m_RunType == ClockRunType.SyncTime)
            {
                if (GTime.ActiveSyncUpdate)
                    UF_UpdateRunTime(GTime.RunDeltaTime);
            }
        }


        public void UF_OnReset (){
			m_EventClockOver = null;
			m_TickOverflow = 0;
		}


	}
}
