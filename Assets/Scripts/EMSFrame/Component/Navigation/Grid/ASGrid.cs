//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace UnityFrame
{

    internal class ASGrid {
        public int width { get; private set; }
        public int height { get; private set; }
        public int count { get; private set; }

        //内置Mesh data对象池
        static ObjectPool<ASGridData> s_GridDataPool = new ObjectPool<ASGridData>();

        private List<ASGridData> m_ListGridDatas = new List<ASGridData>();

        public ASGridData this[int index]
        {
            get {
                index = Mathf.Abs(index);
                if (index < m_ListGridDatas.Count)
                    return m_ListGridDatas[index];
                else
                    return null;
            }
        }

        public ASGrid() {}

        public ASGrid(int w,int h) { this.UF_Reset(w, h); }

        private static ASGridData UF_AcquireASGridData()
        {
            ASGridData data = s_GridDataPool.Get();
            if (data == null)
            {
                data = new ASGridData();
            }
            return data;
        }

        private static void UF_ReleaseASGridData(ASGridData data)
        {
            if (data != null)
            {
                s_GridDataPool.Release(data);
            }
        }


        public void UF_Reset(int w,int h)
        {
            width = w;
            height = h;
            count = w * h;
            for (int k = 0; k < m_ListGridDatas.Count; k++) {
                m_ListGridDatas[k].UF_Reset();
                UF_ReleaseASGridData(m_ListGridDatas[k]);
            }
            m_ListGridDatas.Clear();
            for (int k = 0; k < count; k++) {
                var data = UF_AcquireASGridData();
                data.X = k % width;
                data.Y = k / width;
                m_ListGridDatas.Add(data);
            }
        }


        public ASGridData UF_GetData(int x, int y)
        {
            int idx = y * width + x;
            if (idx >= 0 && idx < m_ListGridDatas.Count)
            {
                return m_ListGridDatas[idx];
            }
            else
            {
                Debugger.UF_Error(string.Format("GetData Out of Index:{0}  But Count:{1}   PosX:{2}  PosY{3}", idx, this.count, x, y));
                return null;
            }
        }

        public byte UF_GetState(int x,int y) {
            var data = UF_GetData(x, y);
            return (byte)(data != null ? data.State : -1);
        }

        public void UF_SetState(int x, int y, byte state) {
            var data = UF_GetData(x, y);
            if (data != null) {
                data.State = state;
            }
        }

        public void UF_FlushState(byte state) {
            foreach (var item in m_ListGridDatas) {
                item.State = state;
            }
        }


        /// 是否在地图范围内
        private bool UF_IsInMap(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

    }






    internal class ASGridData
    {
		public int X;
		public int Y;
        /// 相邻格子数据，表示方式为线性索引
        public int[] Neibourhood;
        /// 与起点的长度
        public int gCost;
        /// 与目标点的长度
        public int hCost;
        //格子状态
        public byte State;
        //父节点
        public ASGridData parent { get; set; }
        //总长度
        public int fCost{get { return gCost + hCost; }}

        public ASGridData(){}

        public ASGridData(int x, int y, byte state,int width,int height)
        {
            UF_Reset(x, y, state, width, height);
        }

        public void UF_Reset(int x, int y, byte state,int width,int height){
            this.X = x;
            this.Y = y;
            this.State = state;
            Neibourhood = null;
        }

        public void UF_Reset() {
            this.X = 0;
            this.Y = 0;
            this.State = 0;
            Neibourhood = null;
        }

        public void UF_GenNeibourhood(int width, int height){
            var tmplist = ListCache<int>.Acquire();
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    // 如果是自己，则跳过
                    if (i == 0 && j == 0)
                        continue;
                    int x = X + i;
                    int y = Y + j;
                    // 判断是否越界，如果没有，加到列表中
                    if (!(x >= 0 && x < width && y >= 0 && y < height))
                        continue;

                    tmplist.Add((int)(y * width + x));
                }
            }
            Neibourhood = tmplist.ToArray();
            ListCache<int>.Release(tmplist);
        }

	
        public static byte UF_WrapCharToSate(char value){
            switch (value) {
                case '0':return 0;
                case '1':return 1;
                case '2':return 2;
                case '3':return 3;
                case '4':return 4;
                case '5':return 5;
                case '6':return 6;
                case '7':return 7;
                case '8':return 8;
                case '9':return 9;
                default:
                    return 0;
            }
        }

    }
}
