//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections.Generic;
using System;
using UnityEngine;

namespace UnityFrame
{
	public static class GTime
	{
		private static int m_FPS;
		private static int m_LastEnvTime;
		private static float m_TimeScale = 1.0f;
		private static float m_TimeScaleBase = 1.0f;

		public static float Time {get{ return UnityEngine.Time.time;}}

		public static float TimeStamp {get{ return UnityEngine.Time.time;}}

		public static int EnvTick{get{ return System.Environment.TickCount;}}

		public static float FPS {get{ return m_FPS;}}

        public static float DeltaTime { get { return UnityEngine.Time.deltaTime; } }

        public static float UnscaleDeltaTime{get{return UnityEngine.Time.unscaledDeltaTime;}}

        //运行时设定的固定帧间隔
        public static float RunDeltaTime { get; set; }

        public static int FrameRate{get{return Mathf.Max(1,Application.targetFrameRate);}set{Application.targetFrameRate = Mathf.Max(1,value);}}

        //固定时间更新频率
        public static float FixedTimeRate { get { return UnityEngine.Time.fixedDeltaTime; } set { UnityEngine.Time.fixedDeltaTime = value; } }

        //固定时间更新频率
        public static float FixedTime { get { return UnityEngine.Time.fixedTime;}}

        public static int FrameCount {get{ return UnityEngine.Time.frameCount;}}

		public static string DateHMS{get{ return System.DateTime.Now.ToString ("hh:mm:ss");}}

		public static string DateMS{get{ return System.DateTime.Now.ToString ("mm:ss");}}

        public static bool ActiveSyncUpdate { get { return FrameHandle.ActiveSyncUpdate; }set { FrameHandle.ActiveSyncUpdate = value; } }

        /// <summary>
        /// 时间缩放系数
        /// </summary>
        public static float TimeScale{
			get{ 
				return m_TimeScale;

            }
			set{
				m_TimeScale = value;
				UnityEngine.Time.timeScale = m_TimeScale * m_TimeScaleBase;
			}
		}
		/// <summary>
		/// 时间缩放基数
		/// </summary>
		public static float TimeScaleBase{
			get{
				return m_TimeScaleBase;
			}
			set{
				m_TimeScaleBase = value;
				UnityEngine.Time.timeScale = m_TimeScale * m_TimeScaleBase;
			}
		}

		internal static void Update(){
			int targetFrameRate = Application.targetFrameRate;
			if (targetFrameRate > 0) {
				if ((UnityEngine.Time.frameCount % targetFrameRate) == 0) {
					int current_time = System.Environment.TickCount;
					int duration = Mathf.Abs (current_time - m_LastEnvTime);
					duration = duration / targetFrameRate;
					if (duration > 0) {   
						m_FPS = 1000 / duration;
					} else {
						m_FPS = targetFrameRate;
					}
					m_LastEnvTime = current_time;
					//m_FPS = (int)(1.0f / unscaleDeltaTime);
				}
			}
		}

		/// <summary>
		/// 时间戳转时间
		/// </summary>
		public static DateTime UF_TimestampToDateTime(long stamp,bool isNormal = false)
		{
			//DateTime dt = isNormal ? new DateTime() : TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0));
            DateTime dt = isNormal ? new DateTime() : TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 0, 0, 0), TimeZoneInfo.Local);
            DateTime date = dt.AddSeconds (stamp);
			return date;
		}


		/// <summary>
		/// 时间转时间戳
		/// </summary>
		public static long UF_DateTimeToTimestamp(DateTime temp)
		{
            //DateTime dt = TimeZone.CurrentTimeZone.ToLocalTime (new DateTime (1970, 1, 1));
            DateTime dt = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 0, 0, 0), TimeZoneInfo.Local);
            long res = (long)temp.Subtract (dt).TotalSeconds;
			return res;
		}

		public static double UF_GetSystemSeconds()
		{
			TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
			return ts.TotalSeconds;
		}

		public static double UF_GetDateTimeSeconds()
		{
			TimeSpan ts = DateTime.Now - DateTime.Now.Date;
			return ts.TotalSeconds;
		}

		public static double UF_GetSystemTimes()
		{
			TimeSpan ts = DateTime.UtcNow - new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
			return ts.TotalSeconds;
		}

		public static double UF_GetTodayTotalMilliseconds()
		{
			TimeSpan ts = DateTime.UtcNow - new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0, 0);
			return ts.TotalMilliseconds;
		}

        public static string UF_GetLongTimeString() {
            return System.DateTime.Now.ToLongTimeString();
        }

		public static string UF_FormatDateTime(string format){
			return DateTime.Now.ToString(format);
		}

        
		public static string UF_FormatDateTime(long time,string format){
			DateTime dt = new DateTime (time);
			return dt.ToString (format);
		}


			

	}
}

