//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace UnityFrame{
	public class UISwitchGroup : MonoBehaviour,IUIUpdate,IOnReset
	{
		[SerializeField]private List<MonoBehaviour> m_Targets = new List<MonoBehaviour>();

		[SerializeField]private string m_UpdateKey;

		//记录原始List Count
		private int m_SourceCount;

		public List<MonoBehaviour> targets{get{return m_Targets;}}

		public string updateKey{
			get{ return m_UpdateKey;}
			set{ m_UpdateKey = value;}
		}

		public RectTransform rectTransform{get{ return this.transform as RectTransform;}}

		public void UF_Add(IUIUpdate target){
			if (target != null) {
				m_Targets.Add ((MonoBehaviour)target);
			}
		}

		public void UF_Switch(string key){
			if (m_SourceCount == 0)
				m_SourceCount = m_Targets.Count;
			if (m_Targets != null && m_Targets.Count > 0) {
				IUIUpdate handle = null;
				foreach (MonoBehaviour target in m_Targets) {
					if(target != null){
						handle = target as IUIUpdate;
						if (handle != null) {
							handle.UF_SetActive (handle.updateKey == key);
						}
					}
				}
			}
		}

		public void UF_SetActive (bool active){
			this.gameObject.SetActive (active);
		}

		public void UF_SetValue (object value){
			if (value == null) {return;}
            UF_Switch(value as string);
		}

		public void UF_OnReset(){
			if (m_Targets != null) {
				if (m_SourceCount != 0) {
					if (m_Targets.Count > m_SourceCount) {
						m_Targets.RemoveRange (m_SourceCount - 1, m_Targets.Count - m_SourceCount);
					}
				} else {
					m_Targets.Clear ();
				}
			}
		}


	}


}