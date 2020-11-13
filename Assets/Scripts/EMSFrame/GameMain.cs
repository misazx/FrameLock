//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using com.unity.mgobe;
using LogicFrameSync.Src.LockStep;
using NetServiceImpl.Client.Data;
using Src.Replays;
using LogicFrameSync.Src.LockStep.Behaviours;
using Src.Log;
using System.IO;
using EntitySystems;
using System.Text;
using LogicFrameSync.Src.LockStep.Net.Pt;
using System.Collections.Concurrent;

namespace UnityFrame{

	//this gameobject is the root of this game
	public class
        GameMain : MonoBehaviour {

		public static GameObject Root{get{return s_Root;}}
		public static GameMain Instance{get{return s_GameMaim;}}

		private static GameObject s_Root;
		private static GameMain s_GameMaim;

		void Awake(){
			try{
                DontDestroyOnLoad(this);
                s_GameMaim = this;
				s_Root = this.gameObject;

				FrameHandle.UF_AddHandle(Debugger.UF_GetInstance());
				//System
				FrameHandle.UF_AddHandle(MessageSystem.UF_GetInstance());
                FrameHandle.UF_AddHandle(UpgradeSystem.UF_GetInstance());
                //FrameHandle.UF_AddHandle(NetworkSystem.UF_GetInstance());
                FrameHandle.UF_AddHandle(AssetSystem.UF_GetInstance());
				FrameHandle.UF_AddHandle(CEntitySystem.UF_GetInstance());
				//FrameHandle.UF_AddHandle(LuaFramework.UF_GetInstance());
				//manager
				FrameHandle.UF_AddHandle(PDataManager.UF_GetInstance());
                FrameHandle.UF_AddHandle(ShaderManager.UF_GetInstance());
				FrameHandle.UF_AddHandle(AudioManager.UF_GetInstance());
				FrameHandle.UF_AddHandle(UIManager.UF_GetInstance());
                FrameHandle.UF_AddHandle(FXManager.UF_GetInstance());
                FrameHandle.UF_AddHandle(NavigateManager.UF_GetInstance());
                FrameHandle.UF_AddHandle(MotionManager.UF_GetInstance());
                //FrameHandle.UF_AddHandle(PerformActionManager.UF_GetInstance());
                FrameHandle.UF_AddHandle(RaycastManager.UF_GetInstance());
                FrameHandle.UF_AddHandle(VoiceManager.UF_GetInstance());
                FrameHandle.UF_AddHandle(RenderPreviewManager.UF_GetInstance());
                FrameHandle.UF_AddHandle(RefObjectManager.UF_GetInstance());
                FrameHandle.UF_AddHandle(CheckPointManager.UF_GetInstance());

                FrameHandle.UF_AddHandle(SimulationManager.Instance);

                //setting
                GTime.FrameRate = 60;
                GTime.FixedTimeRate = 0.016f;
                GTime.RunDeltaTime = 0.016f;

                Screen.sleepTimeout = SleepTimeout.NeverSleep;
				VendorSDK.UF_Init();
                
                //VestBinder.Bind();
            }
			catch(System.Exception e){
				Debugger.UF_Exception (e);
			}
		}


		// Use this for initialization
		void Start () {
            UF_GameStart();
        }

        //开始游戏
        public void UF_GameStart() {
            this.StartCoroutine(UF_CoGameStart());
        }
        IEnumerator UF_CoGameStart() {
            Debugger.UF_Log("GameMain Start Begain");
            MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_MAIN_PRE_START);
            //获取native 配置
            yield return StartCoroutine(GlobalSettings.UF_InitExternInfo());
            //初始化全局配置表
            yield return StartCoroutine(GlobalSettings.UF_InitGameConfigs());
            //检查版本更新
            yield return StartCoroutine(UpgradeSystem.UF_GetInstance().UF_CheckUpgrade());
            //资源系统初始化
            yield return StartCoroutine(AssetSystem.UF_GetInstance().UF_InitAssetSystem());
            //LuaFramework 初始化
            //yield return StartCoroutine(LuaFramework.UF_GetInstance().UF_InitFramework());
            FrameHandle.UF_OnStart();
            MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_MAIN_START);
            Debugger.UF_Log("GameMain Start Over");

            MessageSystem.UF_GetInstance().UF_AddListener(DefineEvent.E_START_BATTLE, OnAllReady);
            MessageSystem.UF_GetInstance().UF_AddListener(DefineEvent.E_CLIENT_MSG, OnClientMsg);
            MessageSystem.UF_GetInstance().UF_AddListener(DefineEvent.E_StartFrameSync, OnStartFrameSync);
            MessageSystem.UF_GetInstance().UF_AddListener(DefineEvent.E_StopFrameSync, OnStopFrameSync);

            com.unity.mgobe.src.Util.RandomUtil.Init(int.Parse(GetTimeStamp()));

            //AllUI.Instance.Show("PanelLogin");
            UIManager.UF_GetInstance().UF_ShowView("UI System", "ui_panellogin", null);
            yield break;
        }

        //重启游戏
        public void UF_GameReboot()
        {
            this.StartCoroutine(UF_CoGameReboot());
        }
        IEnumerator UF_CoGameReboot()
        {
            Debugger.UF_Log("GameMain Reboot Begain");
            yield return null;
            FrameHandle.UF_OnReset();
            yield return null;
            CEntitySystem.UF_GetInstance().UF_ClearAll();
            yield return null;
            AssetSystem.UF_GetInstance().UF_ClearAll(true);
            Debugger.UF_Log("GameMain Reboot Over");
            yield return null;
            System.GC.Collect();
            yield return null;
            UF_GameStart();
        }

        //Unity Message Event
        void Update () {
            GTime.Update();
            FrameHandle.UF_OnUpdate();
            FrameHandle.UF_OnSecondUpdate();

            MgobeHelper.Update();
        }

        private void FixedUpdate()
        {
            FrameHandle.UF_OnFixedUpdate();
            FrameHandle.UF_OnSyncUpdate();

        }

        //Unity Message Event
        void LateUpdate(){
			FrameHandle.UF_OnLateUpdate ();
		}

		//处理Native返回消息
		void NativeEventMsgReceive(string msg){
			MessageSystem.UF_GetInstance ().UF_HandleNativeMessage(msg);
		}


		//Unity Message Event
		void OnGUI(){
			FrameHandle.OnGUI ();
		}

		//Unity Message Event
		void OnApplicationPause(bool state){
			FrameHandle.OnApplicationPause (state);
			if (state) {
				System.GC.Collect ();
			}
		}

		//Unity Message Event
		void OnApplicationQuit(){
            GlobalSettings.IsApplicationQuit = true;
            FrameHandle.OnApplicationQuit ();
        }


        //--------------------------------------------------------------------------------//
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
                if (MgobeHelper.RoomInfo.FrameSyncState == FrameSyncState.Start)
                {
                    //找一个客户端拿数据
                    //拿完直接显示对应数据,开始sim
                    MgobeHelper.SendToClient(null,RecvType.RoomOthers,"ReConnect",() =>
                    {
                    }); 
                }
                else
                {
                    UIManager.UF_GetInstance().UF_ShowView("UI System", "ui_panelcreation", null);
                }
            }
            else
            {
                UIManager.UF_GetInstance().UF_ShowView("UI System", "ui_panellan", null);
            }
        }

        public void OnAllReady(object[] args)
        {
            UIManager.UF_GetInstance().UF_ShowView("UI System", "ui_panelgame", null);
        }

        private void OnClientMsg(object[] args)
        {
            string playerId = args[0] as string;
            string msg = args[1] as string;
            var msgList= ListCache<string>.Acquire();
            msgList.AddRange(msg.Split('|'));
            var msgType = msgList[0];
            switch (msgType)
            {
                case (ClientMsgType.ReConnect):
                    StringBuilder sb = new StringBuilder();
                    sb.Append( ClientMsgType.ReConnectRsp);
                    sb.Append("|");
                    sb.Append(GetTimeStamp());
                    sb.Append("|");
                    var sim = SimulationManager.Instance.GetSimulation(Const.CLIENT_SIMULATION_ID);
                    var frameDatas = sim.GetBehaviour<ComponentsBackupBehaviour>().GetEntityWorldFrameData();
                    var frameIdx = sim.GetBehaviour<LogicFrameBehaviour>().CurrentFrameIdx;
                    sb.Append(frameIdx);
                    sb.Append("|");
                    var frameData = frameDatas[frameIdx];
                    sb.Append(EntityWorldFrameData.Serilize(frameData));
                    sb.Append("|");
                    var arrKF = MgobeHelper.QueueKeyFrameCollection.ToArray();
                    if (arrKF.Length > 0)
                    {
                        for (int i = 0; i < arrKF.Length; i++)
                        {
                            if (i > 0)
                                sb.Append("&");
                            sb.Append(Encoding.UTF8.GetString(PtKeyFrameCollection.Write(arrKF[i])));
                        }
                    }

                    var list = new List<string>() { playerId };
                    MgobeHelper.SendToClient(list, RecvType.RoomSome, sb.ToString());
                    break;
                case (ClientMsgType.ReConnectRsp):
                    var idx = msgList[1];
                    var timestamp = ulong.Parse(msgList[2]);
                    var frameStr = msgList[3];
                    var kfStr = msgList[4];
                    var entData = EntityWorldFrameData.DeSerilize(frameStr);
                    var queueKeyFrameCollection = new ConcurrentQueue<PtKeyFrameCollection>();
                    if (!string.IsNullOrEmpty(kfStr))
                    {
                        var kfStrs = kfStr.Split('&');
                        for (int i = 0; i < kfStrs.Length; i++)
                        {
                            queueKeyFrameCollection.Enqueue(PtKeyFrameCollection.Read(Encoding.UTF8.GetBytes(kfStrs[i])));
                        }
                    }

                    MgobeHelper.QueueKeyFrameCollection = queueKeyFrameCollection;
                    OnAddClient();
                    var sim2 = SimulationManager.Instance.GetSimulation(Const.CLIENT_SIMULATION_ID);
                    sim2.GetBehaviour<LogicFrameBehaviour>().CurrentFrameIdx = int.Parse(idx) ;
                    sim2.GetEntityWorld().RollBack(entData, null);

                    OnAllReady(null);
                    SimulationManager.Instance.Start(timestamp);
                    break;
                default:
                    break;
            }

            ListCache<string>.Release(msgList);
        }

        void OnStartFrameSync(object[] args)
        {
            OnAddClient();
        }

        void OnStopFrameSync(object[] args)
        {
            OnGameEnd();
        }

        void OnAddClient()
        {
            Debugger.UF_Log("OnAddClient----------------------->>>");
            //add a client simulation 
            Simulation sim = new Simulation(Const.CLIENT_SIMULATION_ID);
            sim.AddBehaviour(new LogicFrameBehaviour());
            sim.AddBehaviour(new RollbackBehaviour());
            sim.AddBehaviour(new EntityBehaviour());
            sim.AddBehaviour(new InputBehaviour());
            //sim.AddBehaviour(new TestRandomInputBehaviour());
            sim.AddBehaviour(new ComponentsBackupBehaviour());
            EntityMoveSystem moveSystem = new EntityMoveSystem();
            FrameClockSystem frameClock = new FrameClockSystem();
            EntityCollisionSystem colliderSystem = new EntityCollisionSystem();
            RemoveEntitySystem removeSystem = new RemoveEntitySystem();
            sim.GetBehaviour<EntityBehaviour>().
                AddSystem(moveSystem).
                AddSystem(frameClock).
                AddSystem(colliderSystem).
                AddSystem(removeSystem);
            sim.GetBehaviour<RollbackBehaviour>().
                AddSystem(moveSystem).
                AddSystem(frameClock);
            //AddSystem(colliderSystem).
            //AddSystem(removeSystem);
            SimulationManager.Instance.AddSimulation(sim);

            //SimulationManager.Instance.Start();
        }

        void OnStopSim()
        {
            Debugger.UF_Log("OnStopSim----------------------->>>");
            SimulationManager.Instance.Stop();
            var sim = SimulationManager.Instance.GetSimulation(Const.CLIENT_SIMULATION_ID);
            ReplayInfo replayInfo = new ReplayInfo();
            replayInfo.OwnerId = GameClientData.SelfPlayer.Id;
            string path = Application.dataPath + "/" + string.Format("replay_client_{0}.rep", GameClientData.SelfPlayer.Id);
            replayInfo.Frames = sim.GetBehaviour<LogicFrameBehaviour>().GetFrameIdxInfos();
            var bytes = ReplayInfo.Write(replayInfo);

            var frameData = sim.GetBehaviour<ComponentsBackupBehaviour>().GetEntityWorldFrameData();
            var outstring = GameEntityWorldLog.Write(frameData, GameClientData.SelfPlayer.Id);

            SimulationManager.Instance.RemoveSimulation(sim);

            Debug.Log("create replay " + path);
            File.WriteAllBytes(path, bytes);
            File.WriteAllText(Application.dataPath + "/" + string.Format("log_client_{0}.txt", GameClientData.SelfPlayer.Id)
        , outstring);
        }

        void OnGameEnd()
        {
            OnStopSim();
        }

        public static class ClientMsgType
        {
            public const string ReConnect = "ReConnect";
            public const string ReConnectRsp = "ReConnectRsp";


        }

    }
}

