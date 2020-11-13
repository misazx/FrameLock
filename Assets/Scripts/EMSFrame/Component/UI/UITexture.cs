//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace UnityFrame{
	
	public class UITexture : UnityEngine.UI.RawImage,IUIUpdate,IUIColorable,IOnReset
	{
		public bool autoNativeSize = false;

		[SerializeField]private string m_UpdateKey;

		private DelegateBoolMethod m_OnWebTextureLoaded;

		private bool m_MarkChanged = false;

		public string updateKey{get{ return m_UpdateKey;}set{ m_UpdateKey = value;}}

		public void UF_SetActive(bool active){
			this.gameObject.SetActive (active);
		}
			
		public void UF_SetValue(object value){
			if (value == null)
                UF_Clear();
			else
                UF_SetTexture(value as string);
		}

		public void UF_SetRect(float x,float y,float w,float h)
		{
			base.uvRect = new Rect(x, y, w, h);
		}
			
		public Vector3 anchoredPosition{get{ return rectTransform.anchoredPosition;}set{rectTransform.anchoredPosition = value;}}

		public Vector3 sizeDelta{get{ return rectTransform.sizeDelta;}set{rectTransform.sizeDelta = value;}}

		public bool grey{set{UF_SetGrey (value);}}

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
			
		private void UF_SetRawTexture(Texture2D texture2d){
			if (this.texture == texture2d)
				return;
			if (this.texture != null) {
				RefObjectManager.UF_GetInstance ().UF_ReleaseRef(this.texture);
			}

			this.texture = texture2d;

			if (texture2d != null) {
                RefObjectManager.UF_GetInstance ().UF_RetainRef(this.texture);
				if (autoNativeSize) {
					SetNativeSize ();
				}
			}
		}

		public void UF_SetTexture(string textureName){
			if (string.IsNullOrEmpty (textureName)) {
                UF_SetRawTexture(null);
			} else {
				Texture2D t2d = TextureManager.UF_GetInstance ().UF_LoadTexture(textureName);
                UF_SetRawTexture(t2d);
			}
		}

		public void UF_SetLocalTexture(string textureFile){
			Texture2D t2d = TextureManager.UF_GetInstance().UF_LoadTextureLocal(textureFile);
            UF_SetRawTexture(t2d);
		}
			
		public int UF_SetWebTexture(string url){
			return UF_SetWebTextureWithCallback(url, null);
		}

		public int UF_SetWebTextureWithCallback(string url,DelegateBoolMethod callback){
			//自动调整为不渲染颜色
			this.color = new Color (1, 1, 1, 0);
			if (string.IsNullOrEmpty (url)) {
                UF_SetRawTexture(null);
				return 0;
			}
			m_OnWebTextureLoaded = callback;
			return TextureManager.UF_GetInstance ().UF_LoadTextureFromCacheOrDownload(url, UF_OnWebTextureLoaded);
		}


		private void UF_OnWebTextureLoaded(Texture2D tex2d){
			if (this != null) {
				bool retValue = true;
				if (tex2d != null) {
					this.color = Color.white;
					tex2d.filterMode = FilterMode.Bilinear;
					retValue = false;
				}
				this.UF_SetRawTexture(tex2d);
				if (m_OnWebTextureLoaded != null) {
					DelegateBoolMethod callback = m_OnWebTextureLoaded;
					m_OnWebTextureLoaded = null;
					callback (retValue);
				}
				m_MarkChanged = true;
			}
		}

		public void UF_Clear(){
            UF_SetRawTexture(null);
		}

		public void UF_Clear(bool alphaZero){
			if (alphaZero) {
				this.color = new Color (1, 1, 1, 0);
			}
            UF_SetRawTexture(null);
			m_OnWebTextureLoaded = null;
		}

		public void UF_OnReset(){
			if (m_MarkChanged) {
				if(this.texture != null)
                    UF_SetRawTexture(null);
				m_MarkChanged = false;
			}
		}

		protected override void OnDestroy ()
		{
            RefObjectManager.UF_GetInstance().UF_ReleaseRef(this.texture);
            this.texture = null;
            m_OnWebTextureLoaded = null;
			base.OnDestroy ();
		}

	}

}