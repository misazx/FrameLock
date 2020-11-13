﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityFrame;

namespace LogicFrameSync.Src.LockStep
{
    public class SimulationManager:IFrameHandle, IOnUpdate
    {
        static SimulationManager ins;
        Stopwatch m_StopWatch;
        List<Simulation> m_Sims;
        double m_Accumulator = 0;
        double m_FrameMsLength = 40;
        double m_FrameLerp = 0;
        public double GetFrameMsLength() { return m_FrameMsLength; }
        public void SetFrameMsLength(double val) { m_FrameMsLength = val; }

        public double GetFrameLerp() { return m_FrameLerp; }

        public bool IsStart() { return !m_StopState; }

        bool m_StopState = true;//false;
        public static SimulationManager Instance
        {
            get{
                if (ins == null) ins = new SimulationManager();
                return ins;
            }         
        }


        private SimulationManager()
        {
            m_StopWatch = new Stopwatch();
            m_Sims = new List<Simulation>();           
        }
        /// <summary>
         /// 得到流逝的毫秒
         /// </summary>
         /// <returns></returns>
        public double GetElapsedTime()
        {
            double time = m_StopWatch.Elapsed.TotalMilliseconds;
            m_StopWatch.Restart();
            //if (time > m_FrameMsLength)
            //    time = m_FrameMsLength;
            return time;
        }

        public void Start(ulong time=0)
        {
            foreach (Simulation sim in m_Sims)
                sim.Start(time);
            //ThreadPool.QueueUserWorkItem(ThreadPoolRunner);
            m_StopState = false;
            m_StopWatch.Restart();
        }
        public void Stop()
        {
            m_StopState = true;     
        }
        void ThreadPoolRunner(object state)
        {
            Run();
        }

        public void UF_OnUpdate()
        {
            if (!m_StopState)
            {
                m_Accumulator += GetElapsedTime();
                while (m_Accumulator >= m_FrameMsLength)
                {
                    for (int i = 0; i < m_Sims.Count; ++i)
                    {
                        m_Sims[i].Run();
                    }
                    m_Accumulator -= m_FrameMsLength;
                }
                m_FrameLerp = m_Accumulator / m_FrameMsLength;
            }
        }

        void Run()
        {
            while (!m_StopState)
            {
                m_Accumulator += GetElapsedTime();
                while (m_Accumulator >= m_FrameMsLength)
                {
                    for (int i = 0; i < m_Sims.Count; ++i)
                    {
                        m_Sims[i].Run();
                    }
                    m_Accumulator -= m_FrameMsLength;

                }
                m_FrameLerp = m_Accumulator / m_FrameMsLength;
                Thread.Sleep(10);
            }
            
        }
        public void AddSimulation(Simulation sim)
        {
            if (!ContainSimulation(sim))
                m_Sims.Add(sim);
        }
        public void RemoveSimulation(Simulation sim)
        {
            if (ContainSimulation(sim))
                m_Sims.Remove(sim);
        }
        public void RemoveSimulation(byte id)
        {
            Simulation sim = GetSimulation(id);
            if(sim!=null)
                RemoveSimulation(sim);
        }
        public bool ContainSimulation(Simulation sim)
        {
            return m_Sims.Contains(sim);
        }
        public Simulation GetSimulation(byte id)
        {
            for(int i=0;i<m_Sims.Count;++i)
            {
                if (m_Sims[i].GetSimulationId() == id) return m_Sims[i];
            }
            return null;
        }
        public List<Simulation> GetSimulationList()
        {
            return m_Sims;
        }
    }
}
