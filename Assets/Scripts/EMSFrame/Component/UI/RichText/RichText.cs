//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace UnityFrame
{
	public static class TextTokenType{
		public const int NONE = 0;

		/// <summary>
		/// 图文混排
		/// </summary>
		public const int QUAD = 1; 

		/// <summary>
		/// 超链接头
		/// </summary>
		public const int HEAD_H = 2;
		/// <summary>
		/// 超链接尾
		/// </summary>
		public const int END_H=-2;

		/// <summary>
		/// 颜色头
		/// </summary>
		public const int HEAD_C = 4;
		/// <summary>
		/// 颜色尾
		/// </summary>
		public const int END_C =-4;

		/// <summary>
		/// 渐变头
		/// </summary>
		public const int HEAD_G = 8;
		/// <summary>
		/// 渐变尾
		/// </summary>
		public const int END_G = -8;


		/// <summary>
		/// 下划线头
		/// </summary>
		public const int HEAD_U = 16;
		/// <summary>
		/// 下划线尾
		/// </summary>
		public const int END_U=-16;


		/// <summary>
		/// 描边头
		/// </summary>
		public const int HEAD_O = 32;
		/// <summary>
		/// 描边尾
		/// </summary>
		public const int END_O = -32;


        /// <summary>
		/// 阴影头
		/// </summary>
		public const int HEAD_S = 64;
        /// <summary>
        /// 阴影尾
        /// </summary>
        public const int END_S = -64;

    } 

	public struct TextToken
	{
		//类型
		public int type;
		//当前chunk
		public string buffer;
		//起始索引值
		public int index;
	}




	//富文本语词分析
	public static class RichText
	{
		private static StringBuilder sbChunk = new StringBuilder();
		private static StringBuilder sbBlock = new StringBuilder();

		private static StringBuilder s_strBulder = new StringBuilder(); 

		public static string UF_ReadString(string chunk,int startIdx){
			if (startIdx >= chunk.Length) {
				return string.Empty;
			}
			s_strBulder.Remove (0, s_strBulder.Length);
			char val = char.MinValue;
			for (int k = startIdx; k < chunk.Length; k++) {
				val = chunk [k];
				if (val == ' ' || val == '>' ) {
					return s_strBulder.ToString ();
				} else {
					s_strBulder.Append (chunk [k]);
				}
			}
			return s_strBulder.ToString ();
		}

		public static int UF_ReadInt(string chunk,int startIdx,int defaultVal){
			string strVal = UF_ReadString(chunk,startIdx);
			int val = defaultVal;
			if (string.IsNullOrEmpty (strVal))
				return val;
			int.TryParse (strVal, out val);
			return val;
		}

		public static float UF_ReadFloat(string chunk,int startIdx,float defaultVal){
			string strVal = UF_ReadString(chunk,startIdx);
			float val = defaultVal;
			if (string.IsNullOrEmpty (strVal))
				return val;
			float.TryParse (strVal, out val);
			return val;
		}

		public static Vector4 UF_ReadVector4(string chunk,int startIdx){
			string strVal = UF_ReadString(chunk,startIdx);
			float x, y, z, w;
			string[] array = GHelper.UF_SplitStringWithCount (strVal, 4, ',');
			float.TryParse (array [0],out x);
			float.TryParse (array [1],out y);
			float.TryParse (array [2],out z);
			float.TryParse (array [3],out w);
			return new Vector4(x,y,z,w);
		}

		public static Vector3 UF_ReadVector3(string chunk,int startIdx){
			string strVal = UF_ReadString(chunk,startIdx);
			float x, y, z;
			string[] array = GHelper.UF_SplitStringWithCount (strVal, 3, ',');
			float.TryParse (array [0],out x);
			float.TryParse (array [1],out y);
			float.TryParse (array [2],out z);
			return new Vector3(x,y,z);
		}

		public static Vector2 UF_ReadVector2(string chunk,int startIdx){
			string strVal = UF_ReadString(chunk,startIdx);
			float x, y;
			string[] array = GHelper.UF_SplitStringWithCount (strVal, 2, ',');
			float.TryParse (array [0],out x);
			float.TryParse (array [1],out y);
			return new Vector2(x,y);
		}


		public static Color32 UF_ReadColor(string chunk,int startIdx){
			string strVal = UF_ReadString(chunk,startIdx);
			if (string.IsNullOrEmpty (strVal))
				return new Color32(255,255,255,255);
			return GHelper.UF_UF_ParseStringToColor32 (strVal);
		}




		public static int UF_ParseToken(string buffer, List<TextToken> outTokens,ref string rawchunk,ref int handlebit){
			sbChunk.Remove (0, sbChunk.Length);
			sbBlock.Remove (0, sbBlock.Length);

			bool hook = false;
			outTokens.Clear ();
			char v = char.MinValue;
			for (int k = 0; k < buffer.Length; k++) {
				v = buffer [k];
				if (v == '<') {
					hook = true;
				} 
				else if (v == '>' && hook) {
					sbBlock.Append (v);
					string block = sbBlock.ToString ();
                    sbBlock.Remove (0, sbBlock.Length);
                    hook = false;
					TextToken token = new TextToken();
					token.index = sbChunk.Length;
					//超链接
					if (block.IndexOf ("<h=")>-1) {
						token.type = TextTokenType.HEAD_H;
						token.buffer = block;
						handlebit = handlebit | TextTokenType.HEAD_H;
					}
					else if (block == "</h>") {
						token.type = TextTokenType.END_H;
					} 
					//下划线
					else if (block.IndexOf("<u=")>-1) {
						token.type = TextTokenType.HEAD_U;
						token.buffer = block;
						handlebit = handlebit | TextTokenType.HEAD_U;
					} 
					else if (block == "</u>") {
						token.type = TextTokenType.END_U;
					} 

					//颜色
					else if (block.IndexOf ("<c=")>-1) {
						token.type = TextTokenType.HEAD_C;
						token.buffer = block;
						handlebit = handlebit | TextTokenType.HEAD_C;
					}
					else if (block == "</c>") {
						token.type = TextTokenType.END_C;
					}

					//图文混排
					else if (block.IndexOf ("<f=")>-1) {
						token.type = TextTokenType.QUAD;
						token.buffer = block;
						handlebit = handlebit | TextTokenType.QUAD;
						//图文类型替换为Quad
						int idxa = block.IndexOf ("s=");
						if (idxa > -1) {
							sbChunk.Append (string.Format ("<quad size={0}>", UF_ReadString(block,idxa + 2)));
						} 
						else {
							sbChunk.Append ("<quad>");
						}
					} 

					//渐变
					else if (block.IndexOf ("<g=")>-1) {
						token.type = TextTokenType.HEAD_G;
						token.buffer = block;
						handlebit = handlebit | TextTokenType.HEAD_G;
					}
					else if (block == "</g>") {
						token.type = TextTokenType.END_G;
					}

					//描边
					else if (block.IndexOf ("<o=")>-1) {
						token.type = TextTokenType.HEAD_O;
						token.buffer = block;
						handlebit = handlebit | TextTokenType.HEAD_O;
					}
					else if (block == "</o>") {
						token.type = TextTokenType.END_O;
					}

                    //阴影
                    else if (block.IndexOf("<s=") > -1)
                    {
                        token.type = TextTokenType.HEAD_S;
                        token.buffer = block;
                        handlebit = handlebit | TextTokenType.HEAD_S;
                    }
                    else if (block == "</s>")
                    {
                        token.type = TextTokenType.END_S;
                    }

                    else {
						token.type = TextTokenType.NONE;
						//未定义类型不需要剔除
						sbChunk.Append (block);
						continue;
					}
					outTokens.Add (token);
					continue;
				}
				if (hook)
					sbBlock.Append (v);
				else
					sbChunk.Append (v);					
			}

			rawchunk = sbChunk.ToString ();

			return handlebit;
		}


		public static void UF_OnPopulateMesh(UILabel label,List<TextToken> tokens,List<UIVertex> uivertexs){
            if (tokens.Count == 0) return;
			int handlebit = label.handleTokenBit;
			if ((handlebit & TextTokenType.HEAD_C) > 0)
				RTColor.UF_OnPopulateMesh(label,tokens,uivertexs);
			if ((handlebit & TextTokenType.HEAD_H) > 0)
				RTHyperlink.UF_OnPopulateMesh(label,tokens,uivertexs);
			if((handlebit & TextTokenType.HEAD_U) > 0)
				RTUnderLine.UF_OnPopulateMesh(label,tokens,uivertexs);
			if((handlebit & TextTokenType.QUAD) > 0)
				RTSprite.UF_OnPopulateMesh(label,tokens,uivertexs);
			if ((handlebit & TextTokenType.HEAD_G) > 0)
				RTGradual.UF_OnPopulateMesh(label,tokens,uivertexs, 0);

            //底部显示效果必须在最后，并获取前置索引
            int sourceLen = uivertexs.Count;
            int startIndex = 0;
            if ((handlebit & TextTokenType.HEAD_O) > 0) {
                RTOutline.UF_OnPopulateMesh(label, tokens, uivertexs, startIndex);
                startIndex = uivertexs.Count - sourceLen;
            }
            if ((handlebit & TextTokenType.HEAD_S) > 0)
            {
                RTShadow.UF_OnPopulateMesh(label, tokens, uivertexs, startIndex);
                startIndex = uivertexs.Count - sourceLen;
            }

        }


		public static void UF_OnStart(UILabel label){
			int handlebit = label.handleTokenBit;
			if ((handlebit & TextTokenType.QUAD) > 0)
				RTSprite.UF_OnStart (label);
		}

		public static void UF_OnClick(UILabel label,UnityEngine.EventSystems.PointerEventData eventData){
			int handlebit = label.handleTokenBit;
			if ((handlebit & TextTokenType.HEAD_H) > 0)
				RTHyperlink.UF_OnClick(label, eventData);
		}

		public static void UF_OnReset(UILabel label){
			int handlebit = label.handleTokenBit;
			if ((handlebit & TextTokenType.HEAD_H) > 0)
				RTHyperlink.UF_OnReset (label);
			if ((handlebit & TextTokenType.QUAD) > 0)
				RTSprite.UF_OnReset (label);
		}

	}
}

