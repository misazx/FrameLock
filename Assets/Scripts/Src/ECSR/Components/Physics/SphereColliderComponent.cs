using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Components
{
    public class SphereColliderComponent : AbstractComponent, ICollisionUpdatable
    {

        public VInt3 Center;
        public float Radius;

        public SphereColliderComponent() { }

        public VCollisionShape Collider { set; get; }

        public SphereColliderComponent(VInt3 center, float radius)
        {
            Center = center;
            Radius = radius;
            Collider = VCollisionShape.CreateSphereColliderShape(center, radius);
        }

        override public IComponent Clone()
        {
            SphereColliderComponent comp = new SphereColliderComponent(Center, Radius);
            comp.Enable = Enable;
            comp.EntityId = EntityId;
            return comp;
        }

        override public int GetCommand()
        {
            return 0;
        }
        public override string ToString()
        {
            return string.Format("[SphereColliderComponent Id:{0} Center:{1} Rad:{2}]",EntityId,Center.ToString(),Radius);
        }
        public void UpdateCollision(VInt3 location)
        {
            Collider.UpdateShape(location, new VInt3(0,0,1));
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
            sb.Append(Radius);


            return sb.ToString();
        }

        public override string[] DeSerilize(string str)
        {
            var strs = base.DeSerilize(str);
            Center = new VInt3(int.Parse(strs[0]), int.Parse(strs[1]), int.Parse(strs[2]));
            Radius = float.Parse(strs[3]);
            Collider = VCollisionShape.CreateSphereColliderShape(Center, Radius);

            return null;
        }
    }
}
