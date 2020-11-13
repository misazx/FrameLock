//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityFrame{
	[System.Serializable]
	public class UIEventClicker
	{
		public static float IntervalDoubleClick = 0.25f;

		public string ePressClick = "";
		public string ePressDClick = "";
		public string ePressDown= "";
		public string ePressUp = "";

		public string eSound = "wav_click";

		public string[] eParams = { "" };

		private float m_ClickLastTime = 0;

		public string UF_GetParam(int index){
			if (eParams == null || eParams.Length == 0) {
				return "";
			} else {
				return eParams [0];
			}
		}

		public void UF_SetEParam(string param)
		{
			if (eParams == null || eParams.Length == 0) {
				eParams = new string[] { param };
			} else {
				eParams [0] = param;
			}
		}

		public void UF_SetEParam(int index, string param)
		{
			if (index >= eParams.Length)
			{
				string[] tmp = new string[index + 1];
				System.Array.Copy(eParams, tmp, eParams.Length);
				tmp[index] = param;
				eParams = tmp;
			}
			else
			{
				eParams[index] = param;
			}
		}

		public void UF_ClearEParams()
		{
			eParams = new string[1] { "" };
		}



		private void UF_SendUIOperaMessage(string eventName,Object target){
			MessageSystem msg = MessageSystem.UF_GetInstance ();
			msg.UF_BeginSend();
			msg.UF_PushParam(eventName);
			for (int k = 0; k < eParams.Length; k++) {
				msg.UF_PushParam(eParams [k]);
			}
			msg.UF_PushParam(target);
			msg.UF_EndSend(DefineEvent.E_UI_OPERA);
		}



		public void UF_OnClick(Object target)
		{
			if (!string.IsNullOrEmpty(ePressClick))
			{
                UF_SendUIOperaMessage(ePressClick,target);
			}
		}

		public bool UF_OnDoubleClick(Object target)
		{
			float clickDetla = (Time.unscaledTime - m_ClickLastTime);
			if (clickDetla <= IntervalDoubleClick) {
				m_ClickLastTime = 0;
				if (!string.IsNullOrEmpty(ePressDClick))
				{
                    UF_SendUIOperaMessage(ePressDClick,target);
					return true;
				}
			} else {
				m_ClickLastTime = Time.unscaledTime;
			}
			return false;
		}

		public void UF_OnDown(Object target)
		{
			if (!string.IsNullOrEmpty(ePressDown))
			{
                UF_SendUIOperaMessage(ePressDown,target);
			}
			AudioManager.UF_GetInstance().UF_Play(eSound);
		}


		public void UF_OnUp(Object target)
		{
			if (!string.IsNullOrEmpty(ePressUp))
			{
                UF_SendUIOperaMessage(ePressUp,target);
			}
		}

	}

}