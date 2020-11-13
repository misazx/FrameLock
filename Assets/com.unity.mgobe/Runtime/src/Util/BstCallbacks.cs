using System;
using System.Collections.Generic;
using com.unity.mgobe.Runtime.src.Broadcast;
using com.unity.mgobe.src.Broadcast;

namespace com.unity.mgobe.src.Util
{
    public class BstCallbacks
    {
        // 房间广播
        public InnerRoomBstHandler Room = new InnerRoomBstHandler();
        
        // 队组广播
        public InnerGroupBstHandler Group = new InnerGroupBstHandler();
        
        // 清除全部广播回调函数
        public void ClearCallbacks()
        {
	        this.Room.ClearCallbacks();
	        this.Group.ClearCallbacks();
        }
        
        // 本地网络变化
        public void OnNetwork(ResponseEvent eve)
        {
            this.Room.OnNetwork(eve);
        }
    }

    public class CallbackHandler<T>
    {
        private readonly HashSet<T> _broadcasts = new HashSet<T>();
        
        public void BindCallbacks(T broadcast)
        {
	        if (!this._broadcasts.Contains(broadcast))
	        {
		        this._broadcasts.Add(broadcast);
	        }
        }
        
        public void UnbindCallbacks(T broadcast)
        {
	        if (this._broadcasts.Contains(broadcast))
	        {
		        this._broadcasts.Remove(broadcast);
	        }
        }
        
        public void ClearCallbacks()
        {
            this._broadcasts.Clear();
        }
        
        protected void HandleBst(Action<T> action)
        {
            foreach (var broadcast in this._broadcasts)
            {
                action(broadcast);
            }
        }
    }

    public class InnerRoomBstHandler: CallbackHandler<RoomBroadcast>
    {
	    private static GlobalRoomBroadcast _globalBroadcast;

	    public void BindGlobalCallback(GlobalRoomBroadcast globalBroadcast)
	    {
		    _globalBroadcast = globalBroadcast;
	    }
	    
        //  玩家加入房间广播
        public void OnJoinRoom(BroadcastEvent eve) {
            this.HandleBst(broadcast => broadcast?.OnJoinRoom(eve));
        }

		// 玩家退出房间广播
		public void OnLeaveRoom(BroadcastEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnLeaveRoom(eve));
		}

		// 玩家解散房间广播
		public void OnDismissRoom(BroadcastEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnDismissRoom(eve));
		}

		// 玩家修改房间广播
		public void OnChangeRoom(BroadcastEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnChangeRoom(eve));
		}

		// 玩家被踢广播
		public void OnRemovePlayer(BroadcastEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnRemovePlayer(eve));
		}

		// 匹配超时广播
		public void OnMatchTimeout(BroadcastEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnMatchTimeout(eve));
			// 全局广播
			_globalBroadcast.OnMatchTimeout(eve);
		}

		// 玩家匹配成功广播
		public void OnMatchPlayers(BroadcastEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnMatchPlayers(eve));
			// 全局广播
			_globalBroadcast.OnMatchPlayers(eve);
		}

		// 取消组队匹配广播
		public void OnCancelMatch(BroadcastEvent eve) {
			// 全局广播
			_globalBroadcast.OnCancelMatch(eve);
		}

		// 收到消息广播
		public void OnRecvFromClient(string roomId, BroadcastEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnRecvFromClient(roomId, eve));
		}

		// 自定义服务广播
		public void OnRecvFromGameSvr(string roomId, BroadcastEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnRecvFromGameSvr(roomId, eve));
		}

		// 玩家网络状态变化广播
		public void OnChangePlayerNetworkState(BroadcastEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnChangePlayerNetworkState(eve));
		}

		// 收到帧同步消息
		public void OnRecvFrame(BroadcastEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnRecvFrame(eve));
		}

		// 玩家修改玩家状态广播
		public void OnChangeCustomPlayerStatus(BroadcastEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnChangeCustomPlayerStatus(eve));
		}

		// 开始帧同步广播
		public void OnStartFrameSync(BroadcastEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnStartFrameSync(eve));
		}

		// 结束帧同步广播
		public void OnStopFrameSync(BroadcastEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnStopFrameSync(eve));
		}

		// 本地网络状态变化
		public void OnNetwork(ResponseEvent eve) {
			this.HandleBst(broadcast => broadcast?.OnNetwork(eve));
		}
    }

    public class InnerGroupBstHandler: CallbackHandler<GroupBroadcast>
    {
	    // 加入队组广播
	    public void OnJoinGroup(BroadcastEvent eve) {
		    this.HandleBst(broadcast => broadcast?.OnJoinGroup(eve));
	    }

	    // 退出组队广播
	    public void OnLeaveGroup(BroadcastEvent eve) {
		    this.HandleBst(broadcast => broadcast?.OnLeaveGroup(eve));
	    }

	    // 解散队组广播
	    public void OnDismissGroup(BroadcastEvent eve) {
		    this.HandleBst(broadcast => broadcast?.OnDismissGroup(eve));
	    }

	    // 修改队组广播
	    public void OnChangeGroup(BroadcastEvent eve) {
		    this.HandleBst(broadcast => broadcast?.OnChangeGroup(eve));
	    }

	    // 移除队组内玩家广播
	    public void OnRemoveGroupPlayer(BroadcastEvent eve) {
		    this.HandleBst(broadcast => broadcast?.OnRemoveGroupPlayer(eve));
	    }

	    // 队组内玩家网络状态变化广播
	    public void OnChangeGroupPlayerNetworkState(BroadcastEvent eve) {
		    this.HandleBst(broadcast => broadcast?.OnChangeGroupPlayerNetworkState(eve));
	    }

	    // 队组内玩家自定义状态变化广播
	    public void OnChangeCustomGroupPlayerStatus(BroadcastEvent eve) {
		    this.HandleBst(broadcast => broadcast?.OnChangeCustomGroupPlayerStatus(eve));
	    }

	    // 收到队组内其他玩家消息广播
	    public void OnRecvFromGroupClient(BroadcastEvent eve) {
		    this.HandleBst(broadcast => broadcast?.OnRecvFromGroupClient(eve));
	    }
    }
}