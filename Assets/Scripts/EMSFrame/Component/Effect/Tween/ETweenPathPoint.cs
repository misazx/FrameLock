using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFrame
{
    public class ETweenPathPoint : ETweenBase
    {
        public List<Vector3> pathPoints { get { return m_PathPoints; } }

        public bool spaceWorld = true;

        [SerializeField] private AnimationCurve m_Curve = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField] private List<Vector3> m_PathPoints = new List<Vector3>();

        //如果是匀速 计算每段距离用的时间
        private List<float> m_pointDuration = new List<float>();

        //private int m_side = -1;

        public void UF_ClearPoint()
        {
            if (m_PathPoints != null)
            {
                m_PathPoints.Clear();
            }
        }

        public void UF_AddPoint(float x, float y, float z)
        {
            if (m_PathPoints != null)
            {
                m_PathPoints.Add(new Vector3(x, y, z));
            }
        }
        public void UF_AddPoint(Vector3 val)
        {
            if (m_PathPoints != null)
            {
                m_PathPoints.Add(val);
            }
        }
        private float UF_TotalDistance()
        {
            float d = 0;
            for (int i = 0; i < this.m_PathPoints.Count - 1; i++)
            {
                d += Vector3.Distance(this.m_PathPoints[i], this.m_PathPoints[i + 1]);
            }
            return d;
        }
        protected override void UF_OnPlay()
        {
            m_pointDuration.Clear();
            if (this.m_PathPoints.Count < 2)
            {
                base.m_IsOver = true;
                return;
            }
            //if (modeType == EffectModeType.PingPong)
            //{
            //    m_side *= -1;
            //}
            //else
            //{
            //    m_side = 1;
            //}

            this.m_pointDuration.Add(0);
            float distance = UF_TotalDistance();
            float d = 0;
            for (int i = 0; i < this.m_PathPoints.Count - 1; i++)
            {
                d += Vector3.Distance(this.m_PathPoints[i], this.m_PathPoints[i + 1]);
                this.m_pointDuration.Add(d / distance * base.duration);
            }
        }

        protected virtual void UF_SetPosition(Vector3 val) {
            if (spaceWorld)
            {
                this.transform.position = val;
            }
            else
            {
                this.transform.localPosition = val;
            }
        }

        protected override void UF_OnStop()
        {
            base.UF_OnStop();
            this.m_pointDuration.Clear();
            //this.m_side = -1;
        }

        protected override void UF_OnRun(float progress)
        {
            if (this.m_PathPoints.Count <= 1)
                return;

            //匀速运动                
            float tmpCurrentDuration = progress * base.duration;
            int curIndex = 0;
            int nextIndex = 0;
            float tmpProgress = 0;
            //bool isRever = m_RiseSide == -1;
            bool isRever = isReverse;
            UF_GetDurationIndex(tmpCurrentDuration, isRever, ref curIndex, ref nextIndex);

            if (curIndex == nextIndex)
                tmpProgress = 1;
            else
                tmpProgress = (tmpCurrentDuration - this.m_pointDuration[curIndex]) / (this.m_pointDuration[nextIndex] - this.m_pointDuration[curIndex]);

            Vector3 source = this.m_PathPoints[curIndex];
            Vector3 target = this.m_PathPoints[nextIndex];

            float K = this.m_Curve.Evaluate(tmpProgress);
            Vector3 current = source * (1.0f - K) + target * K;

            this.UF_SetPosition(current);
        }

        protected void UF_GetDurationIndex(float tmpCurrentDuration, bool isRever, ref int curIndex, ref int nextIndex)
        {
            if (tmpCurrentDuration == 0)
            {
                curIndex = 0;
                nextIndex = 0;
            }
            else if (tmpCurrentDuration == base.duration)
            {
                curIndex = this.m_PathPoints.Count - 1;
                nextIndex = curIndex;
            }
            else if (isRever)
            {
                for (int i = 0; i < this.m_pointDuration.Count; i++)
                {
                    if (this.m_pointDuration[i] > tmpCurrentDuration)
                    {
                        curIndex = i;
                        nextIndex = i - 1;
                        break;
                    }
                }
            }
            else
            {
                for (int i = this.m_pointDuration.Count - 1; i >= 0; i--)
                {
                    if (this.m_pointDuration[i] <= tmpCurrentDuration)
                    {
                        curIndex = i;
                        nextIndex = i + 1;
                        break;
                    }
                }
            }
        }
    }
}