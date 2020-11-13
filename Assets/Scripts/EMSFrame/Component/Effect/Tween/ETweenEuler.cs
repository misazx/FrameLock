//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;


namespace UnityFrame{

	public class ETweenEuler : ETweenBase
    {
		
		public Vector3 source = Vector3.zero;

		public Vector3 target = Vector3.zero;

		public bool spaceWorld = false;

        //基于当前forward朝向，重新计算角度
        public bool worldForwardMode = false;

        private Vector3 m_Source;
        private Vector3 m_Target;

        [SerializeField]private AnimationCurve m_Curve = AnimationCurve.Linear(0,0,1,1);

		public AnimationCurve curve{get{ return m_Curve;}}

        protected virtual void SetEuler(Vector3 val) {

            if (spaceWorld)
            {
                this.transform.eulerAngles = val;
            }
            else
            {
                this.transform.localEulerAngles = val;
            }
        }

        protected override void UF_OnStart()
        {
            if (spaceWorld && worldForwardMode)
            {
                //重新计算
                Vector3 fvEuler = MathX.UF_EulerAngle(this.transform.forward);
                m_Source += fvEuler;
                m_Target += fvEuler;
            }
        }

        protected override void UF_OnPlay()
		{
            m_Source = source;
            m_Target = target;
            this.SetEuler(this.isReverse ? m_Target : m_Source);
        }

		protected override void UF_OnRun(float progress)
        {
            float K = m_Curve.Evaluate (progress);
			Vector3 current = m_Source * (1.0f - K) + m_Target * K;
            this.SetEuler(current);
        }


	}

}