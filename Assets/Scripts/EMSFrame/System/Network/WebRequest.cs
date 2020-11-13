//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace UnityFrame{
	
	internal static class WebRequestRetcode{
		public const int SUCCESS = 0;//成功
		public const int ABNORMAL = 1;//异常
		public const int TIME_OUT = -1;	//超时
		public const int EXCEPTION_ON_REQUEST = -2;//在请求中抛出
		public const int EXCEPTION_ON_RESPONSE = -3;//在回复中抛出
		public const int FILE_NOT_EXIST = -301;//上传文件不存在
	}

	public class WebRequest {

		class AsyncRequestStruct
		{
			public int id;
			public int stamp;
			public HttpWebRequest request;
			public DelegateResponse callback;
		}

		//定义异步请求操作
		struct AsyncRequestOpera{
			public int stype;
			public string url;
			public string param;
			public string headinfo;
			public int timeOut;
			public DelegateResponse callback;
			public AsyncRequestOpera(int _stype,string _url,string _param,string _headinfo,int _timeOut,DelegateResponse _callback){
				stype = _stype;url=_url;param=_param;headinfo=_headinfo;timeOut=_timeOut;callback=_callback;
			}
		}

		private const int  TYPE_GET = 1;
		private const int  TYPE_POST = 2;

		private static int m_UID = 0;

		private int GID{
			get{ return m_UID++;}
		}


		private List<AsyncRequestStruct> m_ListAsyncHandle = new List<AsyncRequestStruct>();

		private List<AsyncRequestOpera> m_ListAsyncRequestOpera = new List<AsyncRequestOpera> ();


		private HttpWebRequest UF_CreateRequest(string url,int stype,string param,string headinfo,int timeout){
            HttpWebRequest request = System.Net.WebRequest.CreateHttp(url);
            //HttpWebRequest request = (HttpWebRequest)System.Net.HttpWebRequest.Create(url);

            request.KeepAlive = false;
			request.Proxy = null;
			request.Timeout = timeout;
			if(!string.IsNullOrEmpty(headinfo)){
				request.Headers.Add(HttpRequestHeader.Authorization,headinfo);
			}
			if(stype == TYPE_GET){
				request.Method = "GET";
				request.ContentType = "text/html;charset=UTF-8";
			}
			else{
				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded";
				request.ContentLength = param.Length;
				//请求不存在的服务器地址时,GetRequestStream 内部会进行DNS解析，导致线程阻塞（应该是C#的BUG）
				Stream stream = request.GetRequestStream ();
				StreamWriter writer = new StreamWriter(stream, System.Text.Encoding.ASCII);
				writer.Write(param);
				writer.Close ();
				stream.Close ();
			}

			return request;
		}

		private void UF_RemoveAsyncHandle(int id){
			lock(m_ListAsyncHandle){
				for(int k = 0;k < m_ListAsyncHandle.Count;k++){
					if (m_ListAsyncHandle[k].id == id) {
						m_ListAsyncHandle.RemoveAt(k);
						break;
					}
				}
			}
		}

		private void UF_InvokCallback(DelegateResponse callback,int retcode,object param){
			if (callback != null) {
				FrameHandle.UF_CallMethod(() => {
					if (callback != null) {
						callback(retcode, param);
					}
				});
			}
		}


		private void UF_CallbackAsynRequest(IAsyncResult result)
		{
			AsyncRequestStruct ars = ((AsyncRequestStruct)result.AsyncState);
			DelegateResponse call = ars.callback;
			HttpWebRequest _request = ars.request;
			int retcode = WebRequestRetcode.SUCCESS;
			byte[] bdata = null;
			HttpWebResponse response = null;

            //handle 列表中优先移除
            UF_RemoveAsyncHandle(ars.id);

			try{
				response = (HttpWebResponse)_request.EndGetResponse(result);
				Stream st = response.GetResponseStream();
				List<byte> tmpDatas = new List<byte>();
				int value = -1;
				while((value = st.ReadByte()) != -1){
					tmpDatas.Add((byte)value);
				}
				bdata = tmpDatas.ToArray();
				st.Close();
				//                response.Close();

				//300以上为异常请求
				if(retcode < (int)response.StatusCode ){
					retcode = WebRequestRetcode.SUCCESS;
				}
				else{
					Debugger.UF_Error("Http Response Abnormal: " + (int)response.StatusCode);
					retcode = WebRequestRetcode.ABNORMAL;
				}
			}
			catch(Exception e){
				Debugger.UF_Error("Http error<CallbackAsynRequest>:"+e.Message);
				retcode = WebRequestRetcode.EXCEPTION_ON_RESPONSE;
			}

			if (response != null) {
				response.Close ();
			}
			if (_request != null) {
				_request.Abort();
			}
            //回归主线程调用
            UF_InvokCallback(call,retcode,bdata);

		}



		/// <summary>
		/// 异步请求
		/// 注意回调函数可能在另外一个线程中执行，要避免线程引发的异常
		///  如果已经在执行AsynRequest 已经执行 或 BegineAsynRequest正在执行 ，该接口调用无效，将返回false
		/// </summary>
		public bool UF_AsynRequest(int stype,string url,string param,string headinfo,int timeOut,DelegateResponse callback){
			HttpWebRequest request = null;
			AsyncRequestStruct ars = new AsyncRequestStruct();
			int uid = GID;
			try{
				ars.id = uid;
				ars.stamp =System.Environment.TickCount;
				ars.callback = callback;
				lock(m_ListAsyncHandle){
					m_ListAsyncHandle.Add(ars);
				}

				request = UF_CreateRequest(url,stype,param,headinfo,timeOut);
				ars.request = request;
				ars.request.Timeout = timeOut;
				ars.request.BeginGetResponse(new AsyncCallback(UF_CallbackAsynRequest),ars);

				return true;
			}
			catch(Exception e){
                UF_RemoveAsyncHandle(uid);
				if (request != null) {
					request.Abort ();
				}
				Debugger.UF_Error ("Http error<AsynRequest>:" + e.Message);

                UF_InvokCallback(callback, WebRequestRetcode.EXCEPTION_ON_REQUEST, null);

			}
			return false;
		}


		/// <summary>
		/// 异步上传文件
		/// </summary>
		public bool UF_UploadFile(string url,string filePath,string headinfo,int timeOut,DelegateResponse callback){
			if(!File.Exists(filePath)){
				//文件不存在
				Debugger.UF_Error("UploadFile Not Exist: " + filePath);
				if (callback != null) {
					callback (WebRequestRetcode.FILE_NOT_EXIST,null);
				}
				return false;
			}

			AsyncRequestStruct ars = new AsyncRequestStruct();
			int uid = GID;
			HttpWebRequest request = null;
			try{
				ars.id = uid;
				ars.stamp =System.Environment.TickCount;
				ars.callback = callback;

				lock(m_ListAsyncHandle){
					m_ListAsyncHandle.Add(ars);
				}

				FileStream fs = File.OpenRead (filePath);
				byte[] buffer = new byte[fs.Length];
				fs.Read (buffer,0,(int)fs.Length);
				fs.Close ();

                //request = (HttpWebRequest)HttpWebRequest.Create(url);
                request = System.Net.WebRequest.CreateHttp(url);
                if (!string.IsNullOrEmpty(headinfo)){
					request.Headers.Add(HttpRequestHeader.Authorization,headinfo);
				}
				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded";

				request.ContentLength = buffer.Length;

				Stream postStream = request.GetRequestStream();
				postStream.Write (buffer,0,buffer.Length);
				postStream.Close ();


				ars.request = request;
				ars.request.Timeout = timeOut;
				ars.request.BeginGetResponse(new AsyncCallback(UF_CallbackAsynRequest),ars);
			}
			catch(Exception e){
                UF_RemoveAsyncHandle(uid);
				if (request != null) {
					request.Abort ();
				}
				Debugger.UF_Error ("Http error<UploadFile>:" + e.Message);
				if (callback != null) {
					callback (WebRequestRetcode.EXCEPTION_ON_REQUEST,null);
				}
				return false;
			}
			return true;
		}

        /// <summary>
        /// 异步请求
        /// 注意回调函数可能在另外一个线程中执行，要避免线程引发的异常
        /// 请求操作改为在轮训中进行，避免GetRequestStream中的DNS 请求导致当前请求线程卡住
        /// <summary>
        public bool UF_AsynPostRequest(string url,string postData,string headinfo,int timeOut,DelegateResponse callback){
			lock (m_ListAsyncRequestOpera) {
				m_ListAsyncRequestOpera.Add (new AsyncRequestOpera (TYPE_POST,url,postData,headinfo,timeOut,callback));
			}
			return true;
			//            return AsynRequest(TYPE_POST,url,postData,headinfo,timeOut,callback);

		}

        /// <summary>
        /// 异步请求
        /// 注意回调函数可能在另外一个线程中执行，要避免线程引发的异常
        /// <summary>
        public bool UF_AsynGetRequest(string url,string headinfo,int timeOut,DelegateResponse callback){
			lock (m_ListAsyncRequestOpera) {
				m_ListAsyncRequestOpera.Add (new AsyncRequestOpera (TYPE_GET,url,"",headinfo,timeOut,callback));
			}
			return true;
			//			return AsynRequest(TYPE_GET,url,"",headinfo,timeOut,callback);
		}


		//轮训查询超时的HTTP请求，并主动ABORT
		public void UF_Update(){
			if (m_ListAsyncRequestOpera.Count > 0) {
				lock (m_ListAsyncRequestOpera) {
					AsyncRequestOpera opera = m_ListAsyncRequestOpera [0];
					m_ListAsyncRequestOpera.RemoveAt (0);
                    UF_AsynRequest(opera.stype, opera.url, opera.param, opera.headinfo, opera.timeOut, opera.callback);
				}
			}
			if (m_ListAsyncHandle.Count > 0) {
				lock(m_ListAsyncHandle){
					for (int k = 0; k < m_ListAsyncHandle.Count; k++) {
						AsyncRequestStruct ars = m_ListAsyncHandle[k];
						int timestamp = Math.Abs(System.Environment.TickCount - ars.stamp);
						if (ars.request != null && ars.request.Timeout < timestamp) {
							Debugger.UF_Error(string.Format("Request TimeOut: Current:{0}    Stamp: {1} ", System.Environment.TickCount, ars.stamp));
							m_ListAsyncHandle.RemoveAt(k);
							try{
                                ars.request.Abort();
							}
							catch(System.Exception e){
								Debugger.UF_Exception(e);
							}
                            //回归主线程调用
                            UF_InvokCallback(ars.callback,WebRequestRetcode.TIME_OUT, null);
							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// 同步请求
		/// </summary>
		public byte[] UF_Request(int stype,string url,string param,int timeOut){
			int statusCode = -1;
			byte[] respData = null;
			try{
				HttpWebRequest request = this.UF_CreateRequest(url,stype,param,"",timeOut);

				request.ServicePoint.BindIPEndPointDelegate = delegate(
					ServicePoint servicePoint,
					IPEndPoint remoteEndPoint,
					int retryCount) {

					if (remoteEndPoint.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6) {
						return new IPEndPoint(IPAddress.IPv6Any, 0);
					} else {
						return new IPEndPoint(IPAddress.Any, 0);
					}
				};
				request.Timeout = timeOut;
				using(HttpWebResponse response = (HttpWebResponse)request.GetResponse()){
					Stream datastream = response.GetResponseStream();
					respData = new byte[datastream.Length];
					datastream.Read(respData,0,respData.Length);
					datastream.Close();
					statusCode = (int)response.StatusCode;
					if(statusCode > (int)response.StatusCode ){
						Debugger.UF_Error("Http Response Abnormal: " + statusCode);
					}
					response.Close();
				}
			}
			catch(Exception e){
				Debugger.UF_Error("Http Request Error :"+e.Message);
			}
			return respData;
		}


        //清除当前全部请求
        public void UF_Clear()
        {
            try
            {
                lock (m_ListAsyncRequestOpera)
                {
                    m_ListAsyncRequestOpera.Clear();
                }
                lock (m_ListAsyncHandle)
                {
                    // 中断所有进行中的请求
                    lock (m_ListAsyncHandle)
                    {
                        for (int k = 0; k < m_ListAsyncHandle.Count; k++) {
                            AsyncRequestStruct ars = m_ListAsyncHandle[k];
                            try{ars.request.Abort();}
                            catch (System.Exception e){Debugger.UF_Exception(e);}
                        }
                        m_ListAsyncHandle.Clear();
                    }
                }
            }
            catch (System.Exception ex) {
                Debugger.UF_Exception(ex);
            }
        }


        public byte[] UF_Get(string url, int timeOut){
			return UF_Request(TYPE_GET,url,"",timeOut);
		}
		public byte[] UF_Post(string url,string postData, int timeOut){
			return UF_Request(TYPE_POST,url,postData,timeOut);
		}

		public string UF_GetForMsg(string url, int timeOut){
			byte[] data = UF_Get(url,timeOut);
			if(data != null && data.Length > 0){
				return System.Text.Encoding.UTF8.GetString(data);
			}
			return string.Empty;
		}

		public string UF_PostForMsg(string url,string postData, int timeOut){
			byte[] data = UF_Post(url,postData,timeOut);
			if(data != null && data.Length > 0){
				return System.Text.Encoding.UTF8.GetString(data);
			}
			return string.Empty;
		}




	}


}