//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace UnityFrame {
    //引用资源管理器
    //根据对象名字作为唯一key
    public class RefObjectManager: HandleSingleton<RefObjectManager>, IOnUpdate, IOnReset,IOnApplicationPause
    {
        internal class RefObject
        {
            public Object target = null;
            private int m_Refcount = 0;

            public RefObject(Object o)
            {
                target = o;
            }

            public int Refcount
            {
                get { return m_Refcount; }
            }
            public void Retain()
            {
                m_Refcount++;
            }
            public void Release()
            {
                m_Refcount--;
                m_Refcount = Mathf.Max(0, m_Refcount);
            }
            public void Clear() {
                m_Refcount=0;
            }
            public void Displose()
            {
                target = null;
                m_Refcount = 0;
            }
        }

        public float autoReleaseTime = 30;

        private float m_CurReleaseTime = 0;

        private Dictionary<string, RefObject> m_DicRefObjects = new Dictionary<string, RefObject>();


        //先在引用表中查找，有则直接返回
        //没有则通过AssetSystem 加载引用对象资源
        public Object UF_LoadRefObject(string objName,bool retain) {
            return UF_LoadRefObject<Object>(objName, retain);
        }

        public Object UF_LoadRefObject<T>(string objName, bool retain)
        {
            var retVal = UF_GetRefObject(objName, retain);
            if (retVal == null)
            {
                retVal = AssetSystem.UF_GetInstance().UF_LoadObjectImage(objName);
                if (retVal != null && retVal is T)
                {
                    if(retain)
                        UF_RetainRef(retVal);
                }
            }
            return retVal;
        }


        public Object UF_GetRefObject(string key,bool retain) {
            if (m_DicRefObjects.ContainsKey(key)) {
                if (retain) {
                    m_DicRefObjects[key].Retain();
                }
                return m_DicRefObjects[key].target;
            }
            return null;
        }

        public Object UF_GetRefObject<T>(string key, bool retain)
        {
            if (m_DicRefObjects.ContainsKey(key))
            {
                Object o = UF_GetRefObject(key, false);
                if (o is T)
                {
                    if (retain)
                        UF_RetainRef(o);
                    return o;
                }
            }
            return null;
        }


        public void UF_RetainRef(Object obj) {
            if (obj == null) return;
            string uid = obj.name;
            if (m_DicRefObjects.ContainsKey(uid))
            {
                m_DicRefObjects[uid].Retain();
            }
            else {
                m_DicRefObjects.Add(uid, new RefObject(obj));
            }
        }

        public void UF_ReleaseRef(Object obj) {
            if (obj == null) return;
            string uid = obj.name;
            if (m_DicRefObjects.ContainsKey(uid))
            {
                m_DicRefObjects[uid].Release();
            }
        }

        public void UF_ClearRef(Object obj) {
            if (obj == null) return;
            string uid = obj.name;
            if (m_DicRefObjects.ContainsKey(uid)) {
                m_DicRefObjects[uid].Clear();
            }
        }


        public void UF_Clear()
        {
            foreach (RefObject to in m_DicRefObjects.Values)
            {
                to.Displose();
            }
            m_DicRefObjects.Clear();
            Resources.UnloadUnusedAssets();
        }

        public void UF_ClearUesless()
        {
            List<string> tempList = ListCache<string>.Acquire();
            foreach (KeyValuePair<string, RefObject> item in m_DicRefObjects)
            {
                if (item.Value.Refcount == 0)
                {
                    tempList.Add(item.Key);
                    item.Value.Displose();
                }
            }
            for (int k = 0; k < tempList.Count; k++)
            {
                m_DicRefObjects.Remove(tempList[k]);
            }
            ListCache<string>.Release(tempList);
            Resources.UnloadUnusedAssets();
        }


        public void UF_OnUpdate()
        {
            m_CurReleaseTime += GTime.UnscaleDeltaTime;
            if (m_CurReleaseTime >= autoReleaseTime)
            {
                UF_ClearUesless();
                m_CurReleaseTime = 0;
            }
        }


        public void UF_OnReset()
        {
            foreach (KeyValuePair<string, RefObject> item in m_DicRefObjects)
            {
                item.Value.Displose();
            }
            m_DicRefObjects.Clear();
        }


        public void OnApplicationPause(bool state) {
            if (state) {
                UF_ClearUesless();
            }
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = StrBuilderCache.Acquire();
            sb.Append(string.Format("RefObjectManager count:{0} \n", m_DicRefObjects.Count));
            foreach (RefObject refObj in m_DicRefObjects.Values)
            {
                sb.Append(string.Format("\t {0} | type<{1}> | ref -> {2}\n", refObj.target.name, refObj.target.GetType().Name, refObj.Refcount));
            }
            return StrBuilderCache.GetStringAndRelease(sb);
        }


    }



}
