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
	//<h=XXXX v=12345> 内容 </h>
	//h: 事件名
	//v: 事件参数
	public static class RTHyperlink
	{

		//超链接记录的索引
		struct HyperlinkDatas{
			//触发事件
			public string href;
			//触发值
			public string value;
			//起始索引
			public int idx;
			//url长度
			public int length;
			//包围盒,用于判断点击碰撞
			public Rect[] box;
		}

		private static Dictionary<int,List<HyperlinkDatas>> s_HyperlinkCacheTable = new Dictionary<int, List<HyperlinkDatas>> ();


		public static void UF_OnPopulateMesh(UILabel label,List<TextToken> tokens,List<UIVertex> uivertexs,int startIndex = 0)
        {
			List<HyperlinkDatas> listHyperlinkDatas = null;
			int labelID = label.GetInstanceID ();
			if (s_HyperlinkCacheTable.ContainsKey(labelID)) {
				listHyperlinkDatas = s_HyperlinkCacheTable [labelID];
			} else {
				listHyperlinkDatas = ListPool<HyperlinkDatas>.Get ();
				s_HyperlinkCacheTable.Add(labelID,listHyperlinkDatas);
			}

            UF_HandleToken(tokens, listHyperlinkDatas);
            UF_HandleMesh(uivertexs,listHyperlinkDatas, startIndex);
		}


		private static void UF_HandleToken(List<TextToken> tokens,List<HyperlinkDatas> listHyperlinkDatas){
			int overlying = 0;
			listHyperlinkDatas.Clear ();
			for (int k = 0; k < tokens.Count; k++) {
				if (tokens [k].type == TextTokenType.HEAD_H) {
					overlying++;
					for (int i = k + 1; i < tokens.Count; i++) {
						if (tokens [i].type == TextTokenType.HEAD_H) {
							overlying++;
						} else if (tokens [i].type == TextTokenType.END_H) {
							overlying--;
							if (overlying == 0) {
								HyperlinkDatas hylink = new HyperlinkDatas();
								//解析buff 内容
								int idxHref = tokens[k].buffer.IndexOf ("h=");
								int idxValue = tokens[k].buffer.IndexOf ("v=");

								if (idxHref > -1) {
									hylink.href = RichText.UF_ReadString(tokens [k].buffer, idxHref + 2);
								}
								if (idxValue > -1) {
									hylink.value = RichText.UF_ReadString(tokens [k].buffer, idxValue + 2);
								}
								hylink.idx = tokens [k].index;
								hylink.length = tokens [i].index - tokens [k].index;

								listHyperlinkDatas.Add (hylink);
								//嵌套类型只有最外层有效
								k = i;
								break;
							}
						}
					}
				}
			}
		}

		//解析获取用于射线碰撞检测的Rect Box
		private static void UF_HandleMesh(List<UIVertex> uivertexs,List<HyperlinkDatas> listHyperlinkDatas,int startIndex)
        {
			if (listHyperlinkDatas.Count == 0)
				return;

			if (uivertexs.Count == 0 || listHyperlinkDatas.Count == 0)
				return;

			Vector2 uvPoint = Vector2.zero;

			int charCount = uivertexs.Count / 6;



			for (int k = 0; k < listHyperlinkDatas.Count; k++) {

				HyperlinkDatas hylink = listHyperlinkDatas [k];

				if (hylink.idx + hylink.length > charCount) {
					break;
				}

				Vector2 startUp = uivertexs [hylink.idx * 6].position;
				Vector2 lastBottom = uivertexs [hylink.idx * 6 + 3].position;

//				Rect[] boxrects = new Rect[hylink.length];

				List<Rect> tmpboxrects = ListCache<Rect>.Acquire ();
				for (int j = startIndex; j < hylink.length; j++) {
					int idx = (hylink.idx + j) * 6;
					Vector2 pmin = uivertexs [idx].position;
					Vector2 pmax = uivertexs [idx + 3].position;
                    //分段
                    if (Mathf.Abs (lastBottom.y - pmax.y) < 10 && Mathf.Abs(startUp.y - pmin.y) < 10) {
						lastBottom = pmax;
					} else {
						tmpboxrects.Add (new Rect (startUp.x, startUp.y, lastBottom.x - startUp.x, startUp.y - lastBottom.y));
						startUp = pmin;
						lastBottom = pmax;
                    }
//					boxrects[j] = new Rect (pmin.x, pmin.y,  pmax.x - pmin.x, pmin.y - pmax.y);
				}
				tmpboxrects.Add (new Rect (startUp.x, startUp.y, lastBottom.x - startUp.x, startUp.y - lastBottom.y));

//				hylink.box = boxrects;
				hylink.box = tmpboxrects.ToArray();

				ListCache<Rect>.Release(tmpboxrects);

				listHyperlinkDatas [k] = hylink;
			}
		}


		public static void UF_OnClick(UILabel label,PointerEventData eventData){
			if (s_HyperlinkCacheTable.Count == 0)
				return;
			int labelID = label.GetInstanceID ();
			RectTransform rectTransform = label.rectTransform; 
			List<HyperlinkDatas> listHyperlinkDatas = s_HyperlinkCacheTable [labelID];
			if (listHyperlinkDatas.Count > 0) {
				Vector2 point;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out point);
				for (int k = 0; k < listHyperlinkDatas.Count; k++) {
					Rect[] boxrects = listHyperlinkDatas [k].box;
					if (boxrects != null) {
						for (int j = 0; j < boxrects.Length; j++) {
							if ((boxrects [j].x < point.x && (boxrects [j].x + boxrects [j].width) > point.x) && (boxrects [j].y > point.y && (boxrects [j].y - boxrects [j].height) < point.y)) {
                                //Debug.Log(string.Format("Event: {0}  {1}", listHyperlinkDatas[k].href, listHyperlinkDatas[k].value));
                                MessageSystem.UF_GetInstance ().UF_Send(DefineEvent.E_UI_OPERA, listHyperlinkDatas [k].href, listHyperlinkDatas [k].value);
								return;
							}
						}
					}
				}
			}
		}


		public static void UF_OnReset(UILabel label){
			if (s_HyperlinkCacheTable.Count == 0)
				return;
			int labelID = label.GetInstanceID ();
			if (s_HyperlinkCacheTable.ContainsKey(labelID)) {
				ListPool<HyperlinkDatas>.Release (s_HyperlinkCacheTable [labelID]);
				s_HyperlinkCacheTable.Remove (labelID);
			}
		}


	}
}

