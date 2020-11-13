using System;
using System.Collections.Generic;

namespace com.unity.mgobe.Runtime.src.Broadcast
{
    public class GroupBroadcast
    {
        private readonly com.unity.mgobe.Group _group; 
        public GroupBroadcast (com.unity.mgobe.Group group)
        {
            this._group = group;
        }
        
        // 加入队组广播
        public void OnJoinGroup(BroadcastEvent eve) {
            var groupInfo = ((JoinGroupBst)eve.Data).GroupInfo;
            this.SaveAndInvoke (groupInfo, () => this._group?.OnJoinGroup(eve));
        }

        // 退出组队广播
        public void OnLeaveGroup(BroadcastEvent eve) {
            var groupInfo = ((LeaveGroupBst)eve.Data).GroupInfo;
            this.SaveAndInvoke (groupInfo, () => this._group?.OnLeaveGroup(eve));
        }

        // 解散队组广播
        public void OnDismissGroup(BroadcastEvent eve) {
            var groupInfo = ((DismissGroupBst)eve.Data).GroupInfo;
            this.MatchGroupIdAndInvoke(groupInfo, () =>
            {
                this._group?.OnDismissGroup(eve);
                this._group?.GroupUtil.SetGroupInfo(null);
            });
        }

        // 修改队组广播
        public void OnChangeGroup(BroadcastEvent eve) {
            var groupInfo = ((ChangeGroupBst)eve.Data).GroupInfo;
            this.SaveAndInvoke (groupInfo, () => this._group?.OnChangeGroup(eve));
        }

        // 移除队组内玩家广播
        public void OnRemoveGroupPlayer(BroadcastEvent eve) {
            var groupInfo = ((RemoveGroupPlayerBst)eve.Data).GroupInfo;
            this.SaveAndInvoke (groupInfo, () => this._group?.OnRemoveGroupPlayer(eve));
        }

        // 队组内玩家网络状态变化广播
        public void OnChangeGroupPlayerNetworkState(BroadcastEvent eve)
        {
            var bst = (ChangePlayerNetworkStateBst) eve.Data;
            var groupIdList =  bst.GroupIdList;
            var groupId = (this._group?.GroupInfo?.Id) + "";

            if (groupId.Length <= 0)
            {
                return;
            }
            
            if (groupIdList.Count <= 0)
            {
                return;
            }

            if (!groupIdList.Contains(groupId))
            {
                return;
            }

            if (bst.NetworkState == NetworkState.RelayOffline || bst.NetworkState == NetworkState.RelayOnline)
            {
                return;
            }
            
            // 更新队组信息
            this._group?.GroupUtil?.TriggerNetWorkBroadcast(bst, eve.Seq);
        }

        // 队组内玩家自定义状态变化广播
        public void OnChangeCustomGroupPlayerStatus(BroadcastEvent eve) {
            var groupInfo = ((ChangeCustomGroupPlayerStatusBst)eve.Data).GroupInfo;
            this.SaveAndInvoke (groupInfo, () => this._group?.OnChangeCustomGroupPlayerStatus(eve));
        }

        // 收到队组内其他玩家消息广播
        public void OnRecvFromGroupClient(BroadcastEvent eve) {
            var data = ((RecvFromGroupClientBst)eve.Data);
            if (data?.GroupId == this._group?.GroupInfo.Id)
            {
                this._group?.OnRecvFromGroupClient(eve);
            }
        }

        private bool MatchGroupInfo (GroupInfo groupInfo) {
            return this._group.GroupInfo.Id == groupInfo.Id;
        }
        
        private void SaveAndInvoke (GroupInfo groupInfo, Action callback) {
            if (!this.MatchGroupInfo (groupInfo)) {
                return;
            }
            this._group.GroupUtil.SetGroupInfo(new GroupInfo(groupInfo));
            callback?.Invoke();
        }
        
        private void MatchGroupIdAndInvoke (GroupInfo groupInfo, Action callback) {
            if (!this.MatchGroupInfo (groupInfo)) {
                return;
            }
            callback?.Invoke();
        }

    }
}