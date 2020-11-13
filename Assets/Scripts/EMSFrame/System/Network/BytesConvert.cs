//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

//定义大小端模式，现在宏表示采用大端模式转换数值
#define BIG_ENDIAN

using System;
using System.Collections;
using System.IO;
using System.Text;

namespace UnityFrame{
	public class CBytesConvert
	{
		const byte BIT_8 = 122;
		const byte BIT_16 = 123;
		const byte BIT_32 = 124;
		const byte BIT_64 = 125;

        const ulong VAL_8_S = 122;
		const ulong VAL_8_L = 128;
		const ulong VAL_16 = 32767;
		const ulong VAL_32 = 2147483647;
		const ulong VAL_64 = 0x100000000;


		protected static void UF_Reverse(byte[] array) {
			byte value = 0;
			int count = array.Length;
			int idxcount = count - 1;
			for (int k = 0; k < count / 2; k++) {
				value = array [k];
				array [k] = array [idxcount - k];
				array [idxcount - k] = value;
			}
		}


		public static byte UF_readuint8(CBytesBuffer buffer){
			if(buffer == null)
				throw new Exception("read buffer is null");
			byte _b = 0;
			buffer.UF_read(out _b);
			return _b;
		}

		public static int UF_writeuint8(CBytesBuffer buffer,byte u8){
			if(buffer == null)
				throw new Exception("writing buffer is null");
			return buffer.UF_write(u8);
		}

		public static ushort UF_readuint16(CBytesBuffer buffer){
			byte[] _tmp;
			buffer.UF_read(out _tmp,2);
#if BIG_ENDIAN
            UF_Reverse(_tmp);
			#endif
			return BitConverter.ToUInt16(_tmp,0);
		}

		public static int UF_writeuint16(CBytesBuffer buffer,ushort u16){
			byte[] _tmp = BitConverter.GetBytes(u16);
#if BIG_ENDIAN
            UF_Reverse(_tmp);
			#endif
			return buffer.UF_write(_tmp,2);
		}

		public static uint UF_readuint32(CBytesBuffer buffer){
			byte[] _tmp;
			buffer.UF_read(out _tmp,4);
#if BIG_ENDIAN
            UF_Reverse(_tmp);
			#endif
			return BitConverter.ToUInt32(_tmp,0);
		}
		public static int UF_writeuint32(CBytesBuffer buffer,uint u32){
			byte[] _tmp = BitConverter.GetBytes(u32);
#if BIG_ENDIAN
            UF_Reverse(_tmp);
			#endif
			return buffer.UF_write(_tmp,4);
		}

		public static string UF_readuint64(CBytesBuffer buffer){
			byte[] _tmp;
			buffer.UF_read(out _tmp,8);
#if BIG_ENDIAN
            UF_Reverse(_tmp);
			#endif
			UInt64 data = BitConverter.ToUInt64(_tmp,0);
			return Convert.ToString(data);
		}
		public static int writeuint64(CBytesBuffer buffer,string u64){
			UInt64 data = Convert.ToUInt64(u64);
			byte[] _tmp = BitConverter.GetBytes(data);
#if BIG_ENDIAN
            UF_Reverse(_tmp);
			#endif
			return buffer.UF_write(_tmp, 8);
		}

		public static long UF_readnumber(CBytesBuffer buffer)
		{
			if (buffer.UF_getSize() < 1)
				return 0;

			byte nNumberType;
			buffer.UF_read(out nNumberType);

			switch ( nNumberType)
			{
			case BIT_8:
				{

					byte u8;
					buffer.UF_read(out u8);
					return (sbyte)u8;
				}
			case BIT_16:
				{
					byte[] u16;
					buffer.UF_read(out u16, 2);
#if BIG_ENDIAN
                        UF_Reverse(u16);
					#endif
					return BitConverter.ToInt16(u16, 0);
				}
			case BIT_32:
				{
					byte[] u32;
					buffer.UF_read(out u32, 4);
#if BIG_ENDIAN
                        UF_Reverse(u32);
					#endif
					return BitConverter.ToInt32(u32, 0);
				}
			case BIT_64:
				{
					byte[] u64;
					buffer.UF_read(out u64, 8);
#if BIG_ENDIAN
                        UF_Reverse(u64);
					#endif
					//				return BitConverter.ToString(u64);
					return BitConverter.ToInt64(u64, 0);
				}
			default:
				{
					//返回自己
					return (sbyte)nNumberType;
				}
			}
		}

		public static int UF_writenumber(CBytesBuffer buffer,ulong data){
			//			if (data == 0) 
			//			{
			//				byte type = BIT_0;
			//				return buffer.write(0);
			//			}
			if ( data < VAL_8_L )
			{
				//奇怪的处理方式。。。。
				if (data < VAL_8_S) {
					buffer.UF_write((byte)data);
					return 1;
				} else{
					buffer.UF_write(BIT_8);
					buffer.UF_write((byte)data);
					return 2;
				}
				//				return buffer.write(BitConverter.GetBytes(data), 1);
			}
			else if ( data < VAL_16 )
			{
				buffer.UF_write( BIT_16 );
				byte[] _tmp = BitConverter.GetBytes ((ushort)data);
#if BIG_ENDIAN
                UF_Reverse(_tmp);
				#endif
				return buffer.UF_write(_tmp, 2);
			}
			else if ( data < VAL_32 )
			{
				buffer.UF_write( BIT_32 );
				byte[] _tmp = BitConverter.GetBytes ((uint)data);
#if BIG_ENDIAN
                UF_Reverse(_tmp);
				#endif
				return buffer.UF_write(_tmp, 4);
			}
			else
			{
				buffer.UF_write( BIT_64 );
				byte[] _tmp = BitConverter.GetBytes (data);
#if BIG_ENDIAN
                UF_Reverse(_tmp);
				#endif
				return buffer.UF_write(_tmp, 8 );
			}
		}

		public static string UF_readstring(CBytesBuffer buffer){
			//first: read the size type
			if(buffer == null)
				throw new Exception("reading buffer is null");

			//			int _size = readsize(buffer);
			int _size = (int)UF_readnumber(buffer);
			//second: read the string data
			byte[] _byte;
			buffer.UF_read(out _byte,_size);
			if(_byte == null)
				return "";
			else
				return Encoding.UTF8.GetString(_byte);
		}
		public static int UF_writestring(CBytesBuffer buffer,string str){
            if (str == null)
                str = string.Empty;
            //first:write in the size type;
            if (buffer == null)
				throw new Exception("writing buffer is null");
			byte[] _tmp_str = Encoding.UTF8.GetBytes(str);
			int _len = _tmp_str.Length;

            //			writesize(buffer,_len);
            UF_writenumber(buffer,(ulong)_len);
			//second:write int the string data

			return buffer.UF_write(_tmp_str,_len);
		}

        public static bool UF_readboolean(CBytesBuffer buffer)
        {
            if (buffer == null)
                throw new Exception("reading buffer is null");
            int _val = (int)UF_readnumber(buffer);
            return _val == 0 ? false : true;
        }

        public static int UF_writeboolean(CBytesBuffer buffer, bool val)
        {
            return UF_writenumber(buffer,(ulong)(val ? 1 : 0));
        }

        public static int UF_readsize(CBytesBuffer buffer){
			uint _size = 0;
			byte _tmpb = 0;
			byte[] _tmpbuf;
			if(buffer == null)
				throw new Exception("read buffer is null");

			buffer.UF_read(out _tmpb);
			if((_size = _tmpb) >= 0xff){
				buffer.UF_read(out _tmpbuf,2);
				if((_size = BitConverter.ToUInt16(_tmpbuf,0)) >= 0xffff){
					buffer.UF_read(out _tmpbuf,4);
					_size = BitConverter.ToUInt32(_tmpbuf,0);
				}
			}
			return (int)_size;
		}


		public static int UF_writesize(CBytesBuffer buffer,int size){
			if(buffer == null)
				throw new Exception("writing buffer is null");
			if(size < 0xff){
				return buffer.UF_write((byte)size);
			}	
			else if(size < 0xffff){
				buffer.UF_write(255);
				buffer.UF_write(BitConverter.GetBytes((ushort)size),2);
				return 3;
			}
			else{
				buffer.UF_write(new byte[]{255, 255, 255},3);
				buffer.UF_write(BitConverter.GetBytes((uint)size),4);
				return 7;
			}
		}

		public static byte[] UF_getSize(int size){
			byte[] _data = null;
			if(size < 0)
				size = 0;
			else if(size < 0xff){
				_data = new byte[1];
				_data[0] = (byte)size;
			}
			else if(size < 0xffff){
				_data = new byte[3];
				byte[] _data_s = BitConverter.GetBytes((ushort)size);
				_data[0] = 255;_data[1] = _data_s[0];_data[0] = _data_s[1];
			}
			else{
				byte[] _data_s = BitConverter.GetBytes((uint)size);
				_data = new byte[3];_data[0] = 255;_data[1] = 255;_data[2] = 255;
				_data[3] = _data_s[0];_data[4] = _data_s[1];_data[5] = _data_s[2];_data[6] = _data_s[3];
			}
			return _data;
		}

		public static void UF_writebytes(CBytesBuffer buffer,byte[] bytes)
		{
			//writenumber (buffer, bytes.Length);
			buffer.UF_write(bytes, bytes.Length);
		}

		public static byte[] UF_readbytes(CBytesBuffer buffer, int size)
		{
			byte[] data = null;
			buffer.UF_read(out data, size);
			return data;
		}

		public static byte[] UF_readFixedbytes(CBytesBuffer buffer, int size)
		{
			return UF_readbytes(buffer, buffer.DataSize);
		}

		public static int UF_get_buffer_size(CBytesBuffer buffer)
		{
			return buffer.DataSize;
		}
	}

}