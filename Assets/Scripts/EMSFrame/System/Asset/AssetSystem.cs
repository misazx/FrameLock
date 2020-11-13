//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

#if UNITY_EDITOR
#define __EDITOR_MODE__
#endif

using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;

namespace UnityFrame
{
    [System.Flags]
    public enum LoadAssetBundleOptions
    {
        UNLOAD_IN_NO_REF = 1,   //没有引用的时候释放
        DO_NOT_UNLOAD = 2,  //不释放
    }

    //管理加载AssetBundle，生成对象实例
    public class AssetSystem : HandleSingleton<AssetSystem>, IOnAwake, IOnUpdate, IOnReset
    {

        private float m_TimeStamp;

        private int m_MaxTickCount = 30;

        private float m_TickDuration = 1;

        private AssetBundleManifest m_Manifest;

        private Dictionary<string, AssetBundleData> m_DicBundleBuffer = new Dictionary<string, AssetBundleData>();

        //记录的依赖缓存
        private Dictionary<string, string[]> m_DicBundleDependenBuffer = new Dictionary<string, string[]>();

        //映射bundleName
        private Dictionary<string, string> m_DicMapBundleName = new Dictionary<string, string>();

        //Bundle 不释放标签
        private List<string> m_ListFlagBundleUnload = new List<string>();
        //Bundle 不检查引用标签
        private List<string> m_ListFlagBundleUndependen = new List<string>();

        public int count { get { return m_DicBundleBuffer.Count; } }

        public void UF_OnAwake() {
            //优先加载资源映射表
            AssetDataBases.UF_LoadAssetInfoFormAssetTable(GlobalPath.RawPath + "assettable.asset");
        }

        internal IEnumerator UF_InitAssetSystem()
        {
#if !__EDITOR_MODE__
            //审核状态下不加载本地
            if (!GlobalSettings.IsAppCheck)
                AssetDataBases.UF_LoadAssetInfoFromPersistent();      
            UF_InitBundleAssets();
#else
            //编辑器下默认加载
            AssetDataBases.UF_LoadAssetInfoFromPersistent();
#endif
            yield return null;
        }


        private void UF_InitBundleAssets()
        {
            AssetBundleData adb = UF_LoadAssetBundleDataFromFile("BundleAssets", LoadAssetBundleOptions.UNLOAD_IN_NO_REF);

            if (adb == null)
            {
                throw new System.Exception(string.Format("AssetSystem Exception:Can not Load BundleAssets"));
            }

            m_Manifest = adb.assetbundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            if (m_Manifest != null)
            {
                string[] bundels = m_Manifest.GetAllAssetBundles();
                if (bundels != null)
                {
                    for (int k = 0; k < bundels.Length; k++)
                    {
                        string bundleAssetName = UF_GetBundleAssetName(bundels[k]);
                        if (!m_DicMapBundleName.ContainsKey(bundleAssetName))
                        {
                            m_DicMapBundleName.Add(bundleAssetName, bundels[k]);
                        }
                        else
                        {
                            Debugger.UF_Error(string.Format("Same Key({0}) in AssetBundleManifest", bundleAssetName));
                        }
                    }
                }
            }
            adb.UF_Dispose(false);
        }


        //Load Object From Editor Database
        private Object UF_LoadObjectImageFromDatabase(string name){
#if __EDITOR_MODE__
			string prefix = GHelper.UF_GetNamePrefix(name);
			string[] suffix = {".prefab",".jpg",".png",".controller",".asset"};

            if (GlobalSettings.IsAppCheck) {
                for (int i = 0; i < suffix.Length; i++)
                {
                    string fullname = string.Format("{0}/AssetBases/ReviewAssets/Prefab/{1}{2}", GlobalPath.AppPath,name, suffix[i]);
                    if (System.IO.File.Exists(fullname))
                    {
                        string absname = string.Format("Assets/AssetBases/ReviewAssets/Prefab/{0}{1}", name, suffix[i]);
                        return UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(absname);
                    }
                }
            }

			for(int i = 0;i < suffix.Length;i++){
				string fullname = string.Format ("{0}/AssetBases/PrefabAssets/{1}/{2}{3}", GlobalPath.AppPath,prefix, name, suffix[i]);
				if(System.IO.File.Exists(fullname)){
                    string absname = string.Format ("Assets/AssetBases/PrefabAssets/{0}/{1}{2}",prefix, name, suffix[i]);
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(absname);
				}
			}

            Debugger.UF_Error(string.Format("AssetDatabase File Not Exist: {0}", name));
            return null;
#else
			Debugger.UF_Error(string.Format("Load [{0}] failed, LoadObjectImageFromDatabase()  Only Use In Editor",name));	
			return null;
#endif

        }

		public Object UF_LoadObject(string name){
			Object ret = null;
			int timestamp = GTime.EnvTick;
			Object image = UF_LoadObjectImage(name);
			if (image != null) {
				ret = Object.Instantiate (image);
                UF_RetainRef(name);
				ret.name = image.name;
			}
			Debugger.UF_TrackRes(name,Mathf.Abs(GTime.EnvTick - timestamp));
			return ret;
		}


		public Object UF_LoadObjectImage(string name){
            #if __EDITOR_MODE__
            return this.UF_LoadObjectImageFromDatabase(name);
			#else
			AssetBundleData abd = UF_LoadAssetBundleData (name, LoadAssetBundleOptions.UNLOAD_IN_NO_REF);
			if (abd != null) {
                Object ret = abd.UF_LoadAsset (name);
				return ret;
			}
			else{
				return null;
			}
			#endif
		}
			
		public int UF_AsyncLoadObject(string name,DelegateObject callback){
			DelegateObject finished = delegate(Object image) {
				Object ret = null;
				if(image != null){
					ret = Object.Instantiate(image);
                    UF_RetainRef(name);
					ret.name = image.name;
				}
				if(callback != null)
				callback(ret);
			};
			return UF_AsyncLoadObjectImage(name, finished);
		}

		public int UF_AsyncLoadObjectImage(string name,DelegateObject callback){
			return FrameHandle.UF_AddCoroutine (UF_IAsyncLoadObjectImage(name, callback));
		}

		IEnumerator UF_IAsyncLoadObjectImage(string name,DelegateObject callback){
			Object target = null;
			int timestamp = GTime.EnvTick;
			yield return null;
            #if __EDITOR_MODE__
            target = UF_LoadObjectImageFromDatabase(name);
            #else
            AssetBundleData abd = UF_LoadAssetBundleData(name,LoadAssetBundleOptions.UNLOAD_IN_NO_REF);
			if (abd != null) {
				AssetBundleRequest request = abd.UF_LoadAssetAsync<Object> (name);
				yield return request;
				target = request.asset;
			}
			#endif
			if (callback != null) {
				callback(target);
			}
			Debugger.UF_TrackRes(name,Mathf.Abs(GTime.EnvTick - timestamp));
		}


		public T UF_LoadObjectComponent<T>(string name){
			GameObject temp = UF_LoadObject(name) as GameObject;
			if (temp != null) {
				return temp.GetComponent<T> ();
			} else {
				return default(T);
			}
		}


		public T UF_LoadObjectImageComponent<T>(string name){
			GameObject temp = UF_LoadObjectImage(name) as GameObject;
			if (temp != null) {
				return temp.GetComponent<T> ();
			} else {
				return default(T);
			}
		}

		public void UF_DestroyObject(Object target){
			if (target != null) {
                UF_RelaseRef(target.name);
                if (target is MonoBehaviour)
                {
                    Object.Destroy((target as MonoBehaviour).gameObject);
                }
                else {
                    Object.Destroy(target);
                }
			}
		}
			
		


        //添加该Bundle 的引用计数
        //引用计数 +1
        public void UF_RetainRef(string bundleName){
			AssetBundleData bs = this.UF_GetAssetBundleDataFromBuffer(bundleName);
			if (bs != null) {
				bs.UF_Retain();
				//依赖资源同样计数+1
				string[] dependencies = this.UF_GetAllDependencies(bundleName);
				//避免递归导致资源相互引用而死循环
				if (dependencies != null) {
					for (int k = 0; k < dependencies.Length; k++) {
						AssetBundleData d_bs = this.UF_GetAssetBundleDataFromBuffer(dependencies[k]);
						if (d_bs != null) {
							d_bs.UF_Retain();
						}
					}
				}
			}
		}

		//释放Bundle 的引用计数
		//引用计数 -1
		public void UF_RelaseRef(string bundleName){
			AssetBundleData bs = this.UF_GetAssetBundleDataFromBuffer(bundleName);
			if (bs != null) {
				bs.UF_Release();
				//依赖资源同样计数-1
				string[] dependencies = this.UF_GetAllDependencies(bundleName);
				//避免递归导致资源相互引用而死循环
				if (dependencies != null) {
					for (int k = 0; k < dependencies.Length; k++) {
						AssetBundleData d_bs = this.UF_GetAssetBundleDataFromBuffer(dependencies[k]);
						if (d_bs != null) {
							d_bs.UF_Release();
						}
					}
				}
			}
		}


		private bool UF_CheckInFlag(List<string> list,string name){
			for (int k = 0; k < list.Count; k++) {
				if (name.IndexOf(list[k], System.StringComparison.Ordinal) > -1) {
					return true;
				}
			}
			return false;
		}


		private string UF_GetBundleAssetName(string name){
			int idx = name.LastIndexOf ('/');
			if (idx >= 0) {
				return name.Substring (idx+1);
			} else {
				return name;
			}
		}

		private string[] UF_GetAllDependencies(string bundleName){
			if (m_DicBundleDependenBuffer.ContainsKey (bundleName)) {
				return m_DicBundleDependenBuffer [bundleName];
			} else {
				string[] dependencies = null;
				if (m_DicMapBundleName.ContainsKey (bundleName) && m_Manifest != null) {
					dependencies = m_Manifest.GetAllDependencies (m_DicMapBundleName [bundleName]);

					if (dependencies != null && dependencies.Length > 0) {
						for (int k = 0; k < dependencies.Length; k++) {
							dependencies [k] = UF_GetBundleAssetName(dependencies [k]);
						}
						m_DicBundleDependenBuffer.Add (bundleName, dependencies);
					}
				}
				else{
					Debugger.UF_Error (string.Format ("BundleName[{0}] not Exist In DicMapBundleName", bundleName));
				}
				return dependencies;
			}
		}

		private void UF_CheckDependenBundles(string bundleName){
			if(m_Manifest == null){
				return;
			}
			string[] dependencies = this.UF_GetAllDependencies(bundleName);

			//检查是否存在不依赖标签中
			if(UF_CheckInFlag(m_ListFlagBundleUndependen,bundleName))
				return;

			if (dependencies != null) {
				for (int k = 0; k < dependencies.Length; k++) {
					if (UF_GetAssetBundleDataFromBuffer(dependencies[k]) == null) {
                        UF_LoadAssetBundleData(dependencies[k],LoadAssetBundleOptions.UNLOAD_IN_NO_REF);
					}
				}
			}
		}


		public void UF_AddUndependFlag(string val){
			m_ListFlagBundleUndependen.Add (val);
		}

		public void UF_AddUnloadFlag(string val){
			m_ListFlagBundleUnload.Add (val);
		}


		//处理特殊的组合bundle name
		protected string UF_WrapBundleName(string bundleName){
            int idx = bundleName.IndexOf('@');
            if (idx > -1) return bundleName.Substring(0, idx);
            return bundleName;
		}


        internal AssetBundleData UF_LoadAssetBundleData(string bundleName, LoadAssetBundleOptions flag) {
            try
            {
                bundleName = UF_WrapBundleName(bundleName);
                //检查BUFF中是否存在
                var abd = UF_GetAssetBundleDataFromBuffer(bundleName);

                if (abd != null) { return abd; }

                abd = UF_LoadAssetBundleDataFromFile(bundleName, flag);

                if (abd != null)
                {
                    UF_AddBundleDataToBuffer(abd);
                    //检查依赖
                    UF_CheckDependenBundles(bundleName);
                }
                else
                {
                    Debugger.UF_Error(string.Format("AssetBundleData[{0}] Load Failed", bundleName));
                }
                return abd;
            }
            catch (System.Exception e)
            {
                Debugger.UF_Exception(e);
            }
            return null;
        }


		//审核模式下尝试加载替换资源文件
        //替换资源固有前缀为"re_"
		private AssetBundleData UF_TryGetRebundleAsset(string bundleName){
            AssetBundleData ret = null;
            //固定加入前缀
            string rebundleName = string.Format("re_{0}", bundleName);
            var afi = AssetDataBases.UF_GetAssetInfo(rebundleName);
            if (afi == null || string.IsNullOrEmpty(afi.path)) return null;
            string path = afi.path;
            if (File.Exists(path)) {
                //rebundle 资源字节不加密不偏移
                ret = AssetBundleData.UF_LoadFromFile(bundleName, path, LoadAssetBundleOptions.UNLOAD_IN_NO_REF);
                Debugger.UF_Log(string.Format("Try Load Replacement AssetBundle[{0}] Success", bundleName));
            }
            return ret;
		}


		private AssetBundleData UF_LoadAssetBundleDataFromFile(string bundleName, LoadAssetBundleOptions flag)
        {
            AssetBundleData abd = null;
            
			//审核模式读取Raw中资源,并且需要resKey偏移
			if (GlobalSettings.IsAppCheck && GlobalSettings.IsRawAsset) {
				
                //尝试加载替换资源
                abd = UF_TryGetRebundleAsset(bundleName);
                if (abd != null) return abd;
                //
                var afi = AssetDataBases.UF_GetAssetInfo(bundleName);
				if (afi == null || string.IsNullOrEmpty(afi.path)) { 
                    Debugger.UF_Error(string.Format("bundleName:[{0}] AssetDataBases.UF_GetAssetInfo failed", bundleName));
					return null;
				}
				//需要写入文件偏移
				abd = AssetBundleData.UF_LoadFromStream(bundleName, afi.path, flag, GlobalSettings.EncBKey, GlobalSettings.ResBKey);
            } else {
                var afi = AssetDataBases.UF_GetAssetInfo(bundleName);
				if (afi == null || string.IsNullOrEmpty(afi.path)) { 
                    Debugger.UF_Error(string.Format("bundleName:[{0}] AssetDataBases.UF_GetAssetInfo failed", bundleName));
					return null;
				}
				//abd = AssetBundleData.LoadFromFile(bundleName, afi.path, flag);
				//需要管理Stream随AssetBundle 的释放去释放，否则Strem并不会自己释放导致IO读写错误
				abd = AssetBundleData.UF_LoadFromStream(bundleName, afi.path, flag,GlobalSettings.EncBKey,0);
            }
            return abd;
		}


		private void UF_AddBundleDataToBuffer(AssetBundleData abd)
        {
            if (abd == null) return;

			//检查是否存在不释放标签
			if(UF_CheckInFlag(m_ListFlagBundleUnload, abd.name))
                abd.flag = LoadAssetBundleOptions.DO_NOT_UNLOAD;

			if (!m_DicBundleBuffer.ContainsKey (abd.name)) {
				m_DicBundleBuffer.Add (abd.name, abd);
			} else {
				m_DicBundleBuffer [abd.name].UF_Dispose();
				m_DicBundleBuffer [abd.name] = abd;
                Debugger.UF_Error(string.Format("Same name AssetBunlde[{0}] loaded, AssetBundle has been replace", abd.name));
			}
        }


		private AssetBundleData UF_GetAssetBundleDataFromBuffer(string bundleName){
			if (m_DicBundleBuffer.ContainsKey (bundleName)) {
				return m_DicBundleBuffer [bundleName];
			}
			else{
				return null;
			}
		}

		private AssetBundle UF_GetAsestBundleFromBuffer(string bundleName){
			if (m_DicBundleBuffer.ContainsKey (bundleName)) {
				AssetBundleData bundle = m_DicBundleBuffer[bundleName];
				bundle.tick = 0;
				return bundle.assetbundle;
			}
			return null;
		}


        public static Object UF_LoadFromResources(string name)
        {
            var obj = Resources.Load(name);
            if (obj == null)
                return null;
            if (obj is GameObject)
            {
                GameObject clone = Object.Instantiate(obj) as GameObject;
                clone.name = obj.name;
                return clone;
            }
            else
            {
                return obj;
            }
        }


        public static T UF_LoadComponentFromResources<T>(string name) {
            var obj = UF_LoadFromResources(name);
            if (obj == null)
                return default(T);
            GameObject go = obj as GameObject;
            if (go == null)
                return default(T);
            return go.GetComponent<T>();
        }


        public void UF_ClearAll(bool unloadAllLoadedObjects)
        {
			List<string> listTemp = ListCache<string>.Acquire ();
			foreach (AssetBundleData item in m_DicBundleBuffer.Values) {
				listTemp.Add(item.name);
			}
			for (int k = 0; k < listTemp.Count; k++) {
				m_DicBundleBuffer[listTemp[k]].UF_Dispose(unloadAllLoadedObjects);
			}
			m_DicBundleBuffer.Clear ();
			ListCache<string>.Release (listTemp);

            Resources.UnloadUnusedAssets();
            //AssetBundle.UnloadAllAssetBundles(unloadAllLoadedObjects);
        }




		public void UF_OnUpdate(){
			float currentTime = GTime.Time;
			if((currentTime - m_TimeStamp) >= m_TickDuration) {
				m_TimeStamp = currentTime;
				List<string> listTemp = ListCache<string>.Acquire ();
				foreach (AssetBundleData item in m_DicBundleBuffer.Values) {
					if(item.assetbundle == null){
						listTemp.Add(item.name);
						continue;
					}
					if((item.flag == LoadAssetBundleOptions.UNLOAD_IN_NO_REF) && item.refCount <= 0){
						item.UF_Tick();
						if(item.tick > m_MaxTickCount){
							listTemp.Add(item.name);
						}
					}
				}
				for (int k = 0; k < listTemp.Count; k++) {
					m_DicBundleBuffer[listTemp[k]].UF_Dispose();
					m_DicBundleBuffer.Remove(listTemp[k]);
				}
				ListCache<string>.Release (listTemp);
			}
		}


        public void UF_OnReset() {
            m_DicMapBundleName.Clear();
            m_ListFlagBundleUnload.Clear();
            m_ListFlagBundleUndependen.Clear();
        }




        public override string ToString ()
		{
			System.Text.StringBuilder body = StrBuilderCache.Acquire ();
			int count = m_DicBundleBuffer.Values.Count;
			body.AppendFormat("[{0}/{1}] [AssAssetBundle] <{2}>\n",(int)m_TickDuration,m_MaxTickCount,count);
			foreach (AssetBundleData item in m_DicBundleBuffer.Values) {
                body.AppendFormat("{0} | <color=yellow>ref -> {2}</color> <color={4}>flag[{1}]</color> <color={5}>tick [{3}]</color>\n",
                    item.name, item.flag, item.refCount, item.tick,
                    item.flag == LoadAssetBundleOptions.UNLOAD_IN_NO_REF ? "green" : "white",
                    item.tick > 0?"red":"grey");
            }
			return StrBuilderCache.GetStringAndRelease(body);
		}
	

	}
}

