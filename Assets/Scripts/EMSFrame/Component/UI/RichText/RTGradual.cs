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
	/// 渐变 富文本
	//格式：
	//<g=0 #FFAABCFF #FFFF00FF> 内容 </g>
	//g 类型，0 上下，1左右，3左上右，4左下右上
	//# 颜色
	public static class RTGradual
	{
		struct GradualData{
			//类:横向0，纵向1，左斜2，右斜3
			public int type;
			//起始索引
			public int idx;
			//url长度
			public int length;
			public Color32 colorA;
			public Color32 colorB;
		}


		public static void UF_OnPopulateMesh(UILabel label,List<TextToken> tokens,List<UIVertex> uivertexs,int startIndex = 0)
        {
			List<GradualData> listGraduals = ListCache<GradualData>.Acquire ();

            UF_HandleToken(tokens, listGraduals);
            UF_HandleMesh(uivertexs,listGraduals, startIndex);

			ListCache<GradualData>.Release (listGraduals);
		}

		private static void UF_HandleToken(List<TextToken> tokens,List<GradualData> listColors){
			int overlying = 0;
			listColors.Clear ();
			for (int k = 0; k < tokens.Count; k++) {
				if (tokens [k].type == TextTokenType.HEAD_G) {
					overlying++;
					for (int i = k + 1; i < tokens.Count; i++) {
						if (tokens [i].type == TextTokenType.HEAD_G) {
							overlying++;
						} else if (tokens [i].type == TextTokenType.END_G) {
							overlying--;
							if (overlying == 0) {
								GradualData data = new GradualData();
								data.idx = tokens [k].index;
								data.length = tokens [i].index - tokens [k].index;
								int idxG = tokens [k].buffer.IndexOf ("g=");
								if(idxG > -1)
								data.type = RichText.UF_ReadInt(tokens[k].buffer,idxG + 2,0);
								
								data.colorA = RichText.UF_ReadColor(tokens [k].buffer,tokens [k].buffer.IndexOf('#') + 1);
								data.colorB = RichText.UF_ReadColor(tokens [k].buffer,tokens [k].buffer.LastIndexOf('#') + 1);
								listColors.Add (data);
								break;
							}
						}
					}
				}
			}
		}

		private  static void UF_HandleMesh(List<UIVertex> uivertexs,List<GradualData> listColors,int startIndex)
        {
			if (uivertexs.Count == 0 || listColors.Count == 0)
				return;

			int charCount = uivertexs.Count / 6;
			for (int k = 0; k < listColors.Count; k++) {
				if (listColors [k].idx + listColors [k].length > charCount) {
					break;
				}
				for (int j = startIndex; j < listColors [k].length; j++) {
					int idx = (listColors [k].idx + j) * 6;
					int type = listColors [k].type;
					for (int i = 0; i < 6; i++) {
						UIVertex vertex = uivertexs [idx + i];
						Color32 color = UF_GetTargetColor(type,i,listColors [k]);
						//顶点Alpha颜色保留
						color.a = (byte)((vertex.color.a * color.a) / 255);
						//替换每个顶点的颜色
						vertex.color = color;	
						uivertexs [idx + i] = vertex;
					}
				}
			}

		}
			
		//顶点对应着色
		//015 234
		//顺时针 012 345
		private static int[,] s_MapColorType = {
			{0, 0, 1, 1, 1, 0},
			{0, 1, 1, 1, 0, 0},
			{0, 2, 1, 1, 2, 0},
			{2, 1, 2, 2, 0, 2},
		};

		private static Color32 UF_GetTargetColor(int type,int idx,GradualData data){
			if (type < 0 || type > 3) type = 0;
			if (idx < 0 || idx > 5) idx = 0;
			int val = s_MapColorType [type, idx];
			if (val == 0) {
				return data.colorA;
			} else if (val == 1) {
				return data.colorB;
			} else {
				return (Color)data.colorB + (Color)data.colorA / 2;
			}
		}



	}
}

