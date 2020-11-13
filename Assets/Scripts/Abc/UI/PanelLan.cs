using Components;
using Entitas;
using EntitySystems;
using LogicFrameSync.Src.LockStep;
using LogicFrameSync.Src.LockStep.Behaviours;
using LogicFrameSync.Src.LockStep.Frame;
using NetServiceImpl.Client.Data;
using Src.Log;
using Src.Replays;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityFrame;

public class PanelLan : MonoBehaviour
{
    public Button m_BtnCreate;
    public Button m_BtnRefresh;
    public Button m_BtnJoin;
    public UIGrid grid;
    public Button m_BtnPlayReplay;

    public InputField m_Field;

    // Use this for initialization
    void Start()
    {
        m_BtnCreate.onClick.AddListener(()=> 
        {
            MgobeHelper.CreateRoom(SwitchPanel);
        });
        m_BtnRefresh.onClick.AddListener(() =>
        {
            MgobeHelper.RefreshRoomList(RefreshRoomList);
        });
        m_BtnPlayReplay.onClick.AddListener(OnClickPlayReplay);

        MessageSystem.UF_GetInstance().UF_AddListener(DefineEvent.E_UI_OPERA, OnClick);


        MgobeHelper.RefreshRoomList(RefreshRoomList);

    }

    private void OnDestroy()
    {
        MessageSystem.UF_GetInstance().UF_RemoveListener(DefineEvent.E_UI_OPERA, OnClick);

    }

    void OnClick(object[] args)
    {
        if (args[0].ToString() == "RoomList")
        {
            int idx = int.Parse((string)args[1]) ;
            MgobeHelper.JoinRoom(idx, SwitchPanel);
        }
    }

    void SwitchPanel()
    {
        //AllUI.Instance.Show("PanelCreation");
        UIManager.UF_GetInstance().UF_ShowView("UI System", "ui_panelcreation", null);

    }
    void RefreshRoomList()
    {
        grid.UF_OnReset();
        var list = MgobeHelper.RoomList;
        for (int i = 0; i < list.Count; i++)
        {
            var info = list[i];
            var item = grid.UF_GenUI().rectTransform.GetComponent<UIItem>();
            item.UF_GetUI("lb_name").UF_SetValue(info.Name);
            item.UF_GetUI("lb_count").UF_SetValue(info.PlayerList.Count + "/"+info.MaxPlayers);
            var btn = item.UF_GetUI("bt_join").rectTransform.GetComponent<UIButton>();
            btn.eParamA = i.ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnClickPlayReplay()
    {
        Simulation sim = new Simulation(Const.CLIENT_SIMULATION_ID);
        var bytes = File.ReadAllBytes(Application.dataPath + "/replay_client_" + GameClientData.SelfPlayer.Id + ".rep");
        var info = ReplayInfo.Read(bytes);
        sim.AddBehaviour(new ReplayLogicFrameBehaviour());
        sim.AddBehaviour(new EntityBehaviour());
        sim.AddBehaviour(new ReplayInputBehaviour());
        EntityMoveSystem moveSystem = new EntityMoveSystem();
        FrameClockSystem frameClock = new FrameClockSystem();
        EntityCollisionSystem colliderSystem = new EntityCollisionSystem();
        RemoveEntitySystem removeSystem = new RemoveEntitySystem();
        sim.GetBehaviour<EntityBehaviour>().
            AddSystem(moveSystem).
            AddSystem(frameClock).
            AddSystem(colliderSystem).
            AddSystem(removeSystem);
        sim.GetBehaviour<ReplayLogicFrameBehaviour>().SetFrameIdxInfos(info.Frames);

        SimulationManager.Instance.AddSimulation(sim);
        SimulationManager.Instance.Start();
    }

}
