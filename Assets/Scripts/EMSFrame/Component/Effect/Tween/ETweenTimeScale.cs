//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace UnityFrame{
	public class ETweenTimeScale: ETweenBase
    {
		private float m_SourceTimeScale;

		[SerializeField]private AnimationCurve m_Curve = AnimationCurve.Linear(0,0,1,1);

		public AnimationCurve curve{get{ return m_Curve;}}


		protected override void UF_OnPlay()
		{
			m_SourceTimeScale = GTime.TimeScale;
		}

		protected override void UF_OnStop()
		{
			GTime.TimeScale = m_SourceTimeScale;
		}

		protected override void UF_OnRun(float progress)
		{
			GTime.TimeScale = curve.Evaluate (progress);
		}


	}
}