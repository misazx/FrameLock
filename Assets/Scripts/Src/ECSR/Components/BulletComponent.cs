using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Components
{
    /// <summary>
    /// 子弹组件
    /// </summary>
    public class BulletComponent : AbstractComponent
    {


        /// <summary>
        /// 目标者EntityId
        /// </summary>
        public string TargetEntityId { private set; get; }

        public BulletComponent()
        {
        }
        public BulletComponent(string targetEntityId)
        {
            TargetEntityId = targetEntityId;
        }


        override public IComponent Clone()
        {
            BulletComponent comp = new BulletComponent(TargetEntityId);
            comp.Enable = Enable;
            comp.EntityId = EntityId;
            return comp;
        }

        override public int GetCommand()
        {
            throw new NotImplementedException();
        }

        public override string Serilize()
        {
            base.Serilize();
            sb.Append("&");
            sb.Append(TargetEntityId);

            return sb.ToString();
        }

        public override string[] DeSerilize(string str)
        {
            var strs = base.DeSerilize(str);
            TargetEntityId = strs[0];
            return null;
        }
    }
}
