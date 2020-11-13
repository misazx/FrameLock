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
	//<u> 内容 </h>
	//<u=1 o=-1 t=0 #FF00FFFF> 内容 </u>
	//u 大小
	//o 上下偏移
	//t 类型
	//# 颜色
	//u: 偏移值
	public static class RTUnderLine
	{
		struct ULineData{
			//起始索引
			public int type;
			public int idx;
			public float offset;
			public float size;
			public int length;
			public bool useColor;
			public Color32 color;
		}

		private const char D_CHAR_BLOCK = '■';	//计算下划线所使用特殊字符所需要的UV值

		private const float ZOOM_UNDER_LINE = 10.0f;

		private const int LINE_TYPE_NORMAL = 0;
		//斜线
		private const int LINE_TYPE_SLASH = 1;
		//反斜线
		private const int LINE_TYPE_BACKSLASH = 2;

		private static CharacterInfo CharacterInfoBlock;

		private static void UF_CheckInfoBlock(UILabel label){
			if (label.font == null) {
				return;
			}
			label.font.RequestCharactersInTexture (D_CHAR_BLOCK.ToString ());
			label.font.GetCharacterInfo (D_CHAR_BLOCK, out CharacterInfoBlock);
		}


		public static void UF_OnPopulateMesh(UILabel label,List<TextToken> tokens,List<UIVertex> uivertexs,int startIndex = 0)
        {
			List<ULineData> listULineDatas = ListCache<ULineData>.Acquire ();
			//基于fontsize ,计算size
			float SizeScale = label.fontSize * 0.1f;
            UF_CheckInfoBlock(label);
            UF_HandleToken(tokens, listULineDatas,SizeScale);
            UF_HandleMesh(uivertexs,listULineDatas, startIndex);
			ListCache<ULineData>.Release (listULineDatas);
		}


		private static void UF_HandleToken(List<TextToken> tokens,List<ULineData> listULineDatas,float sizeScale){
			listULineDatas.Clear ();
			int overlying = 0;
			for (int k = 0; k < tokens.Count; k++) {
				if (tokens [k].type == TextTokenType.HEAD_U) {
					overlying++;
					for (int i = k + 1; i < tokens.Count; i++) {
						if (tokens [i].type == TextTokenType.HEAD_U) {
							overlying++;
						} else if (tokens [i].type == TextTokenType.END_U) {
							overlying--;
							if (overlying == 0) {
								ULineData uline = new ULineData();
								uline.idx = tokens [k].index;
								uline.length = tokens [i].index - tokens [k].index;
								//样式
								int idxSize = tokens[k].buffer.IndexOf ("u=");
								if (idxSize > -1)
									uline.size = RichText.UF_ReadFloat(tokens [k].buffer, idxSize + 2, 1) * sizeScale;
								else
									uline.size = 1 * sizeScale;

								int idxOff = tokens[k].buffer.IndexOf ("o=");
								if (idxOff > -1)
									uline.offset = RichText.UF_ReadFloat(tokens [k].buffer, idxOff + 2, 0);

								int idxType = tokens[k].buffer.IndexOf ("t=");
								if (idxType > -1)
									uline.type = RichText.UF_ReadInt(tokens [k].buffer, idxType + 2, 1);
								else
									uline.type = 0;

								int idxCOff = tokens[k].buffer.IndexOf ("#");
								if (idxCOff > -1){
									uline.color = RichText.UF_ReadColor(tokens [k].buffer, idxCOff + 1);
									uline.useColor = true;
								}
								listULineDatas.Add (uline);
								//嵌套类型只有最外层有效
								k = i;
								break;
							}
						}
					}
				}
			}

		}



		private static void UF_HandleMesh(List<UIVertex> uivertexs,List<ULineData> listULineDatas,int startIndex)
        {
			if (uivertexs.Count == 0 || listULineDatas.Count == 0)
				return;

			Vector2 uvPoint = Vector2.zero;

			uvPoint.x = (CharacterInfoBlock.uvBottomLeft.x + CharacterInfoBlock.uvTopRight.x) * 0.5f;

			uvPoint.y = (CharacterInfoBlock.uvBottomLeft.y + CharacterInfoBlock.uvTopRight.y) * 0.5f;

			int charCount = uivertexs.Count / 6;

			for (int k = 0; k < listULineDatas.Count; k++) {

				ULineData ulinfo = listULineDatas [k];

				int len = ulinfo.length;

				if (ulinfo.idx + ulinfo.length > charCount) {
					len = charCount - ulinfo.idx;
				}

				Vector2 startUp = uivertexs [ulinfo.idx * 6].position;
				Vector2 lastBottom = uivertexs [ulinfo.idx * 6 + 3].position;
				Color32 vcolor = uivertexs [ulinfo.idx * 6].color;

				if (ulinfo.useColor) {
					byte sa = vcolor.a;
					vcolor = ulinfo.color;
					vcolor.a = (byte)((sa * vcolor.a) / 255);
				}
					
				for (int j = startIndex; j < len; j++) {
					int idx = (ulinfo.idx + j) * 6;
					Vector2 pmin = uivertexs [idx].position;
					Vector2 pmax = uivertexs [idx + 3].position;
						 
					if (Mathf.Abs (lastBottom.y - pmax.y) < ZOOM_UNDER_LINE) {
						lastBottom = pmax;
					} else {

                        //分段
                        UF_AddUnderLineToVertex(
							startUp,
							lastBottom,
							uivertexs,
							vcolor,
							uvPoint,
							ulinfo.offset,
                            UF_GetLROffset(ulinfo.type,Mathf.Abs (startUp.y - lastBottom.y)),
							ulinfo.size);
						startUp = pmin;
						lastBottom = pmax;
					}
				}

                UF_AddUnderLineToVertex(
					startUp,
					lastBottom,
					uivertexs,
					vcolor,
					uvPoint,
					ulinfo.offset,
                    UF_GetLROffset(ulinfo.type,Mathf.Abs (startUp.y - lastBottom.y)),
					ulinfo.size);
			}
		}



		private static Vector2 UF_GetLROffset(int type,float val){
			switch (type) {
			case LINE_TYPE_NORMAL:return Vector2.zero;
			case LINE_TYPE_SLASH:return new Vector2(val,0);
			case LINE_TYPE_BACKSLASH:return new Vector2(0,val);
			default:return Vector2.zero;
			}
		}
			

		private static void UF_AddUnderLineToVertex(Vector2 pmin,Vector2 pmax,List<UIVertex> uivertexs,Color32 vcolor,Vector2 uv,float offset,Vector2 lroffset,float size){
			float linehegiht = size + offset;
			float lofheight = lroffset.x;
			float rofheight = lroffset.y;
			UIVertex uivt = new UIVertex ();
			uivt.uv0=uv;uivt.color = vcolor;uivt.position = new Vector3 (pmin.x,pmax.y - offset + lofheight,0);uivertexs.Add (uivt);
			uivt = new UIVertex ();
			uivt.uv0=uv;uivt.color = vcolor;uivt.position = new Vector3 (pmax.x,pmax.y- offset + rofheight,0);uivertexs.Add (uivt);
			uivt = new UIVertex ();
			uivt.uv0=uv;uivt.color = vcolor;uivt.position = new Vector3 (pmax.x,pmax.y - linehegiht+ rofheight,0);uivertexs.Add (uivt);
			uivt = new UIVertex ();
			uivt.uv0=uv;uivt.color = vcolor;uivt.position = new Vector3 (pmax.x,pmax.y - linehegiht+ rofheight,0);uivertexs.Add (uivt);
			uivt = new UIVertex ();
			uivt.uv0=uv;uivt.color = vcolor;uivt.position = new Vector3 (pmin.x,pmax.y- linehegiht+ lofheight,0);uivertexs.Add (uivt);
			uivt = new UIVertex ();
			uivt.uv0=uv;uivt.color = vcolor;uivt.position = new Vector3 (pmin.x,pmax.y- offset+ lofheight,0);uivertexs.Add (uivt);
		}


	}
}

