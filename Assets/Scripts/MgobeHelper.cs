using System.Collections;
using System.Collections.Generic;
using com.unity.cloudbase;
using com.unity.mgobe;
using com.unity.mgobe.src.Util;
using UnityEngine;
using System;
using Notify;
using LogicFrameSync.Src.LockStep.Net.Pt;
using System.Collections.Concurrent;
using Google.Protobuf;
using Newtonsoft.Json;
using LogicFrameSync.Src.LockStep.Frame;
using UnityFrame;
using System.Runtime.InteropServices;
using Debugger = UnityFrame.Debugger;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using System.Text;
using LogicFrameSync.Src.LockStep;

public class MgobeHelper
{
    public static ConcurrentQueue<PtKeyFrameCollection> QueueKeyFrameCollection;
    public static int KeyframesCount = 0;
    public static int AllFramesCount = 0;

    static bool isInited;

    static object onFrameLock = new object();
    static List<Action> actionList = new List<Action>();

    static List<RoomInfo> roomList;
    static List<PlayerInfo> playerList;
    static RoomInfo roomInfo;

    public static PlayerInfoPara playerInfoPara
    {
        get => new PlayerInfoPara
        {
            Name = PlayerName,
            CustomPlayerStatus = 0,
            CustomProfile = ""
        };
    }
    public static bool IsInited { get => isInited; }
    public static List<RoomInfo> RoomList { get => roomList; }
    public static List<PlayerInfo> PlayerList { get => playerList; }

    public static string PlayerId { get =>  GamePlayerInfo.GetInfo().Id; }
    public static string PlayerName { get => Global.OpenId; }
    public static RoomInfo RoomInfo { get => roomInfo; 
        private set
        {
            roomInfo = value;

            playerList = new List<PlayerInfo>();
            if (roomInfo != null)
            {
                foreach (var item in roomInfo.PlayerList)
                {
                    playerList.Add(new PlayerInfo(item));
                }
            }

            MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_ROOM_INFO_UPDATE);
        }
    }  //具体玩家信息通过PlayerList 访问

    static bool isTryBattle = false;

    static bool isBattle = false;


    // 初始化 mgobe SDK
    public static IEnumerator InitSDK()
    {
        if (IsInited)
            yield break;

        Global.GameId = "obg-ree69jau";
        Global.SecretKey = "2c8d44838740f690cc756cd3f153e5e9f025b3e6";
        Global.Server = "ree69jau.wxlagame.com";

        GameInfoPara gameInfo = new GameInfoPara
        {
            GameId = Global.GameId,
            SecretKey = Global.SecretKey,
            OpenId = Global.OpenId
        };
        ConfigPara config = new ConfigPara
        {
            Url = Global.Server,
            ReconnectMaxTimes = 5,
            ReconnectInterval = 4000,
            ResendInterval = 2000,
            ResendTimeout = 20000,
            IsAutoRequestFrame = true,
        };

        // 初始化监听器 Listener
        Listener.Init(gameInfo, config, (ResponseEvent eve) =>
        {
            if (eve.Code == ErrCode.EcOk)
            {
                Debugger.UF_Log("init sdk succed");

                Room.GetMyRoom(evt => {
                    RoomInfo info =null;
                    if (evt.Code == 0)
                    {
                        Debugger.UF_Log("in room..");
                        var rsp = (GetRoomByRoomIdRsp)evt.Data;
                        info = rsp.RoomInfo;

                        RoomInfo = info;

                    }
                    else
                    {
                        Debugger.UF_Log("not in room..");

                    }
                    Global.Room = new Room(info);
                    Listener.Add(Global.Room);

                    // 初始化广播回调事件
                    InitBroadcast();

                    isInited = true;
                });

                QueueKeyFrameCollection = new ConcurrentQueue<PtKeyFrameCollection>();
            }
            else
            {
                Debugger.UF_Log("init sdk failed");
                isInited = true;
            }
        });
        while (!IsInited)
        {
            yield return 0;
        }
    }

    public static void Update()
    {
        if (actionList.Count != 0)
        {

            lock (onFrameLock)
            {
                foreach (var item in actionList)
                {
                    if (item != null) item();
                }
                actionList.Clear();
            }
        }

        if (isInited&& Global.IsInRoom()&& RoomInfo!=null&& RoomInfo.FrameSyncState==FrameSyncState.Stop && RoomInfo.MaxPlayers== (ulong)PlayerList.Count)
        {
            bool isAllReady = true;
            foreach (var item in PlayerList)
            {
                if (item.CustomPlayerStatus == 0)
                {
                    isAllReady = false;
                    break;
                }
            }
            if (!isTryBattle && isAllReady)
            {
                isTryBattle = true;
                MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_START_BATTLE);
            }
        }
        else
        {
            isTryBattle = false;
        }
    }

    static void AddAction(Action cb)
    {
        lock (onFrameLock)
        {
            actionList.Add(cb);
        }
    }

    public static PlayerInfo GetPlayerById(string id)
    {
        return playerList.Find((data) => data.Id == id);
    }

    static void InitBroadcast()
    {
        // 设置收帧广播回调函数
        Global.Room.OnRecvFrame = eve =>
        {
            AddAction(() => {
                RecvFrameBst bst = (RecvFrameBst)eve.Data;
                OnFrame(bst.Frame);
            });
        };

        // 设置消息接收广播回调函数
        Global.Room.OnRecvFromClient = eve =>
        {
            AddAction(() =>
            {
                RecvFromClientBst bst = (RecvFromClientBst)eve.Data;
            });
        };

        // 设置服务器接收广播回调函数
        Global.Room.OnRecvFromGameSvr = eve =>
        {
            AddAction(() =>
            {
                RecvFromGameSvrBst bst = (RecvFromGameSvrBst)eve.Data;
            });
        };

        // 设置房间改变广播回调函数
        Global.Room.OnChangeRoom = eve =>
        {
            AddAction(() => {
                ChangeRoomBst bst = (ChangeRoomBst)eve.Data;
                Debugger.UF_Log("Global.Room.OnChangeRoom:{0}", bst.RoomInfo?.Id ?? "null");

                RoomInfo = bst.RoomInfo;
            });
        };

        //房间解散
        Global.Room.OnDismissRoom = eve =>
        {
            AddAction(() => {
                DismissRoomBst bst = (DismissRoomBst)eve.Data;
                Debugger.UF_Log("Global.Room.OnChangeRoom:{0}", bst.RoomInfo?.Id ?? "null");

                RoomInfo = null;
                Global.Room.InitRoom(RoomInfo);
            });
        };

        // 设置匹配成功广播回调函数
        Room.OnMatch = eve =>
        {
            AddAction(() => {
                MatchBst bst = (MatchBst)eve.Data;
                Debugger.UF_Log("on match!");

            });
        };

        // 设置取消匹配广播回调函数
        Room.OnCancelMatch = eve =>
        {
            AddAction(() => {
                CancelMatchBst bst = (CancelMatchBst)eve.Data;
                Debugger.UF_Log("on cancel match! ");

            });
        };

        Global.Room.OnChangeCustomPlayerStatus = eve =>
        {
            AddAction(() =>
            {
                ChangeCustomPlayerStatusBst bst = (ChangeCustomPlayerStatusBst)eve.Data;
                var playerId = bst.ChangePlayerId;
                var status = bst.CustomPlayerStatus;
                var data = GetPlayerById(playerId);
                if (data != null)
                {
                    data.CustomPlayerStatus = status;
                }
                MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_ROOM_INFO_UPDATE);
            });
        };
        Global.Room.OnJoinRoom = eve =>
        {
            AddAction(() =>
            {
                JoinRoomBst bst = (JoinRoomBst)eve.Data;
                Debugger.UF_Log("Global.Room.OnJoinRoom:{0}", bst.JoinPlayerId);

                var playerId = bst.JoinPlayerId;
                RoomInfo = bst.RoomInfo;
            });

        };
        Global.Room.OnLeaveRoom = eve =>
        {
            AddAction(() =>
            {
                LeaveRoomBst bst = (LeaveRoomBst)eve.Data;
                Debugger.UF_Log("Global.Room.OnJoinRoom:{0}", bst.LeavePlayerId);

                var playerId = bst.LeavePlayerId;
                RoomInfo = bst.RoomInfo;
            });
        };
        Global.Room.OnChangePlayerNetworkState = eve =>
        {
            AddAction(() =>
            { 
                ChangePlayerNetworkStateBst bst = (ChangePlayerNetworkStateBst)eve.Data;

            });
        };
        Global.Room.OnStartFrameSync = eve =>
        {
            AddAction(() =>
            {
                if (isBattle) return;
                isBattle = true;

                Debugger.UF_Log("Global.Room.OnStartFrameSync");
                StartFrameSyncBst bst = (StartFrameSyncBst)eve.Data;
                MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_StartFrameSync);
            });
        };
        Global.Room.OnStopFrameSync = eve =>
        {
            AddAction(() =>
            {
                if (!isBattle) return;
                isBattle = false;

                Debugger.UF_Log("Global.Room.OnStopFrameSync");
                StopFrameSyncBst bst = (StopFrameSyncBst)eve.Data;
                MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_StopFrameSync);
            });
        }; 
        Global.Room.OnAutoRequestFrameError = eve =>
        {
            Global.Room.RetryAutoRequestFrame();
        };
        Global.Room.OnRecvFromClient = eve =>
        {
            AddAction(() =>
            {
                RecvFromClientBst bst = (RecvFromClientBst)eve.Data;
                Debugger.UF_Log("Global.Room.OnRecvFromClient:{0},{1}",bst.SendPlayerId, bst.Msg);
                MessageSystem.UF_GetInstance().UF_Send(DefineEvent.E_CLIENT_MSG, bst.SendPlayerId, bst.Msg,bst.RoomId);
            });
        };

    }

    static void InitPlayerData()
    {
        string playerId = PlayerId;

        //myPlayerInfo = new PlayerInfo();
        //myPlayerInfo.Id = playerId;
        //myPlayerInfo.Name = Global.OpenId;
        //myPlayerInfo.TeamId = "0";
        //myPlayerInfo.CustomPlayerStatus = 0;
        //myPlayerInfo.CustomProfile = "";
        // myPlayerInfo.MatchAttributes = new MatchAttribute();
    }
    public static void CreateRoom(Action cb = null)
    {
        CreateTeamRoomPara para = new CreateTeamRoomPara
        {
            RoomName = Global.OpenId,
            MaxPlayers = 2,
            RoomType = "Battle",
            CustomProperties = "0",
            IsPrivate = false,
            PlayerInfo = playerInfoPara,
            TeamNumber = 2
        };

        // 创建团队房间
        Global.Room.CreateTeamRoom(para, eve =>
        {
            if (eve.Code == 0)
            {
                AddAction(() => {
                    CreateRoomRsp rsp = (CreateRoomRsp)eve.Data;
                    RoomInfo = rsp.RoomInfo;
                    cb?.Invoke(); 
                });
            }
            else
            {
                Debugger.UF_Log("create Team Room Fail: {0}", eve.Code);
            }

        });
    }

    public static void RefreshRoomList(Action cb = null)
    {
        GetRoomListPara para = new GetRoomListPara
        {
            PageNo = 1,
            PageSize = 20,
            RoomType = "Battle"
        };

        // 获取房间列表
        Room.GetRoomList(para, (eve) =>
        {
            if (eve.Code == ErrCode.EcOk)
            {
                try
                {
                    AddAction(() => {
                        var rsp = (GetRoomListRsp)eve.Data;
                        var rlist = new List<RoomInfo>();
                        foreach (var item in rsp.RoomList)
                        {
                            rlist.Add(new RoomInfo(item));
                        }
                        roomList = rlist;
                        cb?.Invoke(); });
                }
                catch (System.Exception e)
                {

                    Debug.LogError(e);
                }

            }
            else
            {
                // debugger.UF_Log ("Get room list error: {0}", eve.code);
            }
        });
    }

    public static void JoinRoom(int roomIdx, Action cb = null)
    {
        if (roomIdx != -1)
        {
            RoomInfo roomInfo = roomList[roomIdx];
            var maxPlayer = Convert.ToInt32(roomInfo.MaxPlayers);
            if (maxPlayer == roomInfo.PlayerList.Count)
            {
                return;
            }
            var teams = new HashSet<string>();
            foreach (var item in roomInfo.PlayerList)
            {
                teams.Add(item.TeamId);
            }
            var teamId = 0;
            for (int i = 0; i < maxPlayer; i++)
            {
                if (!teams.Contains(i + ""))
                {
                    teamId = i;
                    break;
                }
            }

            JoinTeamRoomPara para = new JoinTeamRoomPara
            {
                PlayerInfo = playerInfoPara,
                TeamId = teamId + "",
            };

            Global.Room.InitRoom(roomInfo);
            // 加入团队房间
            Global.Room.JoinTeamRoom(para, (eve) =>
            {
                if (eve.Code == ErrCode.EcOk)
                {
                    AddAction(() => {
                        JoinRoomRsp rsp = (JoinRoomRsp)eve.Data;
                        RoomInfo = rsp.RoomInfo;
                        cb?.Invoke(); });
                }
                else
                {
                    RefreshRoomList();
                }
            });
        }
    }

    public static void LeaveRoom(Action cb = null)
    {
        // 离开房间
        Global.Room.LeaveRoom(eve =>
        {
            if (eve.Code == 0)
            {
                AddAction(() => {
                    RoomInfo = null;
                    Global.Room.InitRoom(RoomInfo);
                    cb?.Invoke();
                });
            }
        });

    }

    public static void MatchPlayer(Action cb = null)
    {
        MatchPlayersPara para = new MatchPlayersPara
        {
            MatchCode = "match-XXXXXXXXXXXX",
            PlayerInfoPara = new MatchPlayerInfoPara
            {
                Name = PlayerName,
                CustomPlayerStatus = 0,
                CustomProfile = "",
                MatchAttributes = new List<MatchAttribute>()
            }
        };
        para.PlayerInfoPara.MatchAttributes.Add(new MatchAttribute
        {
            Name = "Score",
            Value = 0
        });

        // 进行玩家匹配
        Global.Room.MatchPlayers(para, eve =>
        {
            if (eve.Code != 0)
            {
                AddAction(() => {
                    Debugger.UF_Log("发起玩家匹配失败", eve.Code);
                });
            }
            else
            {
                AddAction(() => {
                    Debugger.UF_Log("发起玩家匹配");
                    cb?.Invoke();
                });
            }
        });
    }

    public static void MatchRoom(Action cb = null)
    {
        MatchRoomPara para = new MatchRoomPara
        {
            RoomType = "Battle",
            MaxPlayers = 2,
            PlayerInfo = playerInfoPara
        };
        // 进行房间匹配
        Global.Room.MatchRoom(para, eve =>
        {
            if (eve.Code != 0)
            {
                AddAction(() => {
                    Debugger.UF_Log("发起房间匹配失败",eve.Code);
                });
            }
            else
            {
                AddAction(() => {
                    Debugger.UF_Log("发起房间匹配");
                    cb?.Invoke();
                });
            }
        });
    }

    public static void MatchGroup(Action cb = null)
    {
        var playerInfo = new MatchGroupPlayerInfoPara
        {
            Id = PlayerId,
            Name = PlayerName,
            CustomPlayerStatus = 0,
            CustomProfile = "",
            MatchAttributes = new List<MatchAttribute>()
        };

        playerInfo.MatchAttributes.Add(new MatchAttribute { Name = "skill", Value = 9 });
        var para = new MatchGroupPara
        {
            MatchCode = "match-evtp3fdv",
            // matchCode = "match-hel6rt0j",
            PlayerInfoList = new List<MatchGroupPlayerInfoPara>()
        };
        para.PlayerInfoList.Add(playerInfo);
        // 进行组队匹配
        Global.Room.MatchGroup(para, eve =>
        {
            if (eve.Code == 0)
            {
                AddAction(() => {
                    Debugger.UF_Log("进行组队匹配");
                    cb?.Invoke();
                });
            }
            else
            {
                AddAction(() => {
                    Debugger.UF_Log("进行组队匹配失败: {0}", eve.Code);
                });
            }
        });
    }

    public static void cancelPlayerMatch(Action cb = null)
    {
        var para = new CancelPlayerMatchPara
        {
            MatchType = MatchType.PlayerComplex
        };
        // 取消匹配
        Global.Room.CancelPlayerMatch(para, eve =>
        {
            if (eve.Code == 0)
            {
                AddAction(() => {
                    Debugger.UF_Log("取消玩家匹配");
                    cb?.Invoke();
                });
            }
            else{
                AddAction(() => {
                    Debugger.UF_Log("取消玩家匹配失败: {0}", eve.Code);
                });
            }
        });
    }

    public static void SetReadyToBattle(Action cb = null)
    {
        var data = GetPlayerById(PlayerId);
        if (data == null) return;
        var status = data.CustomPlayerStatus == 0 ? 1 : 0;
        // 更改自定义玩家状态
        Global.Room.ChangeCustomPlayerStatus(new ChangeCustomPlayerStatusPara { CustomPlayerStatus = (ulong)status },
            eve =>
            {
                if (eve.Code == ErrCode.EcOk)
                {
                    AddAction(() =>
                    {
                        Debugger.UF_Log("更改玩家状态 :{0}", status);
                        cb?.Invoke();
                    });
                }
                else
                {
                }
            });

    }

    public static void SendToClient(List<string> list,RecvType type, string msg, Action cb = null)
    {
        var para = new SendToClientPara();
        para.Msg = msg;
        para.RecvType = type;
        para.RecvPlayerList = list!=null ?list: new List<string>();
        Global.Room.SendToClient(para,eve=> {
            if (eve.Code == 0)
            {
                AddAction(() =>
                {
                    Debugger.UF_Log("SendToClient {0}",msg);
                    cb?.Invoke();
                });
            }
        });
    }

    public static void StartFrameSync(Action cb = null)
    {
        // 开始帧同步
        Global.Room.StartFrameSync(eve =>
        {
            try
            {
                if (eve.Code == ErrCode.EcOk)
                {
                    AddAction(() =>
                    {
                        Debugger.UF_Log("开始帧同步");
                        cb?.Invoke();
                    });

                    //isStartFrameSync = true;
                    //isInBattle = true;
                }
                else
                {
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        });
    }

    public static void StopFrameSync(Action cb = null)
    {
        // 停止帧同步
        Global.Room.StopFrameSync(eve =>
        {
            try
            {
                if (eve.Code == ErrCode.EcOk)
                {
                    AddAction(() =>
                    {
                        Debugger.UF_Log("停止帧同步");
                        cb?.Invoke();
                    });

                }
                else
                {
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        });
    }

    static StringBuilder sb = new StringBuilder();

    public static void SendFrame(PtKeyFrameCollection data)
    {
        SendFramePara para = new SendFramePara();
        //sb.Append(data.FrameIdx);
        //sb.Append(";");
        for (int i = 0; i < data.KeyFrames.Count; i++)
        {
            sb.Append(FrameIdxInfo.Serialize(data.KeyFrames[i]));
            sb.Append(";");
        }
        para.Data = sb.ToString();
        sb.Clear();

        Global.Room.SendFrame(para, rsp =>
        {
            if (rsp.Code != ErrCode.EcOk)
            {
                AddAction(() =>
                {
                    Debugger.UF_Log("发送帧失败{0}", rsp.Code);
                });
            }
        });
    }

    static void OnFrame(Frame fr)
    {
        //Debugger.UF_Log("OnFrame .... "); // + fr.Id);
        if (RoomInfo!=null&&RoomInfo.FrameSyncState == FrameSyncState.Stop && !SimulationManager.Instance.IsStart())
        {
            SimulationManager.Instance.Start((ulong)fr.Time);
        }

        PtKeyFrameCollection collection = new PtKeyFrameCollection();
        collection.KeyFrames = new List<FrameIdxInfo>();
        if (fr.Items.Count > 0)
        {
            FrameIdxInfo info =new FrameIdxInfo();
            foreach (var item in fr.Items)
            {
                info = FrameIdxInfo.DeSerialize(item.Data);
                collection.KeyFrames.Add(info);
            }
        }
        collection.FrameIdx = (int)fr.Id;// (int)info?.Idx;

        QueueKeyFrameCollection.Enqueue(collection);
        KeyframesCount++;
        AllFramesCount += collection.KeyFrames.Count;
    }
}