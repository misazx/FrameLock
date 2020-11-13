//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace UnityFrame
{
	public class ScreenMask
	{
		private GameObject m_GameObject;
		private MeshRenderer m_Render;
		private Camera m_Camera;
		private float m_OffsetZ = 0.1f;
		private float m_OffsetW = 0.1f;
		private float m_SmoothDuration;
		private float m_SmoothTickbuff;
		private bool m_IsSmoothing;
		private Color m_SourceColor;
		private Color m_TargetColor;

		public void UF_SetActive(bool value){
			if (m_GameObject) {
				m_GameObject.SetActive (value);
			}
		}

		public void UF_OnAwake(Camera camera){
			float _far = 0;
			float _hl_height = 0;
			float _hl_length = 0;

			if (camera.orthographic) {
				float width = camera.orthographicSize * camera.aspect * 2.0f;
				float height = width / camera.aspect;
                _far = camera.nearClipPlane / 2.0f + m_OffsetZ;
                _hl_length = width / 2.0f + m_OffsetW;
				_hl_height = height / 2.0f + m_OffsetW;
			} else {
				_far = camera.nearClipPlane + m_OffsetZ;
				_hl_height = _far * Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView/2.0f) + m_OffsetW;
				_hl_length = _hl_height * camera.aspect + m_OffsetW;
			}

			GameObject gameObject = new GameObject ("ScreenMask");
			gameObject.transform.parent = camera.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localEulerAngles = Vector3.zero;

			Vector2[] _uv = new Vector2[4]{new Vector2(1,0),new Vector2(0,0),new Vector2(0,1),new Vector2(1,1)};
			Vector3[] _vertices =new Vector3[4]{new Vector3(-_hl_length,_hl_height,_far),
				new Vector3(-_hl_length,-_hl_height,_far),
				new Vector3(_hl_length,-_hl_height,_far),
				new Vector3(_hl_length,_hl_height,_far)};
			int[] _index = new int[6]{0,2,1,0,3,2};
			Mesh _mesh = new Mesh();
			MeshFilter _mf = gameObject.GetComponent<MeshFilter>();
			if(_mf == null)_mf = gameObject.AddComponent<MeshFilter>();
			MeshRenderer _mr = gameObject.GetComponent<MeshRenderer>();
			if(_mr == null)_mr = gameObject.AddComponent<MeshRenderer>();
			_mf.mesh = _mesh;
			_mesh.vertices = _vertices;
			_mesh.uv = _uv;
			_mesh.triangles = _index;
			_mr.material = new Material(ShaderManager.UF_GetInstance().UF_Find("Game/Transparent/ScreenBlackMask")); 
			m_Render = _mr;
			m_GameObject = gameObject;
		}

		public void UF_SetAplah(float source,float target,float duration){
			if (m_Render == null) {
				return;
			}
			Color current = m_Render.material.color;
			Color cols = new Color (current.r,current.g,current.b,source);
			Color colt = new Color (current.r,current.g,current.b,target);
			UF_SetColor (cols, colt, duration);
		}

		public void UF_SetColor(Color source,Color target,float duration){
			if (m_Render == null) {
				return;
			} else {
				if (duration <= 0) {
					m_Render.material.color = target;
					m_IsSmoothing = false;
					m_GameObject.SetActive (target.a != 0);
				} else {
					m_TargetColor = target;
					m_SourceColor = source;
					m_IsSmoothing = true;
					m_SmoothDuration = duration;
					m_SmoothTickbuff = 0;
					m_GameObject.SetActive (true);
				}
			}
		}

		public void Update(float timeDetla){
			if (m_IsSmoothing && m_Render != null) {
				m_SmoothTickbuff += timeDetla;
				float k = Mathf.Clamp01 (m_SmoothTickbuff / m_SmoothDuration);
				m_Render.material.color = m_SourceColor * (1 - k) + m_TargetColor * k;
				if (k == 1) {
					m_IsSmoothing = false;	
					m_GameObject.SetActive (m_TargetColor.a != 0);
				}
			}
		}



	}
}

