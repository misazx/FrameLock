//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame{
	public class EAnimationPlayer: EffectBase
	{
		public List<Animation> animations{get{ return m_Animations;}}

		[SerializeField]private List<Animation> m_Animations = new List<Animation>();

		protected override void UF_OnPlay()
		{
			if (animations != null) {
				for (int k = 0; k < animations.Count; ++k) {
					animations[k].Play ();
				}
			}
		}

		protected override void UF_OnStop()
		{
			if (animations != null) {
				for (int k = 0; k < animations.Count; ++k) {
					animations[k].Stop ();
				}
			}
		}

	}

}