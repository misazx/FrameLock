using System;
using Google.Protobuf;

using com.unity.mgobe.src.EventUploader;
using com.unity.mgobe.src.Net;
using com.unity.mgobe.src.Util;
using com.unity.mgobe.src.Util.Def;

namespace com.unity.mgobe.src.Sender {
    public class FrameSender {
        private readonly BstCallbacks _bstCallbacks;

        private readonly ServerSendClientBstWrap2Type _frameBroadcastType = ServerSendClientBstWrap2Type.EPushTypeRelay;
        private readonly ServerSendClientBstWrap2Type _sendMessageExtBroadcastType = ServerSendClientBstWrap2Type.EPushTypeGamesvr;
        private readonly ServerSendClientBstWrap2Type _startGameBroadcastType = ServerSendClientBstWrap2Type.EPushTypeStartGame;
        private readonly ServerSendClientBstWrap2Type _stopGameBroadcastType = ServerSendClientBstWrap2Type.EPushTypeStopGame;
        public FrameSender (BstCallbacks bstCallbacks) {
            this._bstCallbacks = bstCallbacks;
            this.NetUtil1 = new BaseNetUtil (bstCallbacks);
            this.NetUtil2 = new BaseNetUtil (bstCallbacks);

            // socket1 注册广播
            this.NetUtil1.SetBroadcastHandler (_startGameBroadcastType, this.OnStartFrameSync);
            this.NetUtil1.SetBroadcastHandler (_stopGameBroadcastType, this.OnStopFrameSync);

            // socket2 注册广播
            this.NetUtil2.SetBroadcastHandler (_frameBroadcastType, this.OnRecvFrame);
            this.NetUtil2.SetBroadcastHandler (_sendMessageExtBroadcastType, this.OnRecvFromGameSvr);
        }

        public RoomInfo RoomInfo { get; set; }

        public BaseNetUtil NetUtil1 { get; } = null;

        public BaseNetUtil NetUtil2 { get; } = null;

        private RoomInfo GetFrameRoom () {
            if (this.RoomInfo == null) {
                return new RoomInfo {
                Id = "0",
                RouteId = ""
                };
            }
            return this.RoomInfo;
        }

        public void SetFrameRoom (RoomInfo roomInfo) {
            var oldRoom = this.GetFrameRoom ();
            this.RoomInfo = roomInfo ?? new RoomInfo { Id = "0", RouteId = "" };

            var oldRouteId = oldRoom.RouteId;
            var newRouteId = RoomInfo.RouteId;

            if (newRouteId.Length == 0) {
                NetUtil2.client.Socket?.CloseSocketTask (null, null);
            }

            if (!oldRouteId.Equals (newRouteId)) {
                // 重新checklogin
                CheckLoginStatus.SetStatus (CheckLoginStatus.StatusType.Offline);
                this.AutoCheckLogin ();
                return;
            }

            if (oldRouteId.Equals (newRouteId)) {
                this.AutoCheckLogin ();
            }
        }

        // 检查是否需要 checklogin
        private void AutoCheckLogin () {
            this.Connect ();
            if (this.NetUtil2.client.Socket.IsSocketStatus ("connect") && CheckLoginStatus.IsOffline ()) {
                this.NetUtil2.client.Socket.Emit ("autoAuth", new SocketEvent ());
            }
        }

        private void Connect () {
            if (this.NetUtil2.client.Socket.IsSocketStatus ("connect") || this.RoomInfo == null) return;
            this.NetUtil2.client.Socket.Url = Config.Url + ":" + Port.GetRelayPort ();
            this.NetUtil2.client.Socket.ConnectSocketTask ("Framesender connect");
        }

        ///////////////////////////////// 请求 //////////////////////////////////
        // 帧同步开始
        public string StartFrameSync (StartFrameSyncReq para, Action<ResponseEvent> callback) {
            if (this.RoomInfo == null || string.IsNullOrEmpty (this.RoomInfo.Id)) {
                var rspWrap1 = new ClientSendServerRspWrap1 {
                ErrCode = ErrCode.EcSdkNoRoom,
                ErrMsg = "无房间信息"
                };
                var res = new DecodeRspResult (rspWrap1, null, null);
                StartFrameSyncResponse (false, res, callback);
                return "";
            }
            this.AutoCheckLogin ();

            var response = new NetResponseCallback (StartFrameSyncResponse);
            const int subcmd = (int) ProtoCmd.ECmdStartFrameSyncReq;
            var seq = this.NetUtil1.Send (para, subcmd, response, callback);
            Debugger.Log("StartFrameSync_Para {0} {1}", para, seq);
            return seq;
        }
        // 帧同步停止
        public string StopFrameSync (StopFrameSyncReq para, Action<ResponseEvent> callback) {
            if (this.RoomInfo == null || string.IsNullOrEmpty (this.RoomInfo?.Id)) {
                var rspWrap1 = new ClientSendServerRspWrap1 {
                ErrCode = ErrCode.EcSdkNoRoom,
                ErrMsg = "无房间信息"
                };
                var res = new DecodeRspResult (rspWrap1, null, null);
                this.StopFrameSyncResponse (false, res, callback);
                return "";
            }
            this.AutoCheckLogin ();

            var response = new NetResponseCallback (this.StopFrameSyncResponse);
            const int subcmd = (int) ProtoCmd.ECmdStopFrameSyncReq;
            var seq = this.NetUtil1.Send (para, subcmd, response, callback);
            Debugger.Log("StopFrameSync_Para {0} {1}", para, seq);
            return seq;
        }
        // 发送帧同步信息
        public string SendFrame (SendFrameReq para, Action<ResponseEvent> callback) {
            if (this.RoomInfo == null || string.IsNullOrEmpty (this.RoomInfo.Id)) {
                var rspWrap1 = new ClientSendServerRspWrap1 {
                ErrCode = ErrCode.EcSdkNoRoom,
                ErrMsg = "无房间信息"
                };
                var res = new DecodeRspResult (rspWrap1, null, null);
                this.SendFrameResponse (false, res, callback);
                return "";
            }
            this.AutoCheckLogin ();

            var response = new NetResponseCallback (this.SendFrameResponse);
            const int subcmd = (int) ProtoCmd.ECmdRelaySendFrameReq;
            try {
                var seq = this.NetUtil2.Send (para, subcmd, response, callback);
                Debugger.Log("SendFrame_Para {0} {1}", para, seq);
                return seq;
            } catch (System.Exception e) {
                Debugger.Log ("Error: {0}", e.ToString ());
                throw;
            }

        }

        // 请求补帧
        public string RequestFrame (RequestFrameReq para, Action<ResponseEvent> callback) {
            if (this.RoomInfo == null || string.IsNullOrEmpty (this.RoomInfo.Id)) {
                var rspWrap1 = new ClientSendServerRspWrap1 {
                ErrCode = ErrCode.EcSdkNoRoom,
                ErrMsg = "无房间信息"
                };
                var res = new DecodeRspResult (rspWrap1, null, null);
                RequestFrameResponse (false, res, callback);
                return "";
            }
            this.AutoCheckLogin ();

            var response = new NetResponseCallback (RequestFrameResponse);
            const int subcmd = (int) ProtoCmd.ECmdRelayRequestFrameReq;
            var seq = this.NetUtil2.Send (para, subcmd, response, callback);
            Debugger.Log("RequestFrame_Para {0} {1}", para, seq);
            return seq;
        }

        // 确认登录
        public string CheckLogin (Action<ResponseEvent> callback, string tag) {
            if (this.RoomInfo == null || string.IsNullOrEmpty (this.RoomInfo.Id)) {
                Debugger.Log ("无房间信息");
                var rspWrap1 = new ClientSendServerRspWrap1 {
                    ErrCode = ErrCode.EcSdkNoRoom,
                    ErrMsg = "无房间信息"
                };
                var res = new DecodeRspResult (rspWrap1, null, null);
                CheckLoginResponse (false, res, callback);
                return "";
            }
            CheckLoginStatus.SetStatus (CheckLoginStatus.StatusType.Checking);

            var response = new NetResponseCallback (CheckLoginResponse);
            const int subcmd = (int) ProtoCmd.ECmdCheckLoginReq;

            var para = new CheckLoginReq {
                Token = RequestHeader.AuthKey,
                RouteId = this.RoomInfo.RouteId
            };

            var seq = this.NetUtil2.Send (para, subcmd, response, callback);
            CheckLoginStatus.SetRouteId (para.RouteId);
            Debugger.Log("CheckLogin_Para {0} {1}", para, seq);
            return seq;
        }

        // 发送自定义服务消息
        public string SendMessageExt (SendToGameSvrReq para, Action<ResponseEvent> callback) {
            if (this.RoomInfo == null || string.IsNullOrEmpty (this.RoomInfo.Id)) {
                var rspWrap1 = new ClientSendServerRspWrap1 {
                ErrCode = ErrCode.EcSdkNoRoom,
                ErrMsg = "无房间信息"
                };
                var res = new DecodeRspResult (rspWrap1, null, null);
                SendMessageExtResponse (false, res, callback);
                return "";
            }
            this.AutoCheckLogin ();

            var response = new NetResponseCallback (SendMessageExtResponse);
            const int subcmd = (int) ProtoCmd.ECmdRelayClientSendtoGamesvrReq;
            var seq = this.NetUtil2.Send (para, subcmd, response, callback);
            Debugger.Log("SendMessageExt_Para {0} {1}", para, seq);
            return seq;
        }

        ///////////////////////////////// 响应 //////////////////////////////////
        private void SendFrameResponse (bool send, DecodeRspResult res, Action<ResponseEvent> callback) {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent (rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            Debugger.Log("SendFrameResponse {0}", eve);
            callback?.Invoke (eve);
            return;
        }
        private static void CheckLoginResponse (bool send, DecodeRspResult res, Action<ResponseEvent> callback) {
            CheckLoginStatus.SetStatus (CheckLoginStatus.StatusType.Offline);
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent (rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            Debugger.Log("CheckLoginResponse {0}", eve);
            if (eve.Code == ErrCode.EcOk) {
                CheckLoginStatus.SetStatus (CheckLoginStatus.StatusType.Checked);
            }
            callback?.Invoke (eve);
            return;
        }
        private static void RequestFrameResponse (bool send, DecodeRspResult res, Action<ResponseEvent> callback) {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent (rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            Debugger.Log("RequestFrameResponse {0}", eve);
            callback?.Invoke (eve);
            return;
        }
        private static void SendMessageExtResponse (bool send, DecodeRspResult res, Action<ResponseEvent> callback) {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent (rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            Debugger.Log("SendMessageExtResponse {0}", eve);
            callback?.Invoke (eve);
            return;
        }
        private static void StartFrameSyncResponse (bool send, DecodeRspResult res, Action<ResponseEvent> callback) {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent (rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            Debugger.Log("StartFrameSyncResponse {0}", eve);
            callback?.Invoke (eve);
            return;
        }
        private void StopFrameSyncResponse (bool send, DecodeRspResult res, Action<ResponseEvent> callback) {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent (rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            Debugger.Log("StopFrameSyncResponse {0}", eve);
            callback?.Invoke (eve);
            return;
        }

        ///////////////////////////////// 广播 //////////////////////////////////
        // 收到帧同步消息
        private void OnRecvFrame (DecodeBstResult bstResult, string seq) {
            var bst = (RecvFrameBst)bstResult.Body;
            bst.Frame.RoomId = this.GetFrameRoom ().Id;
            var eve = new BroadcastEvent (bst, seq);
            
            Debugger.Log("OnRecvFrame {0}", eve);

            // 用户数据上传
            FrameBst.Trigger ();

            EventUpload.PushFrameRateEvent (Convert.ToInt64 (FrameBst.deltaTime));
            
            this._bstCallbacks.Room.OnRecvFrame (eve);
        }

        // 开始游戏
        private void OnStartFrameSync (DecodeBstResult res, string seq) {
            var eve = new BroadcastEvent (res.Body, seq);
            Debugger.Log("OnStartFrameSync {0}", eve);
            FrameBst.Clear ();
            this._bstCallbacks.Room.OnStartFrameSync (eve);
        }

        // 结束游戏
        private void OnStopFrameSync (DecodeBstResult bst, string seq) {
            var eve = new BroadcastEvent (bst.Body, seq);

            Debugger.Log("OnStopFrameSync {0}", eve);
            
            NetUtil2?.client.ClearQueue ();
            FrameBst.Clear ();
            this._bstCallbacks.Room.OnStopFrameSync (eve);
        }

        // 自定义服务广播
        private void OnRecvFromGameSvr (DecodeBstResult bst, string seq) {

            var body = (RecvFromGameSvrBst)bst.Body;
            var eve = new BroadcastEvent (bst.Body, seq);
            Debugger.Log("OnRecvFromGameSvr {0}", eve);
            this._bstCallbacks.Room.OnRecvFromGameSvr (body.RoomId, eve);
        }

    }
}