//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace UnityFrame
{
	//格式：
	//<f=xxxx s=12 o=1.2>
	//f: 表情sprite名字
	//s: 尺寸大小    (可选)
	//o: 垂直偏移		(可选)
	public static class RTSprite
	{
		//图文混排记录的单个表情索引
		struct SpriteData{
			public string name;
			public int size;
			public float offset;
			public int idx;		
		}

		private static bool alignCenter = true;				//中心对齐

		private static RTSpriteBoard UF_GetSpriteBoard(UILabel label){
			RTSpriteBoard spriteBoard = null;
			var tarnsboard = label.transform.Find ("RTSpriteBoard");
			if (tarnsboard == null) {
				GameObject goRTSprite = new GameObject ("RTSpriteBoard");
				goRTSprite.hideFlags = HideFlags.HideAndDontSave;
				spriteBoard = goRTSprite.AddComponent<RTSpriteBoard> ();
				spriteBoard.rectTransform.SetAsFirstSibling ();
				spriteBoard.rectTransform.SetParent (label.transform);
				spriteBoard.rectTransform.localPosition = Vector3.zero;
				spriteBoard.rectTransform.localScale = Vector3.one;
				spriteBoard.rectTransform.offsetMin = Vector3.zero;
				spriteBoard.rectTransform.offsetMax = Vector3.zero;
				spriteBoard.rectTransform.anchorMax = new Vector2 (1, 1);
				spriteBoard.rectTransform.anchorMin = new Vector2 (0, 0);
				spriteBoard.rectTransform.anchoredPosition = Vector2.zero;
                spriteBoard.rectTransform.pivot = label.rectTransform.pivot;

            } else {
				spriteBoard = tarnsboard.GetComponent<RTSpriteBoard> ();
                spriteBoard.rectTransform.pivot = label.rectTransform.pivot;
            }
			return spriteBoard;
		}


		public static void UF_OnStart(UILabel label){
			var spriteBoard = UF_GetSpriteBoard(label);
			spriteBoard.enabled = true;
		}


		public static void UF_OnPopulateMesh(UILabel label,List<TextToken> tokens,List<UIVertex> uivertexs,int startIndex = 0)
        {
			List<SpriteData> listSpriteDatas = ListCache<SpriteData>.Acquire ();
			RTSpriteBoard spriteBoard = UF_GetSpriteBoard(label);
            UF_HandleToken(tokens, listSpriteDatas,label.fontSize);
            UF_HandleMesh(uivertexs,listSpriteDatas,spriteBoard, startIndex);
			ListCache<SpriteData>.Release (listSpriteDatas);
		}

		private static void UF_HandleToken(List<TextToken> tokens,List<SpriteData> listSpriteDatas,int fontSize){
			listSpriteDatas.Clear ();
			for (int k = 0; k < tokens.Count; k++) {
				if (tokens [k].type == TextTokenType.QUAD) {
					int idxQuad = tokens[k].buffer.IndexOf ("f=");
					if (idxQuad < 0) {continue;}
					string spriteName = RichText.UF_ReadString(tokens[k].buffer,idxQuad + 2);;
					if (string.IsNullOrEmpty (spriteName))
						continue;
					SpriteData spritedata = new SpriteData();
					spritedata.name = spriteName;
					spritedata.idx = tokens [k].index;
					int idxSize = tokens[k].buffer.IndexOf ("s=");
					int idxOffset = tokens[k].buffer.IndexOf ("o=");
					if (idxSize > -1) {
						spritedata.size = RichText.UF_ReadInt(tokens [k].buffer, idxQuad + 2, 1);
					} else {
						spritedata.size = fontSize;
					}
					if (idxOffset > -1) {
						spritedata.offset = RichText.UF_ReadFloat(tokens[k].buffer,idxOffset + 2,0);
					}
					listSpriteDatas.Add (spritedata);
				}
			}
		}


		private static void UF_HandleMesh(List<UIVertex> uivertexs,List<SpriteData> listSpriteDatas,RTSpriteBoard spriteBoard,int startIndex)
        {
			if (spriteBoard == null) {
				return;
			}

			if (listSpriteDatas.Count > 0) {
				spriteBoard.UF_ClearVertexData();

				if (uivertexs.Count > 0) {
					for (int k = 0; k < listSpriteDatas.Count; k++) {
						int idx = listSpriteDatas [k].idx * 6;
						if (idx + 6 >= uivertexs.Count)
							break;

						if (alignCenter) {
                            UF_modifySpriteCenter(uivertexs, idx);
						} else {
                            UF_modifySpriteNormal(uivertexs, idx);
						}

						spriteBoard.UF_OnPopulateVertex(
							uivertexs, 
							idx,
							listSpriteDatas [k].offset,
							listSpriteDatas [k].name
							);
					}

					spriteBoard.UF_UpdateSprite();
				}
			} else {
				spriteBoard.Reset();
				spriteBoard.UF_UpdateSprite();
			}
				
		}

		private static void UF_modifySpriteCenter(List<UIVertex> uivertexs,int idx){
			//上边点
			UIVertex vtex0 = uivertexs [idx];
			UIVertex vtex1 = uivertexs [idx + 1];
			UIVertex vtex5 = uivertexs [idx + 5];
			//下边点
			UIVertex vtex2 = uivertexs [idx + 2];
			UIVertex vtex3 = uivertexs [idx + 3];
			UIVertex vtex4 = uivertexs [idx + 4];

			float hfp_size = (vtex1.position.x -  vtex0.position.x) / 2.0f;
			float cent_pos = (vtex0.position.y + vtex2.position.y) / 2.0f;

			vtex0.position.y = cent_pos + hfp_size;
			vtex1.position.y = cent_pos + hfp_size;
			vtex5.position.y = cent_pos + hfp_size;

			vtex2.position.y = cent_pos - hfp_size;
			vtex3.position.y = cent_pos - hfp_size;
			vtex4.position.y = cent_pos - hfp_size;

			uivertexs[idx] = vtex0 ;
			uivertexs[idx + 1] = vtex1;
			uivertexs[idx + 2] = vtex2;
			uivertexs[idx + 3] = vtex3;
			uivertexs[idx + 4] = vtex4;
			uivertexs[idx + 5] = vtex5;

		}

		 
		private static void UF_modifySpriteNormal(List<UIVertex> uivertexs,int idx){
			//上边点
			UIVertex vtex0 = uivertexs [idx];
			UIVertex vtex1 = uivertexs [idx + 1];
			UIVertex vtex5 = uivertexs [idx + 5];
			//下边点
			UIVertex vtex2 = uivertexs [idx + 2];
			UIVertex vtex3 = uivertexs [idx + 3];
			UIVertex vtex4 = uivertexs [idx + 4];

			float p_size = (vtex1.position.x -  vtex0.position.x);

			vtex0.position.y = vtex4.position.y + p_size;
			vtex1.position.y = vtex2.position.y + p_size;
			vtex5.position.y = vtex4.position.y + p_size;

			uivertexs[idx] = vtex0;
			uivertexs[idx + 1] = vtex1;
			uivertexs[idx + 2] = vtex2;
			uivertexs[idx + 3] = vtex3;
			uivertexs[idx + 4] = vtex4;
			uivertexs[idx + 5] = vtex5;

		}


		public static void UF_OnReset(UILabel label){
			var spriteBoard = UF_GetSpriteBoard(label);
			if (spriteBoard != null) {
				spriteBoard.enabled = false;
				spriteBoard.Reset ();
			}
		}


	}
}

