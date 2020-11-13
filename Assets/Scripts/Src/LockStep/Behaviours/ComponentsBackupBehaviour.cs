﻿
using LogicFrameSync.Src.LockStep.Frame;
using LogicFrameSync.Src.LockStep.Net.Pt;
using NetServiceImpl;
using NetServiceImpl.Client;
using System.Collections.Generic;
namespace LogicFrameSync.Src.LockStep.Behaviours
{
    /// <summary>
    /// 组件内容备份
    /// </summary>
    public class ComponentsBackupBehaviour : ISimulativeBehaviour
    {
        public bool IsActive { get => isActive; set => isActive = value; }
        private bool isActive = true;

        public Simulation Sim
        {
            set;get;
        }
        Dictionary<int, EntityWorldFrameData> m_DictEntityWorldFrameData;
        public ComponentsBackupBehaviour()
        {
            m_DictEntityWorldFrameData = new Dictionary<int, EntityWorldFrameData>();      
        }
        public EntityWorldFrameData GetEntityWorldFrameByFrameIdx(int frameIdx)
        {
            if (m_DictEntityWorldFrameData.ContainsKey(frameIdx))
                return m_DictEntityWorldFrameData[frameIdx];
            return null;
        }

        Queue<int> QueueFrameCache = new Queue<int>();
        public void SetEntityWorldFrameByFrameIdx(int frameIdx, EntityWorldFrameData data)
        {
            QueueFrameCache.Enqueue(frameIdx);
            if (m_DictEntityWorldFrameData.ContainsKey(frameIdx))
                m_DictEntityWorldFrameData[frameIdx].Clear();
            m_DictEntityWorldFrameData[frameIdx]= data; 

            //while(QueueFrameCache.Count>100)
            //{
            //    int fid = QueueFrameCache.Dequeue();
            //    if (m_DictEntityWorldFrameData.ContainsKey(fid))
            //        m_DictEntityWorldFrameData.Remove(fid);
            //}
        }
        public Dictionary<int, EntityWorldFrameData> GetEntityWorldFrameData()
        {
            return m_DictEntityWorldFrameData;
        }

        public void Quit()
        {
            
        }

        public void Start()
        {
            
        }
        void SendKeyFrame(int idx)
        {
            PtKeyFrameCollection collection = KeyFrameSender.GetFrameCommand();
            if(collection.KeyFrames.Count>0)
            {
                //Service.Get<LoginService>().RequestSyncClientKeyframes(idx, collection);
                collection.FrameIdx = idx;
                MgobeHelper.SendFrame(collection);
                KeyFrameSender.ClearFrameCommand();
            }        
        }
        public void Update()
        {
            var logic = Sim.GetBehaviour<LogicFrameBehaviour>();
            int frameIdx = logic.CurrentFrameIdx;

            SetEntityWorldFrameByFrameIdx(frameIdx, new EntityWorldFrameData(Sim.GetEntityWorld().FindAllEntitiesIds(),
                                                                             Sim.GetEntityWorld().FindAllCloneComponents()));
            SendKeyFrame(frameIdx);
        }
    }
}
