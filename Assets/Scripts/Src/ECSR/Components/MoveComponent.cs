﻿using LogicFrameSync.Src.LockStep.Frame;
using Unity.Mathematics;
using UnityEngine;
namespace Components
{
    public class MoveComponent : AbstractComponent, IParamsUpdatable
    {
        float Speed = 1;
        float2 Dir = float2.zero;
        public MoveComponent()
        {
        }
        public MoveComponent(float speed, float2 dir)
        {
            Dir = dir;
            Speed = speed;
        }
        public float GetSpeed() { return Speed; }
        public Vector2 GetDirVector2() { return new Vector2(Dir.x, Dir.y); }

        public float2 GetPathV2()
        {
            return Dir * Speed;
        }

        public void SetDir(float2 vec)
        {
            Dir = vec;
        }

        override public IComponent Clone()
        {
            MoveComponent com = new MoveComponent(Speed, Dir);
            com.Enable = Enable;
            com.EntityId = EntityId;
            return com;
        }
        override public int GetCommand()
        {
            return FrameCommand.SYNC_MOVE;
        }

        public void UpdateParams(string[] paramsStrs)
        {
            SetDir(new float2(float.Parse(paramsStrs[0]), float.Parse(paramsStrs[1])));
        }

        public override string ToString()
        {
            return string.Format("[MoveComponent Id:{0} Dir:{1},{2} Speed:{3}]",EntityId,Dir.x,Dir.y,Speed);
        }

        public override string Serilize()
        {
            base.Serilize();
            sb.Append("&");
            sb.Append(Speed);
            sb.Append("&");
            sb.Append(Dir.x);
            sb.Append("&");
            sb.Append(Dir.y);

            return sb.ToString();
        }

        public override string[] DeSerilize(string str)
        {
            var strs = base.DeSerilize(str);
            Speed = float.Parse(strs[0]) ;
            Dir = new float2(float.Parse(strs[1]), float.Parse(strs[2]));

            return null;
        }
    }
}


