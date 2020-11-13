//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFrame
{
	//对齐目标
	[RequireComponent(typeof(RectTransform))]
    public class UIAligned : MonoBehaviour {
		public RectTransform target;
		private Vector3 m_Offset;
		private RectTransform m_RectTransform;

		void Start(){
			m_RectTransform = this.GetComponent<RectTransform> ();
			if (m_RectTransform != null && target != null) {
				m_Offset = (m_RectTransform.position - target.position);
			}
		}

		void Update(){
			if (m_RectTransform != null && target != null) {
				float x = (m_RectTransform.position.x - target.position.x);
				float y = (m_RectTransform.position.y - target.position.y);
				if (x != m_Offset.x || y != m_Offset.y) {
					//fix 
					m_RectTransform.position = target.position + m_Offset;
				}
			}
		}

	}

}