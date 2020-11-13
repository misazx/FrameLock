//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.IO;

namespace UnityFrame
{

    //AB文件解密流
    internal class AssetBundleStream : FileStream
    {
        public int byteKey { get { return m_ByteKey; } }
        public int byteOfs { get { return m_ByteOfs; } }
        private int m_ByteKey = 0;
        private int m_ByteOfs = 0;
        public AssetBundleStream(string path, FileMode mode, FileAccess access, int bkey,int bofs, FileShare share, int bufferSize, bool useAsync) : base(path, mode, access, share, bufferSize, useAsync)
        {
            m_ByteKey = bkey;
            m_ByteOfs = bofs;
        }
        public AssetBundleStream(string path, FileMode mode, FileAccess access, int bkey,int bofs) : base(path, mode, access)
        {
            m_ByteKey = bkey;
            m_ByteOfs = bofs;
        }
        public override int Read(byte[] array, int offset, int count)
        {
            var index = base.Read(array, offset, count);
            for (int i = 0; i < array.Length; i++)
            {
                int val = array[i];
                //字节偏移
                val = val ^ m_ByteOfs;
                //字节解密
                if (m_ByteKey != 0)
                {
                    val = ~(val ^ m_ByteKey);
                }
                array[i] = (byte)val;
            }
            return index;
        }
    }
}