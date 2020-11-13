//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace UnityFrame{
	public class ESoundPlayer : EffectBase
	{
		public string soundName;
		protected override void UF_OnPlay()
		{
            AudioManager.UF_GetInstance().UF_BatchPlay(soundName);
        }
	}
}

