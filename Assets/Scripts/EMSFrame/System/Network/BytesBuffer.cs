//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System;
using System.IO;
using System.Text;

namespace UnityFrame{
	public class CBytesBuffer{
		public const int MAX_INCREASE_SIZE = 1024;

		public byte[] Buffer{get{return m_Buffer;}}
		public int Length{get{return m_Len;}}
		public int DataSize{get{return UF_getSize();}}
		public int FreeLen{get{return mFreeLen;}}

		private byte[] m_Buffer = null;
		private int m_Len = 0;
		//private int _dataSize = 0;
		private int mFreeLen = 0;

		/// <summary>
		/// 构造函数
		/// </summary>
		public CBytesBuffer(){
			m_Len = MAX_INCREASE_SIZE;
			mFreeLen = MAX_INCREASE_SIZE;
			m_Buffer = new byte[MAX_INCREASE_SIZE];
		}
		/// <summary>
		/// 构造函数
		/// </summary>
		public CBytesBuffer(int nLen){
			if(nLen <= 0){
				m_Len = MAX_INCREASE_SIZE;
				mFreeLen = MAX_INCREASE_SIZE;
				m_Buffer = new byte[MAX_INCREASE_SIZE];
			}
			else{
				m_Len = nLen;
				mFreeLen = nLen;
				m_Buffer = new byte[nLen];
			}
		}
		/// <summary>
		/// 复制构造函数
		/// </summary>
		public CBytesBuffer(CBytesBuffer cCBytesBuffer){
            UF_write(cCBytesBuffer);
		}

		public void UF_clear(){
			mFreeLen = m_Len;
		}

		public int UF_getSize(){
			return m_Len - mFreeLen;
		}


		/// <summary>
		///	压缩buffer 
		/// </summary>
		public void UF_pack(){
			if(mFreeLen > 0){
				m_Len = UF_getSize();
				mFreeLen = 0;
				byte[] _tmp = new byte[m_Len];
				Array.Copy(m_Buffer,0,_tmp,0,m_Len);
				m_Buffer = _tmp;
			}
		}


		public int UF_write(byte b){
			if(Buffer == null)
				return 0;
			else{
				while(mFreeLen < 1){
                    UF_increaceBufferSize();
				}
				m_Buffer[UF_getSize()] = b;
				mFreeLen--;

				return 1;
			}		
		}

		public int UF_write(string sBuffer){
			if(sBuffer == null || sBuffer == "")
				return 0;
			byte[] _str_buf = System.Text.Encoding.UTF8.GetBytes(sBuffer);
			int _str_len = _str_buf.Length;
            UF_write(_str_buf,_str_len);
			return _str_len;
		}



		public int UF_write(byte[] sBuffer,int nWriteSize){
			if(sBuffer == null || nWriteSize <= 0)
				return 0;
			else{
				while(mFreeLen < nWriteSize){
                    UF_increaceBufferSize();
				}

				byte[] _tmp;
				if(nWriteSize >= sBuffer.Length){
					_tmp = sBuffer;
				}
				else{
					_tmp = new byte[nWriteSize];
					Array.Copy(sBuffer,0,_tmp,0,nWriteSize);
				}
				_tmp.CopyTo(m_Buffer, UF_getSize());
				mFreeLen -= nWriteSize;
				return nWriteSize;
			}
		}

		public int UF_write(CBytesBuffer cCBytesBuffer){
			if(cCBytesBuffer != null){
				m_Len = cCBytesBuffer.Length;
				mFreeLen = cCBytesBuffer.FreeLen;
				m_Buffer = new byte[m_Len];
				cCBytesBuffer.Buffer.CopyTo(m_Buffer,0);
				return cCBytesBuffer.UF_getSize();
			}else{
				///抛出参数为空
				throw new Exception("Parameter in Constructor is Null");
			}
			//			return 0;
		}

		/// <summary>
		/// 读取一个byte值
		/// </summary>
		public int UF_read(out byte bValue){
			bValue = 0;
			if(UF_getSize()>0){
				bValue = m_Buffer[0];
                UF_popBytes(1);
				return 0;
			}
			else return -1;
		}

		/// <summary>
		/// 读写字节到buffer
		/// </summary>
		public int UF_read(out byte[] bBuffer,int nReadSize){
			bBuffer = null;
			if(UF_getSize() <= 0||nReadSize <= 0)
				return -1;
			else{
				if(nReadSize > UF_getSize()){
					nReadSize = UF_getSize();
				}
				bBuffer = new byte[nReadSize];
				Array.Copy(m_Buffer,bBuffer,nReadSize);
                UF_popBytes(nReadSize);
				return 0;
			}
		}


		public byte[] UF_getRawData(){
			if(UF_getSize() <= 0)
				return null;
			else{
				byte[] tmp;
                UF_read(out tmp, UF_getSize());
				return tmp;
			}
		}

		public int UF_popBytes(int poplen){
			if(poplen <= 0)
				return -1;
			else if(poplen >= UF_getSize()){
				int _size = UF_getSize();
				mFreeLen = m_Len;
				return _size;
			}
			else{
				Array.Copy(m_Buffer,poplen,m_Buffer,0, UF_getSize() - poplen);
				mFreeLen += poplen;
				return poplen;
			}
		}

		/// <summary>
		/// 倍数增加buffer数组长度，返回增加后buffer的长度
		/// </summary>
		private int UF_increaceBufferSize(){
			if(m_Buffer.Length == 0){
				m_Len = MAX_INCREASE_SIZE;
				mFreeLen = MAX_INCREASE_SIZE;
				m_Buffer = new byte[MAX_INCREASE_SIZE];
				return m_Len;
			}
			int _len = m_Buffer.Length * 2;
			if(mFreeLen == m_Len){
				m_Len = _len;
				mFreeLen = _len;
				m_Buffer = new byte[m_Len];
			}
			else{
				mFreeLen += m_Len;
				m_Len = _len;
				byte[] _tmp = new byte[_len];
				m_Buffer.CopyTo(_tmp,0);
				m_Buffer = _tmp;
			}
			return m_Len;
		}



		//字节翻转，用于处理大小端问题
		public void UF_reverse(){
			int size = UF_getSize();
			if( size > 0){
				for (int k = 0; k < size; k++) {
					Array.Reverse (m_Buffer, 0, size);
				}
			}
		}




	}

}


