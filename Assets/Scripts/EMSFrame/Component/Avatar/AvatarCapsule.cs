//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;

namespace UnityFrame{
	
	[System.Serializable]
	public class AvatarCapsule{
		public Vector3 center = Vector3.zero;
		public float radius = 0.5f;
		public float height = 1.0f;
		public float boundRadius = 0.5f;

        public string collisionMask = "Player;Monster";
        public float pushFactor = 2.0f;

        private CharacterController m_Collider;
        private AvatarController m_Avatar;
        private bool m_IsActive = true;
		private float m_Size = 1.0f;
		private float m_SourceRadius = 0.5f;
		private float m_SourceBoundRadius = 0.5f;
		private float m_SourceHeight = 0.5f;
		private Vector3 m_SourceCenter = Vector3.zero;

        private float m_LastCollisionTime = 0;
        private float m_CollisionrRate = 0.25f;

        private bool m_ActiveTriggerCollision = true;

        internal Collider collider { get { return m_Collider; } }

        public bool activeTriggerCollision { get { return m_ActiveTriggerCollision; } set { m_ActiveTriggerCollision = value; } }

        public bool activeCollider {
            get {
                if (m_Collider != null)
                    return m_Collider.enabled;
                else
                    return false;
            }
            set {
                if (m_Collider != null)
                {
                    m_Collider.enabled = value;
                }
            }
        }

        public float collisionrRate { get { return m_CollisionrRate; }set { m_CollisionrRate = value; } }

        public bool isActive{
			get{ 
				return m_IsActive;
			}
			set{ 
				m_IsActive = value;
				if (m_Collider != null) {
					m_Collider.enabled = m_IsActive;
				}
			}
		}



		public float topHeight{
			get{ 
				return (height*0.5f + center.y);
			}
		}

		public float centerHeight{
			get{ 
				return center.y;
			}
		}

        public void UF_OnAwake(AvatarController avatar)
        {
            m_Avatar = avatar;
			//mark source value
			m_SourceRadius = radius;
			m_SourceHeight = height;
			m_SourceBoundRadius = boundRadius;
			m_SourceCenter = center;
            
            m_Collider = avatar.controller;
            
            m_Collider.height = this.height;
            m_Collider.center = this.center;
            m_Collider.radius = this.radius;
            //m_Collider.isTrigger = true;
			m_Collider.enabled = m_IsActive;
		}


		public AvatarCapsule UF_Copy(){
			AvatarCapsule newAc = new AvatarCapsule();
			newAc.center = this.center;
			newAc.radius = this.radius;
			newAc.height = this.height;
			newAc.boundRadius = this.boundRadius;
			return newAc;
		}

		public void UF_Resize(float size){
			if(System.Math.Abs(m_Size - size) < 0.001f) {return;}
			m_Size = size;
			this.radius = m_Size * m_SourceRadius;
			this.height = m_Size * m_SourceHeight;
			this.boundRadius = m_Size * m_SourceBoundRadius;
			this.center = m_Size *m_SourceCenter;

			if (m_Collider != null) {
				CharacterController capc = m_Collider as CharacterController;
                capc.height = this.height;
                capc.center = this.center;
                capc.radius = this.radius;
            }

		}


		public void UF_OnReset(){
            UF_Resize(1.0f);
            m_LastCollisionTime = 0;
        }

        //触发
        internal void UF_OnTriggerEnter(GameObject triTarget)
        {
            if (!activeTriggerCollision) return;
            //相同标签间不执行碰撞
            if (m_Avatar.tag != triTarget.tag) {
                //直接发送到Lua中处理
                MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_AVATAR_TRIGGER, m_Avatar, triTarget);
            }
        }

        //判断触发间隔
        bool UF_CheckCollisionIntervalTime() {
            float timeSeconds = GTime.FixedTime;
            if (Mathf.Abs(timeSeconds - m_LastCollisionTime) < collisionrRate)
            {
                return false;
            }
            m_LastCollisionTime = timeSeconds;
            return true;
        }


        //碰撞
        internal void UF_OnCollisionHit(ControllerColliderHit hit)
        {
            if (!activeTriggerCollision) return;

            GameObject hitTarget = hit.gameObject;

            if (hitTarget.tag == DefineTag.Ground)
            {
                return;
            }

            if (!UF_CheckCollisionIntervalTime()) return;

            //忽略指定层碰撞
            if (hitTarget.tag == DefineTag.Block || hitTarget.tag == DefineTag.Unwalk)
            {
                //碰撞不可走区域
                MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_AVATAR_BLOCK, m_Avatar, hitTarget, hit.point);
                return;
            }

            //只处理角色间碰撞
            if (m_Avatar.tag != hitTarget.tag)
            {
                //if (!CheckCollisionIntervalTime()) return;
                if (GHelper.UF_CheckStringMask(collisionMask, hitTarget.tag))
                {
                    //推开角色
                    if (hit.collider is CharacterController) {
                        var targetController = hit.collider as CharacterController;
                        Vector3 pushForward = MathX.UF_Foward(m_Avatar.position, hit.transform.position);
                        pushForward.y = 0;
                        //Vector3 pushForward = hit.moveDirection;
                        targetController.Move(pushFactor * pushForward.normalized * GTime.RunDeltaTime);
                    }

                    MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_AVATAR_COLLISION, m_Avatar, hitTarget);
                }
            }
        }

        internal void OnDrawGizmos(Transform mAvatarTrans){
			#if UNITY_EDITOR
			if (mAvatarTrans == null)
				return;

			if(this.radius < 0)
				this.radius = 0;
			if(this.boundRadius < 0)
				this.boundRadius = 0;
			if(this.height < 0)
				this.height = 0;
			if(this.height < this.radius*2)
				this.height = this.radius*2; 

			Gizmos.color = Color.yellow;
			GizmosTools.DrawCapsule(mAvatarTrans.position + this.center,this.radius,this.height);
			Gizmos.color = Color.cyan;
			GizmosTools.DrawCircle(mAvatarTrans.position,this.boundRadius);
			#endif
		}


	}

}