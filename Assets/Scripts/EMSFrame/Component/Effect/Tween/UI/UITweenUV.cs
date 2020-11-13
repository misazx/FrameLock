//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UnityFrame {
    [RequireComponent(typeof(RectTransform))]
    public class UITweenUV : ETweenBase, IUIUpdate
    {
        public Rect source = new Rect(0, 0, 1, 1);
        public Rect target = new Rect(0, 0, 1, 1);

        protected RectTransform m_RectTransform;
        public string updateKey { get { return effectName; } set { effectName = value; } }
        public RectTransform rectTransform { get { if (m_RectTransform == null) { m_RectTransform = this.transform as RectTransform; } return m_RectTransform; } }
        public virtual void UF_SetActive(bool active) { this.gameObject.SetActive(active); }
        public virtual void UF_SetValue(object value) { }
        [SerializeField] private AnimationCurve m_Curve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private List<UnityEngine.UI.RawImage> m_ListGraphic = new List<UnityEngine.UI.RawImage>();
        public List<UnityEngine.UI.RawImage> listGraphic { get { return m_ListGraphic; } }

        protected override void UF_OnPlay()
        {
            if (m_ListGraphic != null && m_ListGraphic.Count > 0)
            {
                for (int k = 0; k < m_ListGraphic.Count; k++)
                {
                    if (m_ListGraphic[k] != null)
                    {
                        
                    }
                }
            }
        }


        protected override void UF_OnRun(float progress)
        {
            float K = m_Curve.Evaluate(progress);
            Vector4 vs = new Vector4(source.x, source.y, source.width, source.height);
            Vector4 vt = new Vector4(target.x, target.y, target.width, target.height);

            Vector4 v = vs * (1.0f - K) + vt * K;
            
            if (m_ListGraphic != null && m_ListGraphic.Count > 0)
            {
                for (int k = 0; k < m_ListGraphic.Count; k++)
                {
                    if (m_ListGraphic[k] != null)
                    {
                        m_ListGraphic[k].uvRect = new Rect(v.x,v.y,v.z,v.w);
                    }
                }
            }

        }


    }


}

