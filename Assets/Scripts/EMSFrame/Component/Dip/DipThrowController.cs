//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UnityFrame
{
    //投射弹道
    //直线投射弹道
    public class DipThrowController : DipController
    {
        public override int type { get { return 1; } }
        //弹道速度
        public float speed = 10;

        //是否穿透（属性相关）
        public bool isPierce;
        //反弹次数（属性相关）
        public int reboundTimes;

        //是否回弹
        public bool isMoveBack;

        //忽略Block
        public bool ignoreBlock;

        protected bool m_isMovingBack;
        protected bool m_isRebounding;
        protected Vector3 m_hitTargetPos;
        protected float m_MoveBackDuration;
        protected float m_MoveBackDurationTick;
        protected Vector3 m_MoveBackSorPositon;

        public Vector3 lastPosition { get; protected set; }

        //已使用弹射次数
        public int useReboundTimes { get { return UF_GetIntParam("use_rebound_times"); } private set { UF_SetParam("use_rebound_times", value); } }
        //穿刺次数
        public int usePierceTimes { get { return UF_GetIntParam("use_pierce_times"); } private set { UF_SetParam("use_pierce_times", value); } }


        protected override void UF_OnStarted()
        {
            base.UF_OnStarted();
            useReboundTimes = 0;
            usePierceTimes = 0;
            //设置初始值
            this.UF_SetParam("is_pierce", isPierce);
            this.UF_SetParam("rebound_times", reboundTimes);
            this.UF_SetParam("ignore_block", ignoreBlock);
            
        }

        protected override void UF_OnPlay(GameObject tar, Vector3 tarPos, Vector3 vecforward)
        {
            if (speed <= 0.01f)
            {
                Debugger.UF_Error("Speed is too samll,play failed!");
                m_IsPlaying = false;
                return;
            }
            tarPos.y = this.position.y;
            useReboundTimes = 0;
            lastPosition = this.position;
            //设置角度指向 
            this.euler = new Vector3(0, MathX.UF_EulerAngle(vecforward).y, 0);

            EffectControl.UF_ResetTailRender(this.gameObject);
        }


        protected void PlayMoveBack() {
            if (ower == null)
                return;
            m_isMovingBack = true;
            m_MoveBackSorPositon = this.position;
            m_MoveBackDuration = Vector3.Distance(m_MoveBackSorPositon, ower.transform.position)/speed;
            m_MoveBackDurationTick = 0;

        }

        protected void PlayRebound(Vector3 tarPos) {
            m_hitTargetPos = tarPos;
            m_isRebounding = true;
        }



        protected override void UF_OnColliderEnter(GameObject hitObject, Vector3 hitPoint)
        {

            //ground 不参与这类型弹道碰撞
            if (hitObject.tag == DefineTag.Ground) {
                return;
            }
            if (hitObject.tag == DefineTag.Block && UF_GetBoolParam("ignore_block"))
                return;
            else if (hitObject.tag == DefineTag.Block || hitObject.tag == DefineTag.Wall)
            {
                if (hitObject.tag == DefineTag.Wall)
                {
                    if (Vector3.Distance(lastPosition, this.position) < 0.01f)
                    {
                        Debugger.UF_Warn("Fixed Trigger Wall while start");
                        return;
                    }
                }

                //是否需要弹射，并更新反弹次数
                int rbTime = UF_GetIntParam("rebound_times");
                if (rbTime > 0 && useReboundTimes < rbTime)
                {
                    PlayRebound(hitObject.transform.position);
                    return;
                }
                else
                {
                    //回弹逻辑
                    if (isMoveBack && !m_isMovingBack)
                    {
                        PlayMoveBack();
                        return;
                    }
                }
                UF_PlayTriggerDip();
                UF_PlayTriggerEffect(hitPoint);
                this.UF_Stop();
            }
            else if (UF_CheckIsMemberTag(hitObject.tag))
            {
                //检查是否被屏蔽
                if (UF_CheckMaskTriggerObject(hitObject.gameObject)) return;
                //击中目标判定
                base.UF_OnColliderEnter(hitObject, hitPoint);
                UF_PlayTriggerEffect(hitPoint);
                if (UF_GetBoolParam("is_pierce"))
                {
                    //穿刺目标后继续移动
                    usePierceTimes++;
                    return;
                }
                else
                {
                    //回弹逻辑
                    if (isMoveBack && !m_isMovingBack)
                    {
                        PlayMoveBack();
                        return;
                    }
                }
                this.UF_Stop();
            }
        }


        virtual protected void UF_UpdateMoveForward(float dtime)
        {
            lastPosition = this.position;
            this.position += speed * m_Forward * dtime;
        }


        protected void UF_UpdateMoveBack(float dtime) {
            if (!m_isMovingBack)
                return;
            if (m_MoveBackDuration <= 0.0001f || ower == null)
            {
                this.UF_Stop();
                return;
            }
            else {
                m_MoveBackDurationTick += dtime;
                float progress = Mathf.Clamp01(m_MoveBackDurationTick / m_MoveBackDuration);
                Vector3 pos = m_MoveBackSorPositon * (1 - progress) + ower.transform.position * progress;
                this.position = pos;
                //设置角度指向 
                this.euler = new Vector3(0, MathX.UF_EulerAngle(this.position, ower.transform.position).y, 0);
                if (progress >= 1) {
                    this.UF_Stop();
                }
            }
        }

        protected void UF_UpdateRebound(float dtime) {
            if (m_isRebounding) {
                m_isRebounding = false;
                useReboundTimes++;
                Vector3 normal = UF_GetNormalFormBlock(m_hitTargetPos, lastPosition, m_Forward);
                //求得反射方向
                m_Forward = m_Forward - 2 * Vector3.Dot(normal, m_Forward) * normal;
                m_Forward.y = 0;
                m_Forward = m_Forward.normalized;
                //设置角度指向 
                this.euler = new Vector3(0, MathX.UF_EulerAngle(m_Forward).y, 0);
                //更新一次Forward避免异常碰撞
                UF_UpdateMoveForward(dtime);
            }
        }


        protected override void UF_OnRun(float dtime)
        {
            if (m_isRebounding) {
                UF_UpdateRebound(dtime);
            }
            else if (m_isMovingBack)
            {
                UF_UpdateMoveBack(dtime);
            }
            else {
                UF_UpdateMoveForward(dtime);
            }
        }



        public override void UF_OnReset()
        {
            base.UF_OnReset();
            m_isMovingBack = false;
            m_isRebounding = false;
            m_MoveBackDurationTick = 0;
        }



    }
}