using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Components
{
    public class IntValueComponent:AbstractComponent
    {
        public int Value { private set; get; }
        public IntValueComponent() { }

        public IntValueComponent(int initValue)
        {
            Value = initValue;
        }
        public void AddDelta(int delta)
        {
            Value += delta;
        }
        public override IComponent Clone()
        {
            IntValueComponent com = new IntValueComponent(Value);
            com.EntityId = EntityId;
            com.Enable = Enable;
            return com;
        }
        public override string ToString()
        {
            return string.Format("[IntValueComponent Id:{0} Value:{1}]",EntityId,Value);
        }

        public override string Serilize()
        {
            base.Serilize();
            sb.Append("&");
            sb.Append(Value);

            return sb.ToString();
        }

        public override string[] DeSerilize(string str)
        {
            var strs = base.DeSerilize(str);
            Value = int.Parse(strs[0]);
            return null;
        }
    }
}
