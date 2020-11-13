//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UnityFrame {
    internal class LerpBase
    {
        public float progress { get; protected set; }

        public float duration { get; set; }

        protected float m_duraionTick;

        public void Reset()
        {
            m_duraionTick = 0;
        }

        public bool UF_Run(float detlaTime)
        {
            if (duration <= 0.0001f)
            {
                return false;
            }
            m_duraionTick += detlaTime;
            float val = m_duraionTick / duration;
            progress = Mathf.Clamp01(val);
            return val <= 1;
        }
    }
}

