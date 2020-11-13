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
	//<o=#FFFFFFFF v=x,y,z,w> 内容 </o>
	//#: 颜色
	//v: 偏移值
	public static class RTOutline
	{
		//超链接记录的索引
		struct OutLineData{
			//起始索引
			public int idx;
			//长度
			public int length;
			public Color32 color;
			public Vector4 offset;
		}

		static Vector2[] s_TempOutlineOfs = new Vector2[4];


		public static void UF_OnPopulateMesh(UILabel label,List<TextToken> tokens,List<UIVertex> uivertexs,int startIndex = 0)
        {
			List<OutLineData> listOutLineDatas = ListCache<OutLineData>.Acquire ();
            UF_HandleToken(tokens, listOutLineDatas);
            UF_HandleMesh(uivertexs,listOutLineDatas, startIndex);
			ListCache<OutLineData>.Release (listOutLineDatas);
		}



		private static void UF_HandleToken(List<TextToken> tokens,List<OutLineData> listOutLineDatas){
			int overlying = 0;
			listOutLineDatas.Clear ();
			for (int k = 0; k < tokens.Count; k++) {
				if (tokens [k].type == TextTokenType.HEAD_O) {
					overlying++;
					for (int i = k + 1; i < tokens.Count; i++) {
						if (tokens [i].type == TextTokenType.HEAD_O) {
							overlying++;
						} else if (tokens [i].type == TextTokenType.END_O) {
							overlying--;
							if (overlying == 0) {
								OutLineData ldData = new OutLineData();
								//解析buff 内容
								int idxHref = tokens[k].buffer.IndexOf ("#");
								int idxValue = tokens[k].buffer.IndexOf ("v=");
								ldData.idx = tokens [k].index;
								ldData.length = tokens [i].index - tokens [k].index;

								ldData.color = new Color32 (0, 0, 0, 255);
								ldData.offset = new Vector4 (0.68f, 0.68f, 0, 0);
								if (idxHref > -1) {
									ldData.color = RichText.UF_ReadColor(tokens [k].buffer, idxHref + 1);
								}
								if (idxValue > -1) {
									ldData.offset = RichText.UF_ReadVector4(tokens [k].buffer, idxValue + 2);
								}
								listOutLineDatas.Add (ldData);
								//嵌套类型只有最外层有效
								k = i;
								break;
							}
						}
					}
				}
			}
		}

		//描边必须放在最后 
		private static void UF_HandleMesh(List<UIVertex> uivertexs,List<OutLineData> listOutLineDatas,int startIndex)
        {
			if (uivertexs.Count == 0 || listOutLineDatas.Count == 0)
				return;

			//Vector2 uvPoint = Vector2.zero;

			int charCount = uivertexs.Count / 6;

			UIVertex[] rawUIVeterxs = uivertexs.ToArray ();
			uivertexs.Clear ();
			for (int k = 0; k < listOutLineDatas.Count; k++) {
				OutLineData ldData = listOutLineDatas [k];
				if (ldData.idx + ldData.length > charCount) {
					break;
				}
                UF_PopulateOutLineMesh(uivertexs,rawUIVeterxs, startIndex + ldData.idx * 6,ldData.length * 6,ldData.offset,ldData.color);
			}
			uivertexs.AddRange (rawUIVeterxs);
		}


        //处理全部顶点，添加Outline mesh
        public static void UF_HandleMesh(List<UIVertex> uivertexs,Color color,Vector4 offset, int startIndex) {
            if (uivertexs.Count == 0) {
                return;
            }
            UIVertex[] rawUIVeterxs = uivertexs.ToArray();
            uivertexs.Clear();
            UF_PopulateOutLineMesh(uivertexs, rawUIVeterxs, startIndex, rawUIVeterxs.Length, offset, color);
            uivertexs.AddRange(rawUIVeterxs);
        }



        private static UIVertex UF_fillOutLineVertex(UIVertex value,Vector2 offset,Color outLineColor){
			Vector3 pos = value.position;
			pos.x += offset.x;
			pos.y += offset.y;
			//Mark！ 设置该值会影响Batch 合批
			//pos.z += 0.01f;
			Color color = outLineColor;
			value.position = pos;
			color.a = (value.color.a  * 0.00392157f) * outLineColor.a;
			value.color = color;
			return value;
		}

		//文字秒边
		public static void UF_PopulateOutLineMesh(List<UIVertex> uivertexs,UIVertex[] rawUIVeterxs,int idx,int length,Vector4 outLineOffset,Color32 olColor){
			s_TempOutlineOfs [0] = new Vector2 (outLineOffset.x + outLineOffset.z, outLineOffset.w);
			s_TempOutlineOfs [1] = new Vector2 (-outLineOffset.x + outLineOffset.z, outLineOffset.w);
			s_TempOutlineOfs [2] = new Vector2 (outLineOffset.z, outLineOffset.y + outLineOffset.w);
			s_TempOutlineOfs [3] = new Vector2 (outLineOffset.z, -outLineOffset.y + outLineOffset.w);
            int loopnum = Mathf.Min(idx + length, rawUIVeterxs.Length);
            for (int k = idx; k < loopnum;) {
				for (int i = 0; i < s_TempOutlineOfs.Length; i++) {
					Vector2 offset = s_TempOutlineOfs [i];
					UIVertex vertex1 = UF_fillOutLineVertex(rawUIVeterxs [k], offset,olColor);uivertexs.Add (vertex1);
					UIVertex vertex2 = UF_fillOutLineVertex(rawUIVeterxs [k + 1], offset,olColor);uivertexs.Add (vertex2);
					UIVertex vertex3 = UF_fillOutLineVertex(rawUIVeterxs [k + 2], offset,olColor);uivertexs.Add (vertex3);
					UIVertex vertex4 = UF_fillOutLineVertex(rawUIVeterxs [k + 3], offset,olColor);uivertexs.Add (vertex4);
					UIVertex vertex5 = UF_fillOutLineVertex(rawUIVeterxs [k + 4], offset,olColor);uivertexs.Add (vertex5);
					UIVertex vertex6 = UF_fillOutLineVertex(rawUIVeterxs [k + 5], offset,olColor);uivertexs.Add (vertex6);
				}
				k += 6;
			}
		}


	}
}

