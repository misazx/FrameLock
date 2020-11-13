//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

using System.Text;

namespace UnityFrame{
	//内容滚动
	public class UIScrollSelection : UIScrollView,IUIUpdate,IOnReset {
		//模版内容
		//动态生成
		public UILabel labelContent;
		//起始空行数
		public int startSpaceCount = 2;
		//结尾空行数
		public int endSpaceCount = 2;
		//调整参数
		public float velocityZoom = 150; 
		public float recoverSpeed = 20;

		//事件附带参数
		public string eValueChange = "";
		public string eParam= "";

		private bool m_Dragging = false;
		private float m_CachePosY = 0;
		private int m_SelectIndex = 0;
		private float m_InsParam;
		private float m_FixParam;
		private float m_LastSelectIndex = 0;
		private bool m_IfInited = false;

		struct ContentUnitData {
			public string data;
			public float pos;
			public ContentUnitData(string _data,float _pos){
				data = _data;
				pos = _pos;
			}
		}

		public int SelectedIndex{
			get{ 
				return m_SelectIndex;
			}
		}

		public int SelectionCount{
			get{ 
				return mListContentUnitData.Count;
			}
		}

		private List<ContentUnitData> mListContentUnitData = new List<ContentUnitData>();


		public void UF_OnReset (){
			mListContentUnitData.Clear ();
			if (labelContent != null) {
				labelContent.text = string.Empty;
				m_CachePosY = 0;
				m_SelectIndex = 0;
			}
		}

		protected override void Start ()
		{
			base.Start ();
            UF_Init();

		}

		public override void OnBeginDrag (UnityEngine.EventSystems.PointerEventData eventData)
		{
			base.OnBeginDrag (eventData);
			if (eventData.button == UnityEngine.EventSystems.PointerEventData.InputButton.Left && this.IsActive ()) {
				m_Dragging = true;
			}
		}
		public override void OnEndDrag (UnityEngine.EventSystems.PointerEventData eventData)
		{
			base.OnEndDrag (eventData);
			if (eventData.button == UnityEngine.EventSystems.PointerEventData.InputButton.Left) {
				m_Dragging = false;
			}
		}


		//处理初始化预设数据
		public void UF_Init(){

			if (labelContent == null && Application.isPlaying && m_IfInited)
				return;

			float a = labelContent.UF_GetPreferredLineHeight(1);
			float b = labelContent.UF_GetPreferredLineHeight();
			m_InsParam = b;
			m_FixParam = a - b;

			//清空之前的，重新解析
			mListContentUnitData.Clear ();

			if (!string.IsNullOrEmpty (labelContent.text)) {
				//解析数据
				string[] data = GHelper.UF_SplitString(labelContent.text.Trim(),'\n');
				if (data != null) {
					for (int k = 0; k < data.Length; k++) {
                        UF_AddContent(data[k]);
					}
				}
			}

            UF_Refresh();

			if (mListContentUnitData.Count > 0) {
				//滚动到目标
				content.anchoredPosition = new Vector2 (content.anchoredPosition.x, mListContentUnitData [m_SelectIndex].pos);;
			}
		}

		StringBuilder mStringTemp = new StringBuilder ();
		//重新计算内容
		public void UF_Refresh(){
			if (labelContent == null)
				return;
			content.anchoredPosition = Vector2.zero;

			mStringTemp.Length = 0;

			for (int k = 0; k < startSpaceCount; k++) {
				mStringTemp.Append ('\n');
			}
			int times = mListContentUnitData.Count - 1;
			for (int k = 0; k <= times ; k++) {
				mStringTemp.Append (mListContentUnitData [k].data);
				if (k != times) {
					mStringTemp.Append ('\n');	
				} 
			}
			for (int k = 0; k < endSpaceCount; k++) {
				mStringTemp.Append ("\n");
			}
			labelContent.text = mStringTemp.ToString ();

			//计算内容大小
			labelContent.UF_AdjustPreferredSize();

		}

		public void UF_AddContent(string info){
            UF_AddContent(info, false);
		}

		public void UF_AddContent(string info,bool needRefresh){
			if (labelContent == null)
				return;

			if (mListContentUnitData.Count == 0) {
				mListContentUnitData.Add (new ContentUnitData (info, 0));
			} else if (mListContentUnitData.Count == 1) {
				//递增固定值
				float pos = labelContent.UF_GetPreferredLineHeight(1);
				mListContentUnitData.Add (new ContentUnitData (info, pos));
			} else {
				float pos = mListContentUnitData [mListContentUnitData.Count - 1].pos + m_InsParam;
				mListContentUnitData.Add (new ContentUnitData (info, pos));	
			}

			//刷新内容
			if (needRefresh) {
                UF_Refresh();
			}
		}
			

		private void UF_DispatchSelectedChange(){
			if (!string.IsNullOrEmpty(eValueChange) && Application.isPlaying) {
				MessageSystem.UF_GetInstance ().UF_Send(DefineEvent.E_UI_OPERA,eValueChange,m_SelectIndex,eParam,this);
			}
		}


		public void UF_SetToIndex(int index){
			index = Mathf.Clamp (index, 0, mListContentUnitData.Count - 1);
			if (index != m_SelectIndex) {
				m_SelectIndex = index;
				m_LastSelectIndex = m_SelectIndex;

				Vector3 target = new Vector2 (content.anchoredPosition.x, mListContentUnitData [m_SelectIndex].pos);
				//滚动到目标
				content.anchoredPosition = target;
                //派发变更
                UF_DispatchSelectedChange();
			}
		}

		//设置到头部
		public void UF_SetToTop(){
			if (mListContentUnitData.Count > 0) {
                UF_SetToIndex(0);
			}
		}

		//设置到底部
		public void UF_SetToBottom(){
			if (mListContentUnitData.Count > 0) {
                UF_SetToIndex(mListContentUnitData.Count - 1);
			}
		}


		protected override void LateUpdate ()
		{
			base.LateUpdate ();

			if (!Application.isPlaying ||
				labelContent == null ||
				mListContentUnitData.Count == 0 ||
				m_CachePosY == content.anchoredPosition.y)
				return;

			if (!m_Dragging) {
				if (Mathf.Abs (this.velocity.y) < velocityZoom) {
					this.velocity = Vector3.zero;

					float detial = content.anchoredPosition.y;

					m_SelectIndex = (int)((Mathf.Abs (detial) + m_FixParam) / m_InsParam + 0.5f);

					m_SelectIndex = Mathf.Clamp (m_SelectIndex, 0, mListContentUnitData.Count - 1);

					m_CachePosY = content.anchoredPosition.y;

					Vector3 target = new Vector2 (content.anchoredPosition.x, mListContentUnitData [m_SelectIndex].pos);

					if (m_LastSelectIndex != m_SelectIndex) {
						m_LastSelectIndex = m_SelectIndex;
						//滚动到目标
						StartCoroutine (UF_IScrollToPos(content.anchoredPosition, target));

                        //派发变更
                        UF_DispatchSelectedChange();

					} else {
						if (Mathf.Abs (content.anchoredPosition.y - mListContentUnitData [m_SelectIndex].pos) > 0.01) {
							//滚动到目标
							StartCoroutine (UF_IScrollToPos(content.anchoredPosition, target));
						}
					}
				}
			}

		}

		IEnumerator UF_IScrollToPos(Vector3 _form,Vector3 _target){
			float tbuffer = 0;
			float speed = recoverSpeed;
			while(tbuffer < 1){
				tbuffer += Time.unscaledDeltaTime * speed;
				Vector3 cur = tbuffer * _target + (1.0f - tbuffer) * _form;
				content.anchoredPosition = cur;
				yield return null;
			}
		}


		#if UNITY_EDITOR
		[ContextMenu("Test Data")]
		void TestData(){
			for (int k = 1; k < 100; k++) {
                UF_AddContent(k.ToString ());
			}
            UF_Refresh();
		}


		[ContextMenu("Test Index")]
		void TestIndex(){
            UF_SetToIndex(3);
		}

		#endif

	}

}
