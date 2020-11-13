//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace UnityFrame
{
    internal class LerpPoint : LerpBase
    {
        public Vector2 source { get; set; }
        public Vector2 target { get; set; }
        public Vector2 current { get; private set; }

        public void Reset(Vector2 s, Vector2 t, float d)
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
            else
            {
                return false;
            }
        }
    }
}

