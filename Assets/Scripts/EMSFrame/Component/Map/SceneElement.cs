//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFrame {
    [RequireComponent(typeof(BoxCollider))]
    public class SceneElement : EntityObject,IOnReset
    {
        public int x { get; set; }
        public int y { get; set; }
        //占用区域
        public int occupyWidth = 1;
        public int occupyHeight = 1;

        private BoxCollider m_Collider;

        public new BoxCollider collider {
            get {
                if (m_Collider == null) {
                    m_Collider = this.GetComponent<BoxCollider>();
                }
                return m_Collider;
            }
        }

        [HideInInspector][SerializeField]private Vector2 m_UnitSize = Vector2.one;

        public Vector2 unitSize {
            get {
                return m_UnitSize;
            }
            set {
                m_UnitSize = value;
                UF_FixColliderSize();
            }
        }

        private Transform m_PivotTransform;
        public Transform pivotTransform
        {
            get {
                if (m_PivotTransform == null) {
                    m_PivotTransform = this.transform.Find("pivot");
                }
                return m_PivotTransform;
            }
        }

        private SpriteRenderer m_Sprite;
        public SpriteRenderer sprite {
            get {
                if (m_Sprite == null) {
                    m_Sprite = this.GetComponentInChildren<SpriteRenderer>();
                }
                return m_Sprite;
            }
        }



        public Vector3 center {
            get {
                Vector3 scale = this.transform.lossyScale;
                return this.position + new Vector3((occupyWidth - 1) * scale.x * m_UnitSize.x / 2, 0, scale.z * (occupyHeight - 1) * m_UnitSize.y / 2);
            }
        }

        public void UF_FixColliderSize() {
            Vector2 hfUnitSize = unitSize / 2;
            Vector3 eWidth = new Vector3(unitSize.x, 0, 0) * (occupyWidth - 1);
            Vector3 eHeight = new Vector3(0, 0, unitSize.y) * (occupyHeight - 1);

            Vector3 size = collider.size;
            Vector3 cent = collider.center;
            size.x = m_UnitSize.x * occupyWidth;
            size.z = m_UnitSize.y * occupyHeight;
            collider.size = size;
            cent = new Vector3((occupyWidth - 1) * m_UnitSize.x / 2, 0, (occupyHeight - 1) * m_UnitSize.y / 2);
            collider.center = cent;
        }


        public void UF_OnReset() {
            this.transform.localScale = Vector3.one;
        }


#if UNITY_EDITOR

        private void OnDrawGizmos()
        {

            //FixColliderSize();

            Vector3 pos = this.transform.position;
            Vector3 scale = this.transform.lossyScale;


            Vector2 hfUnitSize = new Vector2(unitSize.x * scale.x/2, unitSize.y * scale.z / 2);
            Vector3 eWidth = new Vector3(unitSize.x * scale.x, 0, 0) * (occupyWidth - 1);
            Vector3 eHeight= new Vector3(0, 0, unitSize.y * scale.z) * (occupyHeight - 1);
            

            Vector3 posA = new Vector3(pos.x - hfUnitSize.x, 0, pos.z - hfUnitSize.y);
            Vector3 posB = new Vector3(pos.x - hfUnitSize.x, 0, pos.z + hfUnitSize.y) + eHeight;
            Vector3 posC = new Vector3(pos.x + hfUnitSize.x, 0, pos.z + hfUnitSize.y) + eWidth + eHeight;
            Vector3 posD = new Vector3(pos.x + hfUnitSize.x, 0, pos.z - hfUnitSize.y) + eWidth;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(posA, posB);
            Gizmos.DrawLine(posB, posC);
            Gizmos.DrawLine(posC, posD);
            Gizmos.DrawLine(posD, posA);
            //绘制描点和中心点

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(this.position, 0.03f);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(center, 0.03f);

        }



#endif






    }


}

