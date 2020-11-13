using System;
using com.unity.mgobe.Runtime.src.Broadcast;
using com.unity.mgobe.src.Util;

namespace com.unity.mgobe
{
    public class GroupUtil
    {
        
        private readonly Group _group;

        public GroupUtil (Group group) {
            this._group = group;
            this._group.GroupInfo = new GroupInfo();
        }

        public void SetGroupInfo(GroupInfo groupInfo)
        {
            if (groupInfo == null)
            {
                groupInfo = new GroupInfo();
            }

            this._group.GroupInfo = groupInfo;

            this._group?.OnUpdate(this._group);
        }

        public void SaveGroupInfo(ResponseEvent eve, GroupInfo groupInfo)
        {
            if (eve.Code == ErrCode.EcOk)
            {
                this.SetGroupInfo(groupInfo);
            }
        }
        
        public void InitBroadcast()
        {
            if (this._group.GroupBroadcast == null)
            {
                this._group.GroupBroadcast = new GroupBroadcast(this._group);
            }
        }
        
        public void TriggerNetWorkBroadcast(ChangePlayerNetworkStateBst bst, string bstSeq)
        {
            if (this._group?.GroupInfo?.Id + "" == "")
            {
                return;
            }

            Action<GroupInfo, string> action = (groupInfo, seq) =>
            {
                if (groupInfo == null)
                {
                    return;
                }

                if (groupInfo.Id != this._group?.GroupInfo?.Id)
                {
                    return;
                }

                GroupPlayerInfo groupPlayerInfo = null;

                foreach (var playerInfo in groupInfo.GroupPlayerList)
                {
                    if (((GroupPlayerInfo) playerInfo).Id == bst.ChangePlayerId)
                    {
                        groupPlayerInfo = (GroupPlayerInfo) playerInfo;
                    }
                }

                if (groupPlayerInfo == null)
                {
                    return;
                }

                if (groupPlayerInfo.CommonGroupNetworkState != bst.NetworkState)
                {
                    return;
                }
                
                ChangeGroupPlayerNetworkStateBst stateBst = new ChangeGroupPlayerNetworkStateBst
                {
                    ChangePlayerId = bst.ChangePlayerId,
                    NetworkState = bst.NetworkState,
                    GroupInfo = groupInfo
                };

                BroadcastEvent eve = new BroadcastEvent(stateBst, bstSeq + "_" + seq);

                this._group?.OnChangeGroupPlayerNetworkState(eve);
            };

            this._group?.GetGroupDetail(eve =>
            {
                if (eve.Code != ErrCode.EcOk || eve.Data == null)
                {
                    return;
                }

                var rsp = (GetGroupByGroupIdRsp) eve.Data;
                var groupInfo = rsp.GroupInfo;
                
                action?.Invoke(groupInfo, eve.Seq);
            });
        }
    }
}