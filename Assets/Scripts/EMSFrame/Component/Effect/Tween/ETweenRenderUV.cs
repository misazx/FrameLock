//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame{
	
	public class ETweenRenderUV: ETweenBase
    {
		public Vector2 source;
		public Vector2 target;

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
								renders [k].materials [i].mainTextureOffset = source;
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

			Vector2 offset = source * (1.0f - K) + target * K;

			for (int k = 0; k < renders.Count; k++) {
				if (renders [k] != null) {
					for (int i = 0; i < renders [k].materials.Length; i++) {
						if (renders [k].materials[i] != null) {
							renders [k].materials [i].mainTextureOffset = offset;
						}
					}
				}
			}
		}


	}

}