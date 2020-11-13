//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

namespace UnityFrame{
	
	[RequireComponent(typeof(LineRenderer))]
	public class UILineRender :UIObject
	{	
		public LineRenderer lineRender;

		private List<Vector3> m_listLinePoint = new List<Vector3>();

        protected override void Start()
        {
            base.Start();
            if (lineRender == null)
            {
                lineRender = this.GetComponent<LineRenderer>();
            }
        }


        private void UF_UpdateLine(){
			if (lineRender != null) {
				lineRender.positionCount = m_listLinePoint.Count;
				lineRender.SetPositions (m_listLinePoint.ToArray());
			}
		}



		public void UF_SetPoint(int index,Vector3 point){
			if (lineRender != null) {
				if (index < m_listLinePoint.Count && index >= 0) {
					m_listLinePoint [index] = point;
                    UF_UpdateLine();
				}
			}
		}

		public void UF_AddPoint(Vector3 point){
			if (lineRender != null) {
				m_listLinePoint.Add (point);
                UF_UpdateLine();
			}
		}

		public void UF_RemovePoint(Vector3 point){
			if (lineRender != null) {
				m_listLinePoint.Remove (point);
                UF_UpdateLine();
			}
		}

		public void UF_RemovePoint(int index){
			if (lineRender != null) {
				if (m_listLinePoint.Count > index && index >= 0) {
					m_listLinePoint.RemoveAt (index);
                    UF_UpdateLine();
				}
			}
		}


		public void UF_SetLineUVTiling(Vector2 value){
			if (lineRender != null) {
				lineRender.material.mainTextureScale = value;
			}
		}


		public void UF_ClearPoint(){
			if (lineRender != null) {
				m_listLinePoint.Clear ();
				lineRender.positionCount = 0;
			}
		}

        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_listLinePoint = null;
        }


    }
}

