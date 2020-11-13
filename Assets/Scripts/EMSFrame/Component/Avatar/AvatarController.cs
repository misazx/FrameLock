//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityFrame
{
    public class AvatarController : EntityObject, IOnAwake, IOnStart, IOnSyncUpdate,IOnReset
    {

        [SerializeField] private AvatarCapsule m_Capsule = new AvatarCapsule();
        [SerializeField] private AvatarMarkPoint m_MarkPoint = new AvatarMarkPoint();
        [SerializeField] private AvatarMotion m_Motion = new AvatarMotion();
        [SerializeField] private AvatarAnimator m_Animator = new AvatarAnimator();
        [SerializeField] private AvatarRender m_Render = new AvatarRender();

        private Transform m_TransModel;
        private CharacterController m_Controller;
        public AvatarCapsule capsule { get { return m_Capsule; } }
        public AvatarMarkPoint markPoint { get { return m_MarkPoint; } }
        public AvatarMotion motion { get { return m_Motion; } }
        public AvatarAnimator animator { get { return m_Animator; } }
        public AvatarRender render { get { return m_Render; } }

        //采用Controller的方式作为驱动方式
        internal CharacterController controller { get { return m_Controller; } }

        //角色唯一参数
        public string uptr { get; set; }

        public Transform transAvatar { get { return this.transform; } }
        public Transform transModel { get {
                if (m_TransModel == null) {
                    m_TransModel = transAvatar.Find("model");
                }
                return m_TransModel ?? transAvatar;
            }
        }


        public new Vector3 forward { get { return transModel.forward;}}

        public Vector3 topPosition {
            get {
                Vector3 ret = transAvatar.position;
                ret.y += capsule.topHeight;
                return ret;
            }
        }

        public Vector3 focusPosition { get { return capsule.center + transAvatar.position; } }
        public float height { get { return capsule.height; } }
        public float boundRange { get { return capsule.boundRadius; } }

        private Vector3 m_SourceScale = Vector3.one;


        //获取角度返回的向量值
        public Vector3 UF_GetDegForward(float deg) {
            return Quaternion.Euler(0, deg, 0) * forward;
        }

		public void UF_Resize(float size){
			capsule.UF_Resize(size);
			this.localScale = m_SourceScale * size;
		}

        public void UF_OnAwake ()
		{
            //this.SetLayer(DefineLayer.IgoreRaycast);

            m_Controller = this.GetComponent<CharacterController>();
            if (m_Controller == null)
                m_Controller = this.gameObject.AddComponent<CharacterController>();

            m_SourceScale = transModel.localScale;
            m_Capsule.UF_OnAwake (this);
			m_MarkPoint.UF_OnAwake (this);
			m_Motion.UF_OnAwake (this);
			m_Animator.UF_OnAwake (this);
            m_Render.UF_OnAwake(this);
            
		}

        public void UF_OnStart ()
		{
			m_Capsule.isActive = true;
        }

        public void UF_OnSyncUpdate()
		{
			m_Motion.UF_OnUpdate ();
			m_Animator.UF_OnUpdate ();
            m_Render.UF_OnUpdate();
		}

		public void UF_OnReset()
		{
            uptr = string.Empty;
            this.localScale = Vector3.one;
            transModel.localScale = m_SourceScale;
            m_Animator.UF_OnReset ();
			m_MarkPoint.UF_OnReset ();
			m_Motion.UF_OnReset ();
			m_Render.UF_OnReset ();
			m_Capsule.UF_OnReset ();
            //还原标签
            this.tag = DefineTag.Untagged;
        }

        private void OnTriggerEnter(Collider other)
        {
            m_Capsule.UF_OnTriggerEnter(other.gameObject);
        }
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            m_Capsule.UF_OnCollisionHit(hit);
        }


#if UNITY_EDITOR
        void OnDrawGizmos() {
			if (m_Capsule != null) {
				m_Capsule.OnDrawGizmos (this.transform);
			}

			if (m_Motion != null) {
				m_Motion.OnDrawGizmos (this.transform);
			}

		}
		#endif


		
	}
}

