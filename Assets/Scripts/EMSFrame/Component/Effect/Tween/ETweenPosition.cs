//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace UnityFrame{

	public class ETweenPosition : ETweenBase
	{
		public Vector3 source = Vector3.zero;
		public Vector3 target = Vector3.zero;
		public Vector3 radius = Vector3.zero;
		public bool spaceWorld = false;
		[SerializeField]private AnimationCurve m_Curve = AnimationCurve.Linear(0,0,1,1);
		public AnimationCurve curve{get{ return m_Curve;}}


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

		protected override void UF_OnPlay()
		{
            this.UF_SetPosition(this.isReverse ? target : source);
        }

		protected override void UF_OnRun(float progress)
		{
			float K = curve.Evaluate (progress);
			float rad_offset =  Mathf.Sin(Mathf.PI * K);
            Vector3 current = source * (1.0f - K) + target * K + radius * rad_offset;
            this.UF_SetPosition(current);
        }


	}
}