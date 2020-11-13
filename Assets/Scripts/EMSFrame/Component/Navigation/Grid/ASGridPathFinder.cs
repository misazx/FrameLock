//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------


using System;
using UnityEngine;
using System.Collections.Generic;

namespace UnityFrame
{

    /// <summary>
    /// 格子地图寻路
    /// </summary>
    internal static class ASGridPathFinder
    {

        [ThreadStatic] static List<ASGridData> m_LOpen = new List<ASGridData>();
        [ThreadStatic] static HashSet<ASGridData> m_LClose = new HashSet<ASGridData>();

        static bool UF_Contains(List<ASGridData> list,ASGridData target){
			int index = list.Count - 1;
			for (int k = index; k > -1; k--) {
				if (list [k] == target) {
					return true;
				}
			}
			return false;
		}


        static bool UF_CanPass(int state) {
            return state == 0;
        }


        static public void UF_FindingPath(ASGrid asGrid, int startX, int startY, int endX, int endY, List<Vector2> outGrids,bool normalized) {
            List<ASGridData> tempList = ListCache<ASGridData>.Acquire();
            outGrids.Clear();
            UF_FindingPath(asGrid, startX, startY, endX, endY, tempList);

            //normalized 是否泛化同方向点
            //去除同方向点
            if (normalized && tempList.Count > 1)
            {
                int lastForward = 0;
                for (int k = 1; k < tempList.Count; k++)
                {
                    int forward = UF_GetPointForward(tempList[k].X, tempList[k].Y, tempList[k - 1].X, tempList[k - 1].Y);
                    if (forward != lastForward)
                    {
                        lastForward = forward;
                    }
                    else {
                        tempList[k - 1] = null;
                    }
                }
            }


            for (int k = 0; k < tempList.Count; k++) {
                if (tempList[k] != null) {
                    outGrids.Add(new Vector2(tempList[k].X, tempList[k].Y));
                }
            }


            ListCache<ASGridData>.Release(tempList);
        }



        //检查邻居节点合法性
        //*0
        //0*
        //斜方向
        static private bool UF_CheckNeibourhoodValid(ASGrid asGrid, ASGridData a, ASGridData b) {
            int minX = a.X < b.X ? a.X : b.X;
            int minY = a.Y < b.Y ? a.Y : b.Y;
            int vCount = 0;
            int width = asGrid.width - 1;
            int height = asGrid.height - 1;
            for (int k = 0; k < 2; k++) {
                for (int i = 0; i < 2; i++) {
                    if (asGrid.UF_GetState(Math.Min(minX + i, width), Math.Min(minY + k, height)) == 0) {
                        vCount++;
                    }
                }
            }

            return vCount >= 3;
        }

        /// <summary>
        /// 查找线路
        /// </summary>
        /// <returns>如果没有线路 则返回null </returns>
		static public void UF_FindingPath(ASGrid asGrid,int startX, int startY, int endX, int endY,List<ASGridData> outGrids)
        {
            int width = asGrid.width;
            int height = asGrid.height;



            m_LOpen.Clear ();
			m_LClose.Clear ();

			ASGridData startNode = asGrid.UF_GetData(startX, startY);

			ASGridData endNode = asGrid.UF_GetData(endX, endY);

            if (startNode == null || endNode == null) {
                return;
            }

            m_LOpen.Add(startNode);

            while (m_LOpen.Count > 0)
            {
                ASGridData curNode = m_LOpen[0];

                for (int i = 0, max = m_LOpen.Count; i < max; i++)
                {
                    if (m_LOpen[i].fCost <= curNode.fCost &&
                        m_LOpen[i].hCost < curNode.hCost)
                    {
                        curNode = m_LOpen[i];
                    }
                }

                m_LOpen.Remove(curNode);
                m_LClose.Add(curNode);

                // 找到的目标节点
                if (curNode == endNode)
                {
                    UF_GeneratePath(startNode, endNode,outGrids);
					return;
                }

                //未有邻近点，生产并记录
                if (curNode.Neibourhood == null)
                    curNode.UF_GenNeibourhood(width, height);


                // 判断周围节点，选择一个最优的节点GenNeibourhood
                for (int k = 0; k < curNode.Neibourhood.Length; k++) {
					ASGridData tempGrid = asGrid[curNode.Neibourhood [k]];
                    // 如果是墙或者已经在关闭列表中
                    if (!UF_CanPass(tempGrid.State) || m_LClose.Contains(tempGrid))
						continue;

                    //判断斜方向邻居点，共享点必须为可行走区域
                    if (!UF_CheckNeibourhoodValid(asGrid, curNode, tempGrid))
                        continue;

                    // 计算当前相领节点现开始节点距离
                    int newCost = curNode.gCost + UF_GetDistanceNodes(curNode, tempGrid);
					// 如果距离更小，或者原来不在开始列表中
					if (newCost < tempGrid.gCost || !UF_Contains(m_LOpen,tempGrid)) {
						// 更新与开始节点的距离
						tempGrid.gCost = newCost;
						// 更新与终点的距离
						tempGrid.hCost = UF_GetDistanceNodes(tempGrid, endNode);
						// 更新父节点为当前选定的节点
						tempGrid.parent = curNode;
						// 如果节点是新加入的，将它加入打开列表中
						if (!UF_Contains(m_LOpen,tempGrid)) {
							m_LOpen.Add (tempGrid);
						}
					}
				}
            }
            return;
        }
        /// <summary>
        /// 生成路径
        /// </summary>
		static void UF_GeneratePath(ASGridData startNode, ASGridData endNode,List<ASGridData> outGrids)
        {
			outGrids.Clear ();
            if (endNode != null)
            {
                ASGridData temp = endNode;
                while (temp != startNode)
                {
					outGrids.Add(temp);
                    temp = temp.parent;
                }

				outGrids.Add (startNode);
                // 反转路径
				outGrids.Reverse();

            }
        }
        /// <summary>
        /// 获取两个节点之间的距离
        /// </summary>
        static int UF_GetDistanceNodes(ASGridData a, ASGridData b)
        {
            /// 对角线估价法
            int cntX = Math.Abs(a.X - b.X);
            int cntY = Math.Abs(a.Y - b.Y);
            return cntX > cntY ? 14 * cntY + 10 * (cntX - cntY) : 14 * cntX + 10 * (cntY - cntX);


            ////几何估价法
            //int dx = a.X - b.X;
            //int dy = a.Y - b.Y;
            //return (int)(Math.Sqrt(dx * dx + dy * dy) * 10);

            //// 曼哈顿估价法
            //return Math.Abs(a.X - b.X) * 10 + Math.Abs(a.Y + b.Y) * 10;

        }


        //八方向定义
        static int UF_GetPointForward(int Ax,int Ay,int Bx,int By)
        {
            int a = (int)(Ax - Bx);
            int b = (int)(Ay - By);
            if (a == 1 && b == 1)
            {
                return 1;
            }
            else if (a == 0 && b == 1)
            {
                return 2;
            }
            else if (a == -1 && b == 1)
            {
                return 3;
            }
            else if (a == -1 && b == 0)
            {
                return 4;
            }
            else if (a == -1 && b == -1)
            {
                return 5;
            }
            else if (a == 0 && b == -1)
            {
                return 6;
            }
            else if (a == 1 && b == -1)
            {
                return 7;
            }
            else if (a == 1 && b == 0)
            {
                return 8;
            }
            else
            {
                return 0;
            }
        }



    }
}
