using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFrame
{
    [RequireComponent(typeof(RectTransform))]
    public class UITweenPathPoint : ETweenPathPoint,IUIUpdate
    {
        protected RectTransform m_RectTransform;
        public string updateKey { get { return effectName; } set { effectName = value; } }
        public RectTransform rectTransform { get { if (m_RectTransform == null) { m_RectTransform = this.transform as RectTransform; } return m_RectTransform; } }
        public virtual void UF_SetActive(bool active) { this.gameObject.SetActive(active); }
        public virtual void UF_SetValue(object value) { }

        protected override void UF_SetPosition(Vector3 val)
        {
            if (spaceWorld)
            {
                this.rectTransform.position = val;
            }
            else
            {
                this.rectTransform.anchoredPosition3D = val;
            }
        }
    }
}