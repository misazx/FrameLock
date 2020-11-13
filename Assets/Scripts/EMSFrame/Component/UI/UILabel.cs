//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityEngine.EventSystems;


//[DisallowMultipleComponent]

namespace UnityFrame{
	public class UILabel : Text,IUIUpdate,IUIColorable,IOnReset,IPointerClickHandler
	{
        //全局使用的Outline
        public bool outline = false;
		public int outlineColor = 0xff;
        public Vector4 outlineParam = new Vector4(1,1,0,0);

        //全局使用shadow
        public bool shadow = false;
        public int shadowColor = 0xff;
        public Vector2 shadowParam = new Vector2(2,-2);


        //点击触发事件
        public string ePressClick = "";
		public string eParam = "";

		protected static UIVertex[] s_TempQuadVerts = new UIVertex[4];

		//处理富文本解析token
		private List<TextToken> m_TextTokens = null;

		private List<TextToken> textTokens{
			get{
				if (m_TextTokens == null) {
					m_TextTokens = ListPool<TextToken>.Get ();
				}
				return m_TextTokens;
			}
			set{ 
				if (value == null && m_TextTokens != null) {
					ListPool<TextToken>.Release (m_TextTokens);
					m_TextTokens = null;
				}
			}
		}

		//富文本用到的Token Handle
		public int handleTokenBit{get{return m_HandleTokenBit;}}
		private int m_HandleTokenBit = 0;

		public int textLength{ get { return m_Text.Length;} }

		public int rawTextLength{ get { return m_RawText.Length;} }

		private string m_RawText = string.Empty;

        private string m_LastText = string.Empty;

        [SerializeField]private string m_UpdateKey;

		public string updateKey{get{ return m_UpdateKey;}set{ m_UpdateKey = value;}}

		public Vector3 anchoredPosition{get{ return rectTransform.anchoredPosition;}set{rectTransform.anchoredPosition = value;}}

		public Vector3 sizeDelta{get{ return rectTransform.sizeDelta;}set{rectTransform.sizeDelta = value;}}

		public void UF_SetActive(bool active){
			this.gameObject.SetActive (active);
		}

		public void UF_SetValue (object value){
			if (value == null) {return;}
			this.text = value.ToString();
		}
			
		public void UF_SetGrey(bool opera)
		{
			this.material = opera ? ShaderManager.UF_GetInstance().UF_GetUIGreyMaterial():null ;
		}
		public void UF_SetAlpha (float value){
			this.color = new Color (this.color.r, this.color.g, this.color.b, value);
		}

		public void UF_SetColor (UnityEngine.Color value){
			this.color = value;
		}

		//当前实际文本内容
		public string rawText {
			get{
				if (this.supportRichText) {
					return m_RawText;
				} else {
					return m_Text;
				}
			}
		}

        public override float preferredHeight {
			get {
                TextGenerationSettings generationSettings = this.GetGenerationSettings(new Vector2(base.GetPixelAdjustedRect().size.x, 0));
                return this.cachedTextGeneratorForLayout.GetPreferredHeight (rawText, generationSettings) / this.pixelsPerUnit;
			}
		}

		public override float preferredWidth {
			get {
				TextGenerationSettings generationSettings = this.GetGenerationSettings (Vector2.zero);
				return this.cachedTextGeneratorForLayout.GetPreferredWidth (rawText, generationSettings) / this.pixelsPerUnit;
			}
		}


        public override void SetVerticesDirty()
		{

            if (supportRichText)
            {
                if (m_LastText != m_Text)
                {
                    m_LastText = m_Text;
                    //解析token,并且获取RichTextHandles 列表
                    RichText.UF_ParseToken(m_Text, textTokens, ref m_RawText, ref m_HandleTokenBit);
                    RichText.UF_OnStart(this);
                }
            }
            else
            {
                RichText.UF_OnReset(this);
                m_LastText = string.Empty;
                m_HandleTokenBit = 0;
            }

            base.SetVerticesDirty();
        }
			
			
		protected void UF_OnPopulateTextMesh(VertexHelper toFill)
		{
			if (!(this.font == null)) {
				this.m_DisableFontTextureRebuiltCallback = true;
				Vector2 size = base.rectTransform.rect.size;
				//补偿内容框尺寸，避免字符被意外裁剪
				size.x += 1.0f;size.y += 1.0f;
				TextGenerationSettings generationSettings = this.GetGenerationSettings (size);
				//静态字体不需要任何Style与Size
				if (!this.font.dynamic) {
					generationSettings.fontSize = 0;
					generationSettings.fontStyle = FontStyle.Normal;
				}
				//如果使用rich text ，替换text 文本为去除rich text 的 rawtext 文本
				if (supportRichText) {
					this.cachedTextGenerator.Populate (this.m_RawText, generationSettings);
				} else {
					this.cachedTextGenerator.Populate (this.m_Text, generationSettings);
				}

				IList<UIVertex> verts = this.cachedTextGenerator.verts;
				float d = 1 / this.pixelsPerUnit;
				int num = verts.Count;// - 4;
                if (num <= 0) {
                    toFill.Clear();
                    return;
                }
				Vector2 vector = new Vector2 (verts [0].position.x, verts [0].position.y) * d;
				vector = base.PixelAdjustPoint (vector) - vector;
				toFill.Clear ();
				if (vector != Vector2.zero) {
					for (int i = 0; i < num; i++) {
						int num2 = i & 3;
						if (supportRichText) {
							UIVertex uivt = verts [i];
							//alpha值影响富文本
							Color color = uivt.color;
							color.a = this.color.a;
							uivt.color = color;
							s_TempQuadVerts [num2] = uivt;
						} else {
							s_TempQuadVerts [num2] = verts [i];
						}
						UIVertex[] tempVerts = s_TempQuadVerts;
						tempVerts [num2].position = tempVerts [num2].position * d;
						tempVerts [num2].position.x = tempVerts [num2].position.x + vector.x;
						tempVerts [num2].position.y = tempVerts [num2].position.y + vector.y;
						if (num2 == 3) {
							toFill.AddUIVertexQuad (s_TempQuadVerts);
						}
					}
				}
				else {
					for (int j = 0; j < num; j++) {
						int num3 = j & 3;
						if (supportRichText) {
							UIVertex uivt = verts [j];
							//alpha值影响富文本
							Color color = uivt.color;
							color.a = this.color.a;
							uivt.color = color;
							s_TempQuadVerts [num3] = uivt;
						} else {
							s_TempQuadVerts [num3] = verts [j];
						}
						UIVertex[] tempVerts = s_TempQuadVerts;
						tempVerts [num3].position = tempVerts [num3].position * d;
						if (num3 == 3) {
							toFill.AddUIVertexQuad (s_TempQuadVerts);
						}
					}
				}
				this.m_DisableFontTextureRebuiltCallback = false;
			}
		}





		protected override void OnPopulateMesh(VertexHelper vertexHelper)
		{
            UF_OnPopulateTextMesh(vertexHelper);
			//RichText 的处理在这里

			//基于 tokens  处理富文本
			if (this.supportRichText && (textTokens.Count > 0 || outline)) {
				List<UIVertex> tempUIVerts = ListCache<UIVertex>.Acquire ();
				vertexHelper.GetUIVertexStream (tempUIVerts);
				//根据m_HandleTokenBit 值处理对应的RichText
				RichText.UF_OnPopulateMesh(this,textTokens,tempUIVerts);
                
                int sourceLen = tempUIVerts.Count;
                int startIndex = 0;
                //全局描边
                if (outline) {
                    RTOutline.UF_HandleMesh(tempUIVerts, GHelper.UF_IntToColor(outlineColor), outlineParam, startIndex);
                    startIndex = tempUIVerts.Count - sourceLen;
                }
                //全局阴影
                if (shadow) {
                    RTShadow.UF_HandleMesh(tempUIVerts, GHelper.UF_IntToColor(shadowColor), shadowParam, startIndex);
                    startIndex = tempUIVerts.Count - sourceLen;
                }


                vertexHelper.Clear ();
				vertexHelper.AddUIVertexTriangleStream (tempUIVerts);

				ListCache<UIVertex>.Release (tempUIVerts);
			}
		}

		//点击触发
		public void OnPointerClick (PointerEventData eventData){
			//处理触发的Rich text
			if(supportRichText){
				//超链接需要点击操作
				RichText.UF_OnClick(this,eventData);

			}
			if (!string.IsNullOrEmpty (ePressClick)) {
				MessageSystem.UF_GetInstance ().UF_Send(DefineEvent.E_UI_OPERA,ePressClick,eParam,this);
			}
		}

		//获取每一行文字递增的高度
		public float UF_GetPreferredLineHeight(){
			TextGenerationSettings generationSettings = this.GetGenerationSettings (new Vector2 (base.GetPixelAdjustedRect ().size.x, 0));
			float valueA = this.cachedTextGeneratorForLayout.GetPreferredHeight (" ", generationSettings) / this.pixelsPerUnit;
			float valueB = this.cachedTextGeneratorForLayout.GetPreferredHeight (" \n ", generationSettings) / this.pixelsPerUnit;
			return valueB - valueA;
		}

		//获取N行文字计算出的高度
		public float UF_GetPreferredLineHeight(int count){
			TextGenerationSettings generationSettings = this.GetGenerationSettings (new Vector2 (base.GetPixelAdjustedRect ().size.x, 0));
			if (count > 1) {
				string temp = new string ('\n', count);
				return this.cachedTextGeneratorForLayout.GetPreferredHeight (temp, generationSettings) / this.pixelsPerUnit;
			} else {
				return this.cachedTextGeneratorForLayout.GetPreferredHeight (string.Empty, generationSettings) / this.pixelsPerUnit;
			}
		}

		//调整UI ,适配文字尺寸
		public void UF_AdjustPreferredSize(){
			Vector2 preferredSize = new Vector2 (this.preferredWidth,this.preferredHeight);
			if (rectTransform.sizeDelta.x != preferredSize.x || rectTransform.sizeDelta.y != preferredSize.y) {
				Vector3 spos = rectTransform.localPosition;
				rectTransform.sizeDelta = preferredSize;
				rectTransform.localPosition = spos;
			}
		}

		public void UF_OnReset (){
			RichText.UF_OnReset(this);
			m_HandleTokenBit = 0;
			textTokens = null;
            m_LastText = string.Empty;
            m_RawText = string.Empty;

        }

		protected override void OnDisable ()
		{
			base.OnDisable ();
            if(!Application.isPlaying)
                UF_OnReset();
        }

        protected override void OnDestroy ()
		{
            UF_OnReset();
			base.OnDestroy ();
		}


    }
}