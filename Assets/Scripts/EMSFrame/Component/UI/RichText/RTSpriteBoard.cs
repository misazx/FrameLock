//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using UnityEngine.Sprites;
using System.Collections.Generic;

namespace UnityFrame{
	public class RTSpriteBoard : MaskableGraphic,System.IDisposable
	{
		struct VertexData{
			public Vector3 position;
			public Vector2 uv;
		}

		struct SpriteData
		{
			public int tick;
			public VertexData[] vertex;
			public string spriteName;
			public string prefix;
			public bool hasAnima;
		}

		private Color32 SPRITE_COLOR = new Color32 (255, 255, 255, 255);

		private List<SpriteData> m_SpriteDatas = new List<SpriteData>();

		//必须保证Label中所使用的表情或图片在同一个图集中,否则将出现错乱
		private Texture m_TmpMainTexture;

		private float m_BuffTick = 0;

        //播放固定播放间隔
        private static float s_FixedAnimaInterval = 0.25f;

        public override Texture mainTexture {
			get {
				if (m_TmpMainTexture != null) {
					return m_TmpMainTexture;
				} else {
					return null;
				}
			}
		}

		protected override void Awake ()
		{
			base.Awake ();
			this.raycastTarget = false;
		}

		public void UF_ClearVertexData(){
			m_SpriteDatas.Clear ();
		}

		public void UF_SetAlpha(float value){
			SPRITE_COLOR.a = (byte)((value/1.0f) * 255);
		}

		public void UF_SetColor(Color32 value){
			SPRITE_COLOR = value;
		}

		//获取Sprite , 并获取宽高比
		private float UF_GetSpriteAspect(string spriteName){
			Sprite sprite = UIManager.UF_GetInstance().UF_GetSprite(spriteName);
			float aspect;
			if (sprite != null) {
				aspect = sprite.rect.width / sprite.rect.height;
			} else {
				aspect = 1;
			}
			return aspect;
		}


		public void UF_OnPopulateVertex(List<UIVertex> list,int startIndex,float valOffset,string spriteName){
			int count = 6;

			VertexData[] vdArray = new VertexData[count];

			Vector3 offset = new Vector3(0,valOffset,0);

			UIVertex vtex1 = list [startIndex];vtex1.uv0 = Vector2.zero;list [startIndex] = vtex1;
			UIVertex vtex2 = list [startIndex + 1];vtex2.uv0 = Vector2.zero;list [startIndex + 1] = vtex2;
			UIVertex vtex3 = list [startIndex + 2];vtex3.uv0 = Vector2.zero;list [startIndex + 2] = vtex3;
			UIVertex vtex4 = list [startIndex + 3];vtex4.uv0 = Vector2.zero;list [startIndex + 3] = vtex4;
			UIVertex vtex5 = list [startIndex + 4];vtex5.uv0 = Vector2.zero;list [startIndex + 4] = vtex5;
			UIVertex vtex6 = list [startIndex + 5];vtex6.uv0 = Vector2.zero;list [startIndex + 5] = vtex6;

			//基于宽高比缩放点位置
			//0 1 2 2 3 0
			float aspect = UF_GetSpriteAspect(spriteName);
			float size = Mathf.Abs (vtex1.position.x - vtex2.position.x);
			Vector3 aspoffset = Vector3.zero;
			if (aspect > 1) {
				//宽度固定
				aspoffset.y = size - size / aspect;
			} else {
				//高度固定
				aspoffset.x = size - size * aspect;
			}
				
			vdArray[0].position = vtex1.position - aspoffset + offset;
			vdArray[1].position = vtex2.position - aspoffset + offset;
			vdArray[2].position = vtex3.position - aspoffset + offset;
			vdArray[3].position = vtex4.position - aspoffset + offset;
			vdArray[4].position = vtex5.position - aspoffset + offset;
			vdArray[5].position = vtex6.position - aspoffset + offset;


			SpriteData data;
			data.vertex = vdArray;
			data.spriteName = spriteName;
			data.tick = 1;
            data.hasAnima = true;
			if (data.hasAnima) {
				int idx = Mathf.Max(spriteName.LastIndexOf ('_'),0);
				data.prefix = spriteName.Substring (0, idx + 1);
			} else {
				data.prefix = string.Empty;
			}

			m_SpriteDatas.Add (data);
		}
			


		public void UF_UpdateSprite(){
			this.UpdateGeometry();
			this.UpdateMaterial ();
		}
		 

		protected override void OnRectTransformDimensionsChange (){
//			base.OnRectTransformDimensionsChange ();
		}

		protected override void OnTransformParentChanged (){}

		void LateUpdate(){
            UF_UpdateAniam();
		}


		private Sprite UF_GetSprite(string spriteName,string prefix,ref int tick,ref bool hasAnima){
			Sprite ret = null;
			if (string.IsNullOrEmpty (prefix) || !hasAnima) {
				ret = UIManager.UF_GetInstance().UF_GetSprite(spriteName);
			} else {
				string nextSpriteName = prefix + tick;
				ret = UIManager.UF_GetInstance().UF_GetSprite(nextSpriteName);
				if (ret == null) {
					if (tick == 2)
						hasAnima = false;
					tick = 1;
					ret = UIManager.UF_GetInstance().UF_GetSprite(spriteName);
				}
				tick++;
			}
			return ret;
		}

		private void UF_UpdateAniam()
		{
            m_BuffTick += GTime.DeltaTime;
            if (m_BuffTick >= s_FixedAnimaInterval)
            {
                //this.SetVerticesDirty ();
                for (int k = 0; k < m_SpriteDatas.Count; k++) {
                    if (m_SpriteDatas[k].hasAnima) {
                        this.UpdateGeometry();
                        break;
                    }
                }
                m_BuffTick = 0;
            }
        }


		private void UF_UpdateUV()
		{
			for (int k = 0; k < m_SpriteDatas.Count;k++) {

				SpriteData spriteData = m_SpriteDatas [k];

				Sprite sprite = UF_GetSprite(spriteData.spriteName,spriteData.prefix,ref spriteData.tick,ref spriteData.hasAnima);

				if (sprite != null) {
					m_TmpMainTexture = sprite.texture;
				} else {
					m_SpriteDatas.RemoveAt (k);
					k--;
					continue;
				}

				Vector4 outerUV = ((sprite == null) ? Vector4.zero : DataUtility.GetOuterUV (sprite));

				//0 1 2 2 3 0
				//UV需要做旋转处理
				spriteData.vertex[0].uv = new Vector2 (outerUV.x, outerUV.w);
				spriteData.vertex[1].uv = new Vector2 (outerUV.z, outerUV.w);
				spriteData.vertex[2].uv = new Vector2 (outerUV.z, outerUV.y);

				spriteData.vertex[3].uv = new Vector2 (outerUV.z, outerUV.y);
				spriteData.vertex[4].uv = new Vector2 (outerUV.x, outerUV.y);
				spriteData.vertex[5].uv = new Vector2 (outerUV.x, outerUV.w);

				m_SpriteDatas [k] = spriteData;

			}
		}


		protected override void OnPopulateMesh (VertexHelper vh)
		{
			vh.Clear ();
			if (m_SpriteDatas.Count > 0) {
                UF_UpdateUV();
				int index = 0;
				for (int k = 0; k < m_SpriteDatas.Count; k++) {
					VertexData[] array = m_SpriteDatas [k].vertex;
					vh.AddVert (array [0].position, SPRITE_COLOR, array [0].uv);
					vh.AddVert (array [1].position, SPRITE_COLOR, array [1].uv);
					vh.AddVert (array [2].position, SPRITE_COLOR, array [2].uv);
					vh.AddTriangle (index, index + 1, index + 2);
					vh.AddVert (array [3].position, SPRITE_COLOR, array [3].uv);
					vh.AddVert (array [4].position, SPRITE_COLOR, array [4].uv);
					vh.AddVert (array [5].position, SPRITE_COLOR, array [5].uv);
					vh.AddTriangle (index + 3, index + 4, index + 5);
					index += 6;
				}
			} else {
				m_TmpMainTexture = null;
			}
		}

		public new void Reset (){
			m_TmpMainTexture = null;
			m_SpriteDatas.Clear ();
			SPRITE_COLOR = new Color32 (255, 255, 255, 255);
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			m_TmpMainTexture = null;
		}

		public void Dispose(){
			m_TmpMainTexture = null;

		}


	}

}