//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFrame.Assets {
    [System.Serializable]
    public struct SecenLayoutData
    {
        public int x, y;
        public float rotation;
        public string block;
        public string param;

    }
    [CreateAssetMenu(menuName = "UnityFrame/Asset Scene Layout", order = 100)]
    public class AssetSceneLayout : ScriptableObject
    {
        public int mapRow = 10;
        public int mapCol = 10;

        [SerializeField] private List<SecenLayoutData> m_cellDataList = new List<SecenLayoutData>();
        private Dictionary<int, Dictionary<int, int>> m_dataWrap = null;

        protected Dictionary<int, Dictionary<int, int>> dataWrap {
            get {
                if (m_dataWrap == null) {
                    this.m_dataWrap = new Dictionary<int, Dictionary<int, int>>();
                    for (int i = 0; i < this.m_cellDataList.Count; i++)
                    {
                        var temp = this.m_cellDataList[i];
                        Dictionary<int, int> tempSub = null;
                        if (!this.m_dataWrap.ContainsKey(temp.x))
                        {
                            tempSub = new Dictionary<int, int>();
                            this.m_dataWrap.Add(temp.x, tempSub);
                        }
                        tempSub.Add(temp.y, i);
                    }
                }
                return m_dataWrap;
            }
        }

        public List<SecenLayoutData> Data
        {
            get { return this.m_cellDataList; }
            set { this.m_cellDataList = value; }
        }
        public SecenLayoutData UF_GetCell(int index)
        {
            return this.m_cellDataList[index];
        }
        public SecenLayoutData UF_GetCell(int x, int y)
        {
            int index = this.m_dataWrap[x][y];
            return this.m_cellDataList[index];
        }

        public string UF_GetBlock(int x, int y)
        {
            return this.UF_GetCell(x, y).block;
        }
    }
}

