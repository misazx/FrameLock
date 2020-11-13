//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace UnityFrame
{

    [RequireComponent(typeof(RectTransform))]
    public class UITweenScale : ETweenScale,IUIUpdate
    {
        protected RectTransform m_RectTransform;
        public string updateKey { get { return effectName; } set { effectName = value; } }
        public RectTransform rectTransform { get { if (m_RectTransform == null) { m_RectTransform = this.transform as RectTransform; } return m_RectTransform; } }
        public virtual void UF_SetActive(bool active) { this.gameObject.SetActive(active); }
        public virtual void UF_SetValue(object value) { }

        //屏幕尺寸
        public bool sizeMode = false;

        protected override void UF_SetScale(Vector3 val)
        {
            if (sizeMode)
            {
                this.rectTransform.sizeDelta = val;
            }
            else
            {
                this.transform.localScale = val;
            }
        }
    }
}