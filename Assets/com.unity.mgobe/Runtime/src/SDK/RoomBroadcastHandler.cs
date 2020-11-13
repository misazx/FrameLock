using System;


namespace com.unity.mgobe.src.SDK
{
    public abstract class RoomBroadcastHandler {
        
        public Action<BroadcastEvent> OnJoinRoom { get; set; }

        public Action<BroadcastEvent> OnLeaveRoom { get; set; }

        public Action<BroadcastEvent> OnDismissRoom { get; set; }

        public Action<BroadcastEvent> OnChangeRoom { get; set; }

        public Action<BroadcastEvent> OnRemovePlayer { get; set; }

        public Action<BroadcastEvent> OnRecvFromClient { get; set; }

        public Action<BroadcastEvent> OnRecvFromGameSvr { get; set; }

        public Action<BroadcastEvent> OnChangePlayerNetworkState { get; set; }

        public Action<BroadcastEvent> OnChangeCustomPlayerStatus { get; set; }

        public Action<BroadcastEvent> OnStartFrameSync { get; set; }

        public Action<BroadcastEvent> OnStopFrameSync { get; set; }

        public Action<BroadcastEvent> OnRecvFrame { get; set; }

        public Action<BroadcastEvent> OnAutoRequestFrameError { get; set; }

        public static Action<BroadcastEvent> OnMatch { get; set; }

        public static Action<BroadcastEvent> OnCancelMatch { get; set; }
    }
}