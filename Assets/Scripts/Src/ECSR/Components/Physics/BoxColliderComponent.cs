using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Components
{
    public class BoxColliderComponent : AbstractComponent, ICollisionUpdatable
    {
        public VInt3 Center;

        public VInt3 Size;
        public VCollisionShape Collider { set; get; }

        public BoxColliderComponent() { }

        public BoxColliderComponent(VInt3 center,VInt3 size)
        {
            Center = center;
            Size = size;
            Collider = VCollisionShape.CreateBoxColliderShape(center, size);
        }

        override public IComponent Clone()
        {
            BoxColliderComponent comp = new BoxColliderComponent(Center, Size);
            comp.Enable = Enable;
            comp.EntityId = EntityId;
            return comp;
        }

        override public int GetCommand()
        {
            return 0;
        }

        public void UpdateCollision(VInt3 location)
        {
            Collider.UpdateShape(location, new VInt3(0, 0, 1));
        }

        public override string ToString()
        {
            return string.Format("[BoxColliderComponent Id:{0} Center:{1} Size:{2}]",EntityId,Center.ToString(),Size.ToString());
        }

        public override string Serilize()
        {
            base.Serilize();
            sb.Append("&");
            sb.Append(Center.x);
            sb.Append("&");
            sb.Append(Center.y);
            sb.Append("&");
            sb.Append(Center.z);

            sb.Append("&");
            sb.Append(Size.x);
            sb.Append("&");
            sb.Append(Size.y);
            sb.Append("&");
            sb.Append(Size.z);



            return sb.ToString();
        }

        public override string[] DeSerilize(string str)
        {
            var strs = base.DeSerilize(str);
            Center = new VInt3(int.Parse(strs[0]), int.Parse(strs[1]), int.Parse(strs[2]));
            Size = new VInt3(int.Parse(strs[3]), int.Parse(strs[4]), int.Parse(strs[5]));
            Collider = VCollisionShape.CreateBoxColliderShape(Center, Size);

            return null;
        }
    }
}
