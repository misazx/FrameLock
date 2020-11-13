using com.unity.mgobe;
using com.unity.mgobe.src.Util;
using Components;
using Entitas;
using EntitySystems;
using LogicFrameSync.Src.LockStep;
using LogicFrameSync.Src.LockStep.Behaviours;
using LogicFrameSync.Src.LockStep.Frame;
using NetServiceImpl.Client.Data;
using Src.Log;
using Src.Replays;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityFrame;
using Debugger = UnityFrame.Debugger;

public class PanelGame : MonoBehaviour
{
    public Text m_TxtDebug;
    public Button m_BtnAddEntity;
    public Button m_BtnAddBoxEntity;

    public Button m_BtnStop;

    StringBuilder sb = new StringBuilder();


    private void Start()
    {
        m_BtnAddEntity.onClick.AddListener(OnClickAddEntity);
        m_BtnAddBoxEntity.onClick.AddListener(OnClickBoxEntity);
        m_BtnStop.onClick.AddListener(OnClickStop);


        //资源加载完毕开始 同步
        MgobeHelper.StartFrameSync();
    }

    private void OnDestroy()
    {

    }

    void Update()
    {
        lock (EntityWorld.SyncRoot)
        {
            sb.Clear();

            Simulation sim = SimulationManager.Instance.GetSimulation(Const.CLIENT_SIMULATION_ID);
            if (sim != null)
            {
                var world = sim.GetEntityWorld();
                if (world == null) return;
                if (!world.IsActive) return;
                var entities = world.GetEntities();
                for (int i = 0; i < entities.Count; ++i)
                {
                    var e = entities[i];
                    if (!e.IsActive) continue;
                    TransformComponent posComp = e.GetComponent<TransformComponent>();
                    if (posComp != null)
                        sb.Append(string.Format("EntityId {0} Position:{1}", e.Id, posComp.ToString()) + "\n");
                }
                sb.Append("Message count:" + MgobeHelper.KeyframesCount + "\n");
                sb.Append("Keyframe count: " + MgobeHelper.AllFramesCount + "\n");
                sb.Append("FrameIdx:" + sim.GetBehaviour<LogicFrameBehaviour>().CurrentFrameIdx);
            }

            m_TxtDebug.text = sb.ToString();
        }
    }

    void OnClickAddEntity()
    {
        GameClientData.SelfControlEntityId = System.Guid.NewGuid();
        KeyFrameSender.AddCurrentFrameCommand(FrameCommand.SYNC_CREATE_ENTITY, GameClientData.SelfControlEntityId.ToString(), new string[] { ((int)EntityWorld.EntityOperationEvent.CreatePlayer) + "" });
    }

    void OnClickBoxEntity()
    {
        System.GC.Collect();
        KeyFrameSender.AddCurrentFrameCommand(FrameCommand.SYNC_CREATE_ENTITY, Common.Utils.GuidToString(), new string[] { (int)EntityWorld.EntityOperationEvent.CreateBox + "", new System.Random().Next(3, 15).ToString(), new System.Random().Next(-1, 2).ToString(), new System.Random().Next(-1, 2).ToString() });
    }

    void OnClickStop()
    {
        MgobeHelper.StopFrameSync();
    }
}
