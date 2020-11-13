//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UnityFrame
{
    //抛物弹道
    //处理抛物体,酒桶，炮弹等
    public class DipParabolaController : DipController
    {
        public override int type { get { return 2; } }

        //抛物速度速度
        public float speed = 10;

        //是否在地面时才触发判断击中
        public bool inGround;

        //抛物前后偏移位置
        public float tarOffset;

        //高度偏移值
        public float heightFactor;

        //抛物距离
        private float m_Distance;
        //抛物持续时间
        private float m_Duration;

        private Vector3 m_TarPosition;
        private Vector3 m_SorPosition;

        public Vector3 lastPosition { get; private set; }

        private float m_DurationTick = 0;

        protected override void UF_OnPlay(GameObject tar, Vector3 tarPos, Vector3 vecforward)
        {
            if (speed <= 0.01f)
            {
                Debugger.UF_Error("Speed is too samll,play failed!");
                m_IsPlaying = false;
                return;
            }
            vecforward.y = 0;
            m_Forward = vecforward.normalized;

            float len = Vector3.Distance(tarPos, this.position);

            tarPos = len * m_Forward + this.position;

            m_TarPosition = tarPos + tarOffset * m_Forward;
            if (inGround) m_TarPosition.y = 0;

            m_SorPosition = this.position;
            m_Distance = Vector3.Distance(tarPos, this.position);

            m_Duration = m_Distance / speed;
            m_DurationTick = 0;

            lastPosition = this.position;

            //设置角度指向 
            this.euler = new Vector3(0, MathX.UF_EulerAngle(this.position, tarPos).y, 0);
            EffectControl.UF_ResetTailRender(this.gameObject);
        }




        protected override void UF_OnColliderEnter(GameObject hitObject, Vector3 hitPoint)
        {
            //inGround不做任何处理
            if (inGround){return;}

            if (hitObject.tag == DefineTag.Wall) {
                if (Vector3.Distance(lastPosition, this.position) < 0.001f)
                {
                    Debugger.UF_Warn("Fixed Trigger Wall while start");
                    return;
                }

                UF_PlayTriggerDip();
                UF_PlayTriggerEffect(hitPoint);
                this.UF_Stop();
            }
            else if (UF_CheckIsMemberTag(hitObject.tag))
            {
                base.UF_OnColliderEnter(hitObject, hitPoint);
                this.UF_Stop();
            }
            
        }



        protected void UF_UpdateMoveForward(float dtime) {
            if (m_Duration <= 0.001f) {
                this.position = m_TarPosition;
                UF_PlayTriggerDip();
                UF_PlayTriggerEffect(m_TarPosition);
                this.UF_Stop();
            }

            m_DurationTick += dtime;
            float progress = Mathf.Clamp01(m_DurationTick / m_Duration);
            var pos = m_SorPosition * (1 - progress) + m_TarPosition * progress;
            float r = m_Distance / 2;

            //float rx = Mathf.Abs(r  - progress * r * 2);
            //float ry = Mathf.Sqrt(r * r - rx * rx);
            //pos.y += ry * heightFactor;

            float ry = Mathf.Sin(Mathf.Deg2Rad * 180 * progress);
            pos.y += ry * heightFactor;

            this.position = pos;
            if (progress >= 1) {
                UF_PlayTriggerDip();
                UF_PlayTriggerEffect(pos);
                this.UF_Stop();
            }
        }




        protected override void UF_OnRun(float dtime)
        {
            UF_UpdateMoveForward(dtime);
        }


    }

}
