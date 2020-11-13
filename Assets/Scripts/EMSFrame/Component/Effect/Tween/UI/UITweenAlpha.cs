//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UnityFrame
{
    [RequireComponent(typeof(RectTransform))]
    public class UITweenAlpha : ETweenBase,IUIUpdate
    {
        [Range(0, 1.0f)] public float source = 0;
        [Range(0, 1.0f)] public float target = 1;
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

        protected override void UF_OnPlay()
        {
            if (m_DynamicBind)
            {
                m_ListGraphic.Clear();
                this.GetComponentsInChildren<Graphic>(false, m_ListGraphic);
            }

            if (m_ListGraphic != null && m_ListGraphic.Count > 0)
            {
                for (int k = 0; k < m_ListGraphic.Count; k++)
                {
                    if (m_ListGraphic[k] != null)
                    {
                        Color color = m_ListGraphic[k].color;

                        if (m_CtrlRenderer)
                        {
                            m_ListGraphic[k].canvasRenderer.SetColor(this.isReverse ? new Color(color.r, color.g, color.b, target) : new Color(color.r, color.g, color.b, source));
                        }
                        else {
                            m_ListGraphic[k].color = this.isReverse ? new Color(color.r, color.g, color.b, target) : new Color(color.r, color.g, color.b, source);
                        }
                    }
                }
            }
        }

        protected override void UF_OnRun(float progress)
        {
            float K = m_Curve.Evaluate(progress);
            float alpha = Mathf.Clamp01(source * (1.0f - K) + target * K);

            if (m_ListGraphic != null && m_ListGraphic.Count > 0)
            {
                for (int k = 0; k < m_ListGraphic.Count; k++)
                {
                    if (m_ListGraphic[k] != null)
                    {
                        if (m_CtrlRenderer)
                        {
                            m_ListGraphic[k].canvasRenderer.SetAlpha(alpha);
                        }
                        else {
                            Color color = m_ListGraphic[k].color;
                            m_ListGraphic[k].color = new Color(color.r, color.g, color.b, alpha);
                        }
                    }
                }
            }

        }

    }
}