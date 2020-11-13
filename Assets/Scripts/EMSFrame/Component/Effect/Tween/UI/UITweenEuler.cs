//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace UnityFrame
{

    [RequireComponent(typeof(RectTransform))]
    public class UITweenEuler : ETweenEuler,IUIUpdate
    {
        protected RectTransform m_RectTransform;
        public string updateKey { get { return effectName; } set { effectName = value; } }
        public RectTransform rectTransform { get { if (m_RectTransform == null) { m_RectTransform = this.transform as RectTransform; } return m_RectTransform; } }
        public virtual void UF_SetActive(bool active) { this.gameObject.SetActive(active); }
        public virtual void UF_SetValue(object value) { }
    }
}