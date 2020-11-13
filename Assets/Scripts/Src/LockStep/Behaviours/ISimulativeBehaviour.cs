﻿
namespace LogicFrameSync.Src.LockStep.Behaviours
{
    /// <summary>
    /// 模拟器行为接口
    /// </summary>
    public interface ISimulativeBehaviour
    {
        bool IsActive { get; set; }

        Simulation Sim { set; get; }
        void Start();
        void Update();
        void Quit();
    }
}
