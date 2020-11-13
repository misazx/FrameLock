//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace UnityFrame
{

    [RequireComponent(typeof(RectTransform))]
    public class UITweenPosition : ETweenPosition,IUIUpdate
    {
        protected RectTransform m_RectTransform;
        public string updateKey { get { return effectName; } set { effectName = value; } }
        public RectTransform rectTransform { get { if (m_RectTransform == null) { m_RectTransform = this.transform as RectTransform; } return m_RectTransform; } }
        public virtual void UF_SetActive(bool active) { this.gameObject.SetActive(active); }
        public virtual void UF_SetValue(object value) { }

        public void PlayFromTo(Vector3 fromV, Vector3 toV) {
            source = fromV;
            target = toV;
            this.Play();
        }

        public void PlayCurrentTo(Vector3 toV) {
            source = spaceWorld ? this.transform.position : rectTransform.anchoredPosition3D;
            target = toV;
            this.Play();
        }


        protected override void UF_SetPosition(Vector3 val)
        {
            if (spaceWorld)
            {
                this.transform.position = val;
            }
            else
            {
                rectTransform.anchoredPosition3D = val;
            }
        }

    }
}