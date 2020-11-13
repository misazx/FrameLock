using UnityEngine;
using UnityEngine.UI;
using System.IO;
using NetServiceImpl.Client.Data;
using NetServiceImpl;
using NetServiceImpl.Client;
using Src.Replays;
using Src.Log;
using Components;
using EntitySystems;
using Renderers;
using LogicFrameSync.Src.LockStep.Frame;
using System.Collections.Concurrent;
using Entitas;
using System.Collections.Generic;
using Unity.Mathematics;
using System;
using System.Collections;
using com.unity.mgobe.src.Util;
using LogicFrameSync.Src.LockStep;

using LogicFrameSync.Src.LockStep.Behaviours;
using UnityFrame;
using Debugger = UnityFrame.Debugger;

public class PanelCreation : MonoBehaviour
{
    public Button m_BtnReady;
    public Button m_BtnLeave;
    public UIGrid grid;



    void Start()
    {
        m_BtnReady.onClick.AddListener(OnClickReady);
        m_BtnLeave.onClick.AddListener(OnClickLeave);

        MessageSystem.UF_GetInstance().UF_AddListener(DefineEvent.E_ROOM_INFO_UPDATE, OnInforUpdate);

        Refresh();
    }

    private void OnDestroy()
    {
        MessageSystem.UF_GetInstance().UF_RemoveListener(DefineEvent.E_ROOM_INFO_UPDATE, OnInforUpdate);

    }

    void RefreshReadyBtn()
    {
        var data = MgobeHelper.GetPlayerById(MgobeHelper.PlayerId);
        if (data != null)
        {
            m_BtnReady.GetComponentInChildren<Text>().text = data.CustomPlayerStatus == 0 ? "准备" : "取消准备";
        }
    }

    void OnInforUpdate(object[] args)
    {
        Refresh();
    }

    void Refresh()
    {
        RefreshReadyBtn();

        grid.UF_OnReset();
        var list = MgobeHelper.PlayerList;
        for (int i = 0; i < list.Count; i++)
        {
            var info = list[i];
            var item = grid.UF_GenUI().rectTransform.GetComponent<UIItem>();
            item.UF_GetUI("lb_name").UF_SetValue(info.Name);
            item.UF_GetUI("lb_status").UF_SetValue(info.CustomPlayerStatus==0?"":"已准备");
            var btn = item.UF_GetUI("bt_change") as UIButton;
            
        }
    }


    void OnClickReady()
    {
        MgobeHelper.SetReadyToBattle(RefreshReadyBtn);
    }
    
    void OnLeaveRoom()
    {
        //UIManager.UF_GetInstance().UF_CloseView("ui_PanelLan");
        UIManager.UF_GetInstance().UF_ShowView("UI System", "ui_panellan", null);

    }

    void OnClickLeave()
    {
        var data = MgobeHelper.GetPlayerById(MgobeHelper.PlayerId);
        if (data.CustomPlayerStatus == 1)
        {
            return;
        }
        MgobeHelper.LeaveRoom(OnLeaveRoom);
    }

}
