﻿using LogicFrameSync.Src.LockStep.Frame;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LogicFrameSync.Src.LockStep.Behaviours
{
    public class LogicFrameBehaviour : ISimulativeBehaviour
    {
        public bool IsActive { get => isActive; set => isActive = value; }
        private bool isActive = true;

        public Simulation Sim { set; get; }
        public int CurrentFrameIdx { set; get; }//{ private set; get; }

        List<List<FrameIdxInfo>> m_FrameIdxInfos;

        public LogicFrameBehaviour()
        {
            m_FrameIdxInfos = new List<List<FrameIdxInfo>>();
        }
        public List<List<FrameIdxInfo>> GetFrameIdxInfos()
        {
            return m_FrameIdxInfos;
        }
        public void Quit()
        {
            
        }
        public void Start()
        {
            CurrentFrameIdx = -1;
        }
        public void UpdateKeyFrameIdxInfoAtFrameIdx(int frameIdx,FrameIdxInfo info)
        {
            info.Idx = frameIdx;
            if (frameIdx >= m_FrameIdxInfos.Count)
                throw new Exception("Error "+frameIdx);
            List<FrameIdxInfo> frames = m_FrameIdxInfos[frameIdx];
            bool updateState = false;
            foreach (FrameIdxInfo keyframe in frames)
            {
                if (keyframe.EqualsInfo(info))
                {
                    updateState = true;
                    keyframe.Params = info.Params;
                    break;
                }  
            }
            if (!updateState)
                frames.Add(info);
            frames.Sort((a, b) => a.EntityId.CompareTo(b.EntityId));
        }
        public void Update() 
        {
            ++CurrentFrameIdx;
            m_FrameIdxInfos.Add(new List<FrameIdxInfo>());
        }
    }
}
