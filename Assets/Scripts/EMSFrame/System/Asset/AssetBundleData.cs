//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------
using System.IO;
using UnityEngine;

namespace UnityFrame
{

    public class AssetBundleData
    {
        public string name { get; private set; }
        //引用计数
        public int refCount { get; private set; }
        public AssetBundle assetbundle { get; private set; }
        internal AssetBundleStream stream { get; private set; }
        public LoadAssetBundleOptions flag { get; set; }
        public int tick { get; set; }
        public bool isDispose { get; private set; }
        //数据回收池
        static TStack<AssetBundleData> s_DataPool = new TStack<AssetBundleData>(128);

        public void UF_Tick()
        {
            if (isDispose) return;
            tick++;
        }

        //引用计数 +1
        public void UF_Retain()
        {
            if (isDispose) return;
            refCount++;
            //重置周期
            tick = 0;
        }
        //引用计数 -1
        public void UF_Release()
        {
            if (isDispose) return;
            refCount--;
        }


        public UnityEngine.Object UF_LoadAsset(string strName) {
            if (assetbundle == null) return null;
            return assetbundle.LoadAsset(strName);
        }


        public T UF_LoadAsset<T>(string strName) where T : UnityEngine.Object
        {
            if (assetbundle == null) return null;
            return assetbundle.LoadAsset(strName) as T;
        }

        public UnityEngine.AssetBundleRequest UF_LoadAssetAsync(string strName) {
            if (assetbundle == null) return null;
            return assetbundle.LoadAssetAsync(strName);
        }

        public UnityEngine.AssetBundleRequest UF_LoadAssetAsync<T>(string strName) where T : UnityEngine.Object
        {
            if (assetbundle == null) return null;
            return assetbundle.LoadAssetAsync<T>(strName);
        }

        //释放
        public void UF_Dispose(bool unloadAllLoadedObjects = false)
        {
            if (this.isDispose) return;
            if (assetbundle != null)
            {
                assetbundle.Unload(unloadAllLoadedObjects);
                assetbundle = null;
            }
            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
                stream = null;
            }
            if (!isDispose)
            {
                this.isDispose = true;
                s_DataPool.Push(this);
            }
        }

        ~AssetBundleData() {
            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
                stream = null;
            }
        }


        //获取请求
        static AssetBundleData UF_Acquire(string bundleName,AssetBundle ab,AssetBundleStream stream, LoadAssetBundleOptions flag)
        {
            AssetBundleData ret = null;

            lock (s_DataPool)
            {
                ret = s_DataPool.Pop();
                if (ret == null)
                    ret = new AssetBundleData();
            }
            ret.name = bundleName;
            ret.assetbundle = ab;
            ret.stream = stream;
            ret.flag = flag;
            ret.tick = 0;
            ret.isDispose = false;
            return ret;
        }

        public static AssetBundleData UF_LoadFromStream(string bundleName, string path, LoadAssetBundleOptions flag, int byteKey = 0,int byteofs = 0)
        {
            AssetBundleStream abstream = null;
            try
            {
                if (!File.Exists(path)) {
                    Debugger.UF_Error(string.Format("File[{0}] not exist ,LoadFromStream failed", path));
                    return null;
                }
                abstream = new AssetBundleStream(path, FileMode.Open, FileAccess.Read, byteKey, byteofs);
                AssetBundle ab = AssetBundle.LoadFromStream(abstream);
                if (ab != null)
                {
                    return AssetBundleData.UF_Acquire(bundleName, ab, abstream, flag);
                }
            }
            catch (System.Exception e)
            {
                Debugger.UF_Exception(e);
            }
            if (abstream != null)
                abstream.Close(); abstream.Dispose();
            return null;
        }

        public static AssetBundleData UF_LoadFromFile(string bundleName, string path, LoadAssetBundleOptions flag)
        {
            try
            {
                AssetBundle ab = AssetBundle.LoadFromFile(path);
                if (ab != null)
                {
                    return AssetBundleData.UF_Acquire(bundleName, ab, null, flag);
                }
            }
            catch (System.Exception e)
            {
                Debugger.UF_Exception(e);
            }
            return null;
        }


    }
}