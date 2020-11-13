//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System;
using System.Collections.Generic;
using LuaInterface;

namespace UnityFrame
{
	//类主要是优化网络层面在Lua中的调用,加快解包与写包速度
	//ProtocalData 不会被压入到Lua中，减少不必的usedata
	public static class LuaNetwork
	{
		private static List<ProtocalData> m_ProtocalDatas = new List<ProtocalData> ();
		//记录 全局 bytesbuffer 
		private static OrderTable<CBytesBuffer> m_TableCBytesBuffer = new OrderTable<CBytesBuffer>();
		//记录 全局 bytes 
		private static OrderTable<byte[]> m_TableBytes = new OrderTable<byte[]>();

		private static object[,] sub = {
			{new LuaCSFunction(UF_readnumber),"readnumber"},
			{new LuaCSFunction(UF_writenumber),"writenumber"},

			{new LuaCSFunction(UF_readstring),"readstring"},
			{new LuaCSFunction(UF_writestring),"writestring"},

            {new LuaCSFunction(UF_readboolean),"readboolean"},
            {new LuaCSFunction(UF_writeboolean),"writeboolean"},

            {new LuaCSFunction(UF_readnumber64),"readnumber64"},
			{new LuaCSFunction(UF_writenumber64),"writennmber64"},

			{new LuaCSFunction(UF_sendTo),"SendTo"},

			{new LuaCSFunction(UF_addConnection),"AddConnection"},

			{new LuaCSFunction(UF_closeConnection),"CloseConnection"},

            {new LuaCSFunction(UF_closeAllConnection),"CloseAllConnection"},
            
            {new LuaCSFunction(UF_checkConnected),"CheckConnected"},

			{new LuaCSFunction(UF_createCBytesBuffer),"CreateBytesBuffer"},

			{new LuaCSFunction(UF_deleteCBytesBuffer),"DeleteBytesBuffer"},

			{new LuaCSFunction(UF_clearCBytesBuffer),"ClearBytesBuffer"},

			{new LuaCSFunction(UF_getCBytesBufferSize),"GetBytesBufferSize"},

			{new LuaCSFunction(UF_httpGet),"HttpGet"},

			{new LuaCSFunction(UF_httpPost),"HttpPost"},

			{new LuaCSFunction(UF_httpUpload),"HttpUpload"},

			{new LuaCSFunction(UF_httpDownload),"HttpDownload"},

			{new LuaCSFunction(UF_httpDownloadTo),"HttpDownloadTo"},

			{new LuaCSFunction(UF_readbytes),"ReadBytes"},

			{new LuaCSFunction(UF_writebytes),"WriteBytes"},

			{new LuaCSFunction(UF_deletebytes),"Deletebytes"},

			{new LuaCSFunction(UF_readbytes_fromfile),"ReadBytesFromFile"},

			{new LuaCSFunction(UF_writebytes_tofile),"WriteBytesToFile"},

		};

		public static void UF_Register(GLuaState luastate){
			IntPtr L = luastate.LuaGetL ();

			int oldTop = LuaDLL.lua_gettop (L);
			oldTop = oldTop < 0? 0 : oldTop;

			LuaCSFunction rFunc = null;
			string nameFunc = string.Empty;

			LuaDLL.lua_newtable (L);

			for (int k = 0; k < sub.Length/2; k++) {
				rFunc = (LuaCSFunction)sub[k,0];
				nameFunc  = sub[k,1].ToString();
				LuaDLL.tolua_pushcsfunction(L,rFunc);
				LuaDLL.lua_setfield(L,oldTop + 1,nameFunc);
			}	

			LuaDLL.lua_setglobal (L, "CNetwork");
			LuaDLL.lua_settop (L, oldTop);
		} 


		public static void UF_Update(LuaFunction luafunction){
			if (luafunction == null)
				return;
			//接收 ProtocalDatas
			NetworkSystem.UF_GetInstance().UF_ReceiveProtocalDatas(m_ProtocalDatas);
			//解析 ProtocalDatas
			if (m_ProtocalDatas.Count > 0) {
				for (int k = 0; k < m_ProtocalDatas.Count; k++) {
					uint bufferID = m_TableCBytesBuffer.UF_Add(m_ProtocalDatas[k].BodyBuffer);
					try{
						//协议解析必须完整解析，不能在解析的时候使用C#协程
						luafunction.BeginPCall ();
						luafunction.Push (m_ProtocalDatas [k].id);
						luafunction.Push (m_ProtocalDatas [k].retCode);
						luafunction.Push (m_ProtocalDatas [k].corCode);
						luafunction.Push (bufferID);
						luafunction.PCall ();
						luafunction.EndPCall ();
					}catch(System.Exception e){
						Debugger.UF_Exception (e);
						luafunction.EndPCall ();
					}
					m_TableCBytesBuffer.UF_Remove(bufferID);
                    m_ProtocalDatas[k].UF_Release();
                }

				m_ProtocalDatas.Clear ();
			}
		}

		private static void UF_ASSERT_BYTESBUFFER(CBytesBuffer buffer){
			if (buffer == null) {
				throw new Exception("CBytesBuffer is Null");
			}
		}

		static CBytesBuffer UF_getCBytesBufferInPool(IntPtr L){
			#if UNITY_EDITOR
			if(LuaDLL.lua_isnumber(L,1) == 0){
				LuaDLL.luaL_typerror(L, 1, "number");
			}
			#endif
			uint idx = (uint)LuaDLL.lua_tonumber (L, 1);
			CBytesBuffer ret = m_TableCBytesBuffer[idx];
            UF_ASSERT_BYTESBUFFER(ret);
			return ret;
		}


		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_readnumber(IntPtr L){
			CBytesBuffer buffer = UF_getCBytesBufferInPool (L);
			long number = CBytesConvert.UF_readnumber(buffer);
			LuaDLL.lua_pushnumber (L, number);
			return 1;
		}

		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_writenumber(IntPtr L){
			CBytesBuffer buffer = UF_getCBytesBufferInPool (L);
			#if UNITY_EDITOR
			if(LuaDLL.lua_isnumber(L,-1) == 0){
				LuaDLL.luaL_typerror(L, -1, "number");
			}
			#endif
			ulong number = (ulong)LuaDLL.lua_tonumber(L,-1);

			CBytesConvert.UF_writenumber(buffer,number);

			return 0;
		}

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int UF_readboolean(IntPtr L)
        {
            CBytesBuffer buffer = UF_getCBytesBufferInPool(L);
            bool val = CBytesConvert.UF_readboolean(buffer);
            LuaDLL.lua_pushboolean(L, val);
            return 1;
        }


        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int UF_writeboolean(IntPtr L)
        {
            CBytesBuffer buffer = UF_getCBytesBufferInPool(L);
            #if UNITY_EDITOR
            if (!LuaDLL.lua_isboolean(L, -1))
            {
                LuaDLL.luaL_typerror(L, -1, "boolean");
            }
            #endif
            bool val = LuaDLL.lua_toboolean(L, -1);
            CBytesConvert.UF_writeboolean(buffer, val);
            return 0;
        }


        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_readstring(IntPtr L){
			CBytesBuffer buffer = UF_getCBytesBufferInPool (L);
			string str = CBytesConvert.UF_readstring(buffer);
			LuaDLL.lua_pushstring (L, str);
			return 1;
		}

		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_writestring(IntPtr L){
			CBytesBuffer buffer = UF_getCBytesBufferInPool (L);
			#if UNITY_EDITOR
			if(LuaDLL.lua_isstring(L,-1) == 0){
				LuaDLL.luaL_typerror(L, -1, "string");
			}
			#endif

			string str = LuaDLL.lua_tostring (L, -1);

			CBytesConvert.UF_writestring(buffer, str);

			return 0;
		}


		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_readnumber64(IntPtr L){
			CBytesBuffer buffer = UF_getCBytesBufferInPool (L);
			long number = CBytesConvert.UF_readnumber(buffer);
			LuaDLL.lua_pushstring (L, Convert.ToString (number));
			return 1;
		}


		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_writenumber64(IntPtr L){
			CBytesBuffer buffer = UF_getCBytesBufferInPool (L);
			#if UNITY_EDITOR
			if(LuaDLL.lua_isstring(L,-1) == 0){
				LuaDLL.luaL_typerror(L, -1, "string");
			}
			#endif

			string str = LuaDLL.lua_tostring (L, -1);

			CBytesConvert.UF_writenumber(buffer, Convert.ToUInt64(str));

			return 0;
		}


		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_readbytes(IntPtr L){
			CBytesBuffer buffer = UF_getCBytesBufferInPool (L);
			long len = CBytesConvert.UF_readnumber(buffer);
			byte[] value = CBytesConvert.UF_readbytes(buffer, (int)len);

			if (value != null) {
				uint uniqueCode = m_TableBytes.UF_Add(value);
				LuaDLL.lua_pushnumber(L, (double)uniqueCode);
			} else {
				LuaDLL.lua_pushnil(L);
			}
			return 1;
		}

		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_writebytes(IntPtr L){
			CBytesBuffer buffer = UF_getCBytesBufferInPool (L);
			#if UNITY_EDITOR
			if(LuaDLL.lua_isnumber(L,-1) == 0){
				LuaDLL.luaL_typerror(L, -1, "number");
			}
			#endif
			uint uniqueCode = (uint)LuaDLL.lua_tonumber(L, -1);
			byte[] value = m_TableBytes[uniqueCode];
			if (value != null) {
				CBytesConvert.UF_writebytes(buffer, value);
			}

			return 0;
		}


		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_deletebytes(IntPtr L){
			#if UNITY_EDITOR
			if(LuaDLL.lua_isnumber(L,-1) == 0){
				LuaDLL.luaL_typerror(L, -1, "number");
			}
			#endif
			uint uniqueCode = (uint)LuaDLL.lua_tonumber(L, -1);

			m_TableBytes.UF_Remove(uniqueCode);

			return 0;
		}


		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_readbytes_fromfile(IntPtr L){
			#if UNITY_EDITOR
			if(LuaDLL.lua_isstring(L,-1) == 0){
				LuaDLL.luaL_typerror(L, -1, "string");
			}
			#endif
			string path = LuaDLL.lua_tostring(L, -1);
			byte[] value = null;

			if (System.IO.File.Exists(path)) {
				try {
					value = System.IO.File.ReadAllBytes(path);
				} catch (Exception e) {
					Debugger.UF_Exception(e);
				}
			}

			if (value == null || value.Length == 0) {
				LuaDLL.lua_pushnil(L);
			} else {
				uint uniCode = m_TableBytes.UF_Add(value);
				LuaDLL.lua_pushnumber(L,(double)uniCode);
			}
			return 1;
		}


		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_writebytes_tofile(IntPtr L){
			#if UNITY_EDITOR
			if(LuaDLL.lua_isnumber(L,1) == 0){LuaDLL.luaL_typerror(L, 1, "number");}
			if(LuaDLL.lua_isstring(L,2) == 0){LuaDLL.luaL_typerror(L, 1, "string");}
			#endif

			uint uniCode = (uint)LuaDLL.lua_tonumber(L, 1);
			string path = LuaDLL.lua_tostring(L, 2);

			byte[] value = m_TableBytes[uniCode];

			if (value != null) {
				try{
					System.IO.File.WriteAllBytes(path,value);
				}
				catch(Exception e){
					Debugger.UF_Exception(e);
				}
			}
			return 0;
		}



		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_createCBytesBuffer(IntPtr L){
			//后续优化使用对象池形式，减少GC
			CBytesBuffer buffer = new CBytesBuffer ();
			//			uint bufferid = PushInPool (buffer);
			uint bufferid = m_TableCBytesBuffer.UF_Add(buffer);
			LuaDLL.lua_pushnumber (L, bufferid);
			return 1;
		}


		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_clearCBytesBuffer(IntPtr L){
			CBytesBuffer buffer = UF_getCBytesBufferInPool (L);
			buffer.UF_clear();
			return 0;
		}

		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_getCBytesBufferSize(IntPtr L){
			CBytesBuffer buffer = UF_getCBytesBufferInPool (L);
			int size = buffer.UF_getSize();

			LuaDLL.lua_pushnumber (L, size);

			return 1;
		}



		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_deleteCBytesBuffer(IntPtr L){
			#if UNITY_EDITOR
			if(LuaDLL.lua_isnumber(L,1) == 0){
				LuaDLL.luaL_typerror(L, 1, "number");
			}
			#endif

			uint bufferid = (uint)LuaDLL.lua_tonumber (L, 1);

			//			PopInPool (bufferid);
			m_TableCBytesBuffer.UF_Remove(bufferid);

			return 0;
		}

		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_sendTo(IntPtr L){
			#if UNITY_EDITOR
			if(LuaDLL.lua_isstring(L,1) == 0){LuaDLL.luaL_typerror(L, 1, "string");}
			if(LuaDLL.lua_isnumber(L,2) == 0){LuaDLL.luaL_typerror(L, 2, "number");}
			if(LuaDLL.lua_isnumber(L,3) == 0){LuaDLL.luaL_typerror(L, 3, "number");}
			if(LuaDLL.lua_isnumber(L,4) == 0){LuaDLL.luaL_typerror(L, 4, "number");}
			if(LuaDLL.lua_isnumber(L,5) == 0){LuaDLL.luaL_typerror(L, 5, "number");}
			#endif

			string host = LuaDLL.lua_tostring (L, 1);
			int port = (int)LuaDLL.lua_tonumber (L, 2);
			int protocol = (int)LuaDLL.lua_tonumber (L, 3);
			int corcode =  (int)LuaDLL.lua_tonumber (L, 4);
			uint bufferid =  (uint)LuaDLL.lua_tonumber (L, 5);

			CBytesBuffer buffer = m_TableCBytesBuffer[bufferid];
            UF_ASSERT_BYTESBUFFER(buffer);

			//			buffer.pack ();

			NetworkSystem.UF_GetInstance().UF_SendMessage(host, port, protocol, corcode, buffer.Buffer,buffer.DataSize);

			return 0;
		}


		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_addConnection(IntPtr L){
			#if UNITY_EDITOR
			if(LuaDLL.lua_isstring(L,1) == 0){LuaDLL.luaL_typerror(L, 1, "string");}
			if(LuaDLL.lua_isnumber(L,2) == 0){LuaDLL.luaL_typerror(L, 2, "number");}
			if(LuaDLL.lua_isnumber(L,3) == 0){LuaDLL.luaL_typerror(L, 3, "number");}
            if (LuaDLL.lua_isnumber(L, 4) == 0) { LuaDLL.luaL_typerror(L, 4, "number"); }
            #endif

            string host = LuaDLL.lua_tostring (L, 1);
			int port = (int)LuaDLL.lua_tonumber (L, 2);
			int timeout = (int)LuaDLL.lua_tonumber (L, 3);
            int ctype = (int)LuaDLL.lua_tonumber(L, 4);

            bool ret = NetworkSystem.UF_GetInstance().UF_AddConnection(host, port, timeout, ctype == 0 ? ConnectType.TCP : ConnectType.UDP);

			LuaDLL.lua_pushboolean (L, ret);

			return 1;
		}

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int UF_closeAllConnection(IntPtr L)
        {
            #if UNITY_EDITOR
            if (!LuaDLL.lua_isboolean(L, 1)) { LuaDLL.luaL_typerror(L, 1, "boolean"); }
            #endif
            bool notifyState = LuaDLL.lua_toboolean(L, 1);
            NetworkSystem.UF_GetInstance().UF_CloseAllConnection(notifyState);
            return 0;
        }

        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_closeConnection(IntPtr L){
			#if UNITY_EDITOR
			if(LuaDLL.lua_isstring(L,1) == 0){LuaDLL.luaL_typerror(L, 1, "string");}
			if(LuaDLL.lua_isnumber(L,2) == 0){LuaDLL.luaL_typerror(L, 2, "number");}
            if(!LuaDLL.lua_isboolean(L,3)){LuaDLL.luaL_typerror(L, 3, "boolean");}
			#endif

			string host = LuaDLL.lua_tostring (L, 1);
			int port = (int)LuaDLL.lua_tonumber (L, 2);
            bool notifyState = LuaDLL.lua_toboolean(L, 3);

            NetworkSystem.UF_GetInstance().UF_CloseConnection(host,port, notifyState);

			return 0;
		}


		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_checkConnected(IntPtr L){
			#if UNITY_EDITOR
			if(LuaDLL.lua_isstring(L,1) == 0){LuaDLL.luaL_typerror(L, 1, "string");}
			if(LuaDLL.lua_isnumber(L,2) == 0){LuaDLL.luaL_typerror(L, 2, "number");}
			#endif

			string host = LuaDLL.lua_tostring (L, 1);
			int port = (int)LuaDLL.lua_tonumber (L, 2);

			bool ret = NetworkSystem.UF_GetInstance().UF_CheckNetConnected(host,port);

			LuaDLL.lua_pushboolean (L, ret);

			return 1;

		}
			

		static void UF_OnHttpResponse(LuaFunction function,int retcode,object param){
			if (function != null) {
				string msg = string.Empty;
				byte[] data = param as byte[];
				if(data == null){
					msg = "";
				}else{
					msg = System.Text.Encoding.UTF8.GetString(data);
				}

				function.BeginPCall ();
				function.Push (retcode);
				function.Push (msg);
				function.PCall (); 
				function.EndPCall ();
			}
		}


		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_httpGet(IntPtr L){
			#if UNITY_EDITOR
			if(LuaDLL.lua_isstring(L,1) == 0){LuaDLL.luaL_typerror(L, 1, "string");}
			if(LuaDLL.lua_isstring(L,2) == 0){LuaDLL.luaL_typerror(L, 2, "string");}
			if(LuaDLL.lua_isnumber(L,3) == 0){LuaDLL.luaL_typerror(L, 3, "number");}
			#endif

			string url = LuaDLL.lua_tostring (L, 1);
			string headinfo = LuaDLL.lua_tostring (L, 2);
			int timeout = (int)LuaDLL.lua_tonumber (L, 3);

			DelegateResponse callback = null;
			if(LuaDLL.lua_isfunction(L,4)){
				LuaFunction luafunction = ToLua.ToLuaFunction (L, 4);
				callback = delegate(int retcode, object param) {
                    UF_OnHttpResponse(luafunction,retcode,param);
				};
			}

			NetworkSystem.UF_GetInstance().UF_HttpGetRequest(url,headinfo,timeout,callback);

			return 0;
		}

		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_httpPost(IntPtr L){
			#if UNITY_EDITOR
			if(LuaDLL.lua_isstring(L,1) == 0){LuaDLL.luaL_typerror(L, 1, "string");}
			if(LuaDLL.lua_isstring(L,2) == 0){LuaDLL.luaL_typerror(L, 2, "string");}
			if(LuaDLL.lua_isstring(L,3) == 0){LuaDLL.luaL_typerror(L, 3, "string");}
			if(LuaDLL.lua_isnumber(L,4) == 0){LuaDLL.luaL_typerror(L, 4, "number");}
			#endif

			string url = LuaDLL.lua_tostring (L, 1);
			string postdata = LuaDLL.lua_tostring (L, 2);
			string headinfo = LuaDLL.lua_tostring (L, 3);
			int timeout = (int)LuaDLL.lua_tonumber (L, 4);

			DelegateResponse callback = null;
			if(LuaDLL.lua_isfunction(L,5)){
				LuaFunction luafunction = ToLua.ToLuaFunction (L, 5);
				callback = delegate(int retcode, object param) {
                    UF_OnHttpResponse(luafunction,retcode,param);
				};
			}
			NetworkSystem.UF_GetInstance().UF_HttpPostRequest(url,postdata,headinfo,timeout,callback);
			return 0;
		}


		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_httpUpload(IntPtr L){
			#if UNITY_EDITOR
			if(LuaDLL.lua_isstring(L,1) == 0){LuaDLL.luaL_typerror(L, 1, "string");}
			if(LuaDLL.lua_isstring(L,2) == 0){LuaDLL.luaL_typerror(L, 2, "string");}
			if(LuaDLL.lua_isstring(L,3) == 0){LuaDLL.luaL_typerror(L, 3, "string");}
			if(LuaDLL.lua_isnumber(L,4) == 0){LuaDLL.luaL_typerror(L, 4, "number");}
			#endif

			string url = LuaDLL.lua_tostring (L, 1);
			string filePath = LuaDLL.lua_tostring (L, 2);
			string headinfo = LuaDLL.lua_tostring (L, 3);
			int timeout = (int)LuaDLL.lua_tonumber (L, 4);

			DelegateResponse callback = null;
			if(LuaDLL.lua_isfunction(L,5)){
				LuaFunction luafunction = ToLua.ToLuaFunction (L, 5);
				callback = delegate(int retcode, object param) {
                    UF_OnHttpResponse(luafunction,retcode,param);
				};
			}

			NetworkSystem.UF_GetInstance().UF_HttpUploadFile(url,filePath,headinfo,timeout,callback);

			return 0;
		}




		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_httpDownload(IntPtr L){
			#if UNITY_EDITOR
			if(LuaDLL.lua_isstring(L,1) == 0){LuaDLL.luaL_typerror(L, 1, "string");}
			if(LuaDLL.lua_isstring(L,2) == 0){LuaDLL.luaL_typerror(L, 2, "string");}
			if(LuaDLL.lua_isnumber(L,3) == 0){LuaDLL.luaL_typerror(L, 3, "number");}
			#endif

			string url = LuaDLL.lua_tostring (L, 1);
			string headinfo = LuaDLL.lua_tostring (L, 2);
			int timeout = (int)LuaDLL.lua_tonumber (L, 3);

			DelegateResponse callback = null;
			if(LuaDLL.lua_isfunction(L,4)){
				LuaFunction luafunction = ToLua.ToLuaFunction (L, 4);

				callback = delegate(int retcode, object param) {
//					string msg = string.Empty;
					byte[] data = param as byte[];

					LuaFunction func = luafunction; 
					func.BeginPCall ();
					func.Push (retcode);
					if(data != null){
						uint uniCode = m_TableBytes.UF_Add(data);
						func.Push (uniCode);
					}
					func.PCall (); 
					func.EndPCall ();
				};
			}

			NetworkSystem.UF_GetInstance().UF_HttpGetRequest(url,headinfo,timeout,callback);
			return 0;
		}

		[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
		static int UF_httpDownloadTo(IntPtr L){
			#if UNITY_EDITOR
			if(LuaDLL.lua_isstring(L,1) == 0){LuaDLL.luaL_typerror(L, 1, "string");}
			if(LuaDLL.lua_isstring(L,2) == 0){LuaDLL.luaL_typerror(L, 2, "string");}
			if(LuaDLL.lua_isnumber(L,3) == 0){LuaDLL.luaL_typerror(L, 3, "number");}
			if(LuaDLL.lua_isstring(L,4) == 0){LuaDLL.luaL_typerror(L, 4, "string");}
			#endif

			string url = LuaDLL.lua_tostring (L, 1);
			string headinfo = LuaDLL.lua_tostring (L, 2);
			int timeout = (int)LuaDLL.lua_tonumber (L, 3);
			string localPath = LuaDLL.lua_tostring (L, 4);

			if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(localPath))) {
				System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(localPath));
			}

			DelegateResponse callback = null;
			if(LuaDLL.lua_isfunction(L,5)){
				LuaFunction luafunction = ToLua.ToLuaFunction (L, 5);

				callback = delegate(int retcode, object param) {
//					string msg = string.Empty;
					byte[] data = param as byte[];
					//save to local
					try{
						if(System.IO.File.Exists(localPath))
							System.IO.File.Delete(localPath);

						System.IO.FileInfo fi = new System.IO.FileInfo(localPath);
						System.IO.FileStream fs = fi.Open(System.IO.FileMode.CreateNew);
						fs.Write(data,0,data.Length);
						fs.Close();
					}catch(System.Exception e){
						Debugger.UF_Exception(e);
					}

					LuaFunction func = luafunction; 
					func.BeginPCall ();
					func.Push (retcode);
					func.PCall (); 
					func.EndPCall ();
				};
			}

			NetworkSystem.UF_GetInstance().UF_HttpGetRequest(url,headinfo,timeout,callback);
			return 0;

		}



	}
}

