﻿////-----------------------------------------------------------
//// Copyright (c) 2017-2019 chanjanequan
////-----------------------------------------------------------

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;


//namespace UnityFrame {
//    //用于存储Perform数据
//    public class PerformPackage : MonoBehaviour
//    {
//        public List<PerformActionClip> listClips { get { return m_ListClips; } }
//        [SerializeField]List<PerformActionClip> m_ListClips = new List<PerformActionClip>();

//        Dictionary<string, PerformActionClip> m_MapPerforms;

//        public PerformActionClip Get(string strName)
//        {
//            if (m_MapPerforms == null)
//            {
//                m_MapPerforms = new Dictionary<string, PerformActionClip>();
//                //映射数据
//                foreach (var v in m_ListClips)
//                {
//                    m_MapPerforms.Add(v.name, v);
//                }
//            }

//            if (m_MapPerforms.ContainsKey(strName))
//            {
//                return m_MapPerforms[strName];

//            }
//            else
//            {
//                foreach (var v in m_ListClips)
//                {
//                    if (v.name == strName)
//                        return v;
//                }
//            }

//            return null;
//        }
//    }

//}
