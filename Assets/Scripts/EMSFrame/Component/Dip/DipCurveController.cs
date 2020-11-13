//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UnityFrame{
    //曲线弹道
    public class DipCurveController : DipThrowController
    {
        //偏移值
        public float offset = 10;
        //时间速率
        public float degRate = 15;

        public float offsetRate = 0;

        private Vector3 m_SinForward;
        private float m_DegRateTick = 0;
        private float m_OffsetRateTick = 0;

        protected override void UF_OnPlay(GameObject tar, Vector3 tarPos, Vector3 vecforward)
        {
            base.UF_OnPlay(tar, tarPos, vecforward);
            m_SinForward = MathX.UF_DegForward(m_Forward, -90);
            m_DegRateTick = 0;
            m_OffsetRateTick = 0;
        }


        protected override void UF_UpdateMoveForward(float dtime)
        {
            lastPosition = this.position;
            float offsetVal = offset;
            if (offsetRate > 0) {
                m_OffsetRateTick += dtime * offsetRate;
                offsetVal = Mathf.Min(m_OffsetRateTick, offset);
            }
            m_DegRateTick += degRate * dtime;
            Vector3 offsetPos = m_SinForward * (Mathf.Sin(m_DegRateTick)) * offsetVal;
            this.position += speed * m_Forward * dtime + offsetPos * dtime;
            this.euler = MathX.UF_EulerAngle(lastPosition, this.position);

        }

    }
}
