//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------


using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityFrame
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class UISortingObject : UIObject, ISortingOrder
    {
        [HideInInspector][SerializeField] [Range(0, 100)] protected int m_Order;

        protected bool m_MarkDirty;

        protected ISortingRoot m_SortingRoot;

        protected int m_CacheRootOrder = 0;

        public int sortingOrder
        {
            get { return m_Order; }
            set
            {
                if (m_Order != value)
                {
                    m_Order = value;
                    this.UF_SetDirty();
                }
            }
        }

        public int rootSortingOrder {
            get { return m_CacheRootOrder; }
        }

        protected int UF_CacheSortingRoot() {
            Transform parent = this.transform.parent;
            while (parent != null) {
                ISortingRoot root = parent.GetComponent<ISortingRoot>();
                if (root != null && root.isActiveAndEnabled && root.isSortingValidate) {
                    m_SortingRoot = root;
                    m_CacheRootOrder = root.sortingOrder;
                    break;
                }
                parent = parent.parent;
            }
            return 0;
        }

        public void UF_SetDirty()
        {
            if (!Application.isPlaying)
            {
                UF_CacheSortingRoot();
                OnApplySortingOrder();
                return;
            }
            m_MarkDirty = true;
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            this.UF_SetDirty();
        }


        protected override void OnEnable()
        {
            base.OnEnable();
            this.UF_SetDirty();
        }

        protected override void OnCanvasHierarchyChanged()
        {
            base.OnCanvasHierarchyChanged();
            this.UF_SetDirty();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            m_SortingRoot = null;
            m_CacheRootOrder = 0;
        }

        private void OnTransformChildrenChanged()
        {
            this.UF_SetDirty();
        }

        protected void OnRootChange() {
            if (m_SortingRoot != null) {
                if (m_CacheRootOrder != m_SortingRoot.sortingOrder) {
                    m_CacheRootOrder = m_SortingRoot.sortingOrder;
                    OnApplySortingOrder();
                }
            }
        }

        protected virtual void OnApplySortingOrder() { }

        protected void Update()
        {
            if (m_MarkDirty)
            {
                UF_CacheSortingRoot();
                OnApplySortingOrder();
                m_MarkDirty = false;
            }
            OnRootChange();
        }
    }
}