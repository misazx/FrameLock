//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UnityFrame {
    //爆炸弹道
    //处理类型效果
    public class DipExplodeController : DipController
    {
        public override int type { get { return 7; } }
        //延迟触发
        public float delay = 0;

        //爆炸只伤害一次
        protected HashSet<GameObject> m_MapTriggerCache = new HashSet<GameObject>();

        private float m_TickDelay = 0;
        //爆炸特效
        public string explodeEffect = "";

        private GameObject m_AttachTarget;

        public void AttachTo(GameObject atarget)
        {
            m_AttachTarget = atarget;
            if (m_AttachTarget != null)
            {
                this.transform.position = m_AttachTarget.transform.position;
                this.transform.eulerAngles = m_AttachTarget.transform.eulerAngles;
            }
        }

        protected void UpdateAttach()
        {
            if (m_AttachTarget == null) return;
            this.transform.position = m_AttachTarget.transform.position;
            this.transform.eulerAngles = m_AttachTarget.transform.eulerAngles;
        }

        protected override void UF_OnColliderEnter(GameObject hitObject, Vector3 hitPoint)
        {
            //只对Member 生效
            if (GHelper.UF_CheckStringMask(DefineTagMask.Member, hitObject.tag))
            {
                if (!m_MapTriggerCache.Contains(hitObject))
                {
                    m_MapTriggerCache.Add(hitObject);
                    var tarHitPoint = hitObject.transform.position;
                    tarHitPoint.y = hitPoint.y;
                    base.UF_OnColliderEnter(hitObject, tarHitPoint);
                    return;
                }
            }
        }

        protected override void UF_OnPlay(GameObject tar, Vector3 tarPos, Vector3 vecforward)
        {
            base.UF_OnPlay(tar, tarPos, vecforward);
			//base.UF_OnStart();
			//设置角度指向 
			//this.euler = new Vector3(0, MathX.UF_EulerAngle(vecforward).y, 0);
			if (delay > 0)
            {
                collider.enabled = false;
                m_TickDelay = 0;
            }
            else
            {
                FXManager.UF_GetInstance().UF_Play(explodeEffect, this.position);
            }
        }


        protected override void UF_OnRun(float dtime)
        {
            base.UF_OnRun(dtime);
            if (delay > 0) {
                m_TickDelay += dtime;
                if (m_TickDelay >= delay && !collider.enabled)
                {
                    collider.enabled = true;
                    FXManager.UF_GetInstance().UF_Play(explodeEffect, this.position);
                }
            }
            UpdateAttach();
        }



        public override void UF_OnReset()
        {
            base.UF_OnReset();
            m_MapTriggerCache.Clear();
            m_TickDelay = 0;
            m_AttachTarget = null;
        }

    }

}

