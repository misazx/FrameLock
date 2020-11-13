//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame{
	
	public class ETweenBezierPoint : ETweenBase
	{
		private float m_SourceRSide = 1;

		public List<Vector3> pathPoints{get{ return m_PathPoints;}}

		[SerializeField]private List<Vector3> m_PathPoints = new List<Vector3>();


        public void UF_ClearPoint() {
            if (m_PathPoints != null)
            {
                m_PathPoints.Clear();
            }
        }

        public void UF_AddPoint(Vector3 val) {
            if (m_PathPoints != null)
            {
                m_PathPoints.Add(val);
            }
        }

		protected override void UF_OnPlay()
		{
			if (m_SourceRSide != m_RiseSide) {
				m_SourceRSide = m_RiseSide;
				m_PathPoints.Reverse ();
			}
			if (m_PathPoints.Count >= 0) {
				this.transform.localPosition = m_PathPoints[0];
			}
		}


		protected override void UF_OnRun(float progress)
		{
			if (m_PathPoints.Count >= 0) {
                UF_calcPathLine(m_PathPoints, progress);
			}
		}


		private void UF_calcPathLine(List<Vector3> linePoints,float t){
			if (linePoints.Count == 2) {
				this.transform.localPosition = linePoints [0] * (1 - t) + linePoints [1] * t;
			} else {
				List<Vector3> newlinepoints = new List<Vector3> (); 
				int count = linePoints.Count - 1;
				for (int k = 0; k < count; k++) {
					newlinepoints.Add (linePoints [k] * (1 - t) + linePoints [k + 1] * t);
				}
                UF_calcPathLine(newlinepoints, t);
			}
		}





	}

}