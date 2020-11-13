//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace UnityFrame{

	public class NetworkSystem : HandleSingleton<NetworkSystem>,IOnAwake,IOnStart,IOnApplicationPause,IOnApplicationQuit,IOnReset {
		//线程运行频率
		private const int ThreadSleepRate = 10;
		//all connections
		private List<NetConnection> m_ListConnections = new List<NetConnection>();
		//接收到的网络数据包
		private List<ProtocalData> m_ListProtocalDatas = new List<ProtocalData>();

        //sending buffer
        private ProtocalData m_ProtoDataCache = new ProtocalData();

		private Thread m_NetThread;

		private bool m_RunFlag = false;

		//Http请求
		private WebRequest m_WebRequest = new WebRequest();

		private long m_TickCount = 0;

		public long tickCount{get{ return m_TickCount;}}

		public int connectCount{get{ return m_ListConnections.Count;}}

		public int protocalDataCount{get{return m_ListProtocalDatas.Count;}}

		internal void UF_ReceiveProtocalDatas(List<ProtocalData> inList){
			if (inList == null)
				return;
			if (m_ListProtocalDatas.Count > 0) {
				try{
					Monitor.Enter(m_ListProtocalDatas);
					inList.Clear();
                    //拷贝引用
                    for (int k = 0;k < m_ListProtocalDatas.Count;k++){
						inList.Add(m_ListProtocalDatas[k]);
					}
					m_ListProtocalDatas.Clear();
				}
				catch(System.Exception ex){
					Debugger.UF_Exception (ex);
				}
				finally{
					Monitor.Exit(m_ListProtocalDatas);
				}
			}
		}


		private NetConnection UF_GetConnection(string host,int port){
			//for sample
			NetConnection connection = null;
			for (int k = 0; k < m_ListConnections.Count; k++) {
				if (m_ListConnections[k].host == host && m_ListConnections[k].port == port) {
                    connection = m_ListConnections[k];
                    break;
				}
			}
			return connection;
		}


		public void UF_SendMessage(string host,int port,int protocol,int corcode,byte[] buffer,int size){
			NetConnection connection = UF_GetConnection(host, port);
			if (connection == null) {
				Debugger.UF_Error(string.Format("Send Net Message Error: connection<{0}:{1}> is not Exist | protocol<{2}>",host,port,protocol.ToString("x")));
				return;
			}

			if (connection.connectState == NETCONNECT_STATE.OPENED) {
				lock (m_ListConnections) {
					m_ProtoDataCache.UF_Clear();
					m_ProtoDataCache.UF_SetID(protocol);
					m_ProtoDataCache.UF_SetCorCode(corcode);
					m_ProtoDataCache.UF_SetBodyBuffer(buffer, size);
					connection.UF_Send(m_ProtoDataCache);

					Debugger.UF_TrackNetProtol(m_ProtoDataCache.id,m_ProtoDataCache.size,m_ProtoDataCache.corCode);
				}
			} else {
				Debugger.UF_Error(string.Format("Send Net Message Error: connection<{0}:{1}> is not connected | protocol<{2}>",host,port,protocol.ToString("x")));

			}
		}


		/// <summary>
		/// 线程循环
		/// </summary>
		private void UF_ThreadUpdateLoop(){

			while (m_RunFlag) {
				try{
					while(m_RunFlag){
						Thread.Sleep(ThreadSleepRate);
						m_TickCount++;
						NetConnection connection = null;
						if(m_ListConnections.Count != 0){
							//Select Mode
							lock(m_ListConnections){
								for(int k = 0;k < m_ListConnections.Count;k++){
									connection = m_ListConnections[k];
									//轮询更新
									connection.UF_Polling();

									if(connection.connectState == NETCONNECT_STATE.CLOSED ||connection.connectState == NETCONNECT_STATE.TIMEOUT)
                                    {
										connection.Dispose();
										m_ListConnections.RemoveAt(k);
										k--;
									}
									else if(connection.connectState == NETCONNECT_STATE.OPENED){
                                        //读取NetPacket										
                                        bool _ReadOver = false;
                                        while (!_ReadOver && connection.UF_CheckCanReceive(ProtocalData.HEAD_SIZE))
                                        {
                                            ProtocalData packet = ProtocalData.UF_Acquire();
                                            if (connection.UF_Receive(packet))
                                            {
                                                lock (m_ListProtocalDatas)
                                                {
                                                    m_ListProtocalDatas.Add(packet);
                                                    Debugger.UF_TrackNetProtol(packet.id, packet.size, packet.corCode, 1);
                                                }
                                            }
                                            else
                                            {
                                                packet.UF_Release();
                                                _ReadOver = true;
                                            }
                                        }
                                    }
								}
							}
						}

                        UF_UpdateHttpLoop();
					}
				}
				catch(System.Exception e){
					Debugger.UF_Exception (e);
				}
			}
		}

		//检查是否连接到目标地址端口
		public bool UF_CheckNetConnected(string host,int port){
			bool ret = false;
			lock (m_ListConnections) {
				NetConnection connection = UF_GetConnection(host, port);
				if (connection != null) {
					return connection.connectState == NETCONNECT_STATE.OPENED;
				}
			}
			return ret;
		}


		public static void UF_OnConnectionStateChange(string ip,int port,int  state){
			MessageSystem.UF_GetInstance ().UF_Post(DefineEvent.E_NET_CONNECT_STATE,ip,port,state);
		}


        //加入连接
        //connectType == 0 Tcp
        //connectType == 1 Udp
        public bool UF_AddConnection(string host,int port,int timeout, ConnectType connectType){
			lock (m_ListConnections) {
				NetConnection connection = null;
				for (int k = 0; k < m_ListConnections.Count; k++) {
					connection = m_ListConnections [k];
					if (connection.host == host && connection.port == port) {
						NETCONNECT_STATE state = connection.connectState;
						if (state == NETCONNECT_STATE.CLOSED || state == NETCONNECT_STATE.NONE) {
							connection.Dispose ();
							return false;
						} else if (state == NETCONNECT_STATE.WAITING) {
							Debugger.UF_Error(string.Format ("Connection<{0}:{1}> is already waiting for connection!", host, port));
							return false;
						} else if (state == NETCONNECT_STATE.OPENED) {
                            Debugger.UF_Error(string.Format ("Connection<{0}:{1}> has already connected!", host, port));
							return false;
						}
					}
				}
				connection = new NetConnection (host, port, connectType);
				//设置状态回调函数
				connection.onConnectStateChange = UF_OnConnectionStateChange;
				connection.timeOut = timeout;
				m_ListConnections.Add (connection);
				connection.UF_Connect();
			}
			return true;
		}


		public void UF_CloseConnection(string host,int port,bool notifyState){
			lock (m_ListConnections) {
				NetConnection connection = null;
				for (int k = 0; k < m_ListConnections.Count; k++) {
					connection = m_ListConnections [k];
					if (connection.host == host && connection.port == port) {
						connection.UF_Close(notifyState);
						connection.Dispose ();
						m_ListConnections.RemoveAt (k);
						break;
					}
				}
			}
		}

        public bool UF_CheckNetworkReachable() {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        public void UF_CloseAllConnection(bool notifyState) {
            lock (m_ListConnections)
            {
                NetConnection connection = null;
                for (int k = 0; k < m_ListConnections.Count; k++)
                {
                    connection = m_ListConnections[k];
                    connection.UF_Close(notifyState);
                    connection.Dispose();
                }
                m_ListConnections.Clear();
            }
        }

		private void UF_UpdateHttpLoop(){
			if(m_WebRequest != null){
				m_WebRequest.UF_Update();
			}
		}

		//异步的http 请求
		public void UF_HttpGetRequest(string url,string headinfo,int timeOut,DelegateResponse callback){
			if (m_WebRequest != null) {
				m_WebRequest.UF_AsynGetRequest(url,headinfo,timeOut,callback);
			}
		}

		public void UF_HttpPostRequest(string url,string postData,string headinfo,int timeOut,DelegateResponse callback){
			if (m_WebRequest != null) {
				m_WebRequest.UF_AsynPostRequest(url,postData,headinfo,timeOut,callback);
			}
		}


		public void UF_HttpUploadFile(string url,string filePath,string headinfo,int timeOut,DelegateResponse callback){
			if (m_WebRequest != null) {
				m_WebRequest.UF_UploadFile(url,filePath,headinfo,timeOut,callback);
			}
		}

        //动态获取本机ip地址
        public string UF_GetHostIP()
        {
            try
            {
                NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adater in adapters)
                {
                    if (adater.Supports(NetworkInterfaceComponent.IPv4))
                    {
                        UnicastIPAddressInformationCollection UniCast = adater.GetIPProperties().UnicastAddresses;
                        if (UniCast.Count > 0)
                        {
                            foreach (UnicastIPAddressInformation uni in UniCast)
                            {
                                if (uni.Address.AddressFamily == AddressFamily.InterNetwork)
                                {
                                    //Debug.Log(uni.Address.ToString());
                                    return uni.Address.ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception e) {
                Debugger.UF_Exception(e);
            }
            return string.Empty;
        }


        /// <summary>
        /// 启动网络管理器
        /// 网络线程启动
        /// </summary>
        public void UF_Start(){
			if (m_NetThread == null) {
				m_RunFlag = true;
				m_NetThread = new Thread (new ThreadStart (UF_ThreadUpdateLoop));
				m_NetThread.Start ();
			}
		}
		 
		/// <summary>
		/// 关闭网络管理器
		/// 网络线程关闭
		/// 所有连接将断开
		/// </summary>
		public void UF_Stop(){
			m_RunFlag = false;
			if (m_NetThread != null) {
				if (m_NetThread.ThreadState == ThreadState.Running) {
					m_NetThread.Abort ();
				}
                m_NetThread = null;
            }
			lock (m_ListConnections) {
				for (int k = 0; k < m_ListConnections.Count; k++) {
					m_ListConnections [k].Dispose ();
				}
				m_ListConnections.Clear ();
			}
		}

		public override string ToString ()
		{
			System.Text.StringBuilder sb = StrBuilderCache.Acquire ();
			sb.Append (string.Format ("tickCount={0} \n connectCount={1}", tickCount, connectCount));
			sb.Append ("connectInfo:\n");
			for (int k = 0; k < m_ListConnections.Count; k++) {
				if (m_ListConnections [k] != null) {
					NetConnection connection = m_ListConnections [k];	
					sb.Append(string.Format ("\tIP:{0} | Port:{1} | State:{2} | R:{3} | W:{4}\n",
                        connection.host,
                        connection.port,
                        connection.connectState,
                        connection.readBufferSize,
                        connection.writeBufferSize));
				}
			}
			return StrBuilderCache.GetStringAndRelease(sb);
		}


		public void UF_OnStart(){
			//this.Start ();
		}

        public void UF_OnAwake() {
            this.UF_Start();
        }


        public void OnApplicationPause(bool state){
//			if (state) {
//				m_HeartBeatFlag = false;
//			} else {
//				m_HeartBeatFlag = true;
//			}
		}

		public void OnApplicationQuit(){
			this.UF_Stop();
            //清除全部请求
            m_WebRequest.UF_Clear();
        }


        public void UF_OnReset() {
            lock (m_ListConnections)
            {
                foreach (var connection in m_ListConnections) {
                    connection.UF_Close(false);
                }
                m_ListConnections.Clear();
            }
            //清除全部请求
            m_WebRequest.UF_Clear();

            ProtocalData[] pdatas = null;

            lock (m_ListProtocalDatas) {
                if (m_ListProtocalDatas.Count > 0) {
                    pdatas = m_ListProtocalDatas.ToArray();
                    m_ListProtocalDatas.Clear();
                }
            }

            if(pdatas != null)
            foreach (var item in pdatas)
            {
                item.UF_Release();
            }

        }





	}
}