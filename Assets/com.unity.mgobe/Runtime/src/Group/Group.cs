using System;
using System.Collections.Generic;
using Google.Protobuf;

using com.unity.mgobe.src.Net;
using com.unity.mgobe.src.Util;

namespace com.unity.mgobe.src.Group
{
    public class Group: BaseNetUtil
    {
        private readonly ServerSendClientBstWrap2Type _joinGroupBstType = ServerSendClientBstWrap2Type.EPushTypeJoinGroup;
        private readonly ServerSendClientBstWrap2Type _leaveGroupBstType = ServerSendClientBstWrap2Type.EPushTypeLeaveGroup;
        private readonly ServerSendClientBstWrap2Type _dismissGroupBstType = ServerSendClientBstWrap2Type.EPushTypeDismissGroup;
        private readonly ServerSendClientBstWrap2Type _changeGroupBstType = ServerSendClientBstWrap2Type.EPushTypeModifyGroupProperty;
        private readonly ServerSendClientBstWrap2Type _removeGroupBstType = ServerSendClientBstWrap2Type.EPushTypeRemoveGroupPlayer;
        private readonly ServerSendClientBstWrap2Type _changeCustomGroupPlayerStatusBstType = ServerSendClientBstWrap2Type.EPushTypeGroupPlayerState;
        private readonly ServerSendClientBstWrap2Type _recvFromGroupClientType = ServerSendClientBstWrap2Type.EPushTypeGroupChat;
        
        public Group(BstCallbacks bstCallbacks) : base(bstCallbacks)
        {
            // 注册广播
            this.SetBroadcastHandler(this._joinGroupBstType, this.OnJoinGroup);
            this.SetBroadcastHandler(this._leaveGroupBstType, this.OnLeaveGroup);
            this.SetBroadcastHandler(this._dismissGroupBstType, this.OnDismissGroup);
            this.SetBroadcastHandler(this._changeGroupBstType, this.OnChangeGroup);
            this.SetBroadcastHandler(this._removeGroupBstType, this.OnRemoveGroupPlayer);
            this.SetBroadcastHandler(this._changeCustomGroupPlayerStatusBstType, this.OnChangeCustomGroupPlayerStatus);
            this.SetBroadcastHandler(this._recvFromGroupClientType, this.OnRecvFromGroupClient);
        }
        
        ///////////////////////////////// 请求 //////////////////////////////////
        public string CreateGroup(CreateGroupReq para, Action<ResponseEvent> callback)
        {
            const int subcmd = (int)ProtoCmd.ECmdCreateGroupReq;
            var response = new NetResponseCallback(CreateGroupResponse);
            var seq = this.Send(para, subcmd, response, callback);
            Debugger.Log("CreateGroup_Para {0} {1}", para, seq);
            return seq;
        }
        
        public string JoinGroup(JoinGroupReq para, Action<ResponseEvent> callback)
        {
            const int subcmd = (int)ProtoCmd.ECmdJoinGroupReq;
            var response = new NetResponseCallback(JoinGroupResponse);
            var seq = this.Send(para, subcmd, response, callback);
            Debugger.Log("JoinGroup_Para {0} {1}", para, seq);
            return seq;
        }
        
        public string LeaveGroup(LeaveGroupReq para, Action<ResponseEvent> callback)
        {
            const int subcmd = (int)ProtoCmd.ECmdQuitGroupReq;
            var response = new NetResponseCallback(LeaveGroupResponse);
            var seq = this.Send(para, subcmd, response, callback);
            Debugger.Log("LeaveGroup_Para {0} {1}", para, seq);
            return seq;
        }
        
        public string DismissGroup(DismissGroupReq para, Action<ResponseEvent> callback)
        {
            const int subcmd = (int)ProtoCmd.ECmdDismissGroupReq;
            var response = new NetResponseCallback(DismissGroupResponse);
            var seq = this.Send(para, subcmd, response, callback);
            Debugger.Log("DismissGroup_Para {0} {1}", para, seq);
            return seq;
        }
        
        public string ChangeGroup(ChangeGroupReq para, Action<ResponseEvent> callback)
        {
            const int subcmd = (int)ProtoCmd.ECmdChangeGroupPropertiesReq;
            var response = new NetResponseCallback(ChangeGroupResponse);
            var seq = this.Send(para, subcmd, response, callback);
            Debugger.Log("ChangeGroup_Para {0} {1}", para, seq);
            return seq;
        }
        
        public string RemoveGroupPlayer(RemoveGroupPlayerReq para, Action<ResponseEvent> callback)
        {
            const int subcmd = (int)ProtoCmd.ECmdRemoveGroupMemberReq;
            var response = new NetResponseCallback(RemoveGroupPlayerResponse);
            var seq = this.Send(para, subcmd, response, callback);
            Debugger.Log("RemoveGroupPlayer_Para {0} {1}", para, seq);
            return seq;
        }
        
        public string GetGroupByGroupId(GetGroupByGroupIdReq para, Action<ResponseEvent> callback)
        {
            const int subcmd = (int)ProtoCmd.ECmdGetGroupDetailReq;
            var response = new NetResponseCallback(GetGroupByGroupIdResponse);
            var seq = this.Send(para, subcmd, response, callback);
            Debugger.Log("GetGroupByGroupId_Para {0} {1}", para, seq);
            return seq;
        }
        
        public string GetMyGroups(GetMyGroupsReq para, Action<ResponseEvent> callback)
        {
            const int subcmd = (int)ProtoCmd.ECmdGetGroupListReq;
            var response = new NetResponseCallback(GetMyGroupsResponse);
            var seq = this.Send(para, subcmd, response, callback);
            Debugger.Log("GetMyGroups_Para {0} {1}", para, seq);
            return seq;
        }
        
        public string ChangeCustomGroupPlayerStatus(ChangeCustomGroupPlayerStatusReq para, Action<ResponseEvent> callback)
        {
            const int subcmd = (int)ProtoCmd.ECmdChangeGroupPlayerStateReq;
            var response = new NetResponseCallback(ChangeCustomGroupPlayerStatusResponse);
            var seq = this.Send(para, subcmd, response, callback);
            Debugger.Log("ChangeCustomGroupPlayerStatus_Para {0} {1}", para, seq);
            return seq;
        }
        
        public string SendToGroupClient(SendToGroupClientReq para, Action<ResponseEvent> callback)
        {
            const int subcmd = (int)ProtoCmd.ECmdGroupChatReq;
            var response = new NetResponseCallback(SendToGroupClientResponse);
            var seq = this.Send(para, subcmd, response, callback);
            Debugger.Log("SendToGroupClient_Para {0} {1}", para, seq);
            return seq;
        }

        ///////////////////////////////// 响应 //////////////////////////////////
        private void CreateGroupResponse(bool send, DecodeRspResult res, Action<ResponseEvent> callback)
        {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent(rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            Debugger.Log("CreateGroupResponse {0}", eve);
            callback?.Invoke(eve);
            return;
        }
        
        private void JoinGroupResponse(bool send, DecodeRspResult res, Action<ResponseEvent> callback)
        {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent(rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            Debugger.Log("JoinGroupResponse {0}", eve);
            callback?.Invoke(eve);
            return;
        }
        
        private void LeaveGroupResponse(bool send, DecodeRspResult res, Action<ResponseEvent> callback)
        {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent(rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            Debugger.Log("LeaveGroupResponse {0}", eve);
            callback?.Invoke(eve);
            return;
        }
        
        private void DismissGroupResponse(bool send, DecodeRspResult res, Action<ResponseEvent> callback)
        {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent(rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            Debugger.Log("DismissGroupResponse {0}", eve);
            callback?.Invoke(eve);
            return;
        }
        
        private void ChangeGroupResponse(bool send, DecodeRspResult res, Action<ResponseEvent> callback)
        {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent(rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            Debugger.Log("ChangeGroupResponse {0}", eve);
            callback?.Invoke(eve);
            return;
        }
        
        private void RemoveGroupPlayerResponse(bool send, DecodeRspResult res, Action<ResponseEvent> callback)
        {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent(rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            Debugger.Log("RemoveGroupPlayerResponse {0}", eve);
            callback?.Invoke(eve);
            return;
        }
        
        private void GetGroupByGroupIdResponse(bool send, DecodeRspResult res, Action<ResponseEvent> callback)
        {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent(rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            Debugger.Log("GetGroupByGroupIdResponse {0}", eve);
            callback?.Invoke(eve);
            return;
        }
        
        private void GetMyGroupsResponse(bool send, DecodeRspResult res, Action<ResponseEvent> callback)
        {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent(rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            Debugger.Log("GetMyGroupsResponse {0}", eve);
            callback?.Invoke(eve);
            return;
        }
        
        private void ChangeCustomGroupPlayerStatusResponse(bool send, DecodeRspResult res, Action<ResponseEvent> callback)
        {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent(rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            Debugger.Log("ChangeCustomGroupPlayerStatusResponse {0}", eve);
            callback?.Invoke(eve);
            return;
        }
        
        private void SendToGroupClientResponse(bool send, DecodeRspResult res, Action<ResponseEvent> callback)
        {
            var rspWrap1 = res.RspWrap1;
            var eve = new ResponseEvent(rspWrap1.ErrCode, rspWrap1.ErrMsg, rspWrap1.Seq, res.Body);
            Debugger.Log("SendToGroupClientResponse {0}", eve);
            callback?.Invoke(eve);
            return;
        }
        
        ///////////////////////////////// 广播 //////////////////////////////////
        private void OnJoinGroup(DecodeBstResult bst, string seq)
        {
            var eve = new BroadcastEvent(bst.Body, seq);
            Debugger.Log("OnJoinGroup {0}", eve);
            this.bstCallbacks.Group.OnJoinGroup(eve);
        }
        
        private void OnLeaveGroup(DecodeBstResult bst, string seq)
        {
            var eve = new BroadcastEvent(bst.Body, seq);
            Debugger.Log("OnLeaveGroup {0}", eve);
            this.bstCallbacks.Group.OnLeaveGroup(eve);
        }
        
        private void OnDismissGroup(DecodeBstResult bst, string seq)
        {
            var eve = new BroadcastEvent(bst.Body, seq);
            Debugger.Log("OnDismissGroup {0}", eve);
            this.bstCallbacks.Group.OnDismissGroup(eve);
        }
        
        private void OnChangeGroup(DecodeBstResult bst, string seq)
        {
            var eve = new BroadcastEvent(bst.Body, seq);
            Debugger.Log("OnChangeGroup {0}", eve);
            this.bstCallbacks.Group.OnChangeGroup(eve);
        }
        
        private void OnRemoveGroupPlayer(DecodeBstResult bst, string seq)
        {
            var eve = new BroadcastEvent(bst.Body, seq);
            Debugger.Log("OnRemoveGroupPlayer {0}", eve);
            this.bstCallbacks.Group.OnRemoveGroupPlayer(eve);
        }
        
        private void OnChangeCustomGroupPlayerStatus(DecodeBstResult bst, string seq)
        {
            var eve = new BroadcastEvent(bst.Body, seq);
            Debugger.Log("OnChangeCustomGroupPlayerStatus {0}", eve);
            this.bstCallbacks.Group.OnChangeCustomGroupPlayerStatus(eve);
        }
        
        private void OnRecvFromGroupClient(DecodeBstResult bst, string seq)
        {
            var eve = new BroadcastEvent(bst.Body, seq);
            Debugger.Log("OnRecvFromGroupClient {0}", eve);
            this.bstCallbacks.Group.OnRecvFromGroupClient(eve);
        }
    }
}