//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UnityFrame {
    //跟踪弹道
    public class DipTrackController : DipController
    {
        public override int type { get { return 4; } }

        //弹道速度
        public float speed = 10;
        //轨迹偏移幅度
        public Vector2 offset = new Vector2(2,4);

        public bool focusAngle = true;

        //持续时间
        private float m_Duration;
        private float m_TimeTick;
        //播放起始点
        private Vector3 m_SourcePos;
        private Vector3 m_LastPos;

        private Vector2 m_trackOffset = Vector2.zero;

        private Vector3 m_CurTargetPos;

        protected override void UF_OnStarted()
        {
            base.UF_OnStarted();
            collider.enabled = false;
        }

        protected override void UF_OnRun(float dtime)
        {
            float progress = 0;
            m_TimeTick += dtime;
            if (m_Duration > 0) {
                progress = Mathf.Clamp01(m_TimeTick/ m_Duration);
            }
            if (m_Target != null)
            {
                Vector3 tarpos = m_Target.transform.position;
                if (!m_Target.activeSelf)
                    tarpos = m_CurTargetPos;
                tarpos.y = m_SourcePos.y;
                Vector3 current = progress * (tarpos) + (1 - progress) * m_SourcePos;
                float sinVal = Mathf.Sin(Mathf.PI * progress);
                Vector3 currentOffset = new Vector3(m_trackOffset.x * sinVal, m_trackOffset.y * sinVal, 0);
                m_LastPos = this.position;
                m_CurTargetPos = m_Target.transform.position;
                this.position = current + currentOffset;
                if (focusAngle)
                    this.euler = MathX.UF_EulerAngle(m_LastPos, this.position);
                if ((progress) >= 1f) {
                    //this.Stop();
                    UF_OnColliderEnter(m_Target,this.position);
                }
            }
            else {
                this.UF_Stop();
            }

        }


        protected override void UF_OnPlay(GameObject tar, Vector3 tarPos, Vector3 vecforward)
        {
            if (speed <= 0.01f)
            {
                Debugger.UF_Error("Speed is too samll,play failed!");
                this.UF_Stop();
                return;
            }
            if (m_Target == null) {
                this.UF_Stop();
                return;
            }
                
            m_TimeTick = 0;
            m_SourcePos = this.position;
            //计算出总时间
            float distance = Vector3.Distance(m_SourcePos, tar.transform.position);
            m_Duration = distance / speed;
            //设置角度指向 
            this.euler = new Vector3(0, MathX.UF_EulerAngle(vecforward).y, 0);
            //偏移幅度
            m_trackOffset.x = offset.x * (Random.Range(1, 100) - 50.0f) / 50.0f;
            m_trackOffset.y = offset.y * (Random.Range(1, 50)) / 50.0f;
            m_Target = tar;
            m_CurTargetPos = m_Target.transform.position;
            EffectControl.UF_ResetTailRender(this.gameObject);
        }


        protected override void UF_OnColliderEnter(GameObject hitObject, Vector3 hitPoint)
        {
            if (GHelper.UF_CheckStringMask(DefineTagMask.Member, hitObject.tag) && hitObject.activeSelf)
                base.UF_OnColliderEnter(hitObject, hitPoint);
            else {
                UF_PlayTriggerDip();
                UF_PlayTriggerEffect(this.position);
            }
            this.UF_Stop();
        }


    }

}
