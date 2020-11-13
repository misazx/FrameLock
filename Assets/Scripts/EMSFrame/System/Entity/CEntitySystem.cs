//-----------------------------------------------------------
// Copyright (c) 2017-2019 chanjanequan
//-----------------------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace UnityFrame
{
	//管理实体的创建和回收重复利用
	public class CEntitySystem : HandleSingleton<CEntitySystem>,IOnStart,IOnUpdate,IOnSyncUpdate,IOnSecnodUpdate
	{
		private float m_DisposeTime = 40;

		private Transform m_RootReleasePool;

		//激活的实体对象
		private NList<IEntityHnadle> m_ListActive = new NList<IEntityHnadle> ();
		//回收池中的实体对象
		private NPool<string,IEntityHnadle> m_PoolEntitys = new NPool<string, IEntityHnadle> (true);

		//实体总数量
		public int ActiveCount{get{return m_ListActive.Count;}}
		public int PoolCount{get{ return m_PoolEntitys.Count;}}
		public float DisposeTime{get{ return m_DisposeTime;}set{ m_DisposeTime = value;}}

		private DelegateNForeach<IEntityHnadle> m_PtrUpdateActiveEntity;
        private DelegateNForeach<IEntityHnadle> m_PtrFixedUpdateActiveEntity;
        private DelegateNForeach<IEntityHnadle> m_PtrUpdatePoolEntity;

        public int layerMaskUpdate { get; set; }

        protected override void OnInstanced ()
		{
			base.OnInstanced ();
			//先变换类型，减少转换GC
			m_PtrUpdateActiveEntity = UF_UpdateActiveEntity;
			m_PtrUpdatePoolEntity = UF_UpdatePoolEntity;
            m_PtrFixedUpdateActiveEntity = UF_FixedUpdateActiveEntity;
        }


		private void UF_ActiveEntity(IEntityHnadle entity){
			if (entity != null) {
				m_ListActive.UF_Add (entity);
                entity.isReleased = false;
                entity.isActive = true;
                entity.timeStamp = GTime.Time;
                if (entity is MonoBehaviour)
                    (entity as MonoBehaviour).transform.SetParent(null);
                if (entity is IOnStart)
				    (entity as IOnStart).UF_OnStart ();
			}
		}

		private void UF_ReleaseEntity(IEntityHnadle entity){
			if (entity != null) {
				m_PoolEntitys.UF_Add(entity.name, entity);
                entity.isReleased = true;
                entity.isActive = false;
                entity.timeStamp = GTime.Time;
                entity.eLayer = 0;
                if (entity is MonoBehaviour)
                    (entity as MonoBehaviour).transform.SetParent (m_RootReleasePool);
				if (entity is IOnReset)
					(entity as IOnReset).UF_OnReset();
			}
		}
			

		/// <summary>
		/// 同步创建实体
		/// </summary>
		public IEntityHnadle UF_Create(string name){
			//再回收池中查找
			IEntityHnadle entity = m_PoolEntitys.UF_Pop(name);

			if (entity != null) {
                UF_ActiveEntity(entity);
				return entity;
			}

			//加载创建新的实例
			entity = AssetSystem.UF_GetInstance ().UF_LoadObjectComponent<IEntityHnadle> (name);
			if (entity != null) {
                if (entity is IOnAwake)
                    (entity as IOnAwake).UF_OnAwake();
                UF_ActiveEntity(entity);
			}

			return entity;
		}


        public Object UF_CreateObject(string name) {
            return this.UF_Create(name) as Object;
        }


		/// <summary>
		/// 同步创建实体T
		/// </summary>
		internal T UF_Create<T>(string name) where T : UnityEngine.Object{
			return this.UF_Create(name) as T;
		}


        //在对象池中创建
        public IEntityHnadle UF_CreateFromPool(string name) {
            return m_PoolEntitys.UF_Pop(name);
        }



		/// <summary>
		/// 异步创建实体
		/// </summary>
		public int UF_AsyncCreate(string name, DelegateObject callback){
			IEntityHnadle val = m_PoolEntitys.UF_Pop(name);
			if (val != null) {
                UF_ActiveEntity(val);
				if (callback != null) {callback (val as Object);}
				return 0;
			}

			//异步加载
			return AssetSystem.UF_GetInstance ().UF_AsyncLoadObject(name,
				(param)=>{
					if(param == null){
						Debugger.UF_Error(string.Format("Create Entity[{0}] Failed,Entity Asset Not Exist!",name));
                        if (callback != null) callback(null);
                        return;
					}
					GameObject entityGo = param as GameObject;
					if(entityGo == null){
						Debugger.UF_Error(string.Format("Create Entity[{0}] Failed,Asset Entity Is Not GameObject Type!",name));
                        if (callback != null) callback(null);
                        return;
					}
					IEntityHnadle entity = entityGo.GetComponent<IEntityHnadle>();
					if(entity == null){
						Debugger.UF_Error(string.Format("Create Entity[{0}] Failed,GameObject Count Not Find IEntityHnadle Component!",name));
                        if (callback != null) callback(null);
                        return;
					}
                    if (entity is IOnAwake)
                        (entity as IOnAwake).UF_OnAwake();
                    UF_ActiveEntity(entity);
					if(callback != null){callback(entity as Object);}
				}
			);
		} 
			
		private bool UF_UpdateActiveEntity(IEntityHnadle entity){
			if (entity != null && !entity.Equals(null)) {
				//是否被标记释放
				if (entity.isReleased) {
                    UF_ReleaseEntity(entity);
					return false;
				} else {
					if ((layerMaskUpdate & entity.eLayer) == 0 && entity.isActive) {
						if (entity is IOnUpdate) (entity as IOnUpdate).UF_OnUpdate();
                    }
					return true;
				}
			} else {
				Debugger.UF_Error ("UpdateActiveEntity: Get Null Node<IEntityHnadle> in Update,IEntityHnadle has benn Destroyed out of EntityManager");
				return false;
			}				
		}

        private bool UF_FixedUpdateActiveEntity(IEntityHnadle entity)
        {
            if (entity != null && !entity.Equals(null))
            {
                //是否被标记释放
                if (entity.isReleased)
                {
                    UF_ReleaseEntity(entity);
                    return false;
                }
                else
                {
                    if ((layerMaskUpdate & entity.eLayer) == 0 && entity.isActive)
                    {
                        if (entity is IOnSyncUpdate) (entity as IOnSyncUpdate).UF_OnSyncUpdate();
                    }
                    return true;
                }
            }
            else
            {
                Debugger.UF_Error("FixedUpdateActiveEntity: Get Null Node<IEntityHnadle> in Update,IEntityHnadle has benn Destroyed out of EntityManager");
                return false;
            }
        }

        private bool UF_UpdatePoolEntity(IEntityHnadle entity){
			if (entity != null) {
				//是否需要销毁
				if((GTime.Time - entity.timeStamp) >= DisposeTime){
                    if(entity is IOnDestroyed)
                        (entity as IOnDestroyed).UF_OnDestroyed();
                    AssetSystem.UF_GetInstance ().UF_DestroyObject(entity as Object);
					return false;
				}	
				else{
					return true;
				}
			} else {
				Debugger.UF_Error ("UpdateActiveEntity: Get Null Node<IEntityHnadle> in Update,IEntityHnadle has benn Destroyed out of EntityManager");
				return false;
			}	
		}


		public void UF_OnStart(){
			if (m_RootReleasePool == null) {
				GameObject go = new GameObject ("EntityPool");
				go.SetActive (false);
				m_RootReleasePool = go.transform;
				m_RootReleasePool.position = Vector3.zero;
				m_RootReleasePool.eulerAngles = Vector3.zero;
				m_RootReleasePool.localScale = Vector3.one;
			}
		}


		public void UF_OnUpdate(){
			m_ListActive.UF_NForeach (m_PtrUpdateActiveEntity);
		}

        public void UF_OnSyncUpdate() {
            m_ListActive.UF_NForeach(m_PtrFixedUpdateActiveEntity);
        }

		public void UF_OnSecnodUpdate(){
			m_PoolEntitys.UF_NForeach(m_PtrUpdatePoolEntity);
		}
			
		//添加到场景
		public bool UF_AddToActivity(IEntityHnadle entity){
			if (entity == null)
				return false;
			if (!m_ListActive.UF_Exist(entity)) {
				m_ListActive.UF_Add(entity);
				return true;
			} else {
				Debugger.UF_Warn (string.Format ("IEntityHnadle[{0}] has already exist in Active Entitys", entity.name));
				return false;
			}
		}


		//筛选选择
		public IEntityHnadle UF_Select(DelegateNForeach<IEntityHnadle> entity){
			return m_ListActive.UF_Select(entity);
		}

        //释放指定层所有的Entity
        public void UF_ReleaseLayer(int layer) {
            m_ListActive.UF_NForeach((entity) => {
                if ((layer & entity.eLayer) > 0) {
                    entity.isReleased = true;
                }
                return true;
            });
        }


        //清除全部实体对象
        public void UF_ClearAll() {
            m_ListActive.UF_NForeach((entity) => {
                AssetSystem.UF_GetInstance().UF_DestroyObject(entity as Object);
                return false;
            });
            m_PoolEntitys.UF_NForeach((entity) => {
                AssetSystem.UF_GetInstance().UF_DestroyObject(entity as Object);
                return false;
            });
        }

		public string UF_GetActiveEntityInfo(){
			if (m_ListActive.Count > 0) {
				System.Text.StringBuilder infoStr = StrBuilderCache.Acquire ();
				var dicCache = DictionaryCache<string,int>.Acquire ();
				m_ListActive.UF_NForeach(
					(e)=>{
						if(e != null){
							if(!dicCache.ContainsKey(e.name))
								dicCache.Add(e.name,0);
							dicCache[e.name] = dicCache[e.name] + 1;
						}
						return true;
					}
				);
				foreach (KeyValuePair<string,int> item in dicCache) {
					if (item.Value > 0) {
						infoStr.AppendLine (string.Format ("{0} : {1}", item.Key, item.Value));
					}
				}
				DictionaryCache<string,int>.Release (dicCache);
				return StrBuilderCache.GetStringAndRelease (infoStr);
			}
			return string.Empty;
		}

		public string UF_GetPoolEntityInfo(){
			if (m_PoolEntitys.Count > 0) {
				var dicCache = DictionaryCache<string,int>.Acquire ();
				System.Text.StringBuilder infoStr = StrBuilderCache.Acquire ();
				infoStr.Remove (0, infoStr.Length);
				m_PoolEntitys.UF_NForeach(
					(e)=>{
						if(e != null){
							if(!dicCache.ContainsKey(e.name))
								dicCache.Add(e.name,0);
							dicCache[e.name] = dicCache[e.name] + 1;
						}
						return true;
					}
				);
				foreach (KeyValuePair<string,int> item in dicCache) {
					if (item.Value > 0) {
						int tick = (int)(GTime.Time - m_PoolEntitys.UF_Get(item.Key).timeStamp);
						infoStr.AppendLine (string.Format ("{0} : {1}  ->  {2}", item.Key, item.Value,tick));
					}
				}
				DictionaryCache<string,int>.Release (dicCache);
				return StrBuilderCache.GetStringAndRelease (infoStr);
			}
			return string.Empty;
		
		}
			
	}
}