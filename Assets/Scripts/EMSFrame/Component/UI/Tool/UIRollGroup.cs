//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

namespace UnityFrame
{
	//UI 滚动组
	//通过接口RollTo 实现滚动组件常用于老虎机滚动效果
	//Curve 曲线实现滚动停止逻辑
	public class UIRollGroup : UIObject
	{
		[SerializeField]protected float m_Space = 0;

		[SerializeField]protected Vector2 m_CellSize = new Vector2(100,100);

		[SerializeField]private GridLayoutGroup.Axis m_LayoutAxis;

		[SerializeField]private bool m_Reverse;

		[SerializeField]private AnimationCurve m_Curve = AnimationCurve.Linear(1,1,0,0);

		public AnimationCurve curve{get{ return m_Curve;}set{ m_Curve = value;}}

		public GridLayoutGroup.Axis layoutAxis{get{return m_LayoutAxis;}set{m_LayoutAxis = value;}}

		public float space{get{ return m_Space;}set{ m_Space = value;}}

		public Vector2 cellSize{get{ return m_CellSize;}set{ m_CellSize = value;}}

		public bool reverse{get{return m_Reverse;}set{ m_Reverse = value;}}

        protected override void Awake()
        {
            //固定对齐左上角
            this.rectTransform.anchorMin = new Vector2(0, 1);
            this.rectTransform.anchorMax = new Vector2(0, 1);
            this.rectTransform.pivot = new Vector2(0, 1);
        }


        /// <summary>
        /// 设置到指定索引
        /// </summary>
        public void UF_SetToIndex(int index){
			float val = 0;
			float valcell = m_LayoutAxis == GridLayoutGroup.Axis.Horizontal ? m_CellSize.x : m_CellSize.y;
			float valspace = m_Space;
			 
			int childCount = this.rectTransform.childCount;

			index = Mathf.Clamp (index,0,childCount);

			Vector2 pivot = this.rectTransform.pivot;
			RectTransform rectTrans = null;

			//初始化位置
			for (int k = 0; k < this.rectTransform.childCount; k++) {
				rectTrans = this.rectTransform.GetChild (k) as RectTransform;
				rectTrans.anchorMin = pivot;
				rectTrans.anchorMax = pivot;
				Vector2 pos = rectTrans.anchoredPosition;

				if(m_LayoutAxis == GridLayoutGroup.Axis.Vertical)
					pos.y = -((valspace + valcell) * k);
				else
					pos.x = ((valspace + valcell) * k);

				rectTrans.anchoredPosition = pos;
			}

			//起始偏移值
			val = (valspace + valcell) * index;

			//计算位移
			for(int k = index;k < this.rectTransform.childCount;k++){
				rectTrans = this.rectTransform.GetChild (k) as RectTransform;
				Vector2 pos = rectTrans.anchoredPosition;

				if(m_LayoutAxis == GridLayoutGroup.Axis.Vertical)
					pos.y += val;
				else
					pos.x -= val;

				rectTrans.anchoredPosition = pos;
			}

			RectTransform last = this.rectTransform.GetChild (childCount - 1) as RectTransform;
			for (int k = 0; k < index; k++) {
				rectTrans = this.rectTransform.GetChild (k) as RectTransform;
				Vector2 pos = last.anchoredPosition;

				if(m_LayoutAxis == GridLayoutGroup.Axis.Vertical)
					pos.y -= valspace + valcell;
				else
					pos.x += valspace + valcell;

				rectTrans.anchoredPosition = pos;
				last = rectTrans;
			}
		}

		public int UF_RoolTo(int index,int loopCount,float smooth){
			return UF_RoolTo(index,0,loopCount,smooth);
		}

		/// <summary>
		/// 滚动到
		/// </summary>
		/// <param name="index">指定索引.</param>
		/// <param name="startIndex">其实索引.</param>
		/// <param name="LoopCount">循环次数.</param>
		/// <param name="smooth">平滑值，越大越缓慢.</param>
		public int UF_RoolTo(int index,int startIndex,int loopCount,float smooth){
			if (index == startIndex)
				return 0;
            UF_SetToIndex(startIndex);
//			return FrameHandle.UF_AddCoroutine (ILerpToIndex (index, startIndex, duration));
			this.StartCoroutine(UF_IRoolTo(index, startIndex,loopCount, smooth));

			return 0;
		}


		IEnumerator UF_IRoolTo(int index,int startIndex,int loopCount,float smooth){
			int lerpcount = 0;
			int childCount = this.rectTransform.childCount;

			if (m_Reverse)
				index = Mathf.Abs (childCount - index);

			if (startIndex > index) {
				lerpcount = childCount - startIndex + index + loopCount * childCount ;
			} else {
				lerpcount = index - startIndex + loopCount * childCount;
			}

			float val = m_LayoutAxis == GridLayoutGroup.Axis.Vertical ? lerpcount * (m_Space + m_CellSize.y) : lerpcount * (m_Space + m_CellSize.x) ;

			float dbuf = 0;
			while (true) {
				float progress = Mathf.Clamp01 (dbuf / val);
				//加入曲线插值控制速度
				float uval = (val / smooth) * Mathf.Clamp01(m_Curve.Evaluate (progress));
				float ceil = uval * GTime.DeltaTime;
				dbuf += ceil;
				if (dbuf >= val) {
                    UF_UpdateCeil(val - dbuf + ceil);
					break;
				}
                UF_UpdateCeil(ceil);

				yield return null;
			}
		}
			
		private void UF_UpdateCeil(float val){
			int count = this.rectTransform.childCount;
			float lastVal = 0;
			float scp = 0;

			if (m_LayoutAxis == GridLayoutGroup.Axis.Vertical) {
				float csize = m_CellSize.y;
				lastVal = (count - 1) * (m_Space + csize);
				if (m_Reverse) {
					scp = -lastVal;
				} else {
					scp = m_Space + csize;
				}
				for (int k = 0; k < count; k++) {
					RectTransform rectTrans = this.rectTransform.GetChild (k) as RectTransform;
					Vector2 pos = rectTrans.anchoredPosition;
					if (m_Reverse) {
						pos.y -= val;
						// 超过限定值，直接移动到头
						if (pos.y < scp) {
							pos.y = pos.y + m_Space + csize - scp;
						}
					} else {
						pos.y += val;
						// 超过限定值，直接移动到末尾
						if (pos.y > scp) {
							pos.y = -lastVal + pos.y - scp;
						}
					}
					rectTrans.anchoredPosition = pos;
				}
			} else {
				float csize = m_CellSize.x;
				lastVal = (count - 1) * (m_Space + csize);
				if (m_Reverse) {
					scp = lastVal;
				} else {
					scp = m_Space + csize;
				}
				for (int k = 0; k < count; k++) {
					RectTransform rectTrans = this.rectTransform.GetChild (k) as RectTransform;
					Vector2 pos = rectTrans.anchoredPosition;
					if (m_Reverse) {
						pos.x += val;
						// 超过限定值，直接移动到头
						if (pos.x > scp) {
							pos.x = pos.x - m_Space - csize - scp;
						}
					} else {
						pos.x -= val;
						// 超过限定值，直接移动到末尾
						if (pos.x < -scp) {
							pos.x = lastVal + pos.x + m_Space + csize;
						}
					}
					rectTrans.anchoredPosition = pos;
				}

			}
		}

	}
}

