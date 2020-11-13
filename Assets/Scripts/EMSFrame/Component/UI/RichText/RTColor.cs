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
	/// <summary>
	/// 颜色富文本
	/// 格式：  

	//格式：
	//<c=#FFAABCFF> 内容 </c>
	//c: 颜色值
	public static class RTColor
	{
		struct ColorData{
			//起始索引
			public int idx;
			//url长度
			public int length;
			public Color32 color;
		}
			
		public static void UF_OnPopulateMesh(UILabel label,List<TextToken> tokens,List<UIVertex> uivertexs, int startIndex = 0)
        {
			List<ColorData> listColors = ListCache<ColorData>.Acquire ();
            UF_HandleToken(tokens, listColors);
            UF_HandleMesh(uivertexs,listColors, startIndex);
			ListCache<ColorData>.Release (listColors);
		}

		private static void UF_HandleToken(List<TextToken> tokens,List<ColorData> listColors){
			int overlying = 0;
			listColors.Clear ();
			for (int k = 0; k < tokens.Count; k++) {
				if (tokens [k].type == TextTokenType.HEAD_C) {
					overlying++;
					for (int i = k + 1; i < tokens.Count; i++) {
						if (tokens [i].type == TextTokenType.HEAD_C) {
							overlying++;
						} else if (tokens [i].type == TextTokenType.END_C) {
							overlying--;
							if (overlying == 0) {
								int idxV = tokens[k].buffer.IndexOf ("#");
								if (idxV > -1) {
									ColorData data;
									data.idx = tokens [k].index;
									data.length = tokens [i].index - tokens [k].index;
									data.color = RichText.UF_ReadColor(tokens [k].buffer,idxV + 1);
									listColors.Add (data);
								}
								break;
							}
						}
					}
				}
			}
		}


		private static void UF_HandleMesh(List<UIVertex> uivertexs,List<ColorData> listColors, int startIndex)
        {
			if (uivertexs.Count == 0 || listColors.Count == 0)
				return;

			int charCount = uivertexs.Count / 6;

			for (int k = 0; k < listColors.Count; k++) {
				ColorData cdata = listColors [k];
				int len = cdata.length;
				if (cdata.idx + cdata.length > charCount) {
					len = charCount - cdata.idx;
				}

				for (int j = startIndex; j < len; j++) {
					int idx = (cdata.idx + j) * 6;
					for (int i = 0; i < 6; i++) {
						UIVertex vertex = uivertexs [idx + i];
						Color32 color = cdata.color;
						//顶点Alpha颜色保留
						color.a = (byte)((vertex.color.a * color.a) / 255);
						//替换每个顶点的颜色
						vertex.color = color;	
						uivertexs [idx + i] = vertex;
					}
				}
			}
		}


	}
}

