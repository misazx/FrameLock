
using Components;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LogicFrameSync.Src.LockStep
{
    /// <summary>
    /// 帧数据快照
    /// </summary>
    public class EntityWorldFrameData
    {
        /// <summary>
        /// 当前所有的EntityIds快照信息
        /// </summary>
        public List<Guid> EntityIds { private set; get; }
        /// <summary>
        /// 当前所有的Components快照信息
        /// </summary>
        public List<IComponent> Components { private set; get; }
        public EntityWorldFrameData(List<Guid> entities, List<IComponent> comps)
        {
            EntityIds = entities;
            Components = comps;
        }

        public void Clear()
        {
            EntityIds.Clear();
            EntityIds = null;
            Components.Clear();
            Components = null;
        }

        /// <summary>
        /// 获得一个深度拷贝
        /// </summary>
        /// <returns></returns>
        public EntityWorldFrameData Clone()
        {
            List<Guid> cloneEntities = new List<Guid>();
            EntityIds.ForEach((a)=>cloneEntities.Add(a));

            List<IComponent> cloneComps = new List<IComponent>();
            Components.ForEach((a)=>cloneComps.Add(a.Clone()));
            EntityWorldFrameData data = new EntityWorldFrameData(cloneEntities,cloneComps);
            return data;
        }
        public override string ToString()
        {
            string entitystring = string.Join(",", EntityIds);
            string componentstring = "";
            foreach(var icom in Components)
            {
                componentstring += icom.ToString()+"    ";
            }
            return string.Format("[Entitys]={0} [Components]={1}", entitystring, componentstring);
        }

        public static string Serilize(EntityWorldFrameData eWorldFrameData)
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            eWorldFrameData.EntityIds.ForEach(id => {
                if(++i>1)
                    sb.Append(",");
                sb.Append(id); 
            });
            i = 0;
            sb.Append(";");
            eWorldFrameData.Components.ForEach(com => {
                if (++i > 1)
                    sb.Append(",");
                sb.Append(com.Serilize());
            });

            return sb.ToString();
        }

        public static EntityWorldFrameData DeSerilize(string str)
        {
            var strs = str.Split(';');
            var idsStr = strs[0];
            var comStr = strs[1];

            List<Guid> idList = new List<Guid>();
            var idsStrList = idsStr.Split(',');
            for (int i = 0; i < idsStrList.Length; i++)
            {
                idList.Add(new Guid(idsStrList[i]));
            }

            List<IComponent> comList = new List<IComponent>();
            var comStrList = comStr.Split(',');
            for (int i = 0; i < comStrList.Length; i++)
            {
                var temp = comStrList[i].Split(':');
                var strType = temp[0];
                var strContent = temp[1];
                var com = Type.GetType(strType).Assembly.CreateInstance(strType) as IComponent;
                com.DeSerilize(strContent);
                comList.Add(com);
            }
            EntityWorldFrameData eWorldFrameData = new EntityWorldFrameData(idList, comList);

            return eWorldFrameData;
        }
    }
}
