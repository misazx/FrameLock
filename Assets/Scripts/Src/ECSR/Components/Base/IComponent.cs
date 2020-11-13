﻿namespace Components
{
    /// <summary>
    /// 组件数据
    /// </summary>
    public interface IComponent
    {
        System.Guid EntityId { set; get; }
        /// <summary>
        /// 深度拷贝
        /// </summary>
        /// <returns></returns>
        IComponent Clone();
        int GetCommand();

        string Serilize();

        string[] DeSerilize(string str);

    }

    /// <summary>
    /// 参数可更新
    /// </summary>
    public interface IParamsUpdatable
    {
        void UpdateParams(string[] paramsStrs);
    }

    /// <summary>
    /// 碰撞数据更新
    /// </summary>
    public interface ICollisionUpdatable
    {
        VCollisionShape Collider { set; get; }
        void UpdateCollision(VInt3 location);
    }
}
