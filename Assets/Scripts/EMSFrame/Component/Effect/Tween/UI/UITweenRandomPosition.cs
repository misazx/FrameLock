//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace UnityFrame
{
    [RequireComponent(typeof(RectTransform))]
    public class UITweenRandomPosition : ETweenBase,IUIUpdate
    {
        //随机采样范围
        public float rangeX = 10;
        public float rangeY = 10;
        public float rangeZ = 10;

        public bool activeX = true;
        public bool activeY = true;
        public bool activeZ = true;

        public bool spaceWorld = false;
        public Vector3 radius = Vector3.zero;
        private Vector3 source = Vector3.zero;
        private Vector3 target = Vector3.zero;

        [SerializeField] private AnimationCurve m_Curve = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve curve { get { return m_Curve; } }

        protected RectTransform m_RectTransform;
        public string updateKey { get { return effectName; } set { effectName = value; } }
        public RectTransform rectTransform { get { if (m_RectTransform == null) { m_RectTransform = this.transform as RectTransform; } return m_RectTransform; } }
        public virtual void UF_SetActive(bool active) { this.gameObject.SetActive(active); }
        public virtual void UF_SetValue(object value) { }

        public void PlayFromTo(Vector3 fromV, Vector3 toV)
        {
            source = fromV;
            target = toV;
            this.Play();
        }

        public void PlayCurrentTo(Vector3 toV)
        {
            source = spaceWorld ? this.transform.position : rectTransform.anchoredPosition3D;
            target = toV;
            this.Play();
        }

        protected Vector3 UF_GetPosition() {
            return spaceWorld ? this.transform.position : this.transform.localPosition;
        }

        protected Vector3 UF_RandomVector3() {
            float x = Mathf.Abs(rangeX);
            float y = Mathf.Abs(rangeY);
            float z = Mathf.Abs(rangeZ);
            x = activeX ? Random.Range(-x, x) : UF_GetPosition().x;
            y = activeY ? Random.Range(-y, y) : UF_GetPosition().y;
            z = activeZ ? Random.Range(-z, z) : UF_GetPosition().z;
            return new Vector3(x,y,z);
        }

        protected override void UF_OnStart()
        {
            //随机得到位置
            if (spaceWorld)
            {
                source = this.transform.position;
                target = UF_RandomVector3() + source;
                rectTransform.position = this.isReverse ? target : source;
            }
            else
            {
                source = Vector3.zero;
                target = UF_RandomVector3() + source;
                rectTransform.anchoredPosition3D = this.isReverse ? target : source;
            }

        }

        protected override void UF_OnRun(float progress)
        {
            float K = curve.Evaluate(progress);
            float rad_offset = Mathf.Sin(Mathf.PI * K);

            Vector3 current = source * (1.0f - K) + target * K + radius * rad_offset;

            if (spaceWorld)
            {
                this.transform.position = current;
            }
            else
            {
                rectTransform.anchoredPosition3D = current;
            }

        }

    }
}