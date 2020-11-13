using System;

namespace com.unity.mgobe.Runtime.src.Group
{
    public abstract class GroupBroadcastHandler
    {
        public Action<BroadcastEvent> OnJoinGroup { get; set; }
        public Action<BroadcastEvent> OnLeaveGroup { get; set; }
        public Action<BroadcastEvent> OnDismissGroup { get; set; }
        public Action<BroadcastEvent> OnChangeGroup { get; set; }
        public Action<BroadcastEvent> OnRemoveGroupPlayer { get; set; }
        public Action<BroadcastEvent> OnChangeGroupPlayerNetworkState { get; set; }
        public Action<BroadcastEvent> OnChangeCustomGroupPlayerStatus { get; set; }
        public Action<BroadcastEvent> OnRecvFromGroupClient { get; set; }
    }
}