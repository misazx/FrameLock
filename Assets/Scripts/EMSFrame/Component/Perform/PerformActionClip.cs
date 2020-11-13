//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFrame {
    [System.Serializable]
    public class PerformActionClip
    {
        //定义名字
        public string name = string.Empty;
        //详细名字，用于展示
        public string detialName = string.Empty;
        //循环
        public bool loop = true;
        //参数
        public string param = string.Empty;
        //播放时长
        public float length = 10;
        //事件集合
        [SerializeField] private List<ClipEvent> m_ClipEvents = new List<ClipEvent>();
        public List<ClipEvent> clipEvents { get { return m_ClipEvents; } }
    }
}

