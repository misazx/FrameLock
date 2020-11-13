//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using static UnityEngine.RectTransform;

namespace UnityFrame
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class UILayoutFitter : UIBehaviour, IUILayout,ILayoutController
    {
        [SerializeField]protected LayoutCorner m_Alignement;
        [SerializeField]protected SizeFitterType m_FitterType = SizeFitterType.None;
        [SerializeField]protected int m_Constraint = 1;
		[SerializeField]protected Vector2 m_Padding = Vector2.zero;
		[SerializeField]protected Vector2 m_Space = Vector2.zero;


        private RectTransform m_Rect;
        public RectTransform rectTransform
        {
            get
            {
                if (this.m_Rect == null && this != null) {
                    this.m_Rect = base.GetComponent<RectTransform>();
                }
                return this.m_Rect;
            }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            this.SetDirty();
        }
        protected void OnTransformChildrenChanged()
        {
            this.SetDirty();
        }
        protected override void OnTransformParentChanged()
        {
            this.SetDirty();
        }

        public void SetLayoutHorizontal()
        {
            this.SetDirty();
        }

        public void SetLayoutVertical()
        {
            this.SetDirty();
        }

        protected void SetDirty()
        {
            if (!IsDestroyed() && this.IsActive())
            {
                UILayoutTools.UF_MarkLayoutForRebuild(this);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            this.SetDirty();
        }

        protected override void OnDisable()
        {
            UILayoutTools.UF_MarkLayoutForRebuild(this);
            base.OnDisable();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            this.SetDirty();
        }
#endif

        //构建布局
        public void UF_RebuildLoyout()
        {
            if (rectTransform == null) return;

            if (this.IsActive() && m_FitterType != SizeFitterType.None) {
                var gridSize = UILayoutTools.UF_BuildLayoutGrid(this.rectTransform, m_Alignement, m_Padding, m_Space, m_Constraint);
                UILayoutTools.UF_ContentSizeFitter(this.rectTransform, m_FitterType, gridSize);
            }
           
            //重构本层
            UILayoutTools.UF_RebuildSiblingLayout(this);

            //重构上层布局
            UILayoutTools.UF_RebuildParentLayout(this);
        }

    }
}