//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace UnityFrame
{
    public class EScreenShake : EffectBase
    {
        public float range = 0.2f;
        public float rate = 0.02f;
        public float attenuation = 0.02f;

        protected override void UF_OnPlay()
        {
            if (SceneCamera.main != null) {
                SceneCamera.main.UF_Shake(range, rate, attenuation);
            }
        }
    }


}
