//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;


namespace UnityFrame{

	public class ETweenRenderValue: ETweenBase
    {
		public string valueName = "_Value";
		public float source;
		public float target;

		[SerializeField]private AnimationCurve m_Curve = AnimationCurve.Linear(0,0,1,1);
		[SerializeField]private List<Renderer> m_Renders = new List<Renderer> ();
		public AnimationCurve curve{get{ return m_Curve;}}
		public List<Renderer> renders{ get{ return m_Renders;}}

		protected override void UF_OnPlay()
		{
			if (renders == null)
				return;
			if (delay > 0) {
				for (int k = 0; k < renders.Count; k++) {
					if (renders [k] != null) {
						for (int i = 0; i < renders [k].materials.Length; i++) {
							if (renders [k].materials [i] != null) {
								renders [k].materials [i].SetFloat (valueName, source);
							}
						}
					}
				}
			}
		}

		protected override void UF_OnRun(float progress)
		{
			if (renders == null)
				return;
			float K = curve.Evaluate(progress);

			float value = source * (1.0f - K) + target * K;

			for (int k = 0; k < renders.Count; k++) {
				if (renders [k] != null) {
					for (int i = 0; i < renders [k].materials.Length; i++) {
						if (renders [k].materials[i] != null) {
							renders [k].materials [i].SetFloat (valueName, value);
						}
					}
				}
			}
		}

	}
}