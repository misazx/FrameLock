//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame{
	public class EChainLine: EffectBase
	{
		[SerializeField]private List<ULineRenderer> m_Chains = new List<ULineRenderer>();
		public Transform headPoint;
		public Transform tailPoint;

		private Vector3 m_Source;
		private Vector3 m_Target;
		//线连接的头和尾
		private GameObject m_THead;
		private GameObject m_TTail;

		private Vector3 m_TOffsetHead;
		private Vector3 m_TOffsetTail;

		public List<ULineRenderer> chains{get{ return m_Chains;}}

		public void SetChain(Vector3 from,Vector3 to){
			if (m_Chains == null)
				return;
			m_Source = from;
			m_Target = to;
			for(int k = 0;k < m_Chains.Count;k++){
                m_Chains[k].positionCount = 2;
                m_Chains[k].UF_SetPosition(0,m_Source);
				m_Chains[k].UF_SetPosition(1,m_Target);
			}
			if (headPoint != null) {
				headPoint.position = m_Source;
			} 
			if(tailPoint != null) {
				tailPoint.position = m_Target;
			}
		}

		//设置连接
		public void SetChain(GameObject head,GameObject tail){
			m_THead = head;
			m_TTail = tail;
		}

        public void SetChain(Vector3[] array) {
            if (array == null) return;

            for (int k = 0; k < m_Chains.Count; k++)
            {
                //m_Chains[k].positionCount = array.Length;
                m_Chains[k].UF_SetPositions(array);
            }
        }

		public void SetTOffsetHead(Vector3 value){
			m_TOffsetHead = value;
		}

		public void SetTOffsetTail(Vector3 value){
			m_TOffsetTail = value;
		}

		protected override void UF_OnRun(float progress)
		{
			base.UF_OnRun(progress);
			if (m_THead != null && m_TTail != null) {
				SetChain (m_THead.transform.position + m_TOffsetHead,m_TTail.transform.position + m_TOffsetTail);
			}
		}

		protected override void UF_UF_OnReset()
		{
			m_THead = null;
			m_TTail = null;
		}
	}

}