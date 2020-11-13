﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Components
{
    public class PlayerInfoComponent:AbstractComponent
    {
        public int Value = 0;
        public PlayerInfoComponent()
        {

        }
        public override IComponent Clone()
        {
            PlayerInfoComponent com = new PlayerInfoComponent();
            com.Value = Value;
            com.EntityId = EntityId;
            com.Enable = Enable;
            return com;
        }

        public override string ToString()
        {
            return string.Format("[PlayerInfoComponent Id:{0} Value:{1}]",EntityId,Value);
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
