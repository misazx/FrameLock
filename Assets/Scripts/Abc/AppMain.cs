using UnityEngine;
using UnityEngine.UI;
using LogicFrameSync.Src.LockStep;

using LogicFrameSync.Src.LockStep.Behaviours;

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
using com.unity.mgobe;
using UnityFrame;

public class AppMain : MonoBehaviour
{
    public enum Notifications
    {
        ReadyPlayerAndAdd,
    }

    public static AppMain INS;

    // Use this for initialization
    IEnumerator Start()
    {
        INS = this;

        MessageSystem.UF_GetInstance().UF_AddListener(DefineEvent.E_START_BATTLE, OnAllReady);

        yield return StartCoroutine(AssetSystem.UF_GetInstance().UF_InitAssetSystem());

        RandomUtil.Init(int.Parse(GetTimeStamp()));

        AllUI.Instance.Show("PanelLogin");
    }

    public static string GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt32(ts.TotalSeconds).ToString();
    }

    public IEnumerator OnLogined(string account)
    {
        //Debugger.Enable = true;
        Global.OpenId = account;
        Debug.Log(Global.OpenId);
        yield return StartCoroutine(MgobeHelper.InitSDK());

        if (Global.IsInRoom())
        {
            AllUI.Instance.Show("PanelCreation");
        }
        else
        {
            AllUI.Instance.Show("PanelLan");
        }
    }
    
    public void OnAllReady(object[] args)
    {
        AllUI.Instance.Show("PanelGame");
    }


   
    void Update()
    {
        MgobeHelper.Update();

    }

    private void OnApplicationQuit()
    {
        SimulationManager.Instance.Stop();
    }
}

