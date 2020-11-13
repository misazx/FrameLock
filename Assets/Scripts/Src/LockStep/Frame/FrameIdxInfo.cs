using Components;
using System.Text;

namespace LogicFrameSync.Src.LockStep.Frame
{
    /// <summary>
    /// 操作信息
    /// 用于网络通信
    /// </summary>
    public class FrameIdxInfo
    {
        public int Idx { set; get; }
        public int Cmd { set; get; }
        public System.Guid EntityId { set; get; }
        public string[] Params { set; get; }
        

        public FrameIdxInfo(int cmd, string eId,string[] param)
        {
            Cmd = cmd;
            EntityId = new System.Guid( eId);
            if (param == null)
                Params = new string[0];
            else
                Params = param;
        }
        public FrameIdxInfo(int idx,int cmd, string eId, string[] param)
        {
            Idx = idx;
            Cmd = cmd;
            EntityId = new System.Guid(eId);
            if (param == null)
                Params = new string[0];
            else
                Params = param;
        }
        public FrameIdxInfo(int cmd, string eId)
        {
            Cmd = cmd;
            EntityId = new System.Guid(eId);
            Params = new string[0];
        }
        public FrameIdxInfo() { }

        public override string ToString()
        {
            return string.Format("{0},{1},{2}",Cmd,Idx,EntityId);
        }
        public bool EqualsInfo(FrameIdxInfo target)
        {
            return EntityId == target.EntityId && Cmd == target.Cmd && Idx == target.Idx;
        }

        public bool EqualsInfo(IComponent component)
        {
            return EntityId == component.EntityId && Cmd == component.GetCommand();
        }

        public static byte[] Write(FrameIdxInfo info)
        {
            using (ByteBuffer buffer = new ByteBuffer())
            {
                buffer.WriteInt32(info.Idx);
                buffer.WriteInt32(info.Cmd);
                buffer.WriteString(info.EntityId.ToString());
                int size = info.Params.Length;
                buffer.WriteInt32(size);
                for (int i = 0; i < size; ++i)
                    buffer.WriteString(info.Params[i]);
                return buffer.Getbuffer();
            }
             
        }

        public static FrameIdxInfo Read(byte[] bytes)
        {
            using (ByteBuffer buffer = new ByteBuffer(bytes))
            {
                FrameIdxInfo info = new FrameIdxInfo();
                info.Idx = buffer.ReadInt32();
                info.Cmd = buffer.ReadInt32();
                info.EntityId = new System.Guid(buffer.ReadString());
                int size = buffer.ReadInt32();
                info.Params = new string[size];
                for (int i = 0; i < size; ++i)
                    info.Params[i] = buffer.ReadString();
                return info;

            }
                
        }

        static StringBuilder sb = new StringBuilder();

        public static string Serialize(FrameIdxInfo info)
        {
            sb.Clear();

            sb.Append(info.Idx);
            sb.Append(",");
            sb.Append(info.Cmd);
            sb.Append(",");
            sb.Append(info.EntityId);
            sb.Append(",");

            int size = info.Params.Length;
            for (int i = 0; i < size; ++i)
            {
                sb.Append(info.Params[i]);
                sb.Append(",");
            }

            return sb.ToString();
        }

        public static FrameIdxInfo DeSerialize(string str)
        {
            FrameIdxInfo info = new FrameIdxInfo();
            var strs = str.Split(',');
            info.Idx = int.Parse(strs[0]) ;
            info.Cmd = int.Parse(strs[1]);
            info.EntityId = new System.Guid(strs[2]);
            int size = strs.Length - 4;
            if (size > 0)
            {
                info.Params = new string[size];
                for (int i = 0; i < size; i++)
                {
                    info.Params[i] = strs[3 + i];
                }
            }

            return info;
        }

    }
}
