//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace UnityFrame{
	//绘画板，实现涂鸦，画符，刮刮乐等效果
	public class UIDrawBoard : UnityEngine.UI.RawImage,IUIUpdate,IPointerDownHandler,IPointerUpHandler{

		public float penSize = 10.0f;
		//最小笔刷
		public float miniPenSize = 1.0f;
		//硬度
		public float penHard = 1;
		[Range(0,1.0f)]public float penPressure = 0f;

		//笔刷颜色
		public Color penColor = Color.white;
		//画板颜色
		public Color backgroundColor = Color.black;

		public string ePressDown = string.Empty;
		public string ePressUp = string.Empty;
		public string eParam = string.Empty;


		//绘制率
		public float drawingRate{get{ return m_DrawingRate;}}

		private float m_DrawingRate = 0;

		[SerializeField]private string m_UpdateKey;

		private Texture2D m_Paper;

		private Vector2 m_SizePaper;

		private Vector2 m_CurPoint;
		private Vector2 m_LastPoint;

		private bool m_BeginDraw = false;
		private bool m_NewBegin = false;

		private float m_CurPenSize = 0;
		private float m_CurPenPressureBuf = 0;

		public string updateKey{
			get{ return m_UpdateKey;}
			set{ m_UpdateKey = value;}
		}
			
		public void UF_SetActive(bool active){
			this.gameObject.SetActive (active);
		}

		public void UF_SetValue(object value){
			if (value == null) {return;}
			eParam = value.ToString ();
		}


		protected override void Start ()
		{
			base.Start ();
            UF_InitPaper();
            UF_Clear(backgroundColor);

		}


		//画圆
		protected void UF_DrawCircle(int x,int y,float radius,Color col,bool apply){
			if (m_Paper == null) {
				return;
			}
			x += (int)m_SizePaper.x / 2;
			y += (int)m_SizePaper.y / 2;
			int width = m_Paper.width;
			int height = m_Paper.height;
			//圆形采样
			int intRange = (int)radius; 
			int hRange = 0;
			int sqRange = intRange * intRange;
			for (int w =  -intRange; w < intRange; w++) {
				int absW = Mathf.Abs (w);
				hRange = (int)Mathf.Sqrt ((float)(sqRange - absW * absW));
				for (int h = -hRange; h < hRange; h++) {

					m_Paper.SetPixel (Mathf.Clamp(w + x,0,width),Mathf.Clamp(h + y,0,height),col);
				}
			}
			if (apply) {
				m_Paper.Apply ();
			}
		}

		protected void UF_Apply(){
			if (m_Paper) {
				m_Paper.Apply ();
			}
		}

		public void UF_Clear(Color col){
			if (m_Paper != null) {
				for (int w = 0; w < m_Paper.width; w++) {
					for (int h = 0; h < m_Paper.height; h++) {
						m_Paper.SetPixel (w,h,col);
					}
				}
				m_Paper.Apply ();
			}
		}

		[ContextMenu("Clear")]
		public void Clear(){
            UF_Clear(backgroundColor);
		}


		protected void UF_InitPaper(){
			m_SizePaper = this.rectTransform.sizeDelta;
			if (m_Paper == null) {
				m_Paper = new Texture2D ((int)m_SizePaper.x, (int)m_SizePaper.y,TextureFormat.RGBA32, false, false);
			} else {
				m_Paper.Resize ((int)m_SizePaper.x, (int)m_SizePaper.y);
			}
			this.texture = m_Paper;
		}


		public override void SetVerticesDirty ()
		{
			base.SetVerticesDirty ();
			if (m_SizePaper != this.rectTransform.sizeDelta) {
                UF_InitPaper();
			}
		}


		public void OnPointerDown (PointerEventData eventData){
			m_BeginDraw = true;
			m_NewBegin = true;
			m_CurPenPressureBuf = 0;
			if (!string.IsNullOrEmpty (ePressDown)) {
				MessageSystem.UF_GetInstance ().UF_Send(DefineEvent.E_UI_OPERA,ePressDown, eParam,this);
			}
		}

		public void OnPointerUp (PointerEventData eventData){
			m_BeginDraw = false;
			m_NewBegin = false;

			if (!string.IsNullOrEmpty (ePressUp)) {
				MessageSystem.UF_GetInstance ().UF_Send(DefineEvent.E_UI_OPERA,ePressUp, eParam,this);
			}
		}


		void UF_updatePenPressure(){
			if (penPressure <= 0) {
				m_CurPenSize = penSize;
			} else {
				m_CurPenPressureBuf += Time.deltaTime;
				float rate = Mathf.Clamp01 (m_CurPenPressureBuf / penPressure);
				m_CurPenSize = penSize * rate + miniPenSize * (1 - rate);
			}
		}


		void Update(){
			if (!m_BeginDraw)
				return;

            UF_updatePenPressure();

			Vector3 clickPoint = DeviceInput.UF_PressPosition(0);
			Vector2 pos;

			if (RectTransformUtility.ScreenPointToLocalPointInRectangle (this.rectTransform, clickPoint, canvas.worldCamera, out pos)) {
				m_CurPoint = pos;
				if (m_NewBegin) {
					m_LastPoint = m_CurPoint;
					m_NewBegin = false;
				}
				if (m_CurPoint != m_LastPoint) {
                    UF_DrawCircle((int)pos.x, (int)pos.y, m_CurPenSize, penColor,false);
					//点插值补偿	
					float distance = Vector2.Distance(m_CurPoint,m_LastPoint);
					float hfPSize = m_CurPenSize/2;
					if (((int)distance) > hfPSize) {
						int count = (int)(distance / hfPSize + 1);
						Vector2 current = Vector2.zero;
						float p = 0;
						for (int k = 0; k < count; k++) {
							p = (float)k / (float)count;
							current.x = m_LastPoint.x * p + m_CurPoint.x * (1 - p);
							current.y = m_LastPoint.y * p + m_CurPoint.y * (1 - p);

                            UF_DrawCircle((int)current.x, (int)current.y, m_CurPenSize, penColor,false);		
						}
					}
					this.UF_Apply();
					m_LastPoint = m_CurPoint;
				}
			}


		}


		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			if(m_Paper != null){
				DestroyImmediate (m_Paper);
			}
			m_Paper = null;
		}


	}

}