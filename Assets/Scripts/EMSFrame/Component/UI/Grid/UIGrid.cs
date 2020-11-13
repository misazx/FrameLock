//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UnityFrame
{
	public enum LayoutCorner
	{
		UpperLeft,
		UpperRight,
		LowerLeft,
		LowerRight
	}

	[ExecuteAlways]
	[RequireComponent(typeof(RectTransform))]
	public class UIGrid : UIUpdateGroup, IUILayout
    {

		[SerializeField]protected int m_Constraint = 2;
		[SerializeField]protected Vector2 m_Padding = Vector2.zero;
		[SerializeField]protected Vector2 m_Space = Vector2.zero;
		[SerializeField]protected Vector2 m_CellSize = new Vector2(100,100);

		[SerializeField]protected LayoutCorner m_Alignement;

		[SerializeField]protected bool m_FixedCellSize = false;

        [SerializeField]protected SizeFitterType m_FitterType = SizeFitterType.None;

		[HideInInspector][SerializeField]protected string m_PrefabUI = "";

		//property
		public int constraint{get{ return m_Constraint;}set{ m_Constraint = value;UF_SetDirty ();}}
		public Vector2 padding{get{ return m_Padding;}set{ m_Padding = value;UF_SetDirty ();}}
		public Vector2 space{get{ return m_Space;}set{ m_Space = value;UF_SetDirty ();}}

		public Vector2 cellSize{get{ return m_CellSize;}set{ m_CellSize = value;UF_SetDirty ();}}

		public string prefabUI{get{ return m_PrefabUI;}set{ m_PrefabUI = value;}}

		public int childCount{get{ return this.transform.childCount;}}

		public bool fixedCellSize{get{ return m_FixedCellSize;}set{ m_FixedCellSize = value;UF_SetDirty ();}}

		public LayoutCorner alignement{get{return m_Alignement;}set{ m_Alignement = value; UF_SetDirty(); } }

        public SizeFitterType fitterType { get { return m_FitterType;}set { m_FitterType = value; UF_SetDirty(); } }

        public void UF_SetDirty(){
            if (this.IsActive())
            {
                UILayoutTools.UF_MarkLayoutForRebuild(this);
            }
        }

        public IUIUpdate UF_GenUI(bool firstSibling = false){
			return UF_GenUI(null, firstSibling);
		}

		public virtual IUIUpdate UF_GenUI(string spUpdateKey,bool firstSibling){
			IUIUpdate ui = null;
			if (string.IsNullOrEmpty (m_PrefabUI)) {
				Debugger.UF_Error (string.Format("[{0}] -> GenItem Failed,uiName is null",this.name));
			} else {
				ui = UIManager.UF_GetInstance ().UF_CreateItem(m_PrefabUI);
				if (ui != null) {
					if (string.IsNullOrEmpty (spUpdateKey)) {
						ui.updateKey = this.rectTransform.childCount.ToString ();
					} else {
						ui.updateKey = spUpdateKey;
					}
					this.UF_AddUI(ui,firstSibling);
				} else {
					Debugger.UF_Error (string.Format("[{0}] -> Can not Load uiName[{0}]",m_PrefabUI));	
				}
			}
			return ui;
		}


		public override void UF_AddUI(IUIUpdate ui, bool firstSibling)
		{
			base.UF_AddUI(ui, firstSibling);
			if (m_FixedCellSize)
				ui.rectTransform.sizeDelta = m_CellSize;
			this.UF_SetDirty ();
		}


		protected override void OnRectTransformDimensionsChange (){
			this.UF_SetDirty ();
		}
		protected void OnTransformChildrenChanged (){
			this.UF_SetDirty ();
		}
        protected override void OnTransformParentChanged(){
            this.UF_SetDirty();
        }


        //构建布局
        public void UF_RebuildLoyout()
        {
            this.OnRebuildLoyout();
        }

        public void UF_RebuildLoyoutImmediate()
        {
            this.OnRebuildLoyout();
        }

        //构建布局
        protected virtual void OnRebuildLoyout(){
            if (rectTransform == null) return;

            //构建布局
            Vector2 layoutSize = UILayoutTools.UF_BuildLayoutGrid(rectTransform, m_Alignement, m_Padding, m_Space, m_Constraint);
            
            //内容大小自适应
            UILayoutTools.UF_ContentSizeFitter(rectTransform, m_FitterType, layoutSize);

            //重构本层
            UILayoutTools.UF_RebuildSiblingLayout(this);

            //重构上层布局
            UILayoutTools.UF_RebuildParentLayout(this);
        }

        public override void UF_OnReset()
        {
            base.UF_OnReset();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            this.UF_SetDirty();
        }

        public virtual void UF_Clear() {
            UF_ResetMapUI(m_MapDynamicUI, true,true);
            this.UF_SetDirty();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            this.UF_SetDirty();
        }
#endif

    }
}

