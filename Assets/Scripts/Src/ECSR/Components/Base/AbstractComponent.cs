using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace Components
{
    /// <summary>
    /// 所有Component的基类
    /// 实现IComponent
    /// </summary>
    public abstract class AbstractComponent : IComponent
    {
        public Guid EntityId { set; get; }

        public bool Enable = true;

        protected StringBuilder sb = new StringBuilder();

        public AbstractComponent()
        {

        }
        public virtual IComponent Clone()
        {
            return null;
        }

        public virtual int GetCommand()
        {
            return 0;
        }

        public virtual string Serilize()
        {
            sb.Clear();
            sb.Append(this.GetType().FullName);
            sb.Append(":");
            sb.Append(EntityId.ToString());
            sb.Append("&");
            sb.Append(Enable?"1":"0");
            return sb.ToString();
        }

        public virtual string[] DeSerilize(string str)
        {
            var strs = str.Split('&');
            EntityId = new Guid(strs[0]);
            Enable = strs[1].Equals("1")?true:false;
            return strs.SubArray(2, strs.Length-2);
        }
    }
}
