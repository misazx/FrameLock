//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

//UDP发送文件信息
namespace UnityFrame{
	public class UdpNetWriter : IWriter {
		private UdpSocket m_UDPSocket;

		public UdpNetWriter(string ip,int port)
		{
			try{
                UF_Close();
                m_UDPSocket = new UdpSocket(AddressFamily.InterNetwork);
                m_UDPSocket.UF_Connect(new IPEndPoint(IPAddress.Parse(ip), port),false);
            }
			catch(Exception e){
				m_UDPSocket = null;
                Debugger.UF_Exception(e);
            }
		}

		public void UF_Write(object param){
			if (param == null)
				return;
            
            if (m_UDPSocket != null) {
                byte[] bt = Encoding.UTF8.GetBytes(param.ToString());
                if (bt != null && bt.Length > 0) {
                    try {
                        m_UDPSocket.UF_Send(bt, bt.Length);
                    }
                    catch (Exception e)
                    {
                        this.UF_Close();
                        Debugger.UF_Exception(e);
                    }
                }
            }
		}
		
		public void UF_Close(){
			if(m_UDPSocket != null){
				m_UDPSocket.UF_Close();
				m_UDPSocket = null;
			}

		}

		~UdpNetWriter(){
			this.UF_Close();
		}
	}


}