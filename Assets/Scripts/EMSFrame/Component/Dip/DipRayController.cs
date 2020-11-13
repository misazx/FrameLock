//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UnityFrame
{
    //线性弹道
    //处理激光
    public class DipRayController : DipController
    {
        public override int type { get { return 3; } }

        //是否穿透(属性相关)
        public bool isPierce;
        //反弹次数(属性相关)
        public int reboundTimes;

        [SerializeField] private List<ULineRenderer> m_Chains = new List<ULineRenderer>();

        public List<ULineRenderer> chains { get { return m_Chains; } }

        //已使用弹射次数
        public int useReboundTimes { get { return UF_GetIntParam("use_rebound_times"); } private set { UF_SetParam("use_rebound_times", value); } }
        //穿刺次数
        public int usePierceTimes { get { return UF_GetIntParam("use_pierce_times"); } private set { UF_SetParam("use_pierce_times", value); } }

        protected override void UF_OnStarted()
        {
            this.collider.enabled = false;
            useReboundTimes = 0;
            usePierceTimes = 0;
            //设置初始值
            this.UF_SetParam("is_pierce", isPierce);
            this.UF_SetParam("rebound_times", reboundTimes);
        }


        public void UF_SetChain(Vector3 from, Vector3 to)
        {
            if (m_Chains == null)
                return;
            for (int k = 0; k < m_Chains.Count; k++)
            {
                if (m_Chains[k] == null) continue;
                m_Chains[k].UF_SetPosition(0, from);
                m_Chains[k].UF_SetPosition(1, to);
            }
        }

        public void UF_SetChain(Vector3[] positions) {
            if (m_Chains == null && positions != null)
                return;
            for (int k = 0; k < m_Chains.Count; k++) {
                if (m_Chains[k] == null) continue;
                m_Chains[k].UF_SetPositions(positions);
            }
        }

        protected void UF_ClearChain() {
            if (m_Chains == null)
                return;
            for (int k = 0; k < m_Chains.Count; k++)
            {
                if (m_Chains[k] == null) continue;
                m_Chains[k].UF_Clear();
            }
        }
        




        public void UF_GetRayHitReflex(Vector3 point, Vector3 forward, float distance,int reflexTimes, List<RaycastHit> listTemp)
        {
            listTemp.Clear();
            Vector3 sPoint = point;
            Vector3 sForward = forward;
            for (int k = 0; k <= reflexTimes; k++)
            {
                RaycastHit hit;
                if (Physics.Raycast(sPoint, sForward, out hit, distance))
                {
                    listTemp.Add(hit);
                    if (hit.collider.gameObject.layer == DefineLayer.HitRaycast)
                    {
                        //作为新的起始点
                        sPoint = hit.point;
                        //作为新方向
                        sForward = sForward - 2 * Vector3.Dot(hit.normal, sForward) * hit.normal;   
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        //排序rayHit
        //直接插入排序
        private void UF_SortRayHits(RaycastHit[] array) {
            int lenght = array.Length;
            int startIdx = 0;
            int endIdx = startIdx + lenght;
            for (int k = startIdx; k < endIdx; k++)
            {
                int idx = k - 1;
                var tmp = array[k];
                while (idx >= startIdx)
                {
                    if (array[idx].distance > tmp.distance)
                    {
                        tmp = array[idx + 1];
                        array[idx + 1] = array[idx];
                        array[idx] = tmp;
                    }
                    idx--;
                }
            }
        }


        public void UF_GetRayAllHitReflex(Vector3 point, Vector3 forward, float distance, int reflexTimes, List<RaycastHit> listTemp)
        {
            listTemp.Clear();
            Vector3 sPoint = point;
            Vector3 sForward = forward;
            for (int k = 0; k <= reflexTimes; k++)
            {
                //遇到hitRay 类型才能进行反射
                var arryHit = Physics.RaycastAll(sPoint, sForward, distance);
                if (arryHit != null && arryHit.Length > 0)
                {
                    //根据距离值进行排序
                    UF_SortRayHits(arryHit);

                    foreach (var hit in arryHit) {
                        listTemp.Add(hit);
                        if (hit.collider.gameObject.layer == DefineLayer.HitRaycast) {
                            //作为新的起始点
                            sPoint = hit.point;
                            //作为新方向
                            sForward = sForward - 2 * Vector3.Dot(hit.normal, sForward) * hit.normal;
                            break;
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }



        protected override void UF_OnPlay(GameObject tar, Vector3 tarPos, Vector3 vecforward)
        {
            Vector3 sorPoint = this.position;
            //if (tar != null)
            //{
            //    vecforward = (tar.transform.position - sorPoint).normalized;
            //}
            //设置角度指向 
            //this.euler = new Vector3(0, MathX.UF_EulerAngle(vecforward).y, 0);
            vecforward.y = 0;
            vecforward.Normalize();
            usePierceTimes = 0;
            useReboundTimes = 0;
            //采用射线检测
            //通过射线方向计算反射位置
            //忽略子弹间的碰撞
            FrameHandle.UF_AddCoroutine(UF_IOnRayCastHit(sorPoint, vecforward), true);

        }

        IEnumerator UF_WaitFixedInterval() {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
        }


        IEnumerator UF_IOnRayCastHit(Vector3 sorPoint, Vector3 vecforward) {
            var listTemp = ListCache<RaycastHit>.Acquire();
            bool ispierce = UF_GetBoolParam("is_pierce");
            int rbtimes = UF_GetIntParam("rebound_times");
            if (ispierce)
                this.UF_GetRayAllHitReflex(sorPoint, vecforward, 1000, rbtimes, listTemp);
            else
                this.UF_GetRayHitReflex(sorPoint, vecforward, 1000, rbtimes, listTemp);

            Vector3 lastPoint = sorPoint;
            var listVectors = ListCache<Vector3>.Acquire();
            UF_ClearChain();
            for (int k = 0;k < listTemp.Count;k++)
            {
                var hit = listTemp[k];
                bool hitblock = hit.collider.gameObject.layer == DefineLayer.HitRaycast;
                if (hitblock || !ispierce)
                {
                    listVectors.Add(lastPoint);
                    lastPoint = hit.point;
                    listVectors.Add(lastPoint);
                    UF_SetChain(listVectors.ToArray());
                    if(hitblock)
                        useReboundTimes++;
                }

                if (!string.IsNullOrEmpty(triggerMask))
                {
                    if (!GHelper.UF_CheckStringMask(triggerMask, hit.collider.tag))
                    {
                        UF_PlayTriggerEffect(hit.point);
                        yield return UF_WaitFixedInterval();

                        continue;
                    }
                }
                //同类型Member不检测
                if (ower == null || ower.tag == hit.collider.tag)
                {
                    UF_PlayTriggerEffect(hit.point);
                    yield return UF_WaitFixedInterval();
                    continue;
                }

                //检查是否被屏蔽
                if (UF_CheckMaskTriggerObject(hit.collider.gameObject)) continue;
                this.UF_OnColliderEnter(hit.collider.gameObject, hit.point);
                
                if(ispierce)
                    usePierceTimes++;

                yield return UF_WaitFixedInterval();
            }

            //SetChain(listVectors.ToArray());
            ListCache<Vector3>.Release(listVectors);
            ListCache<RaycastHit>.Release(listTemp);

            yield return null;
        }

        protected override void UF_OnColliderEnter(GameObject hitObject, Vector3 hitPoint)
        {
            //只对Member 生效
            if (GHelper.UF_CheckStringMask(DefineTagMask.Member, hitObject.tag))
            {
                base.UF_OnColliderEnter(hitObject, hitPoint);
            }
            else {
                UF_PlayTriggerEffect(hitPoint);
            }
        }







    }

}
