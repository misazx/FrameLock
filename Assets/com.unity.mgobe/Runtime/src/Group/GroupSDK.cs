
using System;
using System.Runtime.CompilerServices;
using com.unity.mgobe.Runtime.src.Broadcast;
using com.unity.mgobe.Runtime.src.Group;
using com.unity.mgobe.src;
using com.unity.mgobe.src.Util.Def;

namespace com.unity.mgobe
{
    public class Group: GroupBroadcastHandler
    {
        internal GroupUtil GroupUtil { get; }
        public GroupBroadcast GroupBroadcast { get; set; }
        public GroupInfo GroupInfo { get; set; }

        public Group(GroupInfo groupInfo): base()
        {
            this.GroupUtil = new GroupUtil (this);
            this.GroupUtil.SetGroupInfo (groupInfo);
        }
        
        public void InitGroup (string id) {
            var groupInfo = new GroupInfo {
                Id = id
            };
            this.InitGroup (groupInfo);
        }
        
        public void InitGroup (GroupInfo groupInfo) {
            this.GroupUtil.SetGroupInfo(groupInfo);
        }
        
        public Action<Group> OnUpdate = group => {};

        public static void GetGroupByGroupId(GetGroupByGroupIdPara para, Action<ResponseEvent> callback)
        {
            GetGroupByGroupIdReq groupByGroupIdReq = new GetGroupByGroupIdReq
            {
                GroupId = para.GroupId
            };

            Core.Group.GetGroupByGroupId(groupByGroupIdReq, eve => callback?.Invoke(eve));
        }

        public static void GetMyGroups(Action<ResponseEvent> callback)
        {
            GetMyGroupsReq getMyGroupsReq = new GetMyGroupsReq
            {
            };

            Core.Group.GetMyGroups(getMyGroupsReq, eve => callback?.Invoke(eve));
        }

        public void CreateGroup(CreateGroupPara para, Action<ResponseEvent> callback)
        {

            GroupPlayerInfo playerInfo = new GroupPlayerInfo
            {
                Id = RequestHeader.PlayerId,
                Name = para.PlayerInfo.Name,
                CustomGroupPlayerProfile = para.PlayerInfo.CustomGroupPlayerProfile,
                CustomGroupPlayerStatus = para.PlayerInfo.CustomGroupPlayerStatus
            };
            
            CreateGroupReq createGroupReq = new CreateGroupReq
            {
                GroupName = para.GroupName,
                GroupType = para.GroupType,
                MaxPlayers = para.MaxPlayers,
                CustomProperties = para.CustomProperties,
                IsForbidJoin = para.IsForbidJoin,
                IsPersistent = para.IsPersistent,
                PlayerInfo = playerInfo,
            };

            Action<ResponseEvent> cb = eve =>
            {
                this.GroupUtil.SaveGroupInfo(eve, ((CreateGroupRsp)eve.Data)?.GroupInfo);
                callback?.Invoke(eve);
            };
            
            Core.Group.CreateGroup(createGroupReq, cb);
        }
        
        public void GetGroupDetail(Action<ResponseEvent> callback)
        {
            GetGroupByGroupIdReq getGroupByGroupIdReq = new GetGroupByGroupIdReq
            {
                GroupId = this.GroupInfo?.Id
            };
            
            Action<ResponseEvent> cb = eve =>
            {
                this.GroupUtil.SaveGroupInfo(eve, ((GetGroupByGroupIdRsp)eve.Data)?.GroupInfo);
                callback?.Invoke(eve);
            };

            Core.Group.GetGroupByGroupId(getGroupByGroupIdReq, cb);
        }
        
        public void JoinGroup(JoinGroupPara para, Action<ResponseEvent> callback)
        {
            GroupPlayerInfo playerInfo = new GroupPlayerInfo
            {
                Id = RequestHeader.PlayerId,
                Name = para.PlayerInfo.Name,
                CustomGroupPlayerProfile = para.PlayerInfo.CustomGroupPlayerProfile,
                CustomGroupPlayerStatus = para.PlayerInfo.CustomGroupPlayerStatus
            };
            
            JoinGroupReq joinGroupReq = new JoinGroupReq
            {
                GroupId = this.GroupInfo.Id,
                PlayerInfo = playerInfo,
            };

            Action<ResponseEvent> cb = eve =>
            {
                this.GroupUtil.SaveGroupInfo(eve, ((JoinGroupRsp)eve.Data)?.GroupInfo);
                callback?.Invoke(eve);
            };
            
            Core.Group.JoinGroup(joinGroupReq, cb);
        }
        
        public void LeaveGroup(Action<ResponseEvent> callback)
        {
            LeaveGroupReq leaveGroupReq = new LeaveGroupReq
            {
                GroupId = this.GroupInfo.Id
            };
            
            Action<ResponseEvent> cb = eve =>
            {
                this.GroupUtil.SaveGroupInfo(eve, ((LeaveGroupRsp)eve.Data)?.GroupInfo);
                callback?.Invoke(eve);
            };
            
            Core.Group.LeaveGroup(leaveGroupReq, cb);
        }
        
        public void DismissGroup(Action<ResponseEvent> callback)
        {
            DismissGroupReq dismissGroupReq = new DismissGroupReq
            {
                GroupId = this.GroupInfo.Id
            };
            
            Action<ResponseEvent> cb = eve =>
            {
                if (eve.Code == ErrCode.EcOk)
                {
                    this.GroupUtil.SetGroupInfo(null);
                }
                
                callback?.Invoke(eve);
            };
            
            Core.Group.DismissGroup(dismissGroupReq, cb);
        }
        
        public void ChangeGroup(ChangeGroupPara para, Action<ResponseEvent> callback)
        {
            ChangeGroupReq changeGroupReq = new ChangeGroupReq
            {
                GroupName = para.GroupName ?? "",
                Owner = para.Owner ?? "",
                CustomProperties = para.CustomProperties ?? "",
                IsForbidJoin = para.IsForbidJoin,
                GroupId = this.GroupInfo?.Id
            };
            
            if (changeGroupReq.GroupName != "") changeGroupReq.ChangeGroupOptionList.Add (ChangeGroupOption.GroupName);
            if (changeGroupReq.Owner != "") changeGroupReq.ChangeGroupOptionList.Add (ChangeGroupOption.GroupOwner);
            if (changeGroupReq.CustomProperties != "") changeGroupReq.ChangeGroupOptionList.Add (ChangeGroupOption.GroupCustomProperties);
            changeGroupReq.ChangeGroupOptionList.Add (ChangeGroupOption.GroupIsForbidJoin);
            
            Action<ResponseEvent> cb = eve =>
            {
                this.GroupUtil.SaveGroupInfo(eve, ((ChangeGroupRsp)eve.Data)?.GroupInfo);
                callback?.Invoke(eve);
            };
            
            Core.Group.ChangeGroup(changeGroupReq, cb);
        }
        
        public void RemoveGroupPlayer(RemoveGroupPlayerPara para, Action<ResponseEvent> callback)
        {
            RemoveGroupPlayerReq removeGroupPlayerReq = new RemoveGroupPlayerReq
            {
                GroupId = this.GroupInfo.Id,
                RemovePlayerId = para.RemovePlayerId
            };
            
            Action<ResponseEvent> cb = eve =>
            {
                this.GroupUtil.SaveGroupInfo(eve, ((RemoveGroupPlayerRsp)eve.Data)?.GroupInfo);
                callback?.Invoke(eve);
            };
            
            Core.Group.RemoveGroupPlayer(removeGroupPlayerReq, cb);
        }
        
        public void ChangeCustomGroupPlayerStatus(ChangeCustomGroupPlayerStatusPara para, Action<ResponseEvent> callback)
        {
            ChangeCustomGroupPlayerStatusReq req = new ChangeCustomGroupPlayerStatusReq
            {
                GroupId = this.GroupInfo.Id,
                CustomGroupPlayerStatus = para.CustomGroupPlayerStatus
            };
            
            Action<ResponseEvent> cb = eve =>
            {
                this.GroupUtil.SaveGroupInfo(eve, ((ChangeCustomGroupPlayerStatusRsp)eve.Data)?.GroupInfo);
                callback?.Invoke(eve);
            };
            
            Core.Group.ChangeCustomGroupPlayerStatus(req, cb);
        }
        
        public void SendToGroupClient(SendToGroupClientPara para, Action<ResponseEvent> callback)
        {
            SendToGroupClientReq req = new SendToGroupClientReq
            {
                GroupId = this.GroupInfo.Id,
                RecvType = para.RecvType,
                Msg = para.Msg
            };

            req.RecvPlayerList.AddRange(para.RecvPlayerList);
            
            Action<ResponseEvent> cb = eve =>
            {
                callback?.Invoke(eve);
            };
            
            Core.Group.SendToGroupClient(req, cb);
        }
    }
}