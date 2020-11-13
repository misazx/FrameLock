//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace UnityFrame {
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class ULineRenderer : MonoBehaviour
	{
        public float width = 1;
        public bool useWorldSpace = true;

        [SerializeField]private List<Vector3> m_Positions = new List<Vector3>();


        private static List<Vector3> s_ListTempVert = new List<Vector3>();
        private static List<Vector2> s_ListTempUV = new List<Vector2>();
        private static List<int> s_ListTempIndex = new List<int>();

        private MeshFilter m_MeshFilter;
        private MeshRenderer m_MeshRenderer;
        private Mesh m_Mesh;


        public int positionCount {
            get {
                return m_Positions.Count;
            }
            set {
                if (value > m_Positions.Count)
                {
                    for (int k = m_Positions.Count - 1; k < value - m_Positions.Count; k++)
                    {
                        m_Positions.Add(Vector3.zero);
                    }
                }
                else if(value < m_Positions.Count)  {
                    m_Positions.RemoveRange(value - 1, m_Positions.Count - value);
                }
            }
        }

        public List<Vector3> positions
        {
            get
            {
                return m_Positions;
            }
        }

        protected MeshFilter meshFilter {
            get {
                if (m_MeshFilter == null) {
                    m_MeshFilter = gameObject.GetComponent<MeshFilter>();
                }
                return m_MeshFilter;
            }
        }

        protected MeshRenderer meshRenderer {
            get {
                if (m_MeshRenderer == null) {
                    m_MeshRenderer = gameObject.GetComponent<MeshRenderer>();
                }
                return m_MeshRenderer;
            }
        }

        protected Mesh mesh {
            get {
                if (m_Mesh == null) m_Mesh = new Mesh();
                return m_Mesh;
            }
        }


        public void UF_SetPosition(int index, Vector3 v) {
            int curCount = m_Positions.Count;
            if (curCount < index) {
                int curIdx = curCount - 1;
                for (int k = curIdx + 1; k <= index; k++) {
                    m_Positions.Add(m_Positions[curIdx]);
                }
            }
            this.UF_SetDirty();
        }

        public void UF_SetPositions(Vector3[] points) {
            m_Positions.Clear();
            if (points != null) {
                foreach (var point in points) {
                    m_Positions.Add(point);
                }
            }
            this.UF_SetDirty();
        }


        public void UF_Clear() {
            m_Positions.Clear();
            this.UF_SetDirty();
        }


        //处理网格
        public void OnPopulateMesh() {
            float hfwidth = width / 2;
            mesh.Clear();
            s_ListTempUV.Clear();
            s_ListTempVert.Clear();
            s_ListTempIndex.Clear();
            if (m_Positions.Count < 1) return;
            for (int k = 0; k < m_Positions.Count - 1; k++) {
                var pointA = m_Positions[k];
                var pointB = m_Positions[k + 1];
                if (useWorldSpace) {
                    pointA = pointA - this.transform.position;
                    pointB = pointB - this.transform.position;
                }
                var forward = (pointB - pointA).normalized * hfwidth;
                Vector3 p1 = MathX.UF_RotateForward(forward, -90) + pointA;
                Vector3 p2 = MathX.UF_RotateForward(forward, -90) + pointB;
                Vector3 p3 = MathX.UF_RotateForward(forward, 90) + pointB;
                Vector3 p4 = MathX.UF_RotateForward(forward, 90) + pointA;
                //加入顶点
                s_ListTempVert.Add(p1);
                s_ListTempVert.Add(p2);
                s_ListTempVert.Add(p3);
                s_ListTempVert.Add(p4);
                //加入UV
                s_ListTempUV.Add(new Vector2(1, 0));
                s_ListTempUV.Add(new Vector2(0, 0));
                s_ListTempUV.Add(new Vector2(0, 1));
                s_ListTempUV.Add(new Vector2(1, 1));
                //加入顶点索引
                //0,2,1,0,3,2
                s_ListTempIndex.Add(3 + k * 4);
                s_ListTempIndex.Add(0 + k * 4);
                s_ListTempIndex.Add(2 + k * 4);
                s_ListTempIndex.Add(2 + k * 4);
                s_ListTempIndex.Add(0 + k * 4);
                s_ListTempIndex.Add(1 + k * 4);

            }


            mesh.vertices = s_ListTempVert.ToArray();
            mesh.uv = s_ListTempUV.ToArray();
            mesh.triangles = s_ListTempIndex.ToArray();

            meshFilter.mesh = mesh;
        }


        protected void UF_SetDirty() {
            OnPopulateMesh();
        }


        private void OnRectTransformDimensionsChange()
        {
            UF_SetDirty();
        }

        private void OnEnable()
        {
            UF_SetDirty();
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            this.UF_SetDirty();
        }

        void OnDrawGizmos()
        {
            for (int k = 0; k < s_ListTempVert.Count; k++) {
                UnityEditor.Handles.Label(s_ListTempVert[k], k.ToString());
                //Gizmos.DrawIcon(s_ListTempVert[k], k.ToString());
            }

        }
#endif

    }
}



