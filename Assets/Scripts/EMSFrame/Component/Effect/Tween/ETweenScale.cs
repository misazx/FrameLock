//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace UnityFrame{
	
	public class ETweenScale : ETweenBase
    {
		
		public Vector3 source = Vector3.zero;

		public Vector3 target = Vector3.zero;

		[SerializeField]private AnimationCurve m_Curve = AnimationCurve.Linear(0,0,1,1);

		public AnimationCurve curve{get{ return m_Curve;}}

        protected virtual void UF_SetScale(Vector3 val) {
            this.transform.localScale = val;
        }

		protected override void UF_OnPlay()
		{
            this.UF_SetScale(this.isReverse ? target : source);
        }

		protected override void UF_OnRun(float progress)
		{
			float K = curve.Evaluate (progress);

			Vector3 current = source * (1.0f - K) + target * K;

            this.UF_SetScale(current);
		}


	}

}