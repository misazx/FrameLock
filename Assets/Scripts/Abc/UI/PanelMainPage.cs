﻿using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using NetServiceImpl.Server.Data;
using Net;
using System;
using LogicFrameSync.Src.LockStep;
using LogicFrameSync.Src.LockStep.Behaviours;
using Unity.Mathematics;
using System.Collections.Generic;
using Components;

public class PanelMainPage : MonoBehaviour
{
    public Button m_BtnLan;
    public Button m_BtnServer;


    // Use this for initialization
    void Start()
    {
        m_BtnLan.onClick.AddListener(() => {
            gameObject.SetActive(false);
            GameClientNetwork.Instance.Connect();
        });

        m_BtnServer.onClick.AddListener(()=> {
            GameServerNetwork.Instance.Start();

            AddServerSim();
        });
    }

    private void AddServerSim()
    {
        Simulation sim = new Simulation(Const.SERVER_SIMULATION_ID);
        sim.AddBehaviour(new ServerLogicFrameBehaviour());
        SimulationManager.Instance.AddSimulation(sim);
    }

  
    // Update is called once per frame
    void Update()
    {
        
    }
}


