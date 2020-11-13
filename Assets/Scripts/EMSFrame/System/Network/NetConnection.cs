//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System;
using System.Net;
using System.Net.Sockets;

namespace UnityFrame{
    public enum ConnectType
    {
        TCP,
        UDP
    }

    internal enum NETCONNECT_STATE{
		NONE = 0,
		CLOSED = -1,
		TIMEOUT = -2,
		WAITING = 2,
		OPENED = 3,
	}

    internal interface ISocket {
        void UF_Connect(IPEndPoint remoteEP,bool blocking);
        bool UF_Poll(SelectMode selectMode);
        int UF_Send(byte[] buffer,int size);
        int UF_Receive(byte[] buffer);
        void UF_Close();
    }
    internal class TcpSocket : ISocket
    {
        private Socket m_Socket;
        public TcpSocket(AddressFamily addressFamily){
            m_Socket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public void UF_Connect(IPEndPoint remoteEP, bool blocking) {
            m_Socket.Blocking = blocking;
            m_Socket.Connect(remoteEP);
        }

        public bool UF_Poll(SelectMode selectMode) {
            return m_Socket.Poll(1, selectMode);
        }

        public int UF_Send(byte[] buffer, int size) {
            if (m_Socket.Connected)
            {
                return m_Socket.Send(buffer, 0, size, SocketFlags.None);
            }
            else {
                Debugger.UF_Error("TCP Socket Need To Connect Before Send Bytes");
                return 0;
            }           
        }

        public int UF_Receive(byte[] buffer) {
            return m_Socket.Receive(buffer);
        }

        public void UF_Close() {
            m_Socket.Close();
        }
    }

    internal class UdpSocket : ISocket
    {
        private Socket m_Socket;
        private IPEndPoint m_SendRemoteEP;
        private EndPoint m_RecvRemoteEP;

        public IPEndPoint sendRemoteEP { get { return m_SendRemoteEP as IPEndPoint; } }
        public IPEndPoint recvRemoteEP { get { return m_RecvRemoteEP as IPEndPoint; } }

        public UdpSocket(AddressFamily addressFamily){
            m_Socket = new Socket(addressFamily, SocketType.Dgram, ProtocolType.Udp);
        }

        public int UF_Send(byte[] buffer, int size)
        {
            if (m_SendRemoteEP == null) {
                Debugger.UF_Error("UDP Socket Need To Connect Before Send Bytes");
                return 0;
            }
            else
                return m_Socket.SendTo(buffer,0,size, SocketFlags.None, m_SendRemoteEP);
        }

        public bool UF_Poll(SelectMode selectMode)
        {
            return m_Socket.Poll(1, selectMode);
        }

        public void UF_Connect(IPEndPoint remoteEP, bool blocking)
        {
            m_Socket.Blocking = blocking;
            //重新拷贝新的EP
            m_SendRemoteEP = new IPEndPoint(remoteEP.Address, remoteEP.Port);
        }

        public int UF_Receive(byte[] buffer)
        {
            if (m_RecvRemoteEP == null) {
                m_RecvRemoteEP = new IPEndPoint(IPAddress.Any, 0) as EndPoint;
            }
            return m_Socket.ReceiveFrom(buffer,ref m_RecvRemoteEP);
        }

        public int Receive(byte[] buffer,ref EndPoint remoteEP)
        {
            return m_Socket.ReceiveFrom(buffer, ref remoteEP);
        }

        public void UF_Close()
        {
            m_Socket.Close();
        }

    }

    internal class NetConnection : IDisposable{
		private ISocket m_Socket;

		private CBytesBuffer m_ReadBuffer;
		private CBytesBuffer m_WriteBuffer;

		private NETCONNECT_STATE m_State;

		private DelegateConnectState m_OnConnectState;

		private int m_LastExecTime;


		private int m_ConnectTimeOut = 5000;

		private string m_Host;
		private int m_Port;
		private string m_HostPort;

		//心跳间隔
		private const int HeatInteral = 10000;
		//心跳协议号
		private const int HeatProtocalID = 10001000;
		//心跳包协议
		private ProtocalData m_ProtocalDataHeard;
		//上一次发送心跳的时间戳
		private int m_HeadTimeStamp = 0;

		//每次最大读入1024
		private byte[] m_ReceiveBytes = new byte[1024];

        private byte[] m_TempCheckBuff = new byte[1];

		

		public NETCONNECT_STATE connectState{get{ return m_State;}}
		public DelegateConnectState onConnectStateChange{set{m_OnConnectState = value;}}
		public int timeOut{get{return m_ConnectTimeOut;}set{ m_ConnectTimeOut = value;}}
		public string host{get{ return m_Host;}}
		public int port{get{ return m_Port;}}
		public string hostPort{get{ return m_HostPort;}}

        internal int readBufferSize { get { return m_ReadBuffer.UF_getSize();} }
        internal int writeBufferSize { get { return m_WriteBuffer.UF_getSize(); } }

        public NetConnection(string host,int port, ConnectType cType)
        {
			
			m_ReadBuffer = new CBytesBuffer (256);
			m_WriteBuffer = new CBytesBuffer (256);

			m_State = NETCONNECT_STATE.NONE;

            m_Host = host;
            m_Port = port;

            AddressFamily addressFamily = AddressFamily.InterNetwork;
            //转化IPV6地址
            UF_ParseIPType(host, port, out host, out addressFamily);

            m_HostPort = string.Format ("{0}:{1}", host, port);
            m_Socket = cType == ConnectType.TCP ? new TcpSocket(addressFamily) as ISocket : new UdpSocket(addressFamily) as ISocket;

			//心跳包协议号
			m_ProtocalDataHeard = new ProtocalData();
			m_ProtocalDataHeard.id = HeatProtocalID;

		}


		private void UF_ParseIPType(String serverIp, int serverPorts, out String newServerIp, out AddressFamily  mIPType)
		{
			mIPType = AddressFamily.InterNetwork;
			newServerIp = serverIp;
			try
			{
                #if UNITY_IPHONE && !UNITY_EDITOR
				string mIP = DLLImport.__GetIPv6(serverIp, serverPorts.ToString());
                #else
                string mIP = m_Host + "&&ipv4";
				#endif

				if (!string.IsNullOrEmpty(mIP))
				{
					string[] m_StrTemp = System.Text.RegularExpressions.Regex.Split(mIP, "&&");
					if (m_StrTemp != null && m_StrTemp.Length >= 2)
					{
						string IPType = m_StrTemp[1];
						if (IPType == "ipv6")
						{
							newServerIp = m_StrTemp[0].Trim();
							mIPType = AddressFamily.InterNetworkV6;

						}
					}
				}
			}
			catch (Exception e)
			{
				Debugger.UF_Exception (e);
			}

		}



		private static IPAddress UF_WrapIPAddress(string strIP,out string error){
			try{
				IPAddress address = null;
				IPAddress.TryParse(strIP,out address);
				if(address == null){
					IPAddress[] ip_ads = Dns.GetHostAddresses(strIP);
					address = ip_ads[0];
				}
				error = "";
				return address;
			}
			catch(Exception e){
				error = "WrapIPAddress error :"+e.Message;
				Debugger.UF_Error(error);
				return null;
			}
		}


        private void UF_ChangeConnectState(NETCONNECT_STATE state, bool notifyState = true)
        {
            if (m_State != state)
            {
                Debugger.UF_Log(string.Format("NetConnect State Change: {0} -> {1}", m_State.ToString(), state.ToString()));
                m_State = state;
                //变更通知
                if (notifyState && m_OnConnectState != null)
                {
                    m_OnConnectState(m_Host, m_Port, (int)m_State);
                }
            }
        }


        private bool UF_CheckIfConnectTimeOut(){
			int tick = Math.Abs (System.Environment.TickCount - m_LastExecTime);
			if (tick > m_ConnectTimeOut) {
				return true;
			}
			return false;
		}



		private bool UF_CheckReceiveZero(){
			try{
				return m_Socket.UF_Receive(m_TempCheckBuff) == 0;
			}
			catch(SocketException se){
				Debugger.UF_Exception (se);
				return true;
			}
		}


		//检查连接状态
		public NETCONNECT_STATE UF_CheckConnectState()
		{
			switch (m_State) {
			case NETCONNECT_STATE.WAITING:
				if (UF_CheckIfConnectTimeOut()) {
                        UF_ChangeConnectState(NETCONNECT_STATE.TIMEOUT);
				} else {
					if (m_Socket.UF_Poll(SelectMode.SelectError)) {
                            UF_ChangeConnectState(NETCONNECT_STATE.CLOSED);
					}
					else if (m_Socket.UF_Poll(SelectMode.SelectWrite)) {
						if ( m_Socket.UF_Poll(SelectMode.SelectRead) && UF_CheckReceiveZero()) {
                                UF_ChangeConnectState(NETCONNECT_STATE.CLOSED);	
						} 
						else {
                                UF_ChangeConnectState(NETCONNECT_STATE.OPENED);
						}
					}
				}
				break;
			case NETCONNECT_STATE.OPENED:
				if (!m_Socket.UF_Poll(SelectMode.SelectWrite)) {
					if (m_Socket.UF_Poll(SelectMode.SelectError)) {
                            UF_ChangeConnectState(NETCONNECT_STATE.CLOSED);
					}
				}
				break;
			}
			return m_State;
		}




		private int UF_ReceiveBuffer(CBytesBuffer readbuffer){
			//采用Poll 检测，非阻塞
			if (readbuffer != null && m_State == NETCONNECT_STATE.OPENED && m_Socket.UF_Poll(SelectMode.SelectRead)) {
				try{
					int readsize = m_Socket.UF_Receive(m_ReceiveBytes);
					//	UnityEngine.Debug.Log (string.Format ("CORE--->> Receive Socket Buffer: {0}", readsize));
					if (readsize > 0) {
						//写入到readbuffer中
						readbuffer.UF_write(m_ReceiveBytes, readsize);
						return readsize;
					} else {
                        UF_ChangeConnectState(NETCONNECT_STATE.CLOSED);
						Debugger.UF_Error (string.Format("Socket readable size is zero,check if socket is break| {0}",this.hostPort));
					}
				}catch(SocketException se){
                    UF_ChangeConnectState(NETCONNECT_STATE.CLOSED);
					Debugger.UF_Error (string.Format("SocketException At HostPort:  {0}",this.hostPort));
					Debugger.UF_Exception (se);
				}
			}
			return 0;
		}




		private bool UF_SendBuffer(CBytesBuffer writeBuffer){
			if (writeBuffer != null && m_State == NETCONNECT_STATE.OPENED && m_Socket.UF_Poll(SelectMode.SelectWrite)) {
				if (writeBuffer.UF_getSize() > 0) {
                    //writeBuffer.reverse ();
					m_Socket.UF_Send(writeBuffer.Buffer,writeBuffer.UF_getSize());
                    return true;
				}
			} else {
                UF_ChangeConnectState(NETCONNECT_STATE.CLOSED);
				Debugger.UF_Error (string.Format("Socket writable failed,check if socket is break| {0}",this.hostPort));
			}
			return false;
		}



		/// <summary>
		/// 从socket 中读取数据并写入到readbuffer中
		/// </summary>
		private int UF_PollReadBuffer(){
			return UF_ReceiveBuffer(this.m_ReadBuffer);
			//			return m_ReadBuffer.getSize();
		}

		/// <summary>
		/// 发送writebuffer数据到连接结点
		/// </summary>
		private void UF_PollWriteBuffer(){
			if (m_WriteBuffer.UF_getSize() > 0) {
				if (UF_SendBuffer(this.m_WriteBuffer)) {
					this.m_WriteBuffer.UF_clear();
				}
			}
		}

		/// <summary>
		/// 轮询 心跳检查
		/// </summary>
		private void UF_PollHeadBeat(){
			int curInv = Math.Abs (System.Environment.TickCount - m_HeadTimeStamp);
			if (curInv > HeatInteral) {
				this.UF_Send(m_ProtocalDataHeard);
				m_HeadTimeStamp = System.Environment.TickCount;
			}
		}


		/// <summary>
		/// 轮询更新
		/// </summary>
		public void UF_Polling(){
			if (this.UF_CheckConnectState() == NETCONNECT_STATE.OPENED) {
                UF_PollReadBuffer();

                UF_PollWriteBuffer();

                UF_PollHeadBeat();
            } 
		}


		public bool UF_Connect(bool block = false){
			try{
				string errorMsg = "";
				IPAddress address = UF_WrapIPAddress(m_Host,out errorMsg);
				if (!string.IsNullOrEmpty (errorMsg)) {
					Debugger.UF_Error (string.Format("IPAddress Invalid:  IP <{0}>  port<{1}>  | error: {2}",m_Host,m_Port,errorMsg));
                    UF_ChangeConnectState(NETCONNECT_STATE.CLOSED);
					return false;
				}
                m_LastExecTime = System.Environment.TickCount;
                UF_ChangeConnectState(NETCONNECT_STATE.WAITING);
                m_Socket.UF_Connect(new IPEndPoint(address,m_Port), block);
				return true;

			}
			catch(SocketException se){
				if (se.ErrorCode == (int)SocketError.WouldBlock || se.ErrorCode == (int)SocketError.InProgress) {
					return true;
				} else {
                    UF_ChangeConnectState(NETCONNECT_STATE.CLOSED);
					Debugger.UF_Error (string.Format ("Connect Failed : IP<{0}>  Port<{1}>", m_Host, m_Port));
					Debugger.UF_Exception (se);
					return false;
				}
			}
		}

		//发送协议数据包
		public void UF_Send(ProtocalData packet){
			if (packet != null) {
				//把数据包中的数据序列号到写入缓冲中
				packet.UF_Write(this.m_WriteBuffer);
			}
		}
			
		//接收协议数据包
		public bool UF_Receive(ProtocalData outData){
			if (outData != null) {
				if (outData.UF_Read(this.m_ReadBuffer)) {
					////过滤心跳包
					//if (outData.id == HeatProtocalID)
					//	return false;
					//else
						return true;
				}
			}
			return false;
		}

		public bool UF_CheckCanReceive(int minPDSize){
			return this.m_ReadBuffer.UF_getSize() >= minPDSize;
		}

		protected void UF_RawClose(){
			try{
				m_Socket.UF_Close();
			}
			catch(Exception e){
				Debugger.UF_Exception (e);
			}
		}

		public void UF_Close(bool notifyState){
            UF_RawClose();
            UF_ChangeConnectState(NETCONNECT_STATE.CLOSED, notifyState);
        }

		public void Dispose (){
            UF_RawClose();
			m_OnConnectState = null;
			m_Socket = null;
		}

	}
}
