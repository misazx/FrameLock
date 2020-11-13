//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UnityFrame {
    //范围性持续
    //处理区域DPS，喷火 等
    public class DipRangeController : DipController
    {
        public override int type { get { return 5; } }

        //持续触发到间隔
        public float interval;

        public bool fixEulerOnPlay = false;

        protected float m_ExistTime;

        protected Dictionary<GameObject, float> m_MapTriggerCache = new Dictionary<GameObject, float>();

        private GameObject m_AttachTarget;

        public void UF_AttachTo(GameObject atarget) {
            m_AttachTarget = atarget;
            if (m_AttachTarget != null) {
                this.transform.position = m_AttachTarget.transform.position;
                this.transform.eulerAngles = m_AttachTarget.transform.eulerAngles;
            }
        }

        protected void UF_UpdateAttach() {
            if (m_AttachTarget == null) return;
            this.transform.position = m_AttachTarget.transform.position;
            this.transform.eulerAngles = m_AttachTarget.transform.eulerAngles;
        }
        

        protected override void UF_OnPlay(GameObject tar, Vector3 tarPos, Vector3 vecforward)
        {
            //this.transform.eulerAngles = MathX.UF_EulerAngle(vecforward);
            //设置角度指向
            if (fixEulerOnPlay)
            {
                this.euler = new Vector3(0, MathX.UF_EulerAngle(vecforward).y, 0);
            }
        }

        protected override void UF_OnRun(float dtime)
        {
            m_ExistTime += dtime;

            if (m_MapTriggerCache.Count > 0 && interval > 0)
            {
                float time = m_ExistTime;
                List<GameObject> tempList = ListCache<GameObject>.Acquire();
                foreach (var key in m_MapTriggerCache.Keys)
                {
                    tempList.Add(key);
                }
                for (int k = 0; k < tempList.Count; k++)
                {
                    var key = tempList[k];
                    float dt = time - m_MapTriggerCache[key];
                    if (dt >= interval)
                    {
                        m_MapTriggerCache[key] = time;
                        var tarHitPoint = key.transform.position;
                        tarHitPoint.y = this.transform.position.y;
                        this.UF_OnColliderEnter(key, tarHitPoint);
                    }
                }
                ListCache<GameObject>.Release(tempList);
            }

            UF_UpdateAttach();
        }

        protected override void UF_OnColliderEnter(GameObject hitObject, Vector3 hitPoint)
        {
            //只对Member 生效
            if (GHelper.UF_CheckStringMask(DefineTagMask.Member, hitObject.tag)) {
                if (!m_MapTriggerCache.ContainsKey(hitObject)) {
                    m_MapTriggerCache.Add(hitObject, m_ExistTime);
                }
                var tarHitPoint = hitObject.transform.position;
                tarHitPoint.y = hitPoint.y;
                base.UF_OnColliderEnter(hitObject, tarHitPoint);
            }
                
        }


        protected override void UF_OnColliderExit(GameObject hitObject, Vector3 hitPoint)
        {
            if (m_MapTriggerCache.ContainsKey(hitObject))
            {
                m_MapTriggerCache.Remove(hitObject);
            }
        }

        public override void UF_OnReset()
        {
            base.UF_OnReset();
            m_MapTriggerCache.Clear();
            m_ExistTime = 0;
            m_AttachTarget = null;
        }


        private void OnDisable()
        {
            m_MapTriggerCache.Clear();
        }

        

    }


}
