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
    //<o=#FFFFFFFF v=x,y> 内容 </o>
    //#: 颜色
    //v: 偏移值x,y
    public static class RTShadow
    {
        //超链接记录的索引
        struct ShadowData
        {
            //起始索引
            public int idx;
            //长度
            public int length;
            public Color32 color;
            public Vector2 offset;
        }

        public static void UF_OnPopulateMesh(UILabel label, List<TextToken> tokens, List<UIVertex> uivertexs,int startIndex = 0)
        {
            List<ShadowData> listShadowDatas = ListCache<ShadowData>.Acquire();
            UF_HandleToken(tokens, listShadowDatas);
            UF_HandleMesh(uivertexs, listShadowDatas, startIndex);
            ListCache<ShadowData>.Release(listShadowDatas);
        }



        private static void UF_HandleToken(List<TextToken> tokens, List<ShadowData> listShadowDatas)
        {
            int overlying = 0;
            listShadowDatas.Clear();
            for (int k = 0; k < tokens.Count; k++)
            {
                if (tokens[k].type == TextTokenType.HEAD_S)
                {
                    overlying++;
                    for (int i = k + 1; i < tokens.Count; i++)
                    {
                        if (tokens[i].type == TextTokenType.HEAD_S)
                        {
                            overlying++;
                        }
                        else if (tokens[i].type == TextTokenType.END_S)
                        {
                            overlying--;
                            if (overlying == 0)
                            {
                                ShadowData ldData = new ShadowData();
                                //解析buff 内容
                                int idxHref = tokens[k].buffer.IndexOf("#", System.StringComparison.Ordinal);
                                int idxValue = tokens[k].buffer.IndexOf("v=", System.StringComparison.Ordinal);
                                ldData.idx = tokens[k].index;
                                ldData.length = tokens[i].index - tokens[k].index;

                                ldData.color = new Color32(0, 0, 0, 255);
                                ldData.offset = new Vector2(1, 1);
                                if (idxHref > -1)
                                {
                                    ldData.color = RichText.UF_ReadColor(tokens[k].buffer, idxHref + 1);
                                }
                                if (idxValue > -1)
                                {
                                    ldData.offset = RichText.UF_ReadVector2(tokens[k].buffer, idxValue + 2);
                                }
                                listShadowDatas.Add(ldData);
                                //嵌套类型只有最外层有效
                                k = i;
                                break;
                            }
                        }
                    }
                }
            }
        }

        //阴影必须放在最后
        //比描边还
        private static void UF_HandleMesh(List<UIVertex> uivertexs, List<ShadowData> listShadowDatas,int startIndex)
        {
            if (uivertexs.Count == 0 || listShadowDatas.Count == 0)
                return;

            //Vector2 uvPoint = Vector2.zero;

            int charCount = uivertexs.Count / 6;

            UIVertex[] rawUIVeterxs = uivertexs.ToArray();
            uivertexs.Clear();
            for (int k = 0; k < listShadowDatas.Count; k++)
            {
                ShadowData ldData = listShadowDatas[k];
                if (ldData.idx + ldData.length > charCount)
                {
                    break;
                }
                UF_PopulateShadowMesh(uivertexs, rawUIVeterxs, startIndex + ldData.idx * 6, ldData.length * 6, ldData.offset, ldData.color);
            }
            uivertexs.AddRange(rawUIVeterxs);
        }


        //处理全部顶点，添加Shadow mesh
        public static void UF_HandleMesh(List<UIVertex> uivertexs, Color color, Vector2 offset, int startIndex)
        {
            if (uivertexs.Count == 0)
            {
                return;
            }
            UIVertex[] rawUIVeterxs = uivertexs.ToArray();
            uivertexs.Clear();
            UF_PopulateShadowMesh(uivertexs, rawUIVeterxs, startIndex, rawUIVeterxs.Length, offset, color);
            uivertexs.AddRange(rawUIVeterxs);
        }



        private static UIVertex UF_fillShadowVertex(UIVertex value, Vector2 offset, Color ShadowColor)
        {
            Vector3 pos = value.position;
            pos.x += offset.x;
            pos.y += offset.y;
            //Mark！ 设置该值会影响Batch 合批
            //pos.z += 0.01f;
            Color color = ShadowColor;
            value.position = pos;
            color.a = (value.color.a * 0.00392157f) * ShadowColor.a;
            value.color = color;
            return value;
        }

        //文字秒边
        public static void UF_PopulateShadowMesh(List<UIVertex> uivertexs, UIVertex[] rawUIVeterxs, int idx, int length, Vector2 offset, Color32 olColor)
        {
            int loopnum = Mathf.Min(idx + length, rawUIVeterxs.Length);

            for (int k = idx; k < loopnum;)
            {
                UIVertex vertex1 = UF_fillShadowVertex(rawUIVeterxs[k], offset, olColor); uivertexs.Add(vertex1);
                UIVertex vertex2 = UF_fillShadowVertex(rawUIVeterxs[k + 1], offset, olColor); uivertexs.Add(vertex2);
                UIVertex vertex3 = UF_fillShadowVertex(rawUIVeterxs[k + 2], offset, olColor); uivertexs.Add(vertex3);
                UIVertex vertex4 = UF_fillShadowVertex(rawUIVeterxs[k + 3], offset, olColor); uivertexs.Add(vertex4);
                UIVertex vertex5 = UF_fillShadowVertex(rawUIVeterxs[k + 4], offset, olColor); uivertexs.Add(vertex5);
                UIVertex vertex6 = UF_fillShadowVertex(rawUIVeterxs[k + 5], offset, olColor); uivertexs.Add(vertex6);
                k += 6;
            }
        }


    }
}

