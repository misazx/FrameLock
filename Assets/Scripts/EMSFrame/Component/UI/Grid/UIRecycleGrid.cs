//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UnityFrame
{
	//循环Grid 重复利用UI 显示内容
	public class UIRecycleGrid : UIGrid,IOnReset
	{
		//用于记录UI 索引变化
		struct UIIndexChanged{
			public int curIndex;
			public int lastIndex;
		}

		[SerializeField]private int m_MaxShowCount = 20;
		[SerializeField]private GridLayoutGroup.Axis m_LayoutAxis;

		public GridLayoutGroup.Axis layoutAxis{get{return m_LayoutAxis;}set{m_LayoutAxis = value;this.UF_SetDirty ();}}

		//最大显示数量
		public int maxShowCount{get{ return m_MaxShowCount;}set{m_MaxShowCount = value;}}

		//显示总数量
		private int m_TotalCount = 0;

		//当前显示起始索引
		private int m_ReposIndex = 0;

		private float m_CurPosVal = 0;

		private float m_SourcePosVal = 0;

        private Vector2 m_SourceAnchoredPosition = Vector2.zero;


        private Dictionary<Transform,IUIUpdate> m_MapTransToUI;

		private Dictionary<IUIUpdate,UIIndexChanged> m_MapIndexChangedBuffer;

		private Dictionary<Transform,IUIUpdate> mapTransToUI{
			get{ 
				if (m_MapTransToUI == null) {
					m_MapTransToUI = DictionaryPool<Transform,IUIUpdate>.Get ();
				}
				return m_MapTransToUI;
			}	
			set{ 
				if (m_MapTransToUI != null && value == null) {
					DictionaryPool<Transform,IUIUpdate>.Release(m_MapTransToUI);
					m_MapTransToUI = null;
				}
			}
		}

		private Dictionary<IUIUpdate,UIIndexChanged> mapIndexChangedBuffer{
			get{ 
				if (m_MapIndexChangedBuffer == null) {
					m_MapIndexChangedBuffer = DictionaryPool<IUIUpdate,UIIndexChanged>.Get ();
				}
				return m_MapIndexChangedBuffer;
			}
			set{ 
				if (m_MapIndexChangedBuffer != null && value == null) {
					DictionaryPool<IUIUpdate,UIIndexChanged>.Release (m_MapIndexChangedBuffer);
					m_MapIndexChangedBuffer = null;
				}
			}
		}

		private DelegateUIResposition m_MethodUIRespositon;

		private bool needReposition{get{ return m_TotalCount > m_MaxShowCount;}}

		private IUIUpdate UF_GetUIAtFirst(){
			return UF_GetUIAtChildIndex (0);
		}

		private IUIUpdate UF_GetUIAtLast(){
			return UF_GetUIAtChildIndex (this.transform.childCount - 1);
		}

		private IUIUpdate UF_GetUIAtChildIndex(int index){
			Transform target = this.transform.GetChild (index);
			if (target != null && mapTransToUI.ContainsKey(target)) {
				return mapTransToUI [target];
			} 
			return null;
		}

		private void UF_AddIdxChangedBuffer(IUIUpdate ui,int curIndex,int lastIndex){
			UIIndexChanged buffer = new UIIndexChanged();
			buffer.curIndex = curIndex;
			buffer.lastIndex = lastIndex;
			if (mapIndexChangedBuffer.ContainsKey (ui)) {
				mapIndexChangedBuffer [ui] = buffer;
			} else {
				mapIndexChangedBuffer.Add(ui,buffer);
			}
		}


		public void UF_RegisterUIRespositon(DelegateUIResposition method){
			m_MethodUIRespositon = method;
		}


		//ui 位置循环重置
		protected void UF_OnUIReposition(IUIUpdate ui,int curIndex,int lastIndex){
			if (m_MethodUIRespositon != null) {
				m_MethodUIRespositon (ui, curIndex, lastIndex);
			}
		}


		public override IUIUpdate UF_GenUI(string spUpdateKey,bool firstSibling)
		{
			IUIUpdate ui = null;
			if (string.IsNullOrEmpty (m_PrefabUI)) {
				Debugger.UF_Error (string.Format("[{0}] -> GenUI Failed,itemName is null",this.name));
				return null;
			}
			if (m_TotalCount < m_MaxShowCount) {
				ui =  base.UF_GenUI(spUpdateKey,firstSibling);
			}
			m_TotalCount++;
			return ui;
		}


		public override void UF_AddUI(IUIUpdate ui, bool firstSibling)
		{
			base.UF_AddUI(ui, firstSibling);
			ui.rectTransform.anchorMin = this.rectTransform.pivot;
			ui.rectTransform.anchorMax = this.rectTransform.pivot;
			mapTransToUI.Add (ui.rectTransform.transform, ui);
		}

        public override void UF_RemoveUI(string key)
		{
			Debug.LogError ("Can not call[RemoveUI (string key)] method to remove UI in grid,use RemoveUIBaseIndex(int index) to instead");
		}

		//移除元素会重新刷新列表全部显示的UI
		public void UF_RemoveUIBaseIndex(int index)
		{
			if (index > m_TotalCount || index < 0)
				return;

			m_TotalCount--;

			if (index < m_ReposIndex) {
				m_ReposIndex--;
			}
			else if (index >= m_ReposIndex && index < m_ReposIndex + m_MaxShowCount) {
				int childIdx = index - m_ReposIndex;
				IUIUpdate ui = UF_GetUIAtChildIndex (childIdx);
				if (m_ReposIndex + m_MaxShowCount > m_TotalCount) {
					base.UF_RemoveUI(ui.updateKey);
                } else {
					ui.rectTransform.SetAsLastSibling ();
                    UF_OnUIReposition(ui,index,m_ReposIndex + m_MaxShowCount);
				}
			}
			this.UF_SetDirty ();
		}
        
		//计算指定索引位置
		protected Vector2 UF_GetPositionBaseIndex(GridLayoutGroup.Axis axis,int index){
			int left = 0;
			int down = 0;
			if (axis == GridLayoutGroup.Axis.Vertical) {
				left = (index) % m_Constraint;
				down = (int)((index) / m_Constraint);
			} else {
				down = (index) % m_Constraint;
				left = (int)((index) / m_Constraint);
			}
			return new Vector2 (left * (m_Space.x+m_CellSize.x)+m_Padding.x,-down * (m_Space.y + m_CellSize.y)-m_Padding.y); 
		}

		/// <summary>
		/// 获取layout 尺寸大小
		/// </summary>
		protected Vector2 UF_GetLayoutSizeDelta(GridLayoutGroup.Axis axis){
			float sizeVal = 0;
			//更新content尺寸
			if (axis == GridLayoutGroup.Axis.Vertical) {
				sizeVal = m_Padding.y * 2 + (m_Space.y + m_CellSize.y) * Mathf.Ceil ((float)m_TotalCount / (float)m_Constraint) - m_Space.y;
				return new Vector2 (this.rectTransform.sizeDelta.x, sizeVal);	
			} else{
				sizeVal = m_Padding.y * 2 + (m_Space.y + m_CellSize.y) * Mathf.Ceil ((float)m_TotalCount / (float)m_Constraint) - m_Space.y;
				 return new Vector2 (sizeVal, this.rectTransform.sizeDelta.y);
			}
		}

			
		protected override void OnRebuildLoyout ()
		{
            if (Application.isPlaying)
            {
                //更新元素布局位置
                int count = this.childCount;
                int idx = 0;
                for (int k = 0; k < count; k++)
                {
                    IUIUpdate ui = UF_GetUIAtChildIndex(k);
                    if (ui == null) continue;
                    ui.rectTransform.anchoredPosition = UF_GetPositionBaseIndex(this.m_LayoutAxis, m_ReposIndex + idx);
                    idx++;
                }
                this.rectTransform.sizeDelta = UF_GetLayoutSizeDelta(this.m_LayoutAxis);
            }
            else {
                base.OnRebuildLoyout();
            }
		}

		protected override void Awake ()
		{
			base.Awake ();
			//固定对齐左上角
			this.rectTransform.anchorMin = new Vector2 (0,1);
			this.rectTransform.anchorMax = new Vector2 (0,1);
			this.rectTransform.pivot = new Vector2 (0,1);
            m_SourceAnchoredPosition = this.anchoredPosition;
        }

		void UF_UpdateVertical(){
			float posVal = this.anchoredPosition.y;
			float cellVal = this.m_CellSize.y;
			float spaceVal = this.m_Space.y;
			float cellSpace = cellVal + spaceVal;

			if (System.Math.Abs(m_CurPosVal - posVal) > 0.01f && this.needReposition) {
				int delta = (int)(posVal - m_SourcePosVal);
				if (delta > 0) {
					while(delta > 0 && delta > cellSpace && (m_ReposIndex + m_MaxShowCount) < m_TotalCount) {
						m_SourcePosVal += cellSpace;
						for (int k = 0; k < m_Constraint; k++) {
							//记录UI 位置的变化，后续更新位置
							IUIUpdate ui = this.UF_GetUIAtFirst();
                            UF_AddIdxChangedBuffer(ui,m_ReposIndex + m_MaxShowCount,m_ReposIndex);
							ui.rectTransform.SetAsLastSibling ();
							m_ReposIndex++;
							if (m_ReposIndex + m_MaxShowCount == m_TotalCount)
								break;
						}
						//loop 
						delta = (int)(posVal - m_SourcePosVal);
					}
				} else {
					while (delta < 0 && Mathf.Abs (delta) > spaceVal && m_ReposIndex > 0) {
						m_SourcePosVal -= cellSpace;
						for (int k = 1; k <= m_Constraint; k++) {
							m_ReposIndex--;
							IUIUpdate ui = this.UF_GetUIAtLast();
							ui.rectTransform.SetAsFirstSibling ();
                            UF_AddIdxChangedBuffer(ui,m_ReposIndex,m_ReposIndex + m_MaxShowCount);
							if (m_ReposIndex == 0)
								break;
						}
						//loop 
						delta = (int)(posVal - m_SourcePosVal);
					}
				}
			} else {
				m_CurPosVal = posVal;
			}
		}


		void UF_UpdateHorizontal(){

			float posVal = this.anchoredPosition.x;
			float cellVal = this.m_CellSize.x;
			float spaceVal = this.m_Space.x;
			float cellSpace = cellVal + spaceVal;

			if (System.Math.Abs(m_CurPosVal - posVal) > 0.01f && this.needReposition) {
				int delta = (int)(posVal - m_SourcePosVal);
				if (delta < 0) {
					while(delta < 0 && Mathf.Abs (delta) > cellSpace && (m_ReposIndex + m_MaxShowCount) < m_TotalCount) {
						m_SourcePosVal -= cellSpace;
						for (int k = 0; k < m_Constraint; k++) {
							//记录UI 位置的变化，后续更新位置
							IUIUpdate ui = this.UF_GetUIAtFirst();
                            UF_AddIdxChangedBuffer(ui,m_ReposIndex + m_MaxShowCount,m_ReposIndex);
							ui.rectTransform.SetAsLastSibling ();
							m_ReposIndex++;
							if (m_ReposIndex + m_MaxShowCount == m_TotalCount)
								break;
						}
						//loop 
						delta = (int)(posVal - m_SourcePosVal);
					}
				} else {
					while (delta > 0 && delta > spaceVal && m_ReposIndex > 0) {
						m_SourcePosVal += cellSpace;
						for (int k = 1; k <= m_Constraint; k++) {
							m_ReposIndex--;
							IUIUpdate ui = this.UF_GetUIAtLast();
							ui.rectTransform.SetAsFirstSibling ();
                            UF_AddIdxChangedBuffer(ui,m_ReposIndex,m_ReposIndex + m_MaxShowCount);
							if (m_ReposIndex == 0)
								break;
						}
						//loop 
						delta = (int)(posVal - m_SourcePosVal);
					}
				}
			} else {
				m_CurPosVal = posVal;
			}

		}


		private void UF_UpdateChangeDirty(){
			if (mapIndexChangedBuffer.Count > 0) {
				foreach (KeyValuePair<IUIUpdate,UIIndexChanged> item in mapIndexChangedBuffer) {
                    UF_OnUIReposition(item.Key,item.Value.curIndex,item.Value.lastIndex);	
				}
				mapIndexChangedBuffer.Clear ();
				this.UF_SetDirty ();
			}
		}


		void Update(){
			if (m_LayoutAxis == GridLayoutGroup.Axis.Vertical)
                UF_UpdateVertical();
			else
                UF_UpdateHorizontal();

            //更新变化缓冲
            UF_UpdateChangeDirty();
		}

        public override void UF_Clear()
        {
            m_TotalCount = 0;
            m_ReposIndex = 0;
            m_SourcePosVal = 0;
            m_CurPosVal = 0;
            mapTransToUI.Clear();
            base.UF_Clear();
            this.anchoredPosition = m_SourceAnchoredPosition;
            this.sizeDelta = UF_GetLayoutSizeDelta(this.m_LayoutAxis);
        }



        public override void UF_OnReset()
        {
            base.UF_OnReset();
            this.UF_Clear();
            mapTransToUI = null;
        }






        protected override void OnDestroy ()
		{
			base.OnDestroy ();
			//释放
			mapTransToUI = null;
			mapIndexChangedBuffer = null;

		}

    }
}

