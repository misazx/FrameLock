//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System;
using System.Collections.Generic;

namespace UnityFrame{
    public class ProtocalData {
        //头大小
        public const int HEAD_SIZE = 18;
        //校验码
        public const uint MAGIC = 0xC0074345;
        //包体 hash 值
        public int hash { get; set; }
        //协议号 
        public int id { get; set; }
        //返回码
        public short retCode { get; set; }
        //索引编码
        public int corCode { get; set; }
        //包体长度
        public int size { get; private set; }
        //包体内容数据
        public CBytesBuffer BodyBuffer { get { return m_BodyBuffer; } }

        private CBytesBuffer m_BodyBuffer = new CBytesBuffer();

        private bool isReleased { get; set; }

        //读入的缓存
        private CBytesBuffer m_TmpBufferToRead = new CBytesBuffer();

        public ProtocalData() { }

        static TStack<ProtocalData> s_DataPool = new TStack<ProtocalData>(512);

        internal static int StaticBufferCount{ get { return s_DataPool.Count; } }

        public static ProtocalData UF_Acquire() {
            ProtocalData ret = null;
            lock (s_DataPool) {
                ret = s_DataPool.Pop();
                if (ret == null)
                    ret = new ProtocalData();
            }
            ret.isReleased = false;
            return ret;
        }


        public void UF_SetID(int num){
            id = num;
		}

        //客户端标识码
        public void UF_SetCorCode(int code)
        {
            corCode = code;
        }

        public bool UF_SetBodyBuffer(byte[] buffer,int size){
			if (buffer == null)
				return false;
			m_BodyBuffer.UF_clear();
			m_BodyBuffer.UF_write(buffer, size);
			return true;
		}

		/// <summary>
		/// 从参数中复制
		/// </summary>
		public bool UF_SetBodyBuffer(CBytesBuffer buffer){
			if (buffer == null)
				return false;
			m_BodyBuffer.UF_write(buffer);
			return true;
		}
			
		/// <summary>
		/// 从缓存数据中读取包数据
		/// 如果缓存数据中包含完整包数据,则从该缓存数据中取出,并读入到bodybuffer中
		/// </summary>
		public bool UF_Read(CBytesBuffer rawBuffer){
			m_TmpBufferToRead.UF_clear();
			m_TmpBufferToRead.UF_write(rawBuffer);
			//			CBytesBuffer tmpbuff = new CBytesBuffer(rawBuffer);
			CBytesBuffer tmpbuff = m_TmpBufferToRead;

			int packetsize = 0;

			if (tmpbuff.UF_getSize() < HEAD_SIZE)
			{
				//不是完整的协议数据包,等待下次读取
				Debugger.UF_Warn(string.Format("NetPacket Buffer Size[{0}] not Long enought to read",tmpbuff.UF_getSize()));
				return false;
			}

			//校验码比较
			uint pMagic = CBytesConvert.UF_readuint32(tmpbuff);
			if (MAGIC != pMagic)
			{
				///校验码不通过，丢弃该包缓存的全部数据
				rawBuffer.UF_popBytes(rawBuffer.Length);

				Debugger.UF_Error(string.Format("discard package: magic<{0}> | MAGIC<{1}>,RawBuffer Clear",MAGIC,pMagic));

				return false;
			}


			//读出包头
			this.id = (int)CBytesConvert.UF_readuint32(tmpbuff);
			this.retCode = (short)CBytesConvert.UF_readuint16(tmpbuff);
			this.corCode = (int)CBytesConvert.UF_readuint32(tmpbuff);
			this.size = (int)CBytesConvert.UF_readuint32(tmpbuff);

			packetsize += HEAD_SIZE;

			//包体不为0，读出包体
			if (this.size > 0) {
				//buffer 比读出的size长
				if (this.size > tmpbuff.UF_getSize() ) {
					//不是完整的协议数据包,等待下次读取
					return false;
				}
				//写入到body中
				this.m_BodyBuffer.UF_write(tmpbuff.Buffer, (int)this.size);	
				packetsize += this.size;
			}

			//清空已经读取的RawBuffer
			rawBuffer.UF_popBytes(packetsize);

			return true;

		}


        /// <summary>
        ///写入完整Raw协议数据,用于网络发送
        ///处理大小包,小包<0xFF 大包>=0xFFFE
        ///如果包体body大于0xFFFE则分包
        /// <returns>The raw buffer.</returns>
        /// <summary>
        public bool UF_Write(CBytesBuffer mRawBuffer){
			if (mRawBuffer == null) {
				return false;
			}

			size = m_BodyBuffer.UF_getSize();

			CBytesConvert.UF_writeuint32(mRawBuffer, MAGIC);
			CBytesConvert.UF_writeuint32(mRawBuffer, (uint)id);
			CBytesConvert.UF_writeuint16(mRawBuffer, (ushort)retCode);
			CBytesConvert.UF_writeuint32(mRawBuffer, (uint)corCode);
			CBytesConvert.UF_writeuint32(mRawBuffer, (uint)size);

			//写入包体
			mRawBuffer.UF_write(m_BodyBuffer.Buffer, size);

			return true;
		}


        public void UF_Reset()
        {
            id = 0;
            retCode = 0;
            corCode = 0;
            size = 0;
            m_BodyBuffer.UF_clear();
        }

        public void UF_Clear(){
            UF_Reset();
        }

        public void UF_Release() {
            if (!isReleased) {
                lock (s_DataPool)
                {
                    isReleased = true;
                    this.UF_Reset();
                    s_DataPool.Push(this);
                }
            }
        }

	}


}