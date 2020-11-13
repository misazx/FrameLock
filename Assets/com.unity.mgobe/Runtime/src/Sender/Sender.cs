using System;
using Google.Protobuf;

using com.unity.mgobe.src.Net;
using com.unity.mgobe.src.Util;

namespace com.unity.mgobe.src.Sender
{
    public class Sender : BaseNetUtil
    {
        private const ServerSendClientBstWrap2Type _messageBroadcastType = ServerSendClientBstWrap2Type.EPushTypeRoomChat;

        public Sender(BstCallbacks bstCallbacks) : base(bstCallbacks)
        {
            var bst = new BroadcastCallback(OnRecvFromClient);
            SetBroadcastHandler(_messageBroadcastType, bst);
        }

        ///////////////////////////////// 请求 //////////////////////////////////
        // 发送消息
        public string SendMessage(SendToClientReq para, Action<ResponseEvent> callback)
        {
            const int subcmd = (int)ProtoCmd.ECmdRoomChatReq;
            var response = new NetResponseCallback(SendMessageResponse);
            var seq = Send(para, subcmd, SendMessageResponse, callback);
            Debugger.Log("SendMessage_Para {0} {1}", para, seq);
            return seq;
        }

        ///////////////////////////////// 响应 //////////////////////////////////
        // 发送消息
        private void SendMessageResponse(bool send, DecodeRspResult res, Action<ResponseEvent> callback)
        {

            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent(rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            Debugger.Log("SendMessageResponse {0}", eve);
            callback?.Invoke(eve);
            return;
        }

        ///////////////////////////////// 广播 //////////////////////////////////
        // 收到普通消息
        private void OnRecvFromClient(DecodeBstResult res, string seq)
        {
            var bst = (RecvFromClientBst)res.Body;
            var eve = new BroadcastEvent(bst, seq);
            Debugger.Log("OnRecvFromClient {0}", eve);
            var roomId = bst.RoomId;
            this.bstCallbacks.Room.OnRecvFromClient(roomId, eve);
            return;
        }
    }
}