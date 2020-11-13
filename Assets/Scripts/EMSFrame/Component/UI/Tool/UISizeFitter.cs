//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using static UnityEngine.RectTransform;

namespace UnityFrame{
    public enum SizeFitterType
    {
        None = 0,
        Horizontal = 1,
        Vertical = 2,
        Aspect = 3, //Horizontal && Vertical
    }


    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class UISizeFitter : UIBehaviour, IUILayout,ILayoutController
    {
        [Serializable]
        public struct SizeFitterAttachedTargets
        {
            
            public RectTransform target;
            public SizeFitterType fitterType;
            public Vector2 padding;
        }

        [SerializeField]
        protected SizeFitterType m_FitterType = SizeFitterType.None;

        [SerializeField]
        protected Vector2 m_MinSize = Vector2.zero;

        [NonSerialized]
        private RectTransform m_Rect;

        [SerializeField]
        protected List<SizeFitterAttachedTargets> m_attachedTargets = new List<SizeFitterAttachedTargets>();


        public RectTransform rectTransform {
			get {
				if (this.m_Rect == null && this != null) {
					this.m_Rect = base.GetComponent<RectTransform> ();
				}
				return this.m_Rect;
			}
		}



        private void UF_UpdateAttachedTargetSize(Vector2 sizeDelta)
        {
			if (m_attachedTargets != null && m_attachedTargets.Count > 0) {
                foreach (var item in m_attachedTargets) {
                    Vector2 size = new Vector2(Mathf.Min(sizeDelta.x, rectTransform.sizeDelta.x), Mathf.Min(sizeDelta.y, rectTransform.sizeDelta.y));
                    
                    if (item.target != null)
                    {
                        UILayoutTools.UF_ContentSizeFitter(item.target, item.fitterType, size, item.padding);
                    }
                }
            }
		}



        private Vector2 UF_GetPreferredSize(RectTransform v) {
            Vector2 size = this.rectTransform.sizeDelta;
            ILayoutElement emelent = this.GetComponent<ILayoutElement>();
            if (emelent != null)
                size = new Vector2(emelent.preferredWidth, emelent.preferredHeight);

            return new Vector2(
                Mathf.Max(size.x, m_MinSize.x),
                Mathf.Max(size.y, m_MinSize.y)
                );
        }



        private Vector2 UF_GetParentSize(RectTransform v)
        {
            RectTransform tar = v.parent as RectTransform;
            if (!(bool)tar)
            {
                return Vector2.zero;
            }
            return tar.rect.size;
        }

        private Vector2 UF_GetSizeWithCurrentAnchors(RectTransform v,Vector2 size)
        {
            Vector2 sizeDelta = v.sizeDelta;
            Vector2 parentSize = UF_GetParentSize(v);
            sizeDelta.x = size.x - parentSize.x * (v.anchorMax.x - v.anchorMin.x);
            sizeDelta.y = size.y - parentSize.y * (v.anchorMax.y - v.anchorMin.y);
            return sizeDelta;
        }


        public void UF_RebuildLoyout()
        {
            if (rectTransform == null) return;
            //float size = LayoutUtility.GetPreferredSize(this.rectTransform, (int)axis);
            if (this.IsActive() && m_FitterType != SizeFitterType.None) {
                Vector2 size = UF_GetPreferredSize(this.rectTransform);
                Vector2 sizeDelta = UF_GetSizeWithCurrentAnchors(this.rectTransform, size);
                UILayoutTools.UF_ContentSizeFitter(this.rectTransform, m_FitterType, sizeDelta);
                UF_UpdateAttachedTargetSize(sizeDelta);
            }
            //重构本层
            UILayoutTools.UF_RebuildSiblingLayout(this);
            //重构上层布局
            UILayoutTools.UF_RebuildParentLayout(this);
        }


        protected override void OnDisable ()
		{
            //LayoutRebuilder.MarkLayoutForRebuild (this.rectTransform);
            UILayoutTools.UF_MarkLayoutForRebuild(this);
            base.OnDisable ();
		}

		protected override void OnEnable ()
		{
			base.OnEnable ();
			this.SetDirty ();
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

        public void SetLayoutHorizontal() {
            this.SetDirty();
        }

        public void SetLayoutVertical() {
            this.SetDirty();
        }


#if UNITY_EDITOR
        protected override void OnValidate ()
		{
			this.SetDirty ();
		}
		#endif

		protected void SetDirty ()
		{
			if (!IsDestroyed() && this.IsActive ()) {
                UILayoutTools.UF_MarkLayoutForRebuild(this);
            }
		}

		public void Refresh(){
			OnEnable ();
		}

	}
}