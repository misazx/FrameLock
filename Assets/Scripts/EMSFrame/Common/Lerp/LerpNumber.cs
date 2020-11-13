//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UnityFrame
{
    internal class LerpNumber : LerpBase
    {
        public float source { get; set; }
        public float target { get; set; }
        public float current { get; private set; }

        public void Reset(float s, float t, float d)
        {
            progress = 0;
            duration = d;
            source = s;
            target = t;
            current = s;
            this.Reset();
        }
        public new bool UF_Run(float detlaTime)
        {
            if (base.UF_Run(detlaTime))
            {
                current = source * (1 - progress) + target * progress;
                return true;
            }
            else {
                return false;
            }

        }
    }
}

