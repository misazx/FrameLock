//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System;
using System.Net;
using System.Collections;
using System.IO;
using UnityEngine;

namespace UnityFrame{
	//Http 下载,集成断点续存
	public class WebDownload
	{
		private string m_Url;
		private string m_SavePath;
		private const int m_ReadBufferSize = 1024*100;
		private bool m_ShowLog;
		private long m_Length;
		private long m_CurrentLength;
		private string m_FileName;
		private HttpWebResponse m_Response;
		private bool m_isWaitingResponse;
		private string m_ErrMsg;
		private int m_RetCode;
		private float m_Speed;


		internal class AsyncStruct
		{
			public WebDownload downloader;
			public HttpWebRequest request;
		}

		private static string UF_ParseFileName(string Url)
		{
			if (Url == null)
				return null;
			int index = Url.LastIndexOf ("\\");
			if (index < 0)
				index = Url.LastIndexOf ("/");
			if (index < 0)
				return Url;
			return Url.Substring (index + 1);
		}

		private void UF_DebugLog(string Message)
		{
			if (m_ShowLog)
				Debugger.UF_Log(Message);
		}

		public void UF_download(string Url,string SavePath,bool ShowLog = false)
		{
			m_Url = Url;
			m_SavePath = SavePath;
			m_ShowLog = ShowLog;
			m_CurrentLength = m_Length = 0;
			m_RetCode = 0;
			m_ErrMsg = "";

		}

		public float Progress
		{
			get { if ( m_Length == 0 ) return 0; return (m_CurrentLength*10000 / m_Length) * 0.0001f; }
		}

		public float Speed{
			get{return m_Speed;}
		}

		public long UF_GetLength()
		{
			return m_Length;
		}

		public long UF_GetCurrentLength()
		{
			return m_CurrentLength;
		}

		public string UF_GetFileName()
		{
			return m_FileName;
		}

		public int UF_GetRetCode()
		{
			return m_RetCode;
		}

		public string UF_GetErrMsg()
		{
			return m_ErrMsg;
		}

		public IEnumerator UF_AsyncGetResp(HttpWebRequest Request)
		{
			UF_DebugLog("begin Get Response");
			this.m_Response = null;
			this.m_isWaitingResponse = true;
			AsyncStruct asyncstruct = new AsyncStruct ();
			asyncstruct.downloader = this;
			asyncstruct.request = Request;
			Request.BeginGetResponse (new AsyncCallback (UF_CallBack_Resp), asyncstruct);
			for ( int i = 0  ; i < 20 && m_isWaitingResponse ; i ++ )
			{
				UF_DebugLog ("checking " + m_isWaitingResponse + " : " + i);
				//check every 0.5 seconds
				yield return new WaitForSeconds(0.5f);
				if ( m_Response != null )
				{
					UF_DebugLog("Get Response success");
					break;
				}
			}
			m_isWaitingResponse = false;
			if ( m_Response == null && m_RetCode == 0 )
			{
				m_RetCode = 401;
				m_ErrMsg = "Wait Response Time Out";
			}
		}

		public static void UF_CallBack_Resp(IAsyncResult result)
		{
			HttpWebRequest req = ((AsyncStruct)result.AsyncState).request;
			WebDownload downloader = ((AsyncStruct)result.AsyncState).downloader;
			if ( downloader.m_isWaitingResponse )
			{
				downloader.m_isWaitingResponse = false;
				try { 
					downloader.m_Response = (HttpWebResponse)req.EndGetResponse(result); 
				} catch (WebException e) {
					downloader.UF_DebugLog(e.ToString());
					downloader.m_ErrMsg = e.ToString();
					downloader.m_RetCode = (int)((HttpWebResponse)e.Response).StatusCode;
				}
			}
		}

		//使用协程异步下载
		public IEnumerator UF_BeginDownLoad(MonoBehaviour Parent,bool isCont=false)
		{
			m_FileName = UF_ParseFileName(m_Url);
			if (m_FileName == null) {
				m_RetCode = -10;
				m_ErrMsg = "FileName is NULL";
				yield break;
			}

			string localfilename = m_SavePath + "/" + m_FileName;
			System.IO.Stream outstream = null;
			FileInfo fi = new FileInfo (localfilename);
			if ( fi.Exists && isCont ) {
				m_CurrentLength = fi.Length;
				outstream = fi.Open(FileMode.Append,FileAccess.Write);
			} else {
				outstream = fi.Create();
			}
			if (outstream == null) {
				m_RetCode = -20;
				m_ErrMsg = "open out stream error";
				yield break;
			}
			HttpWebRequest request = System.Net.WebRequest.Create(m_Url) as HttpWebRequest;
			if (request == null) {
				outstream.Close();
				m_RetCode = -30;
				m_ErrMsg = "create web request error "+m_Url;
				yield break;
			}
			if (isCont && m_CurrentLength > 0) {
				request.AddRange("bytes",(int)m_CurrentLength);
			}

			yield return Parent.StartCoroutine (this.UF_AsyncGetResp(request));
			HttpWebResponse response = m_Response;
			if (response == null) {
				UF_DebugLog("Get Response Error");
				outstream.Close();
				yield break;
			}
			//?????header??Content-Range,?????????????,???????
			if (response.Headers ["Content-Range"] == null) {
				if ( isCont && m_CurrentLength > 0 ) {
					UF_DebugLog ("server do not support continues download");
					m_CurrentLength = 0;
					outstream.Seek(0,SeekOrigin.Begin);
				}
			}
			if (isCont && m_CurrentLength > 0)
				m_Length = response.ContentLength + m_CurrentLength;
			else
				m_Length = response.ContentLength;
			UF_DebugLog("total count is : " + m_Length);
			Stream instream = response.GetResponseStream();
			if (instream == null) {
				m_ErrMsg = "Get Response Stream Error";
				m_RetCode = -2;
				outstream.Close();
				yield break;
			}
			byte[] buffer = new byte[m_ReadBufferSize];
			int readcount = 0;
			do {
				try { readcount = instream.Read(buffer,0,m_ReadBufferSize); } 
				catch (Exception e) {
					readcount = 0 ; 
					UF_DebugLog (e.ToString()); 
					m_RetCode = -3;
					m_ErrMsg = e.ToString();
				}
				if ( readcount > 0 ) {
					outstream.Write(buffer,0,readcount);
					m_CurrentLength += readcount;
					if ( m_CurrentLength % 1024*100 == 0 )
						outstream.Flush();
				}
				UF_DebugLog("current : " + readcount + " total downloaded : "+m_CurrentLength);
				yield return null;
			}
			while ( readcount > 0 );
			instream.Close ();
			outstream.Flush ();
			outstream.Close();
			UF_DebugLog("downloaded ended : " + m_CurrentLength + " total : " + m_Length);
			if ( m_CurrentLength != m_Length ) {
				m_RetCode = (int)response.StatusCode;
				m_ErrMsg = "download interrupted";
				UF_DebugLog (string.Format ("status code {0}",(int)response.StatusCode));
			}
		}



		//同步下载
		public int UF_SyncDownLoad(bool isCont=false)
		{
			m_FileName = UF_ParseFileName(m_Url);
			if (m_FileName == null) {
				m_RetCode = -10;
				m_ErrMsg = "FileName is NULL";
				return m_RetCode;
			}

			string localfilename = m_SavePath + "/" + m_FileName;
			System.IO.Stream outstream = null;
			FileInfo fi = new FileInfo (localfilename);
			if ( fi.Exists && isCont ) {
				m_CurrentLength = fi.Length;
				outstream = fi.Open(FileMode.Append,FileAccess.Write);
			} else {
				outstream = fi.Create();
			}
			if (outstream == null) {
				m_RetCode = -20;
				m_ErrMsg = "open out stream error";
				return m_RetCode;
			}
			HttpWebRequest request = System.Net.WebRequest.Create(m_Url) as HttpWebRequest;
			if (request == null) {
				outstream.Close();
				m_RetCode = -30;
				m_ErrMsg = "create web request error "+m_Url;
				return m_RetCode;
			}
			if (isCont && m_CurrentLength > 0) {
				request.AddRange("bytes",(int)m_CurrentLength);
			}

			HttpWebResponse response = null;
			try {
				response = request.GetResponse() as HttpWebResponse;
			} catch (WebException e ) {
				this.m_ErrMsg = e.ToString ();
				this.m_RetCode = (int)((HttpWebResponse)e.Response).StatusCode;
				outstream.Close();
				return m_RetCode;
			}
			if (response.Headers ["Content-Range"] == null) {
				if ( isCont && m_CurrentLength > 0 ) {
					UF_DebugLog ("server do not support continues download");
					m_CurrentLength = 0;
					outstream.Seek(0,SeekOrigin.Begin);
				}
			}
			if (isCont && m_CurrentLength > 0)
				m_Length = response.ContentLength + m_CurrentLength;
			else
				m_Length = response.ContentLength;
			UF_DebugLog("total count is : " + m_Length);
			Stream instream = response.GetResponseStream();
			if (instream == null) {
				m_ErrMsg = "Get Response Stream Error";
				m_RetCode = -2;
				outstream.Close();
				return m_RetCode;
			}
			byte[] buffer = new byte[m_ReadBufferSize];
			int readcount = 0;
			int persecond_readCound = 0;

			int start_tick = System.Environment.TickCount;

			try {
				do {
					readcount = instream.Read(buffer,0,m_ReadBufferSize);

					int dura = Math.Abs(System.Environment.TickCount - start_tick);
					if(dura > 1000){
						m_Speed = (float)persecond_readCound*1000.0f/(float)(dura);
						persecond_readCound = 0;
						start_tick = System.Environment.TickCount;
					}

					if ( readcount > 0 ) {
						outstream.Write(buffer,0,readcount);
						m_CurrentLength += readcount;
						persecond_readCound += readcount;
						if ( m_CurrentLength % 1024*100 == 0 )
							outstream.Flush();
					}


					//UF_DebugLog("current : " + readcount + " total downloaded : "+m_CurrentLength);
				}
				while ( readcount > 0 );
			} catch (Exception e ) {
				m_RetCode = -3;
				m_ErrMsg = e.ToString();
				m_Speed = -1;
			}
			instream.Close ();
			outstream.Flush ();
			outstream.Close();
			UF_DebugLog("downloaded ended : " + m_CurrentLength + " total : " + m_Length);
			if ( m_CurrentLength != m_Length ) {
				m_RetCode = (int)response.StatusCode;
				m_ErrMsg = "download interrupted";
				UF_DebugLog (string.Format ("status code {0}",(int)response.StatusCode));
			}
			m_Speed = -1;
			return m_RetCode;
		}
	}



}