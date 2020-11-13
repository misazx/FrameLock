//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UnityFrame
{
    [RequireComponent(typeof(RectTransform))]
    public class UITweenColor : ETweenBase,IUIUpdate
    {
        public Color source = new Color(1, 1, 1, 1);
        public Color target = new Color(1, 1, 1, 1);
        [SerializeField] private AnimationCurve m_Curve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private bool m_CtrlRenderer = true;
        [SerializeField] private bool m_DynamicBind = false;
        [HideInInspector] [SerializeField] private List<UnityEngine.UI.Graphic> m_ListGraphic = new List<UnityEngine.UI.Graphic>();
        public AnimationCurve curve { get { return m_Curve; } }
        public bool dynamicBind { get { return m_DynamicBind; } set { m_DynamicBind = value; } }
        public List<UnityEngine.UI.Graphic> listGraphic{get { return m_ListGraphic; }}

        protected RectTransform m_RectTransform;
        public string updateKey { get { return effectName; } set { effectName = value; } }
        public RectTransform rectTransform { get { if (m_RectTransform == null) { m_RectTransform = this.transform as RectTransform; } return m_RectTransform; } }
        public virtual void UF_SetActive(bool active) { this.gameObject.SetActive(active); }
        public virtual void UF_SetValue(object value) { }

        protected override void UF_OnInit()
        {
            base.UF_OnInit();
            if (m_DynamicBind)
            {
                m_ListGraphic.Clear();
                this.GetComponentsInChildren<Graphic>(false, m_ListGraphic);
            }
        }

        protected override void UF_OnPlay()
        {
            if (m_ListGraphic != null && m_ListGraphic.Count > 0)
            {
                for (int k = 0; k < m_ListGraphic.Count; k++)
                {
                    if (m_ListGraphic[k] != null)
                    {
                        if (m_CtrlRenderer)
                        {
                            m_ListGraphic[k].canvasRenderer.SetColor(this.isReverse ? target : source);
                        }
                        else {
                            m_ListGraphic[k].color = this.isReverse ? target : source;
                        }
                    }
                }
            }
        }

        protected override void UF_OnRun(float progress)
        {
            float K = m_Curve.Evaluate(progress);
            Color color = source * (1.0f - K) + target * K;
            color.a = Mathf.Clamp01(color.a);

            if (m_ListGraphic != null && m_ListGraphic.Count > 0)
            {
                for (int k = 0; k < m_ListGraphic.Count; k++)
                {
                    if (m_ListGraphic[k] != null)
                    {
                        if (m_CtrlRenderer)
                        {
                            m_ListGraphic[k].canvasRenderer.SetColor(color);
                        }
                        else {
                            m_ListGraphic[k].color = color;
                        }
                    }
                }
            }

        }


    }
}
