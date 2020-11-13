//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UnityFrame{
	[System.Serializable]
	public class AvatarMotion {
		// 转向速度
		public float speedTurn = 0.08f;
		// 移动速度
		public float speedMove = 1.5f;

        private AvatarController m_Avatar;

		private GameObject m_FollowTarget;

		private float m_DistanceBounds = 0.01f;
		private float m_ClampAngleBounds = 0.1f;
		private float m_VelocityTurnAngle = 0.0f;
		private float m_FollowOffset = 0.3f;
		
		private float m_FollowBounds = 0;
		private float m_TurnAngle = 0;

        private bool mLastMoveSate = true;

        private Vector3 m_DirectionPoint = Vector3.zero;
		private Vector3 m_LastDirectionPoint = Vector3.zero;

		private Vector3 m_MoveForward = Vector3.forward;
        private Vector3 m_Forward = Vector3.forward;
        private List<Vector3> m_ListPathPoints = new List<Vector3>();

		private DelegateVoid m_EventPathFinish = null;
		private DelegateBoolMethod m_EventStateChange = null;
		private DelegateVectorMethod m_EventMoveChange = null;


        private LerpVector m_JumpLerp = new LerpVector();
        private float m_JumpHeight;

        private LerpNumber m_RushLerp = new LerpNumber();
        private Vector3 m_RushForward;
        
        public bool isActive{ get; set; }
        //行动是否执行中
		public bool isMoving{ get; private set; }
		public bool isTurning{ get; private set; }
        public bool isJumping { get; private set; }
        public bool isRushing { get; private set; }
        //锁定对应行动计算
        public bool lockMove { get; set; }
        public bool lockTurn { get; set; }
        public bool lockJump { get; set; }
        public bool lockRush { get; set; }

        public bool lockMoveTurn { get; set; }

        public float distanceBounds{get{return m_DistanceBounds;}set{m_DistanceBounds = Mathf.Abs(value);}}

		public float clampAngleBounds{get{return m_ClampAngleBounds;}set{m_ClampAngleBounds = Mathf.Abs(value);}}

        //当前模型的forward向量
        public Vector3 forward { get { return m_Forward; } }

        /// <summary>
        /// 当前移动目的点
        /// </summary>
        public Vector3 directionPoint{get{ return m_DirectionPoint;}}
		/// <summary>
		/// 当前移动向量
		/// </summary>
		public Vector3 moveForward {get{ return m_MoveForward;}}
		/// <summary>
		/// 水平旋转角 
		/// </summary>
		public float turnAngle{get{ return m_TurnAngle;}}

        public Vector3 euler { get { return new Vector3(0, m_TurnAngle, 0); } }

        /// <summary>
        /// 跟随距离约束
        /// </summary>
        public float followBounds{ get; set; }

		/// <summary>
		/// 移动路径点
		/// </summary>
		internal List<Vector3> listPathPoints{get{ return m_ListPathPoints;}}

        private Vector3 position { get { if (m_Avatar != null) { return m_Avatar.position; } else { return Vector3.zero; } } }

		/// <summary>
		/// 总路程
		/// </summary>
		public float totalDistance{
			get{
				Vector3 start = m_Avatar.position;
				float ret = 0;
				ret += Vector3.Distance (m_DirectionPoint, start);
				for (int k = 0; k < m_ListPathPoints.Count; k++) {
					ret += Vector3.Distance (start, m_ListPathPoints [k]);
					start = m_ListPathPoints [k];
				}
				return ret;
			}
		}
		/// <summary>
		/// 移动状态完成事件
		/// </summary>
		public DelegateVoid eventMoveFinish{set{m_EventPathFinish = value;}}
		/// <summary>
		/// 移动状态变更事件，静止或移动
		/// </summary>
		public DelegateBoolMethod eventStateChange{set{m_EventStateChange = value;}}
		/// <summary>
		/// 移动到下一个点事件
		/// </summary>
		public DelegateVectorMethod eventMoveChange{set{m_EventMoveChange = value;}}

		public void UF_OnAwake(AvatarController contoller){
            isActive = true;
            m_Avatar = contoller;
            m_Avatar.transModel.eulerAngles = new Vector3(m_Avatar.transModel.eulerAngles.x,m_TurnAngle, m_Avatar.transModel.eulerAngles.z);
        }

		public void UF_CopyTo(AvatarMotion motion){
			motion.speedMove = this.speedMove;
			motion.speedTurn = this.speedTurn;
			motion.clampAngleBounds = this.m_ClampAngleBounds;
			motion.distanceBounds = this.m_DistanceBounds;

			motion.UF_SetEuler(m_TurnAngle);
			motion.UF_SetPosition(this.m_Avatar.position);
            motion.UF_AddPathPoints(this.m_ListPathPoints, m_EventPathFinish);
        }


        private void UF_ResetPathPoints() {
            if (m_ListPathPoints.Count > 0)
                m_ListPathPoints.Clear();
        }

		/// <summary>
		/// 添加路径点
		/// </summary>
		/// <param name="point">Point.</param>
		public void UF_AddPathPoint(Vector3 point,DelegateVoid eventPathFinish){
            if (!isActive) return;
			if (m_ListPathPoints.Count == 0 && m_Avatar != null) {
				m_DirectionPoint = m_Avatar.position;
			}
			m_ListPathPoints.Add (point);
			m_EventPathFinish = eventPathFinish;
			isTurning = true;
			isMoving = true;
		}

		internal void UF_AddPathPoints(List<Vector3> points,DelegateVoid eventPathFinish){
            if (!isActive) return;
            if (points != null && points.Count > 0) {
				if (m_ListPathPoints.Count == 0 && m_Avatar != null) {
					m_DirectionPoint = m_Avatar.position;
				}
				for (int k = 0; k < points.Count; k++) {
					m_ListPathPoints.Add (points[k]);
				}
				m_EventPathFinish = eventPathFinish;
				isTurning = true;
				isMoving = true;
			}
		}

        public void UF_ClearPathPoint(bool stopMove) {
            m_ListPathPoints.Clear();
            if(isMoving && stopMove)
                isMoving = false;
        }


        // 移动接口, 基于当前速度往指定方向移动
        public void UF_Move(Vector3 forward){
            if (!isActive) return;
            UF_ResetPathPoints();
            isMoving = true;
            m_DirectionPoint = m_Avatar.position + forward;
            m_MoveForward = (m_DirectionPoint - m_Avatar.position).normalized;

            //MARK: 帧同步
            //UF_MoveAvatarPostion(forward * speedMove * GTime.RunDeltaTime);

            isTurning = true;
            if(!lockMoveTurn)
            UF_TurnTo(m_DirectionPoint);
        }

        /// <summary>
        /// 设置目的地，角色直线运动到目的地
        /// </summary>
        public void UF_MoveTo(Vector3 point, DelegateVoid eventDestinationFinish)
        {
            if (!isActive) return;
            //SetStay();
            UF_AddPathPoint(point, eventDestinationFinish);
        }

        //跳跃到某一点上
        public void UF_JumpTo(Vector3 point,float height,float duration) {
            if (!isActive) return;
            //SetStay();
            m_JumpLerp.Reset(this.position,point, duration);
            m_JumpHeight = height;
            isJumping = true;
            UF_TurnTo(point);
        }

        //处理碰撞，所以采用方向向量 + 持续时间
        public void UF_RushTo(Vector3 point,float duration,bool needTurn) {
            if (!isActive) return;
            if (duration <= 0.0001f)
            {
                Debugger.UF_Warn("Duration is Zero,Rush Failed");
                return;
            }
            float sp = Vector3.Distance(point, this.position) / duration;
            m_RushForward = Vector3.Normalize(point - this.position) * sp;
            m_RushLerp.Reset(0,1,duration);
            isRushing = true;
            if (needTurn) {
                UF_TurnTo(point);
            }
        }

        public void UF_TurnTo(float angle)
        {
            if (!isActive) return;
            //计算动态旋转速度
            float angleClamp = MathX.UF_ClampNormalAngle(m_TurnAngle, angle);
            m_TurnAngle = angle;
            isTurning = true;
        }
        public void UF_TurnTo(Vector3 point)
        {
            if (!isActive) return;
            if (m_Avatar == null)
            {
                Debugger.UF_Warn("m_TransAvatar is null TurnToPoint Fial");
                return;
            }
            UF_TurnTo(MathX.UF_EulerAngle(m_Avatar.position, point).y);
        }

        public void TurnForward(Vector3 forward)
        {
            if (!isActive) return;
            UF_TurnTo(MathX.UF_EulerAngle(forward).y);
        }


        //设置跟随
        public void UF_FollowTo(GameObject target, bool stay)
        {
            if (stay)
            {
                UF_SetStay();
            }
            m_FollowTarget = target;
        }

        public void UF_SetStay()
        {
            isMoving = false;
            isJumping = false;
            isRushing = false;
            m_ListPathPoints.Clear();
            m_EventPathFinish = null;
            m_MoveForward = Vector3.zero;
            //Y轴归零
            Vector3 pos = m_Avatar.position; pos.y = 0;
            m_Avatar.position = pos;
        }

        public void UF_SetEuler(float angle){
			m_TurnAngle = angle;
            m_Avatar.transModel.eulerAngles = new Vector3 (m_Avatar.transModel.eulerAngles.x, m_TurnAngle, m_Avatar.transModel.eulerAngles.z);
		}


		public void UF_SetPosition(Vector3 pos){
			if (m_Avatar == null) {
				return;
			}
			m_Avatar.position = pos;
            UF_SetStay();
		}

		public void UF_SetModelLoalPosition(Vector3 pos){
            m_Avatar.transModel.localPosition = pos;
        }

		//获取与目标最近位置
		public Vector3 UF_GetNearPosition(AvatarController target){
			if (m_Avatar != null && target != null) {
				float rangeA = m_Avatar.capsule.boundRadius;
				float rangeB = target.capsule.boundRadius;

				Vector3 vf = Vector3.Normalize (m_Avatar.position - target.position);

				return target.position + vf * (rangeA + rangeB);
			}
			return Vector3.zero;
		}

		//获取与目标会面位置
		public Vector3 UF_GetMeetPositon(AvatarController target){
			if (m_Avatar != null && target != null) {

				float rangeA = m_Avatar.capsule.boundRadius;
				float rangeB = target.capsule.boundRadius;

				return target.position + target.forward * (rangeA + rangeB);

			}
			return Vector3.zero;
		}

		public void UF_OnReset(){
			isActive = true;
			m_DirectionPoint = Vector3.zero;
			m_MoveForward = Vector3.zero;
			m_EventPathFinish = null;
			m_EventMoveChange = null;
			m_FollowTarget = null;
			m_FollowBounds = 0;
			mLastMoveSate = true;
            if (m_Avatar != null) {
                m_Avatar.transModel.eulerAngles = Vector3.zero;
                m_TurnAngle = 0;
            }
            UF_SetStay();
            lockJump = false;
            lockMove = false;
            lockRush = false;
            lockTurn = false;
        }




		private void UF_UpdateFollowMotion(){
			if (m_FollowTarget != null) {
				if (!m_FollowTarget.activeSelf) {
					m_FollowTarget = null;
					return;
				}
				float bounds = m_Avatar.capsule.radius * 0.5f;
                Vector3 tpos = m_FollowTarget.transform.position;

                if (MathX.UF_Distance(tpos, m_Avatar.position) > (followBounds + bounds + m_FollowOffset) ) {
					m_FollowOffset = 0;
					Vector3 targetPoint = tpos + (m_Avatar.position - tpos).normalized * (bounds + followBounds);
					isMoving = false;
                    UF_AddPathPoint(targetPoint,null);
				}
			}
		}

		private void UF_InvokeMoveStateChange(bool value){
			if (m_EventStateChange != null) {
				m_EventStateChange.Invoke (value);
			}
		}

		private void UF_InvokePathPointChange(Vector3 point){
			if (m_EventMoveChange != null) {
				m_EventMoveChange.Invoke(point);
			}
		}

		private void UF_InvokePathPointFinish(){
			//完成移动
			DelegateVoid tmp = m_EventPathFinish;

			m_EventPathFinish = null;

			GHelper.UF_SafeCallDelegate (tmp);
		}



        private void UF_MoveAvatarPostion(Vector3 vf) {
            if (m_Avatar.controller && m_Avatar.controller.enabled)
            {
                //采用controller的驱动方式
                m_Avatar.controller.Move(vf);
            }
            else {
                //采用普通位移驱动方式
                m_Avatar.position += vf;
            }
            //Y轴归零
            var pos = m_Avatar.position; pos.y = 0;
            m_Avatar.position = pos;
        }



        private void UF_UpdateMove(){
			if (!isMoving || lockMove) return;

            if (MathX.UF_DistanceSquare(m_Avatar.position, m_DirectionPoint) > m_DistanceBounds)
            {
                //重新计算路径点移动向量
                m_MoveForward = (m_DirectionPoint - m_Avatar.position).normalized;
                UF_MoveAvatarPostion(m_MoveForward * speedMove * GTime.RunDeltaTime);
                //判断是否已经超过目标点了
                if (Vector3.Dot(m_MoveForward, m_DirectionPoint - m_Avatar.position) < 0)
                {
                    m_Avatar.position = m_DirectionPoint;
                }
            }
            else
            {
                if (m_ListPathPoints.Count > 0)
                {
                    m_LastDirectionPoint = m_DirectionPoint;
                    m_DirectionPoint = m_ListPathPoints[0];
                    //计算路径点移动向量
                    m_MoveForward = (m_DirectionPoint - m_Avatar.position).normalized;
                    if (!lockMoveTurn)
                        UF_TurnTo(m_DirectionPoint);
                    m_ListPathPoints.RemoveAt(0);
                    //派发路径点变动事件
                    UF_InvokePathPointChange(m_DirectionPoint);
                    //直接执行一次，不需等待下一帧
                    UF_UpdateMove();
                }
                else
                {
                    isMoving = false;
                    m_FollowOffset = 0.3f;
                    //派发路径点变动事件
                    UF_InvokePathPointChange(m_Avatar.position);
                    UF_InvokePathPointFinish();
                }
            }
            //			更新状态
            if (mLastMoveSate != isMoving) {
				//移动状态变更
				mLastMoveSate = isMoving;
                UF_InvokeMoveStateChange(mLastMoveSate);
			}
		}

		private void UF_UpdateTurn(){
            if (!isTurning || lockTurn) return;

            if (Mathf.Abs(m_Avatar.transModel.eulerAngles.y - m_TurnAngle) > m_ClampAngleBounds)
            {
                float _angle = Mathf.SmoothDampAngle(m_Avatar.transModel.eulerAngles.y, m_TurnAngle, ref m_VelocityTurnAngle, speedTurn);
                m_Forward = MathX.UF_DegForward(Vector3.forward, _angle);
                m_Avatar.transModel.eulerAngles = new Vector3(m_Avatar.transModel.eulerAngles.x, _angle, m_Avatar.transModel.eulerAngles.z);
            }
            else
            {
                isTurning = false;
                m_VelocityTurnAngle = 0;
            }
        }



        private void UF_UpdateJump()
        {
            if (!isJumping || lockJump) return;

            if (m_JumpLerp.UF_Run(GTime.RunDeltaTime))
            {
                Vector3 pos = m_JumpLerp.current;
                float h = m_JumpHeight * Mathf.Sin(Mathf.PI * m_JumpLerp.progress);
                pos.y = h;
                m_Avatar.position = pos;
            }
            else
            {
                isJumping = false;
            }
        }


        private void UF_UpdateRush()
        {
            if (!isRushing || lockRush) return;

            if (m_RushLerp.UF_Run(GTime.RunDeltaTime))
            {
                Vector3 lastPos = m_Avatar.position;
                UF_MoveAvatarPostion(m_RushForward * GTime.RunDeltaTime);
                if (Vector3.Distance(lastPos, m_Avatar.position) < 0.001f) {
                    //Debug.Log("Rush Abort");
                    isRushing = false;
                }
            }
            else
            {
                isRushing = false;
            }
        }


        public void UF_OnUpdate(){
			if(!isActive)
				return;

			if (m_Avatar != null) {
                UF_UpdateMove();

                UF_UpdateJump();

                UF_UpdateRush();

                UF_UpdateTurn();

                UF_UpdateFollowMotion();
            }
        }

		/// <summary>
		/// Release this instance.
		/// set ref null
		/// </summary>
		public void Release(){
			m_Avatar = null;
			m_FollowTarget = null;
		}


		internal void OnDrawGizmos(Transform target){
			#if UNITY_EDITOR
			if (!isMoving)
				return;
			Gizmos.color = Color.red;
			Gizmos.DrawLine(target.position,m_DirectionPoint);
			Gizmos.DrawSphere (m_DirectionPoint,0.15f);
			if (m_ListPathPoints.Count > 0 && target != null) {
				Gizmos.DrawLine(m_DirectionPoint,m_ListPathPoints[0]);    
				Gizmos.DrawSphere (target.position,0.15f);
				for (int k = 0; k < m_ListPathPoints.Count - 1; k++) {
					Gizmos.DrawSphere (m_ListPathPoints[k],0.15f);
					Gizmos.DrawLine(m_ListPathPoints[k],m_ListPathPoints[k+1]);
				}
				Gizmos.DrawSphere (m_ListPathPoints[m_ListPathPoints.Count - 1],0.15f);
			}
			#endif
		}
	}


}