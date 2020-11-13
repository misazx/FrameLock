//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace UnityFrame{

	[RequireComponent(typeof(UnityEngine.MeshFilter),typeof(UnityEngine.MeshRenderer))]
	public class UIModelMask : UnityEngine.UI.Image {
		private Vector3[] m_TempPoints = new Vector3[4];
		private MeshRenderer m_MeshRenderer;
		private MeshFilter m_MeshFilter;
		private Mesh m_Mesh;
		private static int[] s_Triangles = { 1, 2, 0, 2, 3, 0 };

		protected override void Awake ()
		{
			base.Awake ();
			if (Application.isPlaying)
				this.color = new Color (0, 0, 0, 0);
			m_Mesh = null;
		}

		private Mesh MMesh{
			get{ 
				if (m_Mesh == null) {
					m_Mesh = new Mesh ();
				}
				return m_Mesh;
			}

		}

		private MeshRenderer MRenderer{
			get{ 
				if (m_MeshRenderer == null)
					m_MeshRenderer = this.gameObject.GetComponent<MeshRenderer> ();
				return m_MeshRenderer;
			}
		}

		private MeshFilter MFilter{
			get{ 
				if (m_MeshFilter == null)
					m_MeshFilter = this.gameObject.GetComponent<MeshFilter> ();
				return m_MeshFilter;
			}
		}



		protected override void OnPopulateMesh(VertexHelper toFill)
		{
			base.OnPopulateMesh (toFill);
            List<UIVertex> tempVertexs = ListCache<UIVertex>.Acquire();

            toFill.GetUIVertexStream (tempVertexs);
			if (tempVertexs.Count >= 6 && MRenderer != null && MFilter != null) {
				m_TempPoints [0] = tempVertexs [0].position;
				m_TempPoints [1] = tempVertexs [1].position;
				m_TempPoints [2] = tempVertexs [2].position;
				m_TempPoints [3] = tempVertexs [4].position;

				MMesh.vertices = m_TempPoints;
				MMesh.triangles = s_Triangles;

				MFilter.mesh = MMesh;
				MRenderer.sharedMaterial = ShaderManager.UF_GetInstance().UF_GetUIModelMaterial (true);
			}
            ListCache<UIVertex>.Release(tempVertexs);
        }


		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			MFilter.mesh = null;
			if(m_Mesh != null)
				Object.DestroyImmediate (m_Mesh);
		}
	}
}
